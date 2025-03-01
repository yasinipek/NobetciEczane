namespace NobetciEczane.Models
{
    public class EczaneModel
    {
        public int Id { get; set; }
        public string Isim { get; set; }
        public string Il { get; set; }
        public string Ilce { get; set; }
        public string Telefon { get; set; }
        public string Adres { get; set; }
        public string Enlem { get; set; }
        public string Boylam { get; set; }
        public string Tarih { get; set; }
        public DateTime KayitZamani { get; set; } = DateTime.Now;
    }
}
