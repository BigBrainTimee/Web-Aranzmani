using Newtonsoft.Json;
using System.Collections.Generic;

namespace WebAranzmani.Models
{
    public enum StatusJedinice { Slobodna, Zauzeta }

    public class SmestajnaJedinicaInfo
    {
        public int JedinicaId { get; set; }

        [JsonProperty("SmestajId")]
        public int SmestajId { get; set; }

        public int BrojGostiju { get; set; }
        public bool Ljubimci { get; set; }
        public float Cena { get; set; }
        public StatusJedinice Status { get; set; }
        public bool Obrisana { get; set; }

        public List<KomentarInfo> Komentari { get; set; } = new List<KomentarInfo>();
        public string Naziv { get; set; } = string.Empty;

        public SmestajnaJedinicaInfo() { }

        public SmestajnaJedinicaInfo(int id, int smestajId, int gosti, bool ljubimci, float cena,
                                    StatusJedinice status, bool obrisana)
        {
            JedinicaId = id;
            SmestajId = smestajId;
            BrojGostiju = gosti;
            Ljubimci = ljubimci;
            Cena = cena;
            Status = status;
            Obrisana = obrisana;
        }

        public override string ToString()
        {
            return $"Jedinica {JedinicaId} - {BrojGostiju} gostiju, {Cena} RSD";
        }


    }
}
