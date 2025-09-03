using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SQLMigrationAssistant.Application.Common.Exceptions;
using SQLMigrationAssistant.Application.Common.Interfaces;
using SQLMigrationAssistant.Infrastructure.Settings;
using System.Text;

namespace SQLMigrationAssistant.Infrastructure.Services
{
    public class CloudStorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<CloudStorageService> _logger;
        private readonly string _bucketName;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider;
        private readonly IRetryPolicy _retryPolicy;

        public CloudStorageService(IAmazonS3 s3Client, IOptions<CloudStorageSettings> s3Settings,
                                ILogger<CloudStorageService> logger, IRetryPolicy retryPolicy)
        {
            _s3Client = s3Client;
            _bucketName = s3Settings.Value.BucketName;
            _logger = logger;
            _contentTypeProvider = new FileExtensionContentTypeProvider();
            _retryPolicy = retryPolicy;
        }

        /// <summary>
        /// Tests connectivity by attempting to list a small number of objects in the configured S3 bucket.
        /// This requires 's3:ListBucket' permission.
        /// </summary>
        /// <returns>True if connectivity is successful, false otherwise.</returns>
        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    // Test bucket access by listing objects with a limit
                    var request = new ListObjectsV2Request
                    {
                        BucketName = _bucketName,
                        MaxKeys = 1
                    };
                    await _s3Client.ListObjectsV2Async(request);

                    _logger.LogInformation("Successfully connected to S3 bucket {BucketName}! Service is available.", _bucketName);
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connectivity to S3 bucket {BucketName} after retry attempts", _bucketName);
                return false;
            }
        }

        /// <summary>
        /// Upload file content to an S3 bucket
        /// </summary>
        /// <param name="content">File content to upload</param>
        /// <param name="fileName">The key (path/name) for the file in the bucket</param>
        /// <param name="contentType">MIME type</param>
        /// <returns>The uploaded file key</returns>
        public async Task<string> UploadFileAsync(string content, string fileName, string contentType)
        {
            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    var bytes = Encoding.UTF8.GetBytes(content);
                    using var stream = new MemoryStream(bytes);

                    var request = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = fileName,
                        ContentType = contentType,
                        InputStream = stream
                    };

                    await _s3Client.PutObjectAsync(request);

                    _logger.LogInformation("Text content uploaded to {BucketName}/{FileName}", _bucketName, fileName);
                    return fileName;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading text content to {FileName} in S3 after retry attempts", fileName);
                throw;
            }
        }

        /// <summary>
        /// Get files by a prefix in the S3 bucket
        /// </summary>
        /// <param name="prefix">Optional prefix to filter files</param>
        /// <returns>List of file keys</returns>
        public async Task<IEnumerable<string>> GetFilesByPrefixAsync(string? prefix = null)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return [];
            }

            try
            {
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    var objectNames = new List<string>();
                    var request = new ListObjectsV2Request
                    {
                        BucketName = _bucketName,
                        Prefix = prefix
                    };

                    ListObjectsV2Response response;
                    do
                    {
                        response = await _s3Client.ListObjectsV2Async(request);
                        objectNames.AddRange(response.S3Objects.Select(o => o.Key));
                        request.ContinuationToken = response.NextContinuationToken;
                    } while (response.IsTruncated != null && response.IsTruncated.Value);

                    _logger.LogInformation("Listed {Count} files with prefix {Prefix} from S3 bucket {BucketName}",
                        objectNames.Count, prefix, _bucketName);

                    return objectNames;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files in S3 bucket {BucketName} with prefix {Prefix} after retry attempts",
                    _bucketName, prefix);
                throw;
            }
        }

        /// <summary>
        /// Downloads a file from S3.
        /// </summary>
        public async Task<byte[]> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Downloading file from S3. Bucket: {BucketName}, File: {FileName}",
                    _bucketName, fileName);

                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    using var response = await _s3Client.GetObjectAsync(_bucketName, fileName, cancellationToken);
                    using var memoryStream = new MemoryStream();
                    await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);

                    var content = memoryStream.ToArray();
                    _logger.LogInformation("Successfully downloaded file {FileName}. Size: {Size} bytes", fileName, content.Length);

                    return content;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file from S3. Bucket: {BucketName}, File: {FileName} after retry attempts",
                    _bucketName, fileName);
                throw;
            }
        }

        /// <summary>
        /// Deletes all objects under a specified "directory" prefix in S3.
        /// </summary>
        public async Task DeleteDirectoryAsync(string directoryPath)
        {
            try
            {
                // Ensure the prefix ends with a '/' to avoid deleting unintended files.
                var directoryPrefix = directoryPath.EndsWith("/") ? directoryPath : $"{directoryPath}/";
                var allKeys = (await GetFilesByPrefixAsync(directoryPrefix)).ToList();

                if (!allKeys.Any())
                {
                    _logger.LogInformation("No files found in directory {DirectoryPath} to delete.", directoryPath);
                    return;
                }

                // S3 can delete up to 1000 objects in a single request.
                foreach (var chunk in allKeys.Chunk(1000))
                {
                    var deleteRequest = new DeleteObjectsRequest
                    {
                        BucketName = _bucketName,
                        Objects = chunk.Select(key => new KeyVersion { Key = key }).ToList()
                    };

                    await _s3Client.DeleteObjectsAsync(deleteRequest);
                }
                _logger.LogInformation("Successfully deleted {Count} objects from directory {DirectoryPath}", allKeys.Count, directoryPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error trying to delete S3 directory: {DirectoryPath}", directoryPath);
                throw new CloudStorageException("The S3 directory could not be accessed for deletion", ex);
            }
        }

        // NO CHANGES NEEDED FOR THE METHODS BELOW
        // These methods call the ones above, so their logic remains the same.

        public async Task<byte[]> SearchAndGetFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Searching for and downloading file from S3. Bucket: {BucketName}, File: {FileName}",
                    _bucketName, fileName);

                var foundFileName = await FindFileByNameAsync(fileName, cancellationToken);

                return await GetFileAsync(foundFileName, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file from S3. Bucket: {BucketName}, File: {FileName} after retry attempts",
                    _bucketName, fileName);
                throw;
            }
        }

        public string GetContentType(string filename)
        {
            if (!_contentTypeProvider.TryGetContentType(filename, out string contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        private async Task<string?> FindFileByNameAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            try
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    var request = new ListObjectsV2Request
                    {
                        BucketName = _bucketName,
                        Prefix = filePath
                    };

                    ListObjectsV2Response response;
                    do
                    {
                        response = await _s3Client.ListObjectsV2Async(request, cancellationToken);
                        foreach (var obj in response.S3Objects)
                        {
                            var currentFileName = Path.GetFileNameWithoutExtension(obj.Key);
                            if (string.Equals(currentFileName, fileName, StringComparison.OrdinalIgnoreCase))
                            {
                                _logger.LogInformation("Found file {FullFileName} for search term {SearchTerm}",
                                    obj.Key, filePath);

                                return obj.Key;
                            }
                        }
                        request.ContinuationToken = response.NextContinuationToken;

                    } while (response.IsTruncated != null && response.IsTruncated.Value);

                    _logger.LogWarning("No file found matching {SearchTerm} in S3 bucket {BucketName}",
                        filePath, _bucketName);
                    throw new FileNotFoundException(Path.GetFileName(filePath));
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for file {SearchTerm} in S3 bucket {BucketName} after retry attempts",
                    filePath, _bucketName);
                throw;
            }
        }

        public async Task<Dictionary<string, byte[]>> GetMultipleFilesAsync(IEnumerable<string> fileNames, CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<string, byte[]>();
            var downloadTasks = new List<Task>();
            var semaphore = new SemaphoreSlim(10); // Limit concurrent downloads

            foreach (var fileName in fileNames)
            {
                downloadTasks.Add(DownloadFileWithSemaphore(fileName, result, semaphore, cancellationToken));
            }

            await Task.WhenAll(downloadTasks);
            return result;
        }

        private async Task DownloadFileWithSemaphore(string fileName, Dictionary<string, byte[]> result,
            SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var content = await GetFileAsync(fileName, cancellationToken);
                lock (result)
                {
                    result[fileName] = content;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download file {FileName} for bulk operation", fileName);
                // Don't fail the entire operation for one file
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}