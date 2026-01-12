using Microsoft.AspNetCore.Http;

namespace JobApi.Core.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, string folder);
    Task<string> UploadFileAsync(IFormFile file, string folder);
    Task DeleteFileAsync(string publicId);
}
