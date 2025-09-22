using WebAranzmani.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebAranzmani.Repositories
{
    public class RezervazcijeRepozitorijum
    {
        private readonly string putanja = HttpContext.Current.Server.MapPath("~/App_Data/rezervacije.json");

        public List<RezervacijaInfo> PronadjiSve()
        {
            if (!File.Exists(putanja))
                return new List<RezervacijaInfo>();

            var json = File.ReadAllText(putanja);
            return JsonConvert.DeserializeObject<List<RezervacijaInfo>>(json) ?? new List<RezervacijaInfo>();
        }

        public RezervacijaInfo PronadjiPoId(int id)
        {
            return PronadjiSve().FirstOrDefault(r => r.RezervacijaId == id);
        }

        public void Dodaj(RezervacijaInfo nova)
        {
            var lista = PronadjiSve();
            nova.RezervacijaId = lista.Any() ? lista.Max(r => r.RezervacijaId) + 1 : 1;
            lista.Add(nova);
            SacuvajSve(lista);
        }

        public void Azuriraj(RezervacijaInfo rezervacija)
        {
            var lista = PronadjiSve();
            var idx = lista.FindIndex(r => r.RezervacijaId == rezervacija.RezervacijaId);
            if (idx != -1)
            {
                lista[idx] = rezervacija;
                SacuvajSve(lista);
            }
        }

        public void SacuvajSve(List<RezervacijaInfo> lista)
        {
            var json = JsonConvert.SerializeObject(lista, Formatting.Indented);
            File.WriteAllText(putanja, json);
        }
    }
}
