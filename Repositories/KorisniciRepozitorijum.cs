using WebAranzmani.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebAranzmani.Repositories
{
    public class KorisniciRepozitorijum
    {
        private readonly string putanja = HttpContext.Current.Server.MapPath("~/App_Data/korisnici.json");

        public List<KorisnikInfo> PronadjiSve()
        {
            if (!File.Exists(putanja))
                return new List<KorisnikInfo>();

            var json = File.ReadAllText(putanja);
            return JsonConvert.DeserializeObject<List<KorisnikInfo>>(json) ?? new List<KorisnikInfo>();
        }

        public KorisnikInfo PronadjiPoKorisnickomImenu(string username)
        {
            return PronadjiSve().FirstOrDefault(k => k.KorisnickoIme == username);
        }

        public void SacuvajSve(List<KorisnikInfo> lista)
        {
            var json = JsonConvert.SerializeObject(lista, Formatting.Indented);
            File.WriteAllText(putanja, json);
        }

        public void Azuriraj(KorisnikInfo korisnik)
        {
            var lista = PronadjiSve();
            var idx = lista.FindIndex(k => k.KorisnickoIme == korisnik.KorisnickoIme);
            if (idx != -1)
            {
                lista[idx] = korisnik;
                SacuvajSve(lista);
            }
        }
    }
}
