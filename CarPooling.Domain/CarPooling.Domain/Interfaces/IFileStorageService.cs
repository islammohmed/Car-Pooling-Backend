using System;
using System.IO;
using System.Threading.Tasks;

namespace CarPooling.Domain.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName);
        Task<bool> DeleteFileAsync(string fileUrl);
    }
} 