using WebAranzmani.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebAranzmani.Repositories
{
    public class KomentariRepozitorijum
    {
        private readonly string putanja = HttpContext.Current.Server.MapPath("~/App_Data/komentari.json");

        public List<KomentarInfo> PronadjiSve()
        {
            if (!File.Exists(putanja))
                return new List<KomentarInfo>();

            var json = File.ReadAllText(putanja);
            return JsonConvert.DeserializeObject<List<KomentarInfo>>(json) ?? new List<KomentarInfo>();
        }

        public KomentarInfo PronadjiPoId(int id)
        {
            return PronadjiSve().FirstOrDefault(k => k.KomentarId == id);
        }

        public void SacuvajSve(List<KomentarInfo> komentari)
        {
            var json = JsonConvert.SerializeObject(komentari, Formatting.Indented);
            File.WriteAllText(putanja, json);
        }

        public void Dodaj(KomentarInfo novi)
        {
            var komentari = PronadjiSve();
            novi.KomentarId = komentari.Any() ? komentari.Max(k => k.KomentarId) + 1 : 1;
            komentari.Add(novi);
            SacuvajSve(komentari);
        }

        public void Azuriraj(KomentarInfo izmenjen)
        {
            var komentari = PronadjiSve();
            var idx = komentari.FindIndex(k => k.KomentarId == izmenjen.KomentarId);
            if (idx != -1)
            {
                komentari[idx] = izmenjen;
                SacuvajSve(komentari);
            }
        }

        // 🔴 Nova metoda - trajno brisanje komentara
        public void Obrisi(int id)
        {
            var komentari = PronadjiSve();
            komentari = komentari.Where(k => k.KomentarId != id).ToList();
            SacuvajSve(komentari);
        }
    }
}
