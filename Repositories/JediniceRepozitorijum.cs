using WebAranzmani.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebAranzmani.Repositories
{
    public class JediniceRepozitorijum
    {
        private readonly string putanja = HttpContext.Current.Server.MapPath("~/App_Data/smestajneJedinice.json");

        public List<SmestajnaJedinicaInfo> PronadjiSve()
        {
            if (!File.Exists(putanja))
                return new List<SmestajnaJedinicaInfo>();

            var json = File.ReadAllText(putanja);
            return JsonConvert.DeserializeObject<List<SmestajnaJedinicaInfo>>(json) ?? new List<SmestajnaJedinicaInfo>();
        }

        public SmestajnaJedinicaInfo PronadjiPoId(int id)
        {
            return PronadjiSve().FirstOrDefault(j => j.JedinicaId == id);
        }

        public void SacuvajSve(List<SmestajnaJedinicaInfo> lista)
        {
            var json = JsonConvert.SerializeObject(lista, Formatting.Indented);
            File.WriteAllText(putanja, json);
        }

        public void Azuriraj(SmestajnaJedinicaInfo jedinica)
        {
            var lista = PronadjiSve();
            var idx = lista.FindIndex(j => j.JedinicaId == jedinica.JedinicaId);
            if (idx != -1)
            {
                lista[idx] = jedinica;
                SacuvajSve(lista);
            }
        }
    }
}
