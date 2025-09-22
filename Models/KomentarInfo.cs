namespace WebAranzmani.Models
{
    public class KomentarInfo
    {
        public int KomentarId { get; set; }
        public string Turista { get; set; } = string.Empty;
        public string SmestajNaziv { get; set; } = string.Empty;
        public string Sadrzaj { get; set; } = string.Empty;
        public int Ocena { get; set; } = 1;
        public bool Prihvacen { get; set; }

        public KomentarInfo() { }

        public KomentarInfo(int id, string turista, string smestaj, string tekst, int ocena, bool prihvacen)
        {
            KomentarId = id;
            Turista = turista;
            SmestajNaziv = smestaj;
            Sadrzaj = tekst;
            Ocena = ocena;
            Prihvacen = prihvacen;
        }

        public override string ToString()
        {
            return $"{Turista} ({Ocena}/5): {Sadrzaj}";
        }
    }
}
