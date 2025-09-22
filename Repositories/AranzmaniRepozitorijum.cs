using WebAranzmani.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebAranzmani.Repositories
{
    public class AranzmaniRepozitorijum
    {
        private readonly string putanja = HttpContext.Current.Server.MapPath("~/App_Data/aranzmani.json");

        public List<AranzmanInfo> PronadjiSve()
        {
            if (!File.Exists(putanja))
                return new List<AranzmanInfo>();

            var json = File.ReadAllText(putanja);
            return JsonConvert.DeserializeObject<List<AranzmanInfo>>(json) ?? new List<AranzmanInfo>();
        }

        public AranzmanInfo PronadjiPoId(int id)
        {
            return PronadjiSve().FirstOrDefault(a => a.Sifra == id);
        }

        public void SacuvajSve(List<AranzmanInfo> lista)
        {
            var json = JsonConvert.SerializeObject(lista, Formatting.Indented);
            File.WriteAllText(putanja, json);
        }

        public void Dodaj(AranzmanInfo novi)
        {
            var lista = PronadjiSve();
            novi.Sifra = lista.Any() ? lista.Max(a => a.Sifra) + 1 : 1;
            lista.Add(novi);
            SacuvajSve(lista);
        }

        public void Azuriraj(AranzmanInfo izmenjen)
        {
            var lista = PronadjiSve();
            var idx = lista.FindIndex(a => a.Sifra == izmenjen.Sifra);
            if (idx != -1)
            {
                lista[idx] = izmenjen;
                SacuvajSve(lista);
            }
        }
    }
}
