using Microsoft.AspNetCore.Http;

namespace TargetMarketApi.Core.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, string folder);
    Task DeleteImageAsync(string imageUrl);
}
