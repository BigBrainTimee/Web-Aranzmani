using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAranzmani.Models;
using WebAranzmani.Repositories;

namespace WebAranzmani.Controllers
{
    public class VodicRezervacijaController : Controller
    {
        private readonly RezervazcijeRepozitorijum _rezervacijeRepo = new RezervazcijeRepozitorijum();
        private readonly AranzmaniRepozitorijum _aranzmaniRepo = new AranzmaniRepozitorijum();
        private readonly SmestajiRepozitorijum _smestajiRepo = new SmestajiRepozitorijum();
        private readonly JediniceRepozitorijum _jediniceRepo = new JediniceRepozitorijum();

        // Lista aranžmana koje je menadžer kreirao
        private HashSet<int> MojiAranzmani(KorisnikInfo menadzer)
        {
            var set = new HashSet<int>();
            if (menadzer?.KreiraniAranzmani != null)
            {
                foreach (var id in menadzer.KreiraniAranzmani)
                    set.Add(id);
            }
            return set;
        }

        // Prikaz svih rezervacija za menadžerove aranžmane
        public ActionResult Index()
        {
            var korisnik = Session["Korisnik"] as KorisnikInfo;
            if (korisnik == null || korisnik.Uloga != Uloga.Menadzer)
                throw new HttpException(401, "❌ Samo menadžer ima pristup.");

            var moji = MojiAranzmani(korisnik);

            var rezervacije = _rezervacijeRepo.PronadjiSve()
                                .Where(r => moji.Contains(r.AranzmanId))
                                .OrderByDescending(r => r.RezervacijaId)
                                .ToList();

            return View(rezervacije);
        }

        // Detalji pojedinačne rezervacije
        public ActionResult Detalji(int id)
        {
            var korisnik = Session["Korisnik"] as KorisnikInfo;
            if (korisnik == null || korisnik.Uloga != Uloga.Menadzer)
                throw new HttpException(401, "❌ Samo menadžer ima pristup.");

            var moji = MojiAranzmani(korisnik);

            var rez = _rezervacijeRepo.PronadjiPoId(id);
            if (rez == null) return HttpNotFound();
            if (!moji.Contains(rez.AranzmanId))
                return new HttpUnauthorizedResult("Nemaš dozvolu za ovu rezervaciju.");

            var aranzman = _aranzmaniRepo.PronadjiPoId(rez.AranzmanId);

            // Probaj da pronađeš smeštaj preko jedinice
            var smestaj = _smestajiRepo.PronadjiSve()
                .FirstOrDefault(sm => (sm.Jedinice ?? new List<int>())
                    .Any(jid => jid == rez.SmestajnaJedinicaId));

            // Ako nije pronađen, probaj preko liste smeštaja u aranžmanu
            if (smestaj == null && aranzman != null && aranzman.ListaSmestaja != null)
                smestaj = _smestajiRepo.PronadjiSve().FirstOrDefault(sm => aranzman.ListaSmestaja.Contains(sm.SmestajId));

            SmestajnaJedinicaInfo jedinica = null;
            if (smestaj != null)
                jedinica = _jediniceRepo.PronadjiPoId(rez.SmestajnaJedinicaId);

            ViewBag.Aranzman = aranzman;
            ViewBag.Smestaj = smestaj;
            ViewBag.Jedinica = jedinica;

            return View(rez);
        }
    }
}
