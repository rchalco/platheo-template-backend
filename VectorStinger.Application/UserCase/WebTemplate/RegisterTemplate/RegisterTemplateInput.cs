using Microsoft.AspNetCore.Http;
using VectorStinger.Foundation.Abstractions.UserCase;

namespace VectorSinger.Modules.WebTemplate.UseCases.RegisterTemplate;

/// <summary>
/// Input para registrar un nuevo template web
/// </summary>
public class RegisterTemplateInput : IUseCaseInput
{
    /// <summary>
    /// ID del usuario propietario del template
    /// </summary>
    public required int UserId { get; init; }

    /// <summary>
    /// Nombre del subportal (dominio)
    /// Example: subportal-name
    /// </summary>
    public required string SubportalName { get; init; }

    /// <summary>
    /// Indica si el template debe publicarse inmediatamente
    /// </summary>
    public bool PublishNow { get; init; } = false;

    /// <summary>
    /// Archivo ZIP del template (HTML, CSS, JS, images, etc.)
    /// </summary>
    public required IFormFile TemplateFile { get; init; }

    /// <summary>
    /// URL base del dominio principal
    /// Example: www.platheo.com
    /// </summary>
    public string BaseDomain { get; init; } = "www.platheo.com";
}
