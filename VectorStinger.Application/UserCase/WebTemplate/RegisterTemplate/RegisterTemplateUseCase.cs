using FluentResults;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Formats.Tiff.Compression.Decompressors;
using VectorSinger.Modules.WebTemplate.Managers;
using VectorStinger.Application.UserCase.WebTemplate.RegisterQuestionsQuiz;
using VectorStinger.Core.Interfaces.Infrastructure.Bucket;
using VectorStinger.Core.Interfaces.Managers.WebTemplate;
using VectorStinger.Foundation.Abstractions.UserCase;
using VectorStinger.Infrastructure.DataAccess.Interface;

namespace VectorSinger.Modules.WebTemplate.UseCases.RegisterTemplate;

/// <summary>
/// Caso de uso para registrar un nuevo template web
/// </summary>
public class RegisterTemplateUseCase : BaseUseCase<RegisterTemplateInput, RegisterTemplateOutput, RegisterTemplateValidation>
{
    private readonly ITemplateManager _templateManager;
    private readonly IFakeDomainManager _fakeDomainManager;
    private readonly IBucketService _bucketService;
    private readonly ILogger<RegisterTemplateUseCase> _logger;





    public RegisterTemplateUseCase(
        RegisterTemplateInput registerTemplateInput,
        RegisterTemplateValidation validationRules,
        IRepository repository,
        ITemplateManager templateManager,
        IFakeDomainManager fakeDomainManager,
        IBucketService bucketService,
        ILogger<RegisterTemplateUseCase> logger
        )
        : base(registerTemplateInput, validationRules, repository, logger)
    {
        _templateManager = templateManager ?? throw new ArgumentNullException(nameof(templateManager));
        _fakeDomainManager = fakeDomainManager ?? throw new ArgumentNullException(nameof(fakeDomainManager));
        _bucketService = bucketService ?? throw new ArgumentNullException(nameof(bucketService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Description = "Register a new web template with file upload to S3 and metadata storage";
        Summary = "Registers a new web template, uploads the template file to S3, creates template and fake domain records";

    }

    public override async Task<Result<RegisterTemplateOutput>> ExecuteBusinessAsync(RegisterTemplateInput input)
    {


        try
        {
            var startTime = DateTime.UtcNow;
            _logger.LogInformation(
                "Starting RegisterTemplate: UserId={UserId}, SubportalName={SubportalName}, PublishNow={PublishNow}, FileSize={FileSize}",
                input.UserId, input.SubportalName, input.PublishNow, input.TemplateFile.Length);



            // Paso 1: Validar input
            var validationResult = ValidateInput(input);
            if (validationResult.IsFailed)
            {
                return validationResult;
            }

            // Paso 2: Sanitizar nombre del subportal
            var sanitizedSubportalName = _fakeDomainManager.SanitizeSubportalName(input.SubportalName);
            var fullDomainName = $"{sanitizedSubportalName}.{input.BaseDomain}";

            _logger.LogInformation("Sanitized subportal name: {SubportalName} -> {SanitizedName}",
                input.SubportalName, sanitizedSubportalName);

            // Paso 3: Verificar si el dominio ya existe
            var domainExists = await _fakeDomainManager.CheckDomainExistsAsync(fullDomainName);
            if (domainExists)
            {
                _logger.LogWarning("Domain already exists: {DomainName}", fullDomainName);

                return Result.Fail<RegisterTemplateOutput>($"Domain '{fullDomainName}' is already taken");
            }

            // Paso 4: Subir archivo a S3
            _logger.LogInformation("Uploading template file to S3: FileName={FileName}", input.TemplateFile.FileName);

            var uploadStartTime = DateTime.UtcNow;

            using var fileStream = input.TemplateFile.OpenReadStream();
            var metadata = new Dictionary<string, string>
            {
                ["userId"] = input.UserId.ToString(),
                ["subportalName"] = sanitizedSubportalName,
                ["originalFileName"] = input.TemplateFile.FileName,
                ["uploadedAt"] = DateTime.UtcNow.ToString("o")
            };

            var uploadResult = await _bucketService.UploadFileAsync(
                fileStream,
                input.TemplateFile.FileName,
                input.TemplateFile.ContentType,
                metadata);

            if (uploadResult.IsFailed)
            {
                _logger.LogError("Failed to upload template file: {Errors}",
                    string.Join(", ", uploadResult.Errors.Select(e => e.Message)));

                return Result.Fail<RegisterTemplateOutput>(uploadResult.Errors);
            }

            var uploadDuration = DateTime.UtcNow - uploadStartTime;

            var s3Result = uploadResult.Value;
            _logger.LogInformation("Template file uploaded successfully: FileUrl={FileUrl}, FileKey={FileKey}",
                s3Result.FileUrl, s3Result.FileKey);

            // Paso 5: Crear registro de Template
            var templateResult = await _templateManager.CreateTemplateAsync(
                input.UserId,
                s3Result.FileUrl,
                s3Result.FileKey,
                input.PublishNow);

            if (templateResult.IsFailed)
            {
                _logger.LogError("Failed to create template record: {Errors}",
                    string.Join(", ", templateResult.Errors.Select(e => e.Message)));

                // Intentar eliminar archivo de S3 si falla la creación del template
                await _bucketService.DeleteFileAsync(s3Result.FileKey);



                return Result.Fail<RegisterTemplateOutput>(templateResult.Errors);
            }

            var template = templateResult.Value;
            _logger.LogInformation("Template record created: TemplateId={TemplateId}", template.TemplateId);

            // Paso 6: Crear registro de FakeDomain
            var fakeDomainResult = await _fakeDomainManager.CreateFakeDomainAsync(
                input.UserId,
                template.TemplateId,
                sanitizedSubportalName,
                input.BaseDomain);

            if (fakeDomainResult.IsFailed)
            {
                _logger.LogError("Failed to create fake domain record: {Errors}",
                    string.Join(", ", fakeDomainResult.Errors.Select(e => e.Message)));

                return Result.Fail<RegisterTemplateOutput>(fakeDomainResult.Errors);
            }

            var fakeDomain = fakeDomainResult.Value;
            _logger.LogInformation("Fake domain record created: DomainId={DomainId}, DomainName={DomainName}",
                fakeDomain.DomainId, fakeDomain.DomainName);

            // Paso 7: Crear output
            var output = new RegisterTemplateOutput
            {
                TemplateId = template.TemplateId,
                FakeDomainId = fakeDomain.DomainId,
                FullDomainName = fakeDomain.DomainName,
                TemplateFileUrl = s3Result.FileUrl,
                TemplateFileKey = s3Result.FileKey,
                IsPublished = input.PublishNow,
                CreatedAt = template.CreatedAt ?? DateTime.UtcNow,
                Message = input.PublishNow
                    ? $"Template registered and published successfully at {fakeDomain.DomainName}"
                    : $"Template registered successfully (not published) at {fakeDomain.DomainName}"
            };

            var totalDuration = DateTime.UtcNow - startTime;


            _logger.LogInformation(
                "RegisterTemplate completed successfully: TemplateId={TemplateId}, DomainId={DomainId}, Duration={Duration}ms",
                output.TemplateId, output.FakeDomainId, totalDuration.TotalMilliseconds);



            return Result.Ok(output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in RegisterTemplate for UserId={UserId}", input.UserId);



            return Result.Fail<RegisterTemplateOutput>($"Unexpected error: {ex.Message}");
        }
    }

    private Result<RegisterTemplateOutput> ValidateInput(RegisterTemplateInput input)
    {
        var errors = new List<string>();

        if (input.UserId <= 0)
        {
            errors.Add("UserId must be greater than 0");
        }

        if (string.IsNullOrWhiteSpace(input.SubportalName))
        {
            errors.Add("SubportalName is required");
        }

        if (input.TemplateFile == null || input.TemplateFile.Length == 0)
        {
            errors.Add("TemplateFile is required and cannot be empty");
        }

        // Validar tamaño máximo (ej: 100 MB)
        const long maxFileSize = 100 * 1024 * 1024;
        if (input.TemplateFile?.Length > maxFileSize)
        {
            errors.Add($"TemplateFile size exceeds maximum allowed size of {maxFileSize / (1024 * 1024)} MB");
        }

        // Validar extensión del archivo (debe ser ZIP)
        if (input.TemplateFile != null)
        {
            var extension = Path.GetExtension(input.TemplateFile.FileName).ToLowerInvariant();
            if (extension != ".zip")
            {
                errors.Add("TemplateFile must be a ZIP file");
            }
        }

        if (errors.Any())
        {
            _logger.LogWarning("Input validation failed: {Errors}", string.Join(", ", errors));
            return Result.Fail<RegisterTemplateOutput>(string.Join("; ", errors));
        }

        return Result.Ok<RegisterTemplateOutput>(null!);
    }
}
