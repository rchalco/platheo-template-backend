using FluentResults;
using VectorStinger.Infrastructure.Bucket.Models;

namespace VectorStinger.Infrastructure.Bucket.Interfaces
{
    /// <summary>
    /// Servicio para interactuar con AWS S3
    /// </summary>
    public interface IS3Service
    {
        /// <summary>
        /// Sube un archivo a S3
        /// </summary>
        /// <param name="fileStream">Stream del archivo</param>
        /// <param name="fileName">Nombre del archivo</param>
        /// <param name="contentType">Content-Type del archivo</param>
        /// <param name="metadata">Metadata adicional (opcional)</param>
        /// <returns>Resultado con información del archivo subido</returns>
        Task<Result<S3UploadResult>> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Obtiene una URL presignada para acceso temporal al archivo
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        /// <param name="expirationMinutes">Tiempo de expiración en minutos (default: 60)</param>
        /// <returns>URL presignada</returns>
        Task<Result<string>> GetPresignedUrlAsync(string key, int expirationMinutes = 60);

        /// <summary>
        /// Elimina un archivo de S3
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        /// <returns>Resultado de la operación</returns>
        Task<Result> DeleteFileAsync(string key);

        /// <summary>
        /// Verifica si un archivo existe en S3
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        /// <returns>True si existe, False si no</returns>
        Task<Result<bool>> FileExistsAsync(string key);

        /// <summary>
        /// Obtiene metadatos de un archivo sin descargarlo
        /// </summary>
        /// <param name="key">Key del archivo en S3</param>
        /// <returns>Metadatos del archivo</returns>
        Task<Result<Dictionary<string, string>>> GetFileMetadataAsync(string key);
    }
}
