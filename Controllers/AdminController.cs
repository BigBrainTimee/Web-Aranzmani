using WebAranzmani.Models;
using WebAranzmani.Repositories;
using WebAranzmani.Service;
using System.Linq;
using System.Web.Mvc;


namespace Aranzmani.Controllers
{
    public class AdminController : Controller
    {
        private readonly KorisniciRepozitorijum _korisniciRepo = new KorisniciRepozitorijum();
        private readonly AranzmaniRepozitorijum _aranzmaniRepo = new AranzmaniRepozitorijum();
        private readonly KomentariRepozitorijum _komentariRepo = new KomentariRepozitorijum();
        public ActionResult Index()
        {
            var sviKorisnici = _korisniciRepo.PronadjiSve();
            return View(sviKorisnici);
        }


        public ActionResult Blokiraj(string korisnickoIme)
        {
            var korisnik = _korisniciRepo.PronadjiPoKorisnickomImenu(korisnickoIme);
            if (korisnik != null)
            {
                korisnik.Blokiran = true;
                _korisniciRepo.Azuriraj(korisnik);
                TempData["Poruka"] = $"Korisnik {korisnickoIme} je uspešno blokiran.";
            }
            else
            {
                TempData["Poruka"] = "Korisnik nije pronađen.";
            }

            return RedirectToAction("Index");
        }


        public ActionResult Odblokiraj(string korisnickoIme)
        {
            var korisnik = _korisniciRepo.PronadjiPoKorisnickomImenu(korisnickoIme);
            if (korisnik != null)
            {
                korisnik.Blokiran = false;
                _korisniciRepo.Azuriraj(korisnik);
                TempData["Poruka"] = $"Korisnik {korisnickoIme} je uspešno odblokiran.";
            }
            else
            {
                TempData["Poruka"] = "Korisnik nije pronađen.";
            }

            return RedirectToAction("Index");
        }


        public ActionResult Komentari()
        {
            var komentari = _komentariRepo.PronadjiSve();
            return View(komentari);
        }


        public ActionResult OdbijKomentar(int id)
        {
            var komentari = _komentariRepo.PronadjiSve();
            var komentar = komentari.FirstOrDefault(k => k.KomentarId == id);
            if (komentar != null)
            {
                komentar.Prihvacen = false;
                _komentariRepo.Azuriraj(komentar);
                TempData["Poruka"] = "Komentar je odbijen.";
            }
            else
            {
                TempData["Poruka"] = "Komentar nije pronađen.";
            }

            return RedirectToAction("Komentari");
        }


        public ActionResult PrihvatiKomentar(int id)
        {
            var komentari = _komentariRepo.PronadjiSve();
            var komentar = komentari.FirstOrDefault(k => k.KomentarId == id);
            if (komentar != null)
            {
                komentar.Prihvacen = true;
                _komentariRepo.Azuriraj(komentar);
                TempData["Poruka"] = "Komentar je prihvaćen.";
            }
            else
            {
                TempData["Poruka"] = "Komentar nije pronađen.";
            }

            return RedirectToAction("Komentari");
        }
        public ActionResult Users()
        {
            var korisnici = _korisniciRepo.PronadjiSve();
            return View(korisnici);
        }

    }
}
