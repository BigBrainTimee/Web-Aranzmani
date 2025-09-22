using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using WebAranzmani.Models;
using WebAranzmani.Service;

namespace WebAranzmani.Controllers
{
    public class KorisnikController : Controller
    {
        private readonly KorisniciServise _korisniciServis = new KorisniciServise();

        // LOGIN
        public ActionResult Prijava() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Prijava(string korisnickoIme, string lozinka)
        {
            var korisnik = _korisniciServis.Login(korisnickoIme, lozinka);
            if (korisnik != null)
            {
                Session["Korisnik"] = korisnik;
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Pogrešno korisničko ime ili lozinka!";
            return View();
        }

        public ActionResult Odjava()
        {
            Session["Korisnik"] = null;
            return RedirectToAction("Index", "Home");
        }

        // REGISTRACIJA TURISTE
        public ActionResult Registracija() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Registracija(KorisnikInfo novi)
        {
            ModelState.Remove("DatumRodjenja");
            var rawDob = Request.Form["DatumRodjenja"];
            if (!string.IsNullOrWhiteSpace(rawDob) &&
                DateTime.TryParseExact(rawDob, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out var dob))
                novi.DatumRodjenja = dob;

            if (!ModelState.IsValid) return View(novi);

            novi.Uloga = Uloga.Turista; // obična registracija = turista
            if (_korisniciServis.Registruj(novi))
            {
                TempData["Success"] = "✅ Uspešno ste se registrovali!";
                return RedirectToAction("Prijava");
            }

            ViewBag.Error = "Korisničko ime već postoji!";
            return View(novi);
        }

        // REGISTRACIJA MENADŽERA (samo admin)
        public ActionResult DodajMenadzera()
        {
            var admin = Session["Korisnik"] as KorisnikInfo;
            if (admin == null || admin.Uloga != Uloga.Admin) return RedirectToAction("Prijava");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DodajMenadzera(KorisnikInfo menadzer)
        {
            var admin = Session["Korisnik"] as KorisnikInfo;
            if (admin == null || admin.Uloga != Uloga.Admin) return RedirectToAction("Prijava");

            menadzer.Uloga = Uloga.Menadzer;
            if (_korisniciServis.Registruj(menadzer))
            {
                TempData["Success"] = "✅ Menadžer dodat.";
                return RedirectToAction("Lista");
            }
            ViewBag.Error = "❌ Korisničko ime već postoji!";
            return View(menadzer);
        }

        // PREGLED PROFILA
        [HttpGet]
        public ActionResult Profil()
        {
            if (Session["Korisnik"] == null) return RedirectToAction("Prijava");
            var korisnik = (KorisnikInfo)Session["Korisnik"];

            // Ako je default vrednost, postavi npr. 10 godina unazad
            if (korisnik.DatumRodjenja == DateTime.MinValue)
            {
                korisnik.DatumRodjenja = DateTime.Today.AddYears(-10);
            }

            return View(korisnik);
        }



        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Profil(KorisnikInfo izmenjen)
        {
            var user = (KorisnikInfo)Session["Korisnik"];
            if (user == null) return RedirectToAction("Prijava");

            user.KorisnickoIme = izmenjen.KorisnickoIme;
            user.Ime = izmenjen.Ime;
            user.Prezime = izmenjen.Prezime;
            user.Email = izmenjen.Email;
            user.Pol = izmenjen.Pol;
            user.DatumRodjenja = izmenjen.DatumRodjenja;
            if (!string.IsNullOrWhiteSpace(izmenjen.Lozinka))
                user.Lozinka = izmenjen.Lozinka;

            _korisniciServis.Azuriraj(user);
            Session["Korisnik"] = user;
            TempData["Success"] = "✅ Profil ažuriran.";
            return RedirectToAction("Profil");
        }

        // LISTA KORISNIKA (samo admin)
        public ActionResult Lista(string pretragaIme, string pretragaPrezime, string pretragaKorisnicko, string pretragaUloga)
        {
            var admin = Session["Korisnik"] as KorisnikInfo;
            if (admin == null || admin.Uloga != Uloga.Admin) return RedirectToAction("Prijava");

            var lista = _korisniciServis.PronadjiSve();

            if (!string.IsNullOrEmpty(pretragaIme))
                lista = lista.Where(k => k.Ime.Contains(pretragaIme)).ToList();
            if (!string.IsNullOrEmpty(pretragaPrezime))
                lista = lista.Where(k => k.Prezime.Contains(pretragaPrezime)).ToList();
            if (!string.IsNullOrEmpty(pretragaKorisnicko))
                lista = lista.Where(k => k.KorisnickoIme.Contains(pretragaKorisnicko)).ToList();
            if (!string.IsNullOrEmpty(pretragaUloga))
                lista = lista.Where(k => k.Uloga.ToString() == pretragaUloga).ToList();

            return View(lista);
        }

        // BRISANJE KORISNIKA (samo admin)
        public ActionResult DeleteUser(string username)
        {
            var admin = Session["Korisnik"] as KorisnikInfo;
            if (admin == null || admin.Uloga != Uloga.Admin)
                return RedirectToAction("Prijava");

            if (string.IsNullOrEmpty(username))
                return RedirectToAction("Lista");

            // poziva servis da obriše korisnika
            _korisniciServis.Obrisi(username);

            TempData["Success"] = $"🗑️ Korisnik {username} je obrisan.";
            return RedirectToAction("Lista");
        }

    }
}
