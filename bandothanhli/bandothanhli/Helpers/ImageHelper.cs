namespace bandothanhli.Helpers
{
    public static class ImageHelper
    {
        private const string DefaultImagePath = "/images/picture1.png";

        public static string GetImagePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return DefaultImagePath;

            var trimmed = path.Trim();

            // N?u là URL tuy?t ??i thì dùng luôn
            if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return trimmed;

            // Replace backslash v?i forward slash
            trimmed = trimmed.Replace("\\", "/");

            // Lo?i b? kho?ng tr?ng
            trimmed = trimmed.Trim();

            // Ensure ???ng d?n b?t ??u b?ng /
            if (!trimmed.StartsWith("/"))
                trimmed = "/" + trimmed;

            // N?u path không ch?a "images/" và là /images/... thì dùng nh? là (vd: /images/picture1.png)
            // N?u path là /abc/def.png thì ki?m tra xem có ph?i là h?p l? không
            
            return trimmed;
        }

        public static bool IsValidImagePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".svg" };
            var extension = Path.GetExtension(path).ToLower();
            
            return validExtensions.Contains(extension);
        }
    }
}
