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

        // GET: /Admin/
        public ActionResult Index()
        {
            var sviKorisnici = _korisniciRepo.PronadjiSve();
            return View(sviKorisnici);
        }

        // Blokiranje korisnika
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

        // Odblokiranje korisnika
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

        // Pregled komentara
        public ActionResult Komentari()
        {
            var komentari = _komentariRepo.PronadjiSve();
            return View(komentari);
        }

        // Odbijanje komentara
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

        // Prihvatanje komentara
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

        // Pregled svih korisnika
        public ActionResult Users()
        {
            var korisnici = _korisniciRepo.PronadjiSve();
            return View(korisnici);
        }

        // GET: Dodavanje menadžera
        public ActionResult CreateManager()
        {
            return View();
        }

        // POST: Dodavanje menadžera
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateManager(KorisnikInfo novi)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Popunite sva obavezna polja.";
                return View(novi);
            }

            var korisniciServis = new KorisniciServise();
            if (korisniciServis.PronadjiSve().Any(k => k.KorisnickoIme == novi.KorisnickoIme))
            {
                ViewBag.Error = "Korisničko ime već postoji.";
                return View(novi);
            }

            novi.Uloga = Uloga.Menadzer; // OVDE postavljamo ulogu
                                         // direktno sačuvamo korisnika, a ne kroz Registruj, da se ne prepisuje uloga
            var svi = korisniciServis.PronadjiSve();
            svi.Add(novi);
            new KorisniciRepozitorijum().SacuvajSve(svi);

            TempData["Success"] = $"Menadžer {novi.Ime} {novi.Prezime} je uspešno dodat.";
            return RedirectToAction("Users");
        }

        // GET ili POST: /Admin/DeleteUser
        public ActionResult DeleteUser(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                TempData["Poruka"] = "Korisničko ime nije prosleđeno.";
                return RedirectToAction("Users");
            }

            var korisniciServis = new KorisniciServise();
            var user = korisniciServis.PronadjiSve().FirstOrDefault(k => k.KorisnickoIme == username);

            if (user == null)
            {
                TempData["Poruka"] = "Korisnik nije pronađen.";
            }
            else if (user.Uloga == Uloga.Admin)
            {
                TempData["Poruka"] = "Admin ne može da obriše samog sebe ili drugog admina.";
            }
            else
            {
                korisniciServis.Obrisi(username);
                TempData["Poruka"] = $"Korisnik {username} je uspešno obrisan.";
            }

            return RedirectToAction("Users");
        }


    }
}
