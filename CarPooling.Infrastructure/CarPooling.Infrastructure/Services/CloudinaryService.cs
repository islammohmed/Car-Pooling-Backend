using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using CarPooling.Domain.Interfaces;
using CarPooling.Infrastructure.Settings;

namespace CarPooling.Infrastructure.Services
{
    public class CloudinaryService : IFileStorageService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(fileName, fileStream),
                Folder = "carpooling/documents" // Organizing user documents in a specific folder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Failed to upload image: {uploadResult.Error.Message}");
            }

            return uploadResult.SecureUrl.ToString();
        }

        public async Task<bool> DeleteFileAsync(string fileUrl)
        {
            try
            {
                // Extract public ID from the URL (including folder)
                var uri = new Uri(fileUrl);
                var pathSegments = uri.Segments;
                var publicId = string.Join("", pathSegments.Skip(pathSegments.Length - 2)).TrimEnd('/');
                publicId = publicId.Split('.')[0]; // Remove file extension

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);

                return result.Result == "ok";
            }
            catch
            {
                return false;
            }
        }
    }
} 