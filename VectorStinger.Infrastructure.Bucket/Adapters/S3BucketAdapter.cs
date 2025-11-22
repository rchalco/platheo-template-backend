using FluentResults;
using Microsoft.Extensions.Logging;
using VectorStinger.Core.Domain.Infrastructure.Bucket;
using VectorStinger.Core.Interfaces.Infrastructure.Bucket;
using VectorStinger.Infrastructure.Bucket.Interfaces;

namespace VectorStinger.Infrastructure.Bucket.Adapters;

/// <summary>
/// Adaptador que implementa IBucketService usando AWS S3
/// </summary>
public class S3BucketAdapter : IBucketService
{
    private readonly IS3Service _s3Service;
    private readonly ILogger<S3BucketAdapter> _logger;

    public S3BucketAdapter(
        IS3Service s3Service,
        ILogger<S3BucketAdapter> logger)
    {
        _s3Service = s3Service ?? throw new ArgumentNullException(nameof(s3Service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<BucketUploadResult>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        Dictionary<string, string>? metadata = null)
    {
        _logger.LogInformation(
            "Uploading file via S3 adapter: FileName={FileName}, Size={Size}, ContentType={ContentType}",
            fileName, fileStream.Length, contentType);

        var result = await _s3Service.UploadFileAsync(fileStream, fileName, contentType, metadata);

        if (result.IsFailed)
        {
            _logger.LogError("S3 upload failed: {Errors}", string.Join(", ", result.Errors.Select(e => e.Message)));
            return Result.Fail<BucketUploadResult>(result.Errors);
        }

        var s3Result = result.Value;

        var bucketResult = new BucketUploadResult
        {
            FileUrl = s3Result.FileUrl,
            FileName = s3Result.FileName,
            FileKey = s3Result.Key,
            FileSize = s3Result.FileSize,
            ContentType = s3Result.ContentType,
            UploadedAt = s3Result.UploadedAt,
            Metadata = s3Result.Metadata
        };

        _logger.LogInformation(
            "File uploaded successfully via S3: FileUrl={FileUrl}, FileKey={FileKey}",
            bucketResult.FileUrl, bucketResult.FileKey);

        return Result.Ok(bucketResult);
    }

    public async Task<Result<string>> GetTemporaryUrlAsync(string fileKey, int expirationMinutes = 60)
    {
        _logger.LogDebug("Getting temporary URL for file: FileKey={FileKey}, ExpirationMinutes={ExpirationMinutes}",
            fileKey, expirationMinutes);

        return await _s3Service.GetPresignedUrlAsync(fileKey, expirationMinutes);
    }

    public async Task<Result> DeleteFileAsync(string fileKey)
    {
        _logger.LogInformation("Deleting file via S3: FileKey={FileKey}", fileKey);
        return await _s3Service.DeleteFileAsync(fileKey);
    }

    public async Task<Result<bool>> FileExistsAsync(string fileKey)
    {
        return await _s3Service.FileExistsAsync(fileKey);
    }
}
