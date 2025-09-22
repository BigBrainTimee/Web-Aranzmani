using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using WebAranzmani.Models;

namespace WebAranzmani.Service
{
    public class AranzmaniServise
    {
        private readonly List<AranzmanInfo> _aranzmani;
        private readonly List<SmestajInfo> _smestaji;
        private readonly List<SmestajnaJedinicaInfo> _jedinice;

        public AranzmaniServise()
        {
            _aranzmani = JsonConvert.DeserializeObject<List<AranzmanInfo>>(
                File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/aranzmani.json"))
            ) ?? new List<AranzmanInfo>();

            _smestaji = JsonConvert.DeserializeObject<List<SmestajInfo>>(
                File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/smestaji.json"))
            ) ?? new List<SmestajInfo>();

            _jedinice = JsonConvert.DeserializeObject<List<SmestajnaJedinicaInfo>>(
                File.ReadAllText(HttpContext.Current.Server.MapPath("~/App_Data/smestajneJedinice.json"))
            ) ?? new List<SmestajnaJedinicaInfo>();

            // povezivanje jedinica i smeštaja
            foreach (var sm in _smestaji)
            {
                sm.SmestajneJedinice = _jedinice.Where(j => j.SmestajId == sm.SmestajId).ToList();
            }

            // povezivanje aranžmana i smeštaja
            foreach (var a in _aranzmani)
            {
                a.Smestaji = _smestaji.Where(s => a.ListaSmestaja.Contains(s.SmestajId)).ToList();
            }
        }

        public List<AranzmanInfo> PronadjiSve() => _aranzmani;

        public AranzmanInfo PronadjiPoId(int id)
        {
            var aranzman = _aranzmani.FirstOrDefault(a => a.Sifra == id);
            if (aranzman == null) return null;

            foreach (var sm in aranzman.Smestaji)
            {
                sm.SmestajneJedinice = _jedinice
                    .Where(j => sm.Jedinice.Contains(j.JedinicaId))
                    .ToList();
            }

            return aranzman;
        }
    }
}
