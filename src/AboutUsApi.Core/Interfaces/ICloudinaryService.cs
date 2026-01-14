using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AboutUsApi.Core.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder);
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string publicId);
    }
}
