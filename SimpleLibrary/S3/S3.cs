using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Autofac;
using SimpleLibrary.Logger;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleLibrary.S3
{
    public class S3 : PrintLogger
    {
        private S3Info _S3Info = null;
        private AmazonS3Config _S3Config = new AmazonS3Config();
        private AmazonS3Client _S3Client = null;
        private TransferUtility _fileTransferUtility = null;

        public S3(string bucketName, string accessKeyID, string secretAccessKey, 
                 Amazon.RegionEndpoint region = null, ContainerBuilder builder = null)
        {
            ILogger log_ = InitLogger(builder);

            _S3Info = new S3Info(bucketName, accessKeyID, secretAccessKey, log_);

            _S3Config.RegionEndpoint = region ?? Amazon.RegionEndpoint.USWest2;

            _S3Client = new AmazonS3Client(accessKeyID, secretAccessKey, _S3Config);
            _fileTransferUtility = new TransferUtility(_S3Client);
        }

        public string UploadFile(string fileFullPath)
        {
            return UploadFile(fileFullPath, null);
        }

        public string UploadFile(string fileFullPath, IProgress<int> progress)
        {
            string preSignedUrl_ = "";
            if (!File.Exists(fileFullPath))
            {
                Print($"Try to upload {fileFullPath} not exist", Color.Red);
                return preSignedUrl_;
            }
            string fileName_ = Path.GetFileName(fileFullPath);

            try
            {
                if (progress != null)
                {
                    var uploadRequest = new TransferUtilityUploadRequest
                    {
                        FilePath = fileFullPath,
                        BucketName = _S3Info.BucketName,
                        Key = fileName_
                    };
                    uploadRequest.UploadProgressEvent += (sender, e) =>
                    {
                        progress.Report(e.PercentDone);
                    };
                    _fileTransferUtility.Upload(uploadRequest);
                }
                else
                {
                    _fileTransferUtility.Upload(fileFullPath, _S3Info.BucketName, fileName_);
                }

                GetPreSignedUrlRequest request_ = new GetPreSignedUrlRequest
                {
                    BucketName = _S3Info.BucketName,
                    Key = fileName_,
                    Expires = DateTime.Now.AddDays(5)
                };
                preSignedUrl_ = _S3Client.GetPreSignedURL(request_);
            }
            catch (AmazonS3Exception e)
            {
                Print($"Error encountered on S3 server. Message: {e.Message} when upload an object: {fileFullPath}", Color.Red);
            }
            catch (Exception e)
            {
                Print($"Error encountered on server. Message: {e.Message} when upload an object: {fileFullPath}", Color.Red);
            }

            return preSignedUrl_;
        }

        public async Task<string> UploadFileAsync(string fileFullPath, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => UploadFile(fileFullPath), cancellationToken);
        }

        public async Task<string> UploadFileAsync(string fileFullPath, IProgress<int> progress, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => UploadFile(fileFullPath, progress), cancellationToken);
        }

        public bool DeleteFile(string fileName)
        {
            try
            {
                var deleteRequest = new DeleteObjectRequest
                {
                    BucketName = _S3Info.BucketName,
                    Key = fileName
                };
                _S3Client.DeleteObjectAsync(deleteRequest).Wait();
                Print($"Deleted file: {fileName}", Color.Green);
                return true;
            }
            catch (AmazonS3Exception e)
            {
                Print($"Error deleting file: {e.Message}", Color.Red);
                return false;
            }
            catch (Exception e)
            {
                Print($"Error: {e.Message}", Color.Red);
                return false;
            }
        }

        public async Task<bool> DeleteFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => DeleteFile(fileName), cancellationToken);
        }

        public bool FileExists(string fileName)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _S3Info.BucketName,
                    Key = fileName
                };
                _S3Client.GetObjectMetadataAsync(request).Wait();
                return true;
            }
            catch (AmazonS3Exception)
            {
                return false;
            }
        }

        public async Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => FileExists(fileName), cancellationToken);
        }

        public string GetPreSignedUrl(string fileName, TimeSpan expiration)
        {
            try
            {
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                {
                    BucketName = _S3Info.BucketName,
                    Key = fileName,
                    Expires = DateTime.Now.Add(expiration)
                };
                return _S3Client.GetPreSignedURL(request);
            }
            catch (Exception e)
            {
                Print($"Error generating pre-signed URL: {e.Message}", Color.Red);
                return null;
            }
        }

        public void Dispose()
        {
            _fileTransferUtility?.Dispose();
            _S3Client?.Dispose();
        }
    }
}
