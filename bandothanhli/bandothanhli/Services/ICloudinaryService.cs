using Microsoft.AspNetCore.Http;

namespace bandothanhli.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file, CancellationToken cancellationToken = default);
    }
}

