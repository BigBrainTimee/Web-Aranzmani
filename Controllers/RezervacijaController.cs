using System;
using System.Linq;
using System.Web.Mvc;
using WebAranzmani.Models;
using WebAranzmani.Repositories;
using System.Collections.Generic;

namespace WebAranzmani.Controllers
{
    public class RezervacijaController : Controller
    {
        private readonly RezervazcijeRepozitorijum _rezRepo = new RezervazcijeRepozitorijum();
        private readonly AranzmaniRepozitorijum _aranzmaniRepo = new AranzmaniRepozitorijum();
        private readonly SmestajiRepozitorijum _smestajiRepo = new SmestajiRepozitorijum();
        private readonly JediniceRepozitorijum _jediniceRepo = new JediniceRepozitorijum();

        // === PREGLED REZERVACIJA (turista vidi svoje, menadžer vidi sve za svoje aranžmane) ===
        public ActionResult Index(string pretragaId, string pretragaNaziv, string pretragaStatus, string sortiranje)
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            if (user == null)
                return RedirectToAction("Login", "Korisnik");

            IEnumerable<RezervacijaInfo> lista = _rezRepo.PronadjiSve();

            if (user.Uloga == Uloga.Turista)
            {
                // samo svoje
                lista = lista.Where(r => r.Turista == user.KorisnickoIme);
            }
            else if (user.Uloga == Uloga.Menadzer)
            {
                // samo rezervacije za aranžmane koje je kreirao menadžer
                var mojiAranzmani = _aranzmaniRepo.PronadjiSve()
                    .Where(a => a.Menadzer == user.KorisnickoIme)
                    .Select(a => a.Sifra)
                    .ToList();

                lista = lista.Where(r => mojiAranzmani.Contains(r.AranzmanId));
            }

            // Pretraga
            if (!string.IsNullOrEmpty(pretragaId))
                lista = lista.Where(r => r.RezervacijaId.ToString().Contains(pretragaId));

            if (!string.IsNullOrEmpty(pretragaNaziv))
                lista = lista.Where(r =>
                {
                    var ar = _aranzmaniRepo.PronadjiPoId(r.AranzmanId);
                    return ar != null && ar.Naziv.IndexOf(pretragaNaziv, StringComparison.OrdinalIgnoreCase) >= 0;
                });

            if (!string.IsNullOrEmpty(pretragaStatus))
                lista = lista.Where(r => r.Status.ToString().Equals(pretragaStatus, StringComparison.OrdinalIgnoreCase));

            // Sortiranje
            switch (sortiranje)
            {
                case "naziv_rastuce":
                    lista = lista.OrderBy(r => _aranzmaniRepo.PronadjiPoId(r.AranzmanId)?.Naziv);
                    break;
                case "naziv_opadajuce":
                    lista = lista.OrderByDescending(r => _aranzmaniRepo.PronadjiPoId(r.AranzmanId)?.Naziv);
                    break;
                case "id_rastuce":
                    lista = lista.OrderBy(r => r.RezervacijaId);
                    break;
                case "id_opadajuce":
                    lista = lista.OrderByDescending(r => r.RezervacijaId);
                    break;
                default:
                    lista = lista.OrderByDescending(r => r.RezervacijaId);
                    break;
            }

            // 🔑 dodaj kolekcije koje view koristi
            ViewBag.Aranzmani = _aranzmaniRepo.PronadjiSve();
            ViewBag.Smestaji = _smestajiRepo.PronadjiSve();
            ViewBag.SmestajneJedinice = _jediniceRepo.PronadjiSve();

            return View(lista.ToList());
        }

        // === TURISTA KREIRA REZERVACIJU ===
        [ActionName("Kreiraj")]
        public ActionResult Create(int aranzmanId, int smestajId, int jedinicaId)
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            if (user == null || user.Uloga != Uloga.Turista)
                return RedirectToAction("Login", "Korisnik");

            var aranzman = _aranzmaniRepo.PronadjiPoId(aranzmanId);
            if (aranzman == null || !aranzman.ListaSmestaja.Contains(smestajId))
                return HttpNotFound();

            var jedinica = _jediniceRepo.PronadjiPoId(jedinicaId);
            if (jedinica == null || jedinica.Status != StatusJedinice.Slobodna)
            {
                TempData["Greska"] = "❌ Jedinica nije slobodna.";
                return RedirectToAction("Detalji", "Aranzman", new { id = aranzmanId });
            }

            int noviId = _rezRepo.PronadjiSve().Any() ? _rezRepo.PronadjiSve().Max(r => r.RezervacijaId) + 1 : 1;
            var nova = new RezervacijaInfo(noviId, user.KorisnickoIme, StatusRezervacije.Aktivna, aranzmanId, jedinicaId);

            jedinica.Status = StatusJedinice.Zauzeta;
            _jediniceRepo.Azuriraj(jedinica);

            var sve = _rezRepo.PronadjiSve();
            sve.Add(nova);
            _rezRepo.SacuvajSve(sve);

            TempData["OK"] = "✅ Rezervacija je uspešno kreirana.";
            return RedirectToAction("Detalji", "Aranzman", new { id = aranzmanId });
        }

        // === TURISTA OTKAZUJE REZERVACIJU ===
        public ActionResult Otkazi(int id)
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            if (user == null || user.Uloga != Uloga.Turista)
                return RedirectToAction("Login", "Korisnik");

            var rez = _rezRepo.PronadjiPoId(id);
            if (rez == null || rez.Turista != user.KorisnickoIme)
                return HttpNotFound();

            var aranzman = _aranzmaniRepo.PronadjiPoId(rez.AranzmanId);
            if (aranzman == null) return HttpNotFound();

            if (aranzman.DatumZavrsetka <= DateTime.Now)
            {
                TempData["Greska"] = "❌ Ne možete otkazati jer je aranžman završen.";
                return RedirectToAction("Index");
            }

            rez.Status = StatusRezervacije.Otkazana;
            _rezRepo.Azuriraj(rez);

            var jedinica = _jediniceRepo.PronadjiPoId(rez.SmestajnaJedinicaId);
            if (jedinica != null)
            {
                jedinica.Status = StatusJedinice.Slobodna;
                _jediniceRepo.Azuriraj(jedinica);
            }

            TempData["OK"] = "Rezervacija je otkazana.";
            return RedirectToAction("Index");
        }

        // === MENADŽER REZERVIŠE ZA GOSTA ===
        public ActionResult RezervisiMenadzer(int aranzmanId, int smestajId, int jedinicaId)
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            if (user == null || user.Uloga != Uloga.Menadzer)
                return RedirectToAction("Login", "Korisnik");

            var jedinica = _jediniceRepo.PronadjiPoId(jedinicaId);
            if (jedinica == null || jedinica.Status != StatusJedinice.Slobodna)
            {
                TempData["Greska"] = "❌ Jedinica nije slobodna.";
                return RedirectToAction("Detalji", "Home", new { naziv = _aranzmaniRepo.PronadjiPoId(aranzmanId)?.Naziv });
            }

            jedinica.Status = StatusJedinice.Zauzeta;
            _jediniceRepo.Azuriraj(jedinica);

            TempData["OK"] = "✅ Menadžer je uspešno rezervisao jedinicu.";
            return RedirectToAction("Detalji", "Home", new { naziv = _aranzmaniRepo.PronadjiPoId(aranzmanId)?.Naziv });
        }

        // === MENADŽER OTKAZUJE ===
        public ActionResult OtkaziMenadzer(int jedinicaId)
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            if (user == null || user.Uloga != Uloga.Menadzer)
                return RedirectToAction("Login", "Korisnik");

            var jedinica = _jediniceRepo.PronadjiPoId(jedinicaId);
            if (jedinica == null)
                return HttpNotFound();

            jedinica.Status = StatusJedinice.Slobodna;
            _jediniceRepo.Azuriraj(jedinica);

            TempData["OK"] = "Rezervacija otkazana od strane menadžera.";
            return RedirectToAction("Detalji", "Home", new { naziv = _aranzmaniRepo.PronadjiPoId(jedinica.SmestajId)?.Naziv });
        }

        // === DETALJI JEDNE REZERVACIJE ===
        public ActionResult Detalji(int id)
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            if (user == null) return RedirectToAction("Login", "Korisnik");

            var rez = _rezRepo.PronadjiPoId(id);
            if (rez == null) return HttpNotFound();

            if (user.Uloga == Uloga.Turista && rez.Turista != user.KorisnickoIme)
                return HttpNotFound();

            var aranzman = _aranzmaniRepo.PronadjiPoId(rez.AranzmanId);
            var jedinica = _jediniceRepo.PronadjiPoId(rez.SmestajnaJedinicaId);
            var smestaj = jedinica != null ? _smestajiRepo.PronadjiPoId(jedinica.SmestajId) : null;

            ViewBag.Aranzman = aranzman;
            ViewBag.Smestaj = smestaj;
            ViewBag.Jedinica = jedinica;
            ViewBag.MozeKomentar = aranzman != null && aranzman.DatumZavrsetka <= DateTime.Now;

            return View(rez);
        }

        // === KOMENTARISANJE ===
        public ActionResult DodajKomentar(int id)
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            if (user == null || user.Uloga != Uloga.Turista)
                return RedirectToAction("Prijava", "Korisnik");

            var rez = _rezRepo.PronadjiPoId(id);
            if (rez == null || rez.Turista != user.KorisnickoIme)
                return HttpNotFound();

            var aranzman = _aranzmaniRepo.PronadjiPoId(rez.AranzmanId);
            if (aranzman == null || aranzman.DatumZavrsetka > DateTime.Now)
            {
                TempData["Greska"] = "❌ Komentar se može ostaviti tek nakon završetka putovanja.";
                return RedirectToAction("Detalji", new { id });
            }

            ViewBag.RezervacijaId = id;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DodajKomentar(int rezervacijaId, int ocena, string tekst)
        {
            var user = Session["Korisnik"] as KorisnikInfo;
            if (user == null || user.Uloga != Uloga.Turista)
                return RedirectToAction("Prijava", "Korisnik");

            var rez = _rezRepo.PronadjiPoId(rezervacijaId);
            if (rez == null || rez.Turista != user.KorisnickoIme)
                return HttpNotFound();

            var jedinica = _jediniceRepo.PronadjiPoId(rez.SmestajnaJedinicaId);
            if (jedinica == null) return HttpNotFound();

            var smestaj = _smestajiRepo.PronadjiPoId(jedinica.SmestajId);

            var komentar = new KomentarInfo
            {
                Turista = user.KorisnickoIme,
                SmestajNaziv = smestaj?.Naziv ?? string.Empty,
                Sadrzaj = tekst,
                Ocena = ocena,
                Prihvacen = false // uvek NA ČEKANJU dok menadžer ne odobri
            };

            // čuvaj u zajedničkom repo fajlu
            var repo = new KomentariRepozitorijum();
            repo.Dodaj(komentar);

            TempData["OK"] = "💬 Komentar dodat i čeka odobrenje menadžera.";
            return RedirectToAction("Detalji", new { id = rezervacijaId });
        }

    }
}
