using VectorStinger.Foundation.Abstractions.UserCase;

namespace VectorSinger.Modules.WebTemplate.UseCases.RegisterTemplate;

/// <summary>
/// Output del registro de template
/// </summary>
public class RegisterTemplateOutput : IUseCaseOutput
{
    /// <summary>
    /// ID del template creado
    /// </summary>
    public required int TemplateId { get; init; }

    /// <summary>
    /// ID del dominio fake creado
    /// </summary>
    public required int FakeDomainId { get; init; }

    /// <summary>
    /// Nombre completo del dominio generado
    /// Example: subportal-name.www.platheo.com
    /// </summary>
    public required string FullDomainName { get; init; }

    /// <summary>
    /// URL del archivo del template en S3
    /// </summary>
    public required string TemplateFileUrl { get; init; }

    /// <summary>
    /// Key del archivo en S3 (para referencia)
    /// </summary>
    public required string TemplateFileKey { get; init; }

    /// <summary>
    /// Indica si el template fue publicado
    /// </summary>
    public bool IsPublished { get; init; }

    /// <summary>
    /// Fecha de creación
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Mensaje descriptivo del resultado
    /// </summary>
    public string Message { get; init; } = "Template registered successfully";
}
