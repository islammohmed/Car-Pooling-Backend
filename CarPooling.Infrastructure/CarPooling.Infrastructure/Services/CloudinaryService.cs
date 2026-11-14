using CarPooling.Application.Interfaces;
using CarPooling.Infrastructure.Settings;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;

namespace CarPooling.Infrastructure.Services
{
    public class CloudinaryService : IFileStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Transformation = new Transformation().Width(500).Height(500).Crop("fill")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            return uploadResult.SecureUrl.ToString();
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            // Extract public ID from URL
            var publicId = GetPublicIdFromUrl(fileUrl);

            if (string.IsNullOrEmpty(publicId))
                return false;

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }

        private string GetPublicIdFromUrl(string fileUrl)
        {
            try
            {
                // Extract public ID from Cloudinary URL
                var uri = new Uri(fileUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                
                // The public ID is typically after /upload/ and before the file extension
                var uploadIndex = Array.IndexOf(pathSegments, "upload");
                if (uploadIndex >= 0 && pathSegments.Length > uploadIndex + 1)
                {
                    var fileName = pathSegments[pathSegments.Length - 1];
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    
                    // Construct the public ID from the path segments between "upload" and the file name
                    var publicIdParts = new List<string>();
                    for (int i = uploadIndex + 1; i < pathSegments.Length - 1; i++)
                    {
                        publicIdParts.Add(pathSegments[i]);
                    }
                    publicIdParts.Add(fileNameWithoutExtension);
                    
                    return string.Join("/", publicIdParts);
                }
                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
} 