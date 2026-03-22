using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace bandothanhli.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary? _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            if (string.IsNullOrWhiteSpace(cloudName) ||
                string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(apiSecret))
            {
                _cloudinary = null;
                return;
            }

            _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (_cloudinary == null)
                throw new InvalidOperationException("Chưa cấu hình Cloudinary. Vui lòng set Cloudinary:CloudName/ApiKey/ApiSecret trong appsettings hoặc biến môi trường.");

            if (file.Length <= 0) throw new ArgumentException("File rỗng");

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "ecoshop/san-pham",
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
            if (result.Error != null) throw new InvalidOperationException(result.Error.Message);

            return result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? throw new InvalidOperationException("Upload thất bại");
        }
    }
}
