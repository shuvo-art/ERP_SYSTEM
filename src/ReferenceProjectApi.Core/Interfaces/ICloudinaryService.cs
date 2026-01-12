using Microsoft.AspNetCore.Http;

namespace ReferenceProjectApi.Core.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, string folder);
    Task<string> UploadFileAsync(IFormFile file, string folder); // For documents (raw files)
    Task DeleteFileAsync(string fileUrl);
}

