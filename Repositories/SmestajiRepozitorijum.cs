using WebAranzmani.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebAranzmani.Repositories
{
    public class SmestajiRepozitorijum
    {
        private readonly string putanja = HttpContext.Current.Server.MapPath("~/App_Data/smestaji.json");

        public List<SmestajInfo> PronadjiSve()
        {
            if (!File.Exists(putanja))
                return new List<SmestajInfo>();

            var json = File.ReadAllText(putanja);
            return JsonConvert.DeserializeObject<List<SmestajInfo>>(json) ?? new List<SmestajInfo>();
        }

        public SmestajInfo PronadjiPoId(int id)
        {
            return PronadjiSve().FirstOrDefault(s => s.SmestajId == id);
        }

        public void SacuvajSve(List<SmestajInfo> lista)
        {
            var json = JsonConvert.SerializeObject(lista, Formatting.Indented);
            File.WriteAllText(putanja, json);
        }

        public void Azuriraj(SmestajInfo smestaj)
        {
            var lista = PronadjiSve();
            var idx = lista.FindIndex(s => s.SmestajId == smestaj.SmestajId);
            if (idx != -1)
            {
                lista[idx] = smestaj;
                SacuvajSve(lista);
            }
        }
    }
}
