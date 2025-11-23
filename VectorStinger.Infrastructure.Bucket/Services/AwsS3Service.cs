using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VectorStinger.Infrastructure.Bucket.Configuration;
using VectorStinger.Infrastructure.Bucket.Interfaces;
using VectorStinger.Infrastructure.Bucket.Models;

namespace VectorStinger.Infrastructure.Bucket.Services
{
    /// <summary>
    /// Implementación del servicio de AWS S3
    /// </summary>
    public class AwsS3Service : IS3Service, IDisposable
    {
        private readonly AwsS3Settings _settings;
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<AwsS3Service> _logger;
        private bool _disposed = false;

        public AwsS3Service(
            IOptions<AwsS3Settings> settings,
            ILogger<AwsS3Service> logger)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (!_settings.IsValid())
            {
                throw new InvalidOperationException(
                    "AWS S3 settings are not configured correctly. " +
                    "Please check AccessKey, SecretKey, BucketName, and Region in configuration.");
            }

            try
            {
                // Crear cliente S3 con credenciales
                _s3Client = new AmazonS3Client(
                    _settings.AccessKey,
                    _settings.SecretKey,
                    RegionEndpoint.GetBySystemName(_settings.Region));

                _logger.LogInformation(
                    "AWS S3 Client initialized successfully. Bucket: {BucketName}, Region: {Region}",
                    _settings.BucketName,
                    _settings.Region);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize AWS S3 Client");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Result<S3UploadResult>> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            Dictionary<string, string>? metadata = null)
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                return Result.Fail<S3UploadResult>("File stream is empty or null");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Result.Fail<S3UploadResult>("File name is required");
            }

            try
            {
                // Generar key único con timestamp
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var safeFileName = SanitizeFileName(fileName);
                var key = $"{_settings.PathPrefix}/{timestamp}_{safeFileName}";

                _logger.LogInformation(
                    "Uploading file to S3: Key={Key}, Size={Size} bytes, ContentType={ContentType}",
                    key,
                    fileStream.Length,
                    contentType);

                // Configurar request de subida
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = fileStream,
                    Key = key,
                    BucketName = _settings.BucketName,
                    ContentType = contentType,
                    CannedACL = S3CannedACL.Private, // Archivo privado por defecto
                    StorageClass = S3StorageClass.Standard
                };

                // Agregar metadata si existe
                if (metadata != null && metadata.Count > 0)
                {
                    foreach (var (metaKey, metaValue) in metadata)
                    {
                        uploadRequest.Metadata.Add($"x-amz-meta-{metaKey}", metaValue);
                    }
                }

                // Subir archivo usando TransferUtility (optimizado para archivos grandes)
                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(uploadRequest);

                // Construir URL del archivo
                var fileUrl = $"https://{_settings.BucketName}.s3.{_settings.Region}.amazonaws.com/{key}";

                var result = new S3UploadResult
                {
                    FileUrl = fileUrl,
                    FileName = safeFileName,
                    Key = key,
                    FileSize = fileStream.Length,
                    ContentType = contentType,
                    UploadedAt = DateTime.UtcNow,
                    Metadata = metadata
                };

                _logger.LogInformation(
                    "File uploaded successfully: Key={Key}, URL={FileUrl}",
                    key,
                    fileUrl);

                return Result.Ok(result);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex,
                    "AWS S3 error uploading file: StatusCode={StatusCode}, ErrorCode={ErrorCode}, Message={Message}",
                    ex.StatusCode,
                    ex.ErrorCode,
                    ex.Message);

                return Result.Fail<S3UploadResult>(
                    $"S3 Error: {ex.ErrorCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error uploading file to S3");
                return Result.Fail<S3UploadResult>($"Unexpected error: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<Result<string>> GetPresignedUrlAsync(string key, int expirationMinutes = 60)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Result.Fail<string>("Key is required");
            }

            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _settings.BucketName,
                    Key = key,
                    Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    Protocol = Protocol.HTTPS
                };

                var url = await _s3Client.GetPreSignedURLAsync(request);

                _logger.LogDebug(
                    "Generated presigned URL for Key={Key}, Expires in {Minutes} minutes",
                    key,
                    expirationMinutes);

                return Result.Ok(url);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error generating presigned URL for Key={Key}", key);
                return Result.Fail<string>($"S3 Error: {ex.ErrorCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error generating presigned URL");
                return Result.Fail<string>($"Unexpected error: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<Result> DeleteFileAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Result.Fail("Key is required");
            }

            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _settings.BucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(deleteRequest);

                _logger.LogInformation("File deleted successfully: Key={Key}", key);
                return Result.Ok();
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: Key={Key}", key);
                return Result.Fail($"S3 Error: {ex.ErrorCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting file");
                return Result.Fail($"Unexpected error: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> FileExistsAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Result.Fail<bool>("Key is required");
            }

            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _settings.BucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return Result.Ok(true);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Result.Ok(false);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence: Key={Key}", key);
                return Result.Fail<bool>($"S3 Error: {ex.ErrorCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error checking file existence");
                return Result.Fail<bool>($"Unexpected error: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<Result<Dictionary<string, string>>> GetFileMetadataAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return Result.Fail<Dictionary<string, string>>("Key is required");
            }

            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _settings.BucketName,
                    Key = key
                };

                var response = await _s3Client.GetObjectMetadataAsync(request);

                var metadata = new Dictionary<string, string>
                {
                    ["ContentType"] = response.Headers.ContentType,
                    ["ContentLength"] = response.ContentLength.ToString(),
                    ["LastModified"] = response.LastModified?.ToString() ?? string.Empty, 
                    ["ETag"] = response.ETag
                };

                // Agregar metadata personalizada
                foreach (var key2 in response.Metadata.Keys)
                {
                    metadata[key2] = response.Metadata[key2];
                }

                return Result.Ok(metadata);
            }
            catch (AmazonS3Exception ex)
            {
                _logger.LogError(ex, "Error getting file metadata: Key={Key}", key);
                return Result.Fail<Dictionary<string, string>>($"S3 Error: {ex.ErrorCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error getting file metadata");
                return Result.Fail<Dictionary<string, string>>($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sanitiza el nombre del archivo removiendo caracteres no válidos
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return "unnamed_file";
            }

            // Remover caracteres no válidos para nombres de archivo
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // Reemplazar espacios con guiones bajos
            sanitized = sanitized.Replace(" ", "_");

            // Limitar longitud
            if (sanitized.Length > 200)
            {
                var extension = Path.GetExtension(sanitized);
                var nameWithoutExt = Path.GetFileNameWithoutExtension(sanitized);
                sanitized = nameWithoutExt.Substring(0, 200 - extension.Length) + extension;
            }

            return sanitized;
        }

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _s3Client?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
