using Microsoft.AspNetCore.Http;
using ProductApi.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ProductApi.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _storageRoot;

    public FileService(IConfiguration configuration)
    {
        _storageRoot = configuration["FileSettings:StorageRoot"] ?? "wwwroot/uploads";
        if (!Directory.Exists(_storageRoot))
        {
            Directory.CreateDirectory(_storageRoot);
        }
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subDirectory)
    {
        if (file == null || file.Length == 0) return string.Empty;

        var directoryPath = Path.Combine(_storageRoot, subDirectory);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(directoryPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }

    public Task DeleteFileAsync(string fileName, string subDirectory)
    {
        if (string.IsNullOrEmpty(fileName)) return Task.CompletedTask;

        var filePath = Path.Combine(_storageRoot, subDirectory, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }

    public string GetFileUrl(string fileName, string subDirectory)
    {
        if (string.IsNullOrEmpty(fileName)) return string.Empty;
        // In a real app, this would return a full URL or a relative path handled by static files middleware
        return $"/uploads/{subDirectory}/{fileName}";
    }
}
