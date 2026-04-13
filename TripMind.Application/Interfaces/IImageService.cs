using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TripMind.Application.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadProfilePhotoAsync(IFormFile file);
        Task DeleteAsync(string? imageUrl);
    }
}