using FluentResults;
using Microsoft.Extensions.Logging;
using VectorStinger.Core.Domain.DataBase.DataContext;
using VectorStinger.Core.Domain.DataBase.Models;
using VectorStinger.Core.Interfaces.Managers.WebTemplate;
using VectorStinger.Foundation.Abstractions.Manager;
using VectorStinger.Infrastructure.DataAccess.Interface;
using VectorStinger.Infrastructure.DataAccess.Wrapper;

namespace VectorSinger.Modules.WebTemplate.Managers;

/// <summary>
/// Manager para operaciones de FakeDomain
/// </summary>
public class FakeDomainManager : BaseManager<BdPlatheoTemplateContext>, IFakeDomainManager
{
    private readonly ILogger<FakeDomainManager> _logger;

    public FakeDomainManager(
        IRepository repository,
        ILogger<FakeDomainManager> logger)
        : base(repository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Crea un nuevo dominio fake
    /// </summary>
    public async Task<Result<FakeDomain>> CreateFakeDomainAsync(
        int userId,
        int templateId,
        string subportalName,
        string baseDomain)
    {
        try
        {
            // Generar nombre completo del dominio
            var fullDomainName = $"{subportalName}.{baseDomain}";

            // Verificar si el dominio ya existe
            var existingDomain = await CheckDomainExistsAsync(fullDomainName);
            if (existingDomain)
            {
                _logger.LogWarning("Domain already exists: {DomainName}", fullDomainName);
                return Result.Fail<FakeDomain>($"Domain '{fullDomainName}' is already taken");
            }

            var fakeDomain = new FakeDomain
            {
                UserId = userId,
                TemplateId = templateId,
                DomainName = fullDomainName,
                IsAvailable = true,
                ConfiguredAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Creating fake domain: UserId={UserId}, TemplateId={TemplateId}, DomainName={DomainName}",
                userId, templateId, fullDomainName);

            _repository.SaveObject<FakeDomain>(new Entity<FakeDomain>
            {
                EntityDB = fakeDomain,
                stateEntity = StateEntity.add
            });


            _logger.LogInformation(
                "Fake domain created successfully: DomainId={DomainId}, DomainName={DomainName}",
                fakeDomain.DomainId, fullDomainName);

            return Result.Ok(fakeDomain);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fake domain: SubportalName={SubportalName}", subportalName);
            return Result.Fail<FakeDomain>($"Error creating fake domain: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si un dominio ya existe
    /// </summary>
    public async Task<bool> CheckDomainExistsAsync(string domainName)
    {
        try
        {
            var exists = _repository.SimpleSelect<FakeDomain>(d => d.DomainName == domainName);
            return exists.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking domain existence: DomainName={DomainName}", domainName);
            throw;
        }
    }

    /// <summary>
    /// Obtiene un dominio fake por ID
    /// </summary>
    public async Task<Result<FakeDomain>> GetFakeDomainByIdAsync(int domainId)
    {
        try
        {
            var domain = _repository.SimpleSelect<FakeDomain>(x => x.DomainId == domainId);

            if (domain == null || domain.Count == 0)
            {
                return Result.Fail<FakeDomain>($"Fake domain with ID {domainId} not found");
            }

            return Result.Ok(domain.First());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting fake domain: DomainId={DomainId}", domainId);
            return Result.Fail<FakeDomain>($"Error getting fake domain: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza la disponibilidad de un dominio
    /// </summary>
    public async Task<Result> UpdateDomainAvailabilityAsync(int domainId, bool isAvailable)
    {
        try
        {
            var resultDomain = await GetFakeDomainByIdAsync(domainId);

            if (resultDomain.IsFailed)
            {
                return resultDomain.ToResult();
            }

            resultDomain.Value.IsAvailable = isAvailable;

            _repository.SaveObject(new Entity<FakeDomain>
            {
                EntityDB = resultDomain.Value,
                stateEntity = StateEntity.modify
            });

            _logger.LogInformation(
                "Fake domain availability updated: DomainId={DomainId}, IsAvailable={IsAvailable}",
                domainId, isAvailable);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating domain availability: DomainId={DomainId}", domainId);
            return Result.Fail($"Error updating domain availability: {ex.Message}");
        }
    }

    /// <summary>
    /// Genera un nombre de subportal válido
    /// </summary>
    public string SanitizeSubportalName(string subportalName)
    {
        if (string.IsNullOrWhiteSpace(subportalName))
        {
            throw new ArgumentException("Subportal name cannot be empty", nameof(subportalName));
        }

        // Convertir a minúsculas
        var sanitized = subportalName.ToLowerInvariant();

        // Reemplazar espacios con guiones
        sanitized = sanitized.Replace(" ", "-");

        // Remover caracteres no permitidos (solo letras, números y guiones)
        sanitized = new string(sanitized.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

        // Remover guiones consecutivos
        while (sanitized.Contains("--"))
        {
            sanitized = sanitized.Replace("--", "-");
        }

        // Remover guiones al inicio y final
        sanitized = sanitized.Trim('-');

        return sanitized;
    }
}
