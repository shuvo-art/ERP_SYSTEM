using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PartnerApi.Core.Interfaces;

namespace PartnerApi.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var account = new Account(
            configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"],
            configuration["Cloudinary:ApiSecret"]
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0) return string.Empty;

        using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = folder,
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl?.ToString() ?? string.Empty;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder)
    {
        if (file == null || file.Length == 0) return string.Empty;

        using var stream = file.OpenReadStream();
        
        // Check if it's a video file based on extension/content type
        bool isVideo = file.ContentType.StartsWith("video/") || 
                       new[] { ".mp4", ".mov", ".avi", ".mkv", ".gif" }.Contains(Path.GetExtension(file.FileName).ToLower());

        RawUploadParams uploadParams;
        if (isVideo)
        {
            uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };
        }
        else
        {
            uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };
        }

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        return uploadResult.SecureUrl?.ToString() ?? string.Empty;
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        try
        {
            var uri = new Uri(fileUrl);
            // Public ID extraction logic: everything after 'upload/v[number]/' and before the extension
            var segments = uri.Segments;
            var publicIdWithExt = string.Join("", segments.SkipWhile(s => !s.StartsWith("v") && !s.All(char.IsDigit) || s.Length < 2).Skip(1));
            
            // Simpler reliable extraction for Cloudinary public IDs
            var publicId = Path.ChangeExtension(publicIdWithExt, null);
            
            // Try deleting as different resource types
            await _cloudinary.DestroyAsync(new DeletionParams(publicId) { ResourceType = ResourceType.Image });
            await _cloudinary.DestroyAsync(new DeletionParams(publicId) { ResourceType = ResourceType.Raw });
            await _cloudinary.DestroyAsync(new DeletionParams(publicId) { ResourceType = ResourceType.Video });
        }
        catch { }
    }
}
