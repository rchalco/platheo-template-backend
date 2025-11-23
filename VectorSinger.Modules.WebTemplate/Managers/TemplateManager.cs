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
/// Manager para operaciones de Template
/// </summary>
public class TemplateManager : BaseManager<BdPlatheoTemplateContext>, ITemplateManager
{
    
    private readonly ILogger<TemplateManager> _logger;

    public TemplateManager(
        IRepository repository,
        ILogger<TemplateManager> logger)
        : base(repository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Crea un nuevo template en la base de datos
    /// </summary>
    public async Task<Result<Template>> CreateTemplateAsync(
        int userId,
        string templateFileUrl,
        string templateFileKey,
        bool isPublished)
    {
        try
        {
            var template = new Template
            {
                UserId = userId,
                TemplateUrl = templateFileUrl,
                TemplateFileKey = templateFileKey,
                IsActive = isPublished,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Creating template: UserId={UserId}, FileUrl={FileUrl}, IsActive={IsActive}",
                userId, templateFileUrl, isPublished);

            await _repository.SaveObjectAsync(new Entity<Template>
            {
                EntityDB = template,
                stateEntity = StateEntity.add
            }, default);

            _logger.LogInformation("Template created successfully: TemplateId={TemplateId}", template.TemplateId);

            return Result.Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template for UserId={UserId}", userId);
            return Result.Fail<Template>($"Error creating template: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene un template por ID
    /// </summary>
    public async Task<Result<Template>> GetTemplateByIdAsync(int templateId)
    {
        try
        {
            var template = await _repository.SimpleSelectAsync<Template>(x => x.TemplateId == templateId);

            if (template == null || template.Count == 0)
            {
                return Result.Fail<Template>($"Template with ID {templateId} not found");
            }

            return Result.Ok(template.First());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template: TemplateId={TemplateId}", templateId);
            return Result.Fail<Template>($"Error getting template: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza el estado de publicación de un template
    /// </summary>
    public async Task<Result> UpdatePublishStatusAsync(int templateId, bool isActive)
    {
        try
        {
            var templateResult = await GetTemplateByIdAsync(templateId);

            if (templateResult.IsFailed)
            {
                return templateResult.ToResult();
            }

            templateResult.Value.IsActive = isActive;
            templateResult.Value.UpdatedAt = DateTime.UtcNow;

            await _repository.SaveObjectAsync(new Entity<Template>
            {
                EntityDB = templateResult.Value,
                stateEntity = StateEntity.modify
            });


            _logger.LogInformation(
                "Template publish status updated: TemplateId={TemplateId}, IsActive={IsActive}",
                templateId, isActive);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template status: TemplateId={TemplateId}", templateId);
            return Result.Fail($"Error updating template status: {ex.Message}");
        }
    }
}
