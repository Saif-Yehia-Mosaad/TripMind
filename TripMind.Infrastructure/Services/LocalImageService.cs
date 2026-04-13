using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TripMind.Application.Interfaces;

namespace TripMind.Infrastructure.Services
{
    public sealed class LocalImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _baseUrl;
        private readonly ILogger<LocalImageService> _logger;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public LocalImageService(IWebHostEnvironment env, IConfiguration config, ILogger<LocalImageService> logger)
        {
            _env = env;
            _logger = logger;
            _baseUrl = config["BaseUrl"] ?? "https://tripmind.runasp.net";
        }

        public async Task<string> UploadProfilePhotoAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided.");

            if (!Array.Exists(AllowedContentTypes, ct => ct == file.ContentType.ToLower()))
                throw new ArgumentException("File must be a valid image (jpg, png, webp).");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Array.Exists(AllowedExtensions, e => e == ext))
                throw new ArgumentException("File extension not allowed.");

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException("File size must be less than 5MB.");

            var folder = Path.Combine(_env.WebRootPath, "images", "profiles");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(folder, fileName);

            try
            {
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                _logger.LogInformation("Profile photo saved: {FileName}", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save profile photo: {FileName}", fileName);
                throw new InvalidOperationException("Failed to save image.");
            }

            return $"{_baseUrl}/images/profiles/{fileName}";
        }

        public Task DeleteAsync(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return Task.CompletedTask;

            try
            {
                if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)) return Task.CompletedTask;

                var fileName = Path.GetFileName(uri.LocalPath);
                var folder = Path.Combine(_env.WebRootPath, "images", "profiles");
                var filePath = Path.Combine(folder, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted old profile photo: {FileName}", fileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete old profile photo: {Url}", imageUrl);
            }

            return Task.CompletedTask;
        }
    }
}