using System;
using System.ComponentModel.DataAnnotations;

namespace FilmDiziSitesi.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        public string KullaniciAdi { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [DataType(DataType.Password)]
        public string Sifre { get; set; } = string.Empty;

        // Token alanı eklendi
        public string? Token { get; set; }

        public DateTime KayitTarihi { get; set; } = DateTime.Now;

        public string? Role { get; set; } // Admin veya User
    }
}
