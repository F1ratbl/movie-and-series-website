using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmDiziSitesi.Models
{
    public class MovieModel
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Tür { get; set; } = string.Empty;
        public int Yil { get; set; }

        // Yeni Eklenen Özellikler
        public string? Yönetmen { get; set; }            // Film yönetmeni
        public string? Oyuncular { get; set; }           // Başlıca oyuncular (virgülle ayrılmış)
        public string? Ülke { get; set; }                // Yapım ülkesi
        public string? Dil { get; set; }                 // Konuşulan dil
        [Column(TypeName = "REAL")]
        public decimal? Bütçe { get; set; }              // Film bütçesi (USD vb.)
        [Column(TypeName = "REAL")]
        public decimal? GiseHasilati { get; set; }       // Gişe hasılatı (USD vb.)
        public int? IzlenmeSayisi { get; set; }          // Kaç kez izlendi
        public double Puan { get; set; }                // Ortalama izleyici puanı (örn: 8.2)
        public DateTime? VizyonTarihi { get; set; }      // İlk vizyon tarihi
        public string? Süre { get; set; }                // Film süresi (string olarak)

        // İsteğe Bağlı Alanlar
        public string? Açıklama { get; set; }            // Film özeti/açıklaması
        public string? AfişUrl { get; set; }             // Film afişinin URL'si
        public string? FragmanUrl { get; set; }          // Fragman videosu linki
    }
}

