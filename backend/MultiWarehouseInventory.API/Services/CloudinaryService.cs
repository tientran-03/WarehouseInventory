using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace MultiWarehouseInventory.API.Services;

public interface ICloudinaryService
{
    Task<string?> UploadImageAsync(IFormFile file);
    Task<string?> DeleteImageAsync(string publicId);
}

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
    {
        _logger = logger;
        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];
        var uploadPreset = configuration["Cloudinary:UploadPreset"];

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(uploadPreset))
        {
            throw new InvalidOperationException("Cloudinary configuration is missing.");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _uploadPreset = uploadPreset;
        
        _logger.LogInformation("Cloudinary initialized with CloudName: {CloudName}", cloudName);
    }

    private readonly string _uploadPreset;

    public async Task<string?> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return null;

        try
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                UploadPreset = _uploadPreset,
                Transformation = new Transformation().Quality(80).FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            
            if (uploadResult == null || uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary upload failed: {Error}", uploadResult?.Error?.Message);
                return null;
            }
            
            _logger.LogInformation("Image uploaded successfully: {Url}", uploadResult.SecureUrl);
            return uploadResult.SecureUrl?.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to Cloudinary");
            return null;
        }
    }

    public async Task<string?> DeleteImageAsync(string publicId)
    {
        if (string.IsNullOrEmpty(publicId))
            return null;

        try
        {
            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);
            
            return result?.Result == "ok" ? "deleted" : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image from Cloudinary");
            return null;
        }
    }
}
