namespace FilmDiziSitesi.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        public string KullaniciAdi { get; set; }

        public string Email { get; set; }

        public string Sifre { get; set; }

        // Varsayılan olarak "User" atanır
        public string Role { get; set; } = "User";

        // Token alanı eklendi
         public string? Token { get; set; }

        public DateTime KayitTarihi { get; set; } = DateTime.Now;
    }
}
