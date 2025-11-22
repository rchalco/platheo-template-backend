using FluentResults;
using VectorStinger.Core.Domain.Infrastructure.Bucket;

namespace VectorStinger.Core.Interfaces.Infrastructure.Bucket;

/// <summary>
/// Interfaz agnóstica para servicios de almacenamiento de archivos (bucket storage)
/// </summary>
public interface IBucketService
{
    /// <summary>
    /// Sube un archivo al bucket storage
    /// </summary>
    /// <param name="fileStream">Stream del archivo</param>
    /// <param name="fileName">Nombre del archivo</param>
    /// <param name="contentType">Content-Type del archivo</param>
    /// <param name="metadata">Metadata adicional (opcional)</param>
    /// <returns>Resultado con la URL pública del archivo subido</returns>
    Task<Result<BucketUploadResult>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Dictionary<string, string>? metadata = null);

    /// <summary>
    /// Obtiene una URL temporal de acceso al archivo
    /// </summary>
    /// <param name="fileKey">Key/ruta del archivo en el bucket</param>
    /// <param name="expirationMinutes">Tiempo de expiración en minutos</param>
    /// <returns>URL temporal de acceso</returns>
    Task<Result<string>> GetTemporaryUrlAsync(string fileKey, int expirationMinutes = 60);

    /// <summary>
    /// Elimina un archivo del bucket
    /// </summary>
    /// <param name="fileKey">Key/ruta del archivo en el bucket</param>
    /// <returns>Resultado de la operación</returns>
    Task<Result> DeleteFileAsync(string fileKey);

    /// <summary>
    /// Verifica si un archivo existe en el bucket
    /// </summary>
    /// <param name="fileKey">Key/ruta del archivo en el bucket</param>
    /// <returns>True si existe, False si no</returns>
    Task<Result<bool>> FileExistsAsync(string fileKey);
}
