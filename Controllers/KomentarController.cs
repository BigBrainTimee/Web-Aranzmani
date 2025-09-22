using System.Linq;
using System.Web.Mvc;
using WebAranzmani.Models;
using WebAranzmani.Repositories;
using System.Collections.Generic;

namespace WebAranzmani.Controllers
{
    public class KomentarController : Controller
    {
        private readonly KomentariRepozitorijum _komentariRepo = new KomentariRepozitorijum();
        private readonly SmestajiRepozitorijum _smestajiRepo = new SmestajiRepozitorijum();
        private readonly AranzmaniRepozitorijum _aranzmaniRepo = new AranzmaniRepozitorijum();

        // Pregled komentara
        public ActionResult Index()
        {
            var korisnik = Session["Korisnik"] as KorisnikInfo;
            if (korisnik == null)
                return RedirectToAction("Login", "Korisnik");

            var svi = _komentariRepo.PronadjiSve();

            if (korisnik.Uloga == Uloga.Turista)
            {
                return View(svi.Where(k => k.Turista == korisnik.KorisnickoIme).ToList());
            }
            else if (korisnik.Uloga == Uloga.Menadzer)
            {
                // svi aranžmani koje menadžer vodi
                var aranzmaniMenadzera = _aranzmaniRepo.PronadjiSve()
                    .Where(a => a.Menadzer == korisnik.KorisnickoIme)
                    .ToList();

                // njihovi smeštaji
                var smestajiMenadzera = aranzmaniMenadzera
                    .SelectMany(a => a.ListaSmestaja)
                    .Select(id => _smestajiRepo.PronadjiPoId(id)?.Naziv)
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();

                // komentari samo za te smeštaje i samo oni koji čekaju odobrenje
                var komentariMenadzera = svi
                    .Where(k => smestajiMenadzera.Contains(k.SmestajNaziv) && !k.Prihvacen)
                    .ToList();

                return View(komentariMenadzera);
            }

            // Admin ili ostali vide sve
            return View(svi.ToList());
        }

        // Detalji komentara
        public ActionResult Detalji(int id)
        {
            var komentar = _komentariRepo.PronadjiPoId(id);
            if (komentar == null)
                return HttpNotFound();

            return View(komentar);
        }

        // GET: Komentar/Create
        public ActionResult Create(string smestajNaziv)
        {
            var korisnik = Session["Korisnik"] as KorisnikInfo;
            if (korisnik == null || korisnik.Uloga != Uloga.Turista)
                return RedirectToAction("Login", "Korisnik");

            ViewBag.SmestajNaziv = smestajNaziv;
            return View();
        }

        // POST: Komentar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string smestajNaziv, string sadrzaj, int ocena)
        {
            var korisnik = Session["Korisnik"] as KorisnikInfo;
            if (korisnik == null || korisnik.Uloga != Uloga.Turista)
                return RedirectToAction("Login", "Korisnik");

            var noviId = _komentariRepo.PronadjiSve().Any()
                         ? _komentariRepo.PronadjiSve().Max(k => k.KomentarId) + 1
                         : 1;

            var komentar = new KomentarInfo(
                noviId,
                korisnik.KorisnickoIme,
                smestajNaziv,
                sadrzaj,
                ocena,
                false // menadžer mora da odobri
            );

            _komentariRepo.Dodaj(komentar);
            TempData["Poruka"] = "✅ Komentar je dodat. Čeka na odobrenje menadžera.";

            return RedirectToAction("Index");
        }

        // Menadžer odobrava komentar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Odobri(int id)
        {
            var korisnik = Session["Korisnik"] as KorisnikInfo;
            if (korisnik == null || korisnik.Uloga != Uloga.Menadzer)
                return RedirectToAction("Login", "Korisnik");

            var komentar = _komentariRepo.PronadjiPoId(id);
            if (komentar == null) return HttpNotFound();

            komentar.Prihvacen = true;
            _komentariRepo.Azuriraj(komentar);

            TempData["OK"] = "✅ Komentar je odobren i sada je javan.";
            return RedirectToAction("Index");
        }


        // Menadžer odbija komentar
        // Menadžer odbija komentar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Odbij(int id)
        {
            var korisnik = Session["Korisnik"] as KorisnikInfo;
            if (korisnik == null || korisnik.Uloga != Uloga.Menadzer)
                return RedirectToAction("Login", "Korisnik");

            var komentar = _komentariRepo.PronadjiPoId(id);
            if (komentar == null) return HttpNotFound();

            // 🚮 trajno brisanje komentara
            _komentariRepo.Obrisi(id);

            TempData["Greska"] = "❌ Komentar je odbijen i trajno uklonjen.";
            return RedirectToAction("Index");
        }


        // Javni komentari za prikaz u Detalji.cshtml
        [ChildActionOnly]
        public ActionResult PublicComments(string smestajNaziv)
        {
            if (string.IsNullOrEmpty(smestajNaziv))
                return PartialView("_Lista", new List<KomentarInfo>());

            var komentari = _komentariRepo.PronadjiSve()
                .Where(k => k.SmestajNaziv.Equals(smestajNaziv, System.StringComparison.OrdinalIgnoreCase)
                         && k.Prihvacen)
                .ToList();

            return PartialView("_Lista", komentari);
        }
    }
}
