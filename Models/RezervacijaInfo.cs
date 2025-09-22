namespace WebAranzmani.Models
{
    public enum StatusRezervacije { Aktivna, Otkazana }

    public class RezervacijaInfo
    {
        public int RezervacijaId { get; set; }
        public string Turista { get; set; } = string.Empty;
        public StatusRezervacije Status { get; set; }
        public int AranzmanId { get; set; }
        public int SmestajnaJedinicaId { get; set; }

        public RezervacijaInfo() { }

        public RezervacijaInfo(int id, string turista, StatusRezervacije status, int aranzmanId, int smestajnaJedinicaId)
        {
            RezervacijaId = id;
            Turista = turista;
            Status = status;
            AranzmanId = aranzmanId;
            SmestajnaJedinicaId = smestajnaJedinicaId;
        }

        public override string ToString()
        {
            return $"{Turista} - {Status}";
        }
    }
}
