using System;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TripMind.Application.Interfaces;

namespace TripMind.Infrastructure.Services
{
    public sealed class CloudinaryImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryImageService> _logger;

        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public CloudinaryImageService(IConfiguration config, ILogger<CloudinaryImageService> logger)
        {
            _logger = logger;

            var cloudName = config["Cloudinary:CloudName"] ?? throw new InvalidOperationException("Cloudinary:CloudName is not configured.");
            var apiKey = config["Cloudinary:ApiKey"] ?? throw new InvalidOperationException("Cloudinary:ApiKey is not configured.");
            var apiSecret = config["Cloudinary:ApiSecret"] ?? throw new InvalidOperationException("Cloudinary:ApiSecret is not configured.");

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
        }

        public async Task<string> UploadProfilePhotoAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided.");

            if (!Array.Exists(AllowedContentTypes, ct => ct == file.ContentType.ToLower()))
                throw new ArgumentException("File must be a valid image (jpg, png, webp).");

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException("File size must be less than 5MB.");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "tripmind/profiles",
                PublicId = Guid.NewGuid().ToString("N"),
                Transformation = new Transformation()
                    .Width(400).Height(400)
                    .Crop("fill").Gravity("face")
                    .Quality("auto").FetchFormat("auto"),
                Overwrite = true
            };

            try
            {
                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                    throw new InvalidOperationException($"Cloudinary error: {result.Error.Message}");

                _logger.LogInformation("Profile photo uploaded to Cloudinary: {PublicId}", result.PublicId);
                return result.SecureUrl.ToString();
            }
            catch (Exception ex) when (ex is not InvalidOperationException && ex is not ArgumentException)
            {
                _logger.LogError(ex, "Failed to upload profile photo to Cloudinary");
                throw new InvalidOperationException("Failed to upload image.");
            }
        }

        public async Task DeleteAsync(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                var publicId = ExtractPublicId(imageUrl);
                if (string.IsNullOrEmpty(publicId)) return;

                var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
                _logger.LogInformation("Deleted Cloudinary image: {PublicId}, Result: {Result}", publicId, result.Result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete Cloudinary image: {Url}", imageUrl);
            }
        }

        private static string ExtractPublicId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var path = uri.AbsolutePath;
                var upload = path.IndexOf("/upload/", StringComparison.Ordinal);
                if (upload < 0) return string.Empty;

                var afterUpload = path[(upload + 8)..];
                var versionEnd = afterUpload.IndexOf('/');
                if (versionEnd >= 0 && afterUpload.StartsWith("v"))
                    afterUpload = afterUpload[(versionEnd + 1)..];

                var dotIndex = afterUpload.LastIndexOf('.');
                return dotIndex >= 0 ? afterUpload[..dotIndex] : afterUpload;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}