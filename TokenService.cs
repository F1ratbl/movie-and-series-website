using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FilmDiziSitesi.Services
{
    public static class TokenService
    {
        // IConfiguration instance'ı null olabilir, ama kullanmadan önce Configure ile set edilmeli
        private static IConfiguration? _configuration;

        // Bu metodu uygulama başlatılırken çağırmalısın
        public static void Configure(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public static string CreateToken(string kullaniciAdi)
        {
        if (_configuration == null)
        throw new InvalidOperationException("TokenService yapılandırılmamış. Önce Configure() çağrılmalı.");

        var jwtSettings = _configuration.GetSection("JwtSettings");

        var keyString = jwtSettings["Key"] ?? throw new Exception("JWT Key yapılandırmada eksik.");
        var issuer = jwtSettings["Issuer"] ?? throw new Exception("JWT Issuer yapılandırmada eksik.");
        var audience = jwtSettings["Audience"] ?? throw new Exception("JWT Audience yapılandırmada eksik.");
        var expiresInMinutes = jwtSettings.GetValue<double>("ExpiresInMinutes");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, kullaniciAdi),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
        signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}



