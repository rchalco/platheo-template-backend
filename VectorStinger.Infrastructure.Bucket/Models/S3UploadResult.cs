namespace VectorStinger.Infrastructure.Bucket.Models
{
    /// <summary>
    /// Resultado de subida de archivo a S3
    /// </summary>
    public class S3UploadResult
    {
        /// <summary>
        /// URL pública del archivo
        /// </summary>
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del archivo
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Key (ruta) del archivo en S3
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Tamaño del archivo en bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Content-Type del archivo
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora de subida
        /// </summary>
        public DateTime UploadedAt { get; set; }

        /// <summary>
        /// ETag del archivo (identificador único de S3)
        /// </summary>
        public string? ETag { get; set; }

        /// <summary>
        /// Metadata adicional del archivo
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
