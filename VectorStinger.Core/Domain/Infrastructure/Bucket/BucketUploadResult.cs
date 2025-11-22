namespace VectorStinger.Core.Domain.Infrastructure.Bucket;

/// <summary>
/// Resultado de la subida de un archivo al bucket storage
/// </summary>
public class BucketUploadResult
{
    /// <summary>
    /// URL pública o key del archivo subido
    /// </summary>
    public required string FileUrl { get; init; }

    /// <summary>
    /// Nombre del archivo
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Key/ruta del archivo en el bucket
    /// </summary>
    public required string FileKey { get; init; }

    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long FileSize { get; init; }

    /// <summary>
    /// Content-Type del archivo
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// Fecha y hora de subida
    /// </summary>
    public DateTime UploadedAt { get; init; }

    /// <summary>
    /// Metadata adicional del archivo
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}
