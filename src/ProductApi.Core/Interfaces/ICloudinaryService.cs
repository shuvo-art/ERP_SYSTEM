using Microsoft.AspNetCore.Http;

namespace ProductApi.Core.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, string folder, string? customFileName = null);
    Task<string> UploadFileAsync(IFormFile file, string folder, string? customFileName = null);
    Task DeleteFileAsync(string fileUrl);
}
