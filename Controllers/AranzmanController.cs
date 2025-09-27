using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebAranzmani.Models;
using WebAranzmani.Repositories;

namespace WebAranzmani.Controllers
{
    public class AranzmanController : Controller
    {
        private readonly AranzmaniRepozitorijum _aranzmaniRepo = new AranzmaniRepozitorijum();
        private readonly SmestajiRepozitorijum _smestajiRepo = new SmestajiRepozitorijum();
        private readonly JediniceRepozitorijum _jediniceRepo = new JediniceRepozitorijum();
        private readonly RezervazcijeRepozitorijum _rezervacijeRepo = new RezervazcijeRepozitorijum();

        public ActionResult Index()
        {
            var korisnik = Session["Korisnik"] as KorisnikInfo;
            var lista = _aranzmaniRepo.PronadjiSve().Where(a => !a.Obrisano);
            if (korisnik?.Uloga == Uloga.Menadzer)
                lista = lista.Where(a => a.Menadzer == korisnik.KorisnickoIme);
            return View(lista.ToList());
        }

        public ActionResult Detalji(int id)
        {
            var ar = _aranzmaniRepo.PronadjiPoId(id);
            if (ar == null || ar.Obrisano) return RedirectToAction("Index");

            var smestaji = new List<SmestajInfo>();
            if (ar.ListaSmestaja != null)
            {
                foreach (var smId in ar.ListaSmestaja)
                {
                    var smestaj = _smestajiRepo.PronadjiPoId(smId);
                    if (smestaj == null) continue;

                    // učitaj sve jedinice
                    smestaj.SmestajneJedinice = smestaj.Jedinice
                        .Select(jid => _jediniceRepo.PronadjiPoId(jid))
                        .Where(j => j != null)
                        .ToList();

                    smestaji.Add(smestaj);
                }
            }

            ViewBag.Smestaji = smestaji;
            return View(ar); // ovo sada ide na ~/Views/Aranzman/Detalji.cshtml
        }

        public ActionResult Kreiraj()
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            if (user == null || user.Uloga != Uloga.Menadzer) return RedirectToAction("Login", "Korisnik");
            ViewBag.Smestaji = _smestajiRepo.PronadjiSve();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Kreiraj(AranzmanInfo model)
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            if (user == null || user.Uloga != Uloga.Menadzer) return RedirectToAction("Login", "Korisnik");

            if (ModelState.IsValid)
            {
                model.Sifra = _aranzmaniRepo.PronadjiSve().Any() ? _aranzmaniRepo.PronadjiSve().Max(a => a.Sifra) + 1 : 1;
                model.Menadzer = user.KorisnickoIme;
                model.Obrisano = false;
                _aranzmaniRepo.Dodaj(model);
                return RedirectToAction("Index");
            }
            ViewBag.Smestaji = _smestajiRepo.PronadjiSve();
            return View(model);
        }

        public ActionResult Izmeni(int id)
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            if (user == null || user.Uloga != Uloga.Menadzer) return RedirectToAction("Login", "Korisnik");
            var ar = _aranzmaniRepo.PronadjiPoId(id);
            if (ar == null || ar.Obrisano || ar.Menadzer != user.KorisnickoIme) return RedirectToAction("Index");
            return View(ar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Izmeni(AranzmanInfo model)
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            if (user == null || user.Uloga != Uloga.Menadzer) return RedirectToAction("Login", "Korisnik");

            if (_rezervacijeRepo.PronadjiSve().Any(r => r.AranzmanId == model.Sifra && r.Status == StatusRezervacije.Aktivna))
                return RedirectToAction("Index");

            _aranzmaniRepo.Azuriraj(model);
            return RedirectToAction("Index");
        }

        public ActionResult Obrisi(int id)
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            var ar = _aranzmaniRepo.PronadjiPoId(id);
            if (user == null || ar == null || ar.Obrisano || ar.Menadzer != user.KorisnickoIme) return RedirectToAction("Index");

            if (_rezervacijeRepo.PronadjiSve().Any(r => r.AranzmanId == ar.Sifra)) return RedirectToAction("Index");

            ar.Obrisano = true;
            _aranzmaniRepo.Azuriraj(ar);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Rezervisi(int id, int jedinicaId)
        {
            var ar = _aranzmaniRepo.PronadjiPoId(id);
            var user = Session["Korisnik"] as KorisnikInfo;
            if (ar == null || ar.Obrisano || user == null) return RedirectToAction("Index");

            var smestaj = _smestajiRepo.PronadjiPoId(ar.ListaSmestaja.FirstOrDefault());
            var jedinica = smestaj?.SmestajneJedinice.FirstOrDefault(j => j.JedinicaId == jedinicaId && j.Status == StatusJedinice.Slobodna);
            if (jedinica == null) return RedirectToAction("Detalji", new { id });

            var nova = new RezervacijaInfo
            {
                RezervacijaId = _rezervacijeRepo.PronadjiSve().Any() ? _rezervacijeRepo.PronadjiSve().Max(r => r.RezervacijaId) + 1 : 1,
                Turista = user.KorisnickoIme,
                AranzmanId = ar.Sifra,
                SmestajnaJedinicaId = jedinica.JedinicaId,
                Status = StatusRezervacije.Aktivna
            };

            jedinica.Status = StatusJedinice.Zauzeta;
            _rezervacijeRepo.Dodaj(nova);
            _smestajiRepo.Azuriraj(smestaj);

            return RedirectToAction("Detalji", new { id });
        }
    }
}
