using Microsoft.AspNetCore.Http;

namespace ProductApi.Core.Interfaces;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file, string subDirectory);
    Task DeleteFileAsync(string fileName, string subDirectory);
    string GetFileUrl(string fileName, string subDirectory);
}
