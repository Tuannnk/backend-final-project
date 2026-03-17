namespace bandothanhli.DTOs
{
    public class UploadAnhDTO
    {
        public Guid SanPhamId { get; set; }
        public IFormFile AnhFile { get; set; }
        public bool LaAnhDaiDien { get; set; } = false;
    }
}
