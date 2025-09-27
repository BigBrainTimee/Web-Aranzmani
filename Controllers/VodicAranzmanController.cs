using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAranzmani.Models;
using WebAranzmani.Repositories;

namespace WebAranzmani.Controllers
{
    public class VodicAranzmanController : Controller
    {
        private readonly AranzmaniRepozitorijum _aranzmaniRepo = new AranzmaniRepozitorijum();
        private readonly RezervazcijeRepozitorijum _rezervacijeRepo = new RezervazcijeRepozitorijum();
        private readonly SmestajiRepozitorijum _smestajiRepo = new SmestajiRepozitorijum();

        public ActionResult Index()
        {
            var aranzmani = _aranzmaniRepo.PronadjiSve();
            return View(aranzmani);
        }

        public ActionResult Detalji(int id)
        {
            var aranzman = _aranzmaniRepo.PronadjiPoId(id);
            if (aranzman == null)
                return HttpNotFound();

            return View(aranzman);
        }

        public ActionResult Kreiraj()
        {
            ViewBag.Smestaji = _smestajiRepo.PronadjiSve().Where(s => !s.Obrisano).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Kreiraj(AranzmanInfo novi, int[] smestajiIds, HttpPostedFileBase PosterFile)
        {
            if (ModelState.IsValid)
            {
                var lista = _aranzmaniRepo.PronadjiSve();
                novi.Sifra = lista.Any() ? lista.Max(a => a.Sifra) + 1 : 1;

                // Upload slike
                if (PosterFile != null && PosterFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(PosterFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);

                    Directory.CreateDirectory(Server.MapPath("~/Content/Images")); // osiguraj da folder postoji

                    PosterFile.SaveAs(path);
                    novi.Poster = fileName; // čuvamo samo ime fajla
                }

                // poveži smeštaje ako su odabrani
                if (smestajiIds != null)
                    novi.ListaSmestaja = smestajiIds.ToList();

                lista.Add(novi);
                _aranzmaniRepo.SacuvajSve(lista);

                TempData["Poruka"] = "✅ Novi aranžman je uspešno dodat.";
                return RedirectToAction("Index");
            }

            ViewBag.Smestaji = _smestajiRepo.PronadjiSve().Where(s => !s.Obrisano).ToList();
            return View(novi);
        }


        public ActionResult Izmeni(int id)
        {
            var aranzman = _aranzmaniRepo.PronadjiPoId(id);
            if (aranzman == null)
                return HttpNotFound();

            ViewBag.Smestaji = _smestajiRepo.PronadjiSve().Where(s => !s.Obrisano).ToList();
            return View(aranzman);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Izmeni(AranzmanInfo form, HttpPostedFileBase PosterFile)
        {
            var svi = _aranzmaniRepo.PronadjiSve();
            var stari = svi.FirstOrDefault(a => a.Sifra == form.Sifra);
            if (stari == null) return HttpNotFound();

            // Ako je uploadovana nova slika, zameni staru
            if (PosterFile != null && PosterFile.ContentLength > 0)
            {
                var fileName = Path.GetFileName(PosterFile.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                PosterFile.SaveAs(path);
                form.Poster = fileName;
            }
            else
            {
                // Ako nije uploadovana nova slika, zadrži staru
                form.Poster = stari.Poster;
            }

            // prepiši polja koja su se promenila
            stari.Naziv = form.Naziv;
            stari.DatumPocetka = form.DatumPocetka;
            stari.DatumZavrsetka = form.DatumZavrsetka;
            stari.BrojPutnika = form.BrojPutnika;
            stari.Opis = form.Opis;
            stari.Plan = form.Plan;
            stari.ListaSmestaja = form.ListaSmestaja ?? new List<int>();
            stari.Poster = form.Poster;

            _aranzmaniRepo.Azuriraj(stari);

            TempData["OK"] = "✅ Aranžman uspešno izmenjen.";
            return RedirectToAction("Index");
        }


        public ActionResult Obrisi(int id)
        {
            var lista = _aranzmaniRepo.PronadjiSve();
            var aranzman = lista.FirstOrDefault(a => a.Sifra == id);

            if (aranzman == null)
                return HttpNotFound();

            // proveri da li postoje aktivne rezervacije
            var aktivneRez = _rezervacijeRepo.PronadjiSve()
                                .Where(r => r.AranzmanId == id && r.Status == StatusRezervacije.Aktivna)
                                .ToList();

            if (aktivneRez.Any())
            {
                TempData["Greska"] = "❌ Ne može se obrisati aranžman jer postoje aktivne rezervacije.";
                return RedirectToAction("Index");
            }

            lista.Remove(aranzman);
            _aranzmaniRepo.SacuvajSve(lista);

            TempData["Poruka"] = "🗑️ Aranžman je uspešno obrisan.";
            return RedirectToAction("Index");
        }
    }
}
