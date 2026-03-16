using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using bandothanhli.Models;
using Microsoft.IdentityModel.Tokens;

namespace bandothanhli.Services
{
    public interface IJwtService
    {
        string TaoToken(NguoiDung nguoiDung);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string TaoToken(NguoiDung nguoiDung)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, nguoiDung.Id.ToString()),
                new Claim(ClaimTypes.Email, nguoiDung.Email),
                new Claim(ClaimTypes.Role, nguoiDung.VaiTro),
                new Claim(ClaimTypes.Name, nguoiDung.HoTen)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}