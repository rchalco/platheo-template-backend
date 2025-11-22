namespace VectorStinger.Infrastructure.Bucket.Configuration
{
    /// <summary>
    /// Configuración para AWS S3
    /// </summary>
    public class AwsS3Settings
    {
        /// <summary>
        /// AWS Access Key ID
        /// </summary>
        public string AccessKey { get; set; } = string.Empty;

        /// <summary>
        /// AWS Secret Access Key
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del bucket de S3
        /// </summary>
        public string BucketName { get; set; } = string.Empty;

        /// <summary>
        /// Región de AWS (ej: us-east-1, us-west-2)
        /// </summary>
        public string Region { get; set; } = "us-east-1";

        /// <summary>
        /// Prefijo para las rutas de los archivos (opcional)
        /// </summary>
        public string PathPrefix { get; set; } = "templates";

        /// <summary>
        /// Tiempo de expiración de URLs presignadas (en minutos)
        /// </summary>
        public int PresignedUrlExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Validar configuración
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(AccessKey) &&
                   !string.IsNullOrEmpty(SecretKey) &&
                   !string.IsNullOrEmpty(BucketName) &&
                   !string.IsNullOrEmpty(Region);
        }
    }
}
