using Newtonsoft.Json;
using System.Collections.Generic;

namespace WebAranzmani.Models
{
    public enum VrstaSmestaja { Hotel, Motel, Vila }

    public class SmestajInfo
    {
        public int SmestajId { get; set; }
        public int AranzmanId { get; set; }
        public VrstaSmestaja Vrsta { get; set; }
        public string Naziv { get; set; } = string.Empty;

        private int? _zvezdice;
        public int? Zvezdice
        {
            get => Vrsta == VrstaSmestaja.Hotel ? _zvezdice : null;
            set
            {
                if (Vrsta == VrstaSmestaja.Hotel)
                    _zvezdice = value;
                else
                    _zvezdice = null; // Motel i Vila nikad nemaju zvezdice
            }
        }

        public bool Bazen { get; set; }
        public bool SpaCentar { get; set; }
        public bool Prilagodjen { get; set; }
        public bool Wifi { get; set; }

        public List<int> Jedinice { get; set; } = new List<int>();
        public List<KomentarInfo> Komentari { get; set; } = new List<KomentarInfo>();

        [JsonIgnore]
        public List<SmestajnaJedinicaInfo> SmestajneJedinice { get; set; } = new List<SmestajnaJedinicaInfo>();

        public bool Obrisano { get; set; }

        public SmestajInfo() { }

        public SmestajInfo(int id, int aranzmanId, VrstaSmestaja vrsta, string naziv, int? zvezdice,
                          bool bazen, bool spa, bool prilagodjen, bool wifi,
                          List<int> jedinice, List<KomentarInfo> komentari, bool obrisano)
        {
            SmestajId = id;
            AranzmanId = aranzmanId;
            Vrsta = vrsta;
            Naziv = naziv;
            Zvezdice = zvezdice; // setter će automatski obrisati vrednost ako nije Hotel
            Bazen = bazen;
            SpaCentar = spa;
            Prilagodjen = prilagodjen;
            Wifi = wifi;
            Jedinice = jedinice ?? new List<int>();
            Komentari = komentari ?? new List<KomentarInfo>();
            Obrisano = obrisano;
        }

        public override string ToString()
        {
            return Vrsta == VrstaSmestaja.Hotel
                ? $"{Naziv} ({Vrsta}, {Zvezdice}*)"
                : $"{Naziv} ({Vrsta})";
        }
    }
}
