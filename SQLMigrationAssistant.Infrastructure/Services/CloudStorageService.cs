//using Microsoft.AspNetCore.StaticFiles;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using SQLMigrationAssistant.Application.Common.Exceptions;
//using SQLMigrationAssistant.Application.Common.Interfaces;
//using SQLMigrationAssistant.Infrastructure.Settings;
//using System.Text;

//namespace SQLMigrationAssistant.Infrastructure.Services
//{
//    public class CloudStorageService : IFileStorageService
//    {
//        //private readonly StorageClient _storageClient;
//        private readonly ILogger<CloudStorageService> _logger;
//        private readonly string _bucketName;
//        private readonly FileExtensionContentTypeProvider _contentTypeProvider;
//        private readonly IRetryPolicy _retryPolicy;

//        public CloudStorageService(StorageClient storageClient, IOptions<CloudStorageSettings> cloudSettings,
//                                    ILogger<CloudStorageService> logger, IRetryPolicy retryPolicy)
//        {
//            //_storageClient = storageClient;
//            _bucketName = cloudSettings.Value.BucketName;
//            _logger = logger;
//            _contentTypeProvider = new FileExtensionContentTypeProvider();
//            _retryPolicy = retryPolicy;
//        }

//        /// <summary>
//        /// Tests connectivity by attempting to list a small number of objects in the configured bucket.
//        /// This requires 'storage.objects.list' permission.
//        /// </summary>
//        /// <returns>True if connectivity is successful, false otherwise.</returns>
//        public async Task<bool> IsServiceAvailableAsync()
//        {
//            try
//            {
//                return await _retryPolicy.ExecuteAsync(async () =>
//                {
//                    // Test bucket access by listing objects with a limit
//                    var objects = _storageClient.ListObjects(_bucketName, options: new ListObjectsOptions { PageSize = 1 });
//                    var objectCount = objects.Take(1).Count(); // Just test if we can access the bucket

//                    _logger.LogInformation("Successfully connected to bucket {BucketName}! Service is available.", _bucketName);
//                    return true;
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error testing connectivity to bucket {BucketName} after retry attempts", _bucketName);
//                return false;
//            }
//        }

//        /// <summary>
//        /// Upload file content to GCP bucket
//        /// </summary>
//        /// <param name="content">file content to upload</param>
//        /// <param name="fileName">Name for the file in the bucket</param>
//        /// <param name="contentType">MIME type (defaults to text/plain)</param>
//        /// <returns>The uploaded file name</returns>
//        public async Task<string> UploadFileAsync(string content, string fileName, string contentType)
//        {
//            try
//            {
//                return await _retryPolicy.ExecuteAsync(async () =>
//                {
//                    var bytes = Encoding.UTF8.GetBytes(content);
//                    using var stream = new MemoryStream(bytes);

//                    var uploadedObject = await _storageClient.UploadObjectAsync(
//                        _bucketName,
//                        fileName,
//                        contentType,
//                        stream);

//                    _logger.LogInformation("Text content uploaded to {BucketName}/{FileName}", _bucketName, fileName);
//                    return uploadedObject.Name;
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error uploading text content to {FileName} after retry attempts", fileName);
//                throw;
//            }
//        }

//        /// <summary>
//        /// Get files by a prefix in the bucket
//        /// </summary>
//        /// <param name="prefix">Optional prefix to filter files</param>
//        /// <returns>List of file names</returns>
//        public async Task<IEnumerable<string>> GetFilesByPrefixAsync(string? prefix = null)
//        {
//            if (string.IsNullOrWhiteSpace(prefix))
//            {
//                return [];
//            }

//            try
//            {
//                return await _retryPolicy.ExecuteAsync(async () =>
//                {
//                    var objects = _storageClient.ListObjectsAsync(_bucketName, prefix);
//                    var objectNames = new List<string>();

//                    await foreach (var obj in objects)
//                    {
//                        objectNames.Add(obj.Name);
//                    }

//                    _logger.LogInformation("Listed {Count} files with prefix {Prefix} from bucket {BucketName}",
//                        objectNames.Count, prefix, _bucketName);

//                    return (IEnumerable<string>)objectNames;
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error listing files in bucket {BucketName} with prefix {Prefix} after retry attempts",
//                    _bucketName, prefix);
//                throw;
//            }
//        }

//        public async Task<byte[]> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                _logger.LogInformation("Downloading file from GCP Storage. Bucket: {BucketName}, File: {FileName}",
//                    _bucketName, fileName);

//                return await _retryPolicy.ExecuteAsync(async () =>
//                {
//                    using var memoryStream = new MemoryStream();
//                    await _storageClient.DownloadObjectAsync(_bucketName, fileName, memoryStream, cancellationToken: cancellationToken);

//                    var content = memoryStream.ToArray();
//                    _logger.LogInformation("Successfully downloaded file {FileName}. Size: {Size} bytes", fileName, content.Length);

//                    return content;
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error downloading file from GCP Storage. Bucket: {BucketName}, File: {FileName} after retry attempts",
//                    _bucketName, fileName);
//                throw;
//            }
//        }

//        public async Task<byte[]> SearchAndGetFileAsync(string fileName, CancellationToken cancellationToken = default)
//        {
//            try
//            {
//                _logger.LogInformation("Downloading file from GCP Storage. Bucket: {BucketName}, File: {FileName}",
//                    _bucketName, fileName);

//                var foundFileName = await FindFileByNameAsync(fileName, cancellationToken);

//                return await _retryPolicy.ExecuteAsync(async () =>
//                {
//                    using var memoryStream = new MemoryStream();
//                    await _storageClient.DownloadObjectAsync(_bucketName, foundFileName, memoryStream, cancellationToken: cancellationToken);

//                    var content = memoryStream.ToArray();
//                    _logger.LogInformation("Successfully downloaded file {FileName}. Size: {Size} bytes", fileName, content.Length);

//                    return content;
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error downloading file from GCP Storage. Bucket: {BucketName}, File: {FileName} after retry attempts",
//                    _bucketName, fileName);
//                throw;
//            }
//        }

//        public string GetContentType(string filename)
//        {
//            if (!_contentTypeProvider.TryGetContentType(filename, out string contentType))
//            {
//                contentType = "application/octet-stream";
//            }
//            return contentType;
//        }

//        public async Task DeleteDirectoryAsync(string directoryPath)
//        {
//            try
//            {
//                var directoryPrefix = directoryPath.EndsWith("/") ? directoryPath : $"{directoryPath}/";
//                await foreach (var storageObject in _storageClient.ListObjectsAsync(_bucketName, directoryPrefix))
//                {
//                    await _storageClient.DeleteObjectAsync(_bucketName, storageObject.Name);
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error trying to delete directory: {DirectoryPath}", directoryPath);

//                throw new CloudStorageException("The directory could not be accessed for deletion", ex);
//            }
//        }

//        /// <summary>
//        /// Finds a file by name. Returns the first match found.
//        /// </summary>
//        /// <param name="filePath">The file path</param>
//        /// <param name="cancellationToken">Cancellation token</param>
//        /// <returns>The full filename with extension, or null if not found</returns>
//        private async Task<string?> FindFileByNameAsync(string filePath, CancellationToken cancellationToken = default)
//        {
//            if (string.IsNullOrWhiteSpace(filePath))
//                return null;

//            try
//            {
//                var fileName = Path.GetFileNameWithoutExtension(filePath);
//                return await _retryPolicy.ExecuteAsync(async () =>
//                {
//                    var objects = _storageClient.ListObjectsAsync(_bucketName, filePath);

//                    await foreach (var obj in objects.WithCancellation(cancellationToken))
//                    {
//                        var currentFileName = Path.GetFileNameWithoutExtension(obj.Name);
//                        if (string.Equals(currentFileName, fileName, StringComparison.OrdinalIgnoreCase))
//                        {
//                            _logger.LogInformation("Found file {FullFileName} for search term {SearchTerm}",
//                                obj.Name, filePath);
//                            return obj.Name;
//                        }
//                    }

//                    _logger.LogWarning("No file found matching {SearchTerm} in bucket {BucketName}",
//                        filePath, _bucketName);
//                    throw new FileNotFoundException(Path.GetFileName(filePath));
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error searching for file {SearchTerm} in bucket {BucketName} after retry attempts",
//                    filePath, _bucketName);
//                throw;
//            }
//        }

//        /// <summary>
//        /// Downloads multiple files efficiently in parallel for ZIP creation
//        /// </summary>
//        /// <param name="fileNames">List of file names to download</param>
//        /// <param name="cancellationToken">Cancellation token</param>
//        /// <returns>Dictionary mapping file names to their content</returns>
//        public async Task<Dictionary<string, byte[]>> GetMultipleFilesAsync(IEnumerable<string> fileNames, CancellationToken cancellationToken = default)
//        {
//            var result = new Dictionary<string, byte[]>();
//            var downloadTasks = new List<Task>();
//            var semaphore = new SemaphoreSlim(10); // Limit concurrent downloads to 10

//            foreach (var fileName in fileNames)
//            {
//                downloadTasks.Add(DownloadFileWithSemaphore(fileName, result, semaphore, cancellationToken));
//            }

//            await Task.WhenAll(downloadTasks);
//            return result;
//        }

//        private async Task DownloadFileWithSemaphore(string fileName, Dictionary<string, byte[]> result,
//            SemaphoreSlim semaphore, CancellationToken cancellationToken)
//        {
//            await semaphore.WaitAsync(cancellationToken);
//            try
//            {
//                var content = await GetFileAsync(fileName, cancellationToken);
//                lock (result)
//                {
//                    result[fileName] = content;
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to download file {FileName} for bulk operation", fileName);
//                // Don't fail the entire operation for one file
//            }
//            finally
//            {
//                semaphore.Release();
//            }
//        }
//    }
//}