using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAranzmani.Models;
using WebAranzmani.Repositories;

namespace WebAranzmani.Controllers
{
    public class VodicKomentarController : Controller
    {
        private readonly KomentariRepozitorijum _komentariRepo = new KomentariRepozitorijum();
        private readonly AranzmaniRepozitorijum _aranzmaniRepo = new AranzmaniRepozitorijum();
        private readonly SmestajiRepozitorijum _smestajiRepo = new SmestajiRepozitorijum();

        private KorisnikInfo Trenutni()
        {
            var k = Session["Korisnik"] as KorisnikInfo;
            if (k == null || k.Uloga != Uloga.Menadzer)
                throw new HttpException(401, "Samo menadžer ima pristup ovde.");
            return k;
        }

        private HashSet<int> MojiAranzmani(KorisnikInfo korisnik)
        {
            return _aranzmaniRepo.PronadjiSve()
                .Where(a => a.Menadzer == korisnik.KorisnickoIme)
                .Select(a => a.Sifra)
                .ToHashSet();
        }

        private bool KomentarNaMojSmestaj(KomentarInfo komentar, HashSet<int> moji)
        {
            var smestaj = _smestajiRepo.PronadjiSve()
                .FirstOrDefault(s => string.Equals(s.Naziv, komentar.SmestajNaziv, StringComparison.OrdinalIgnoreCase));

            if (smestaj == null) return false;

            return _aranzmaniRepo.PronadjiSve()
                .Any(a => moji.Contains(a.Sifra) && a.ListaSmestaja != null && a.ListaSmestaja.Contains(smestaj.SmestajId));
        }

        public ActionResult Index(bool? samoNeprihvaceni = true)
        {
            var korisnik = Trenutni();
            var moji = MojiAranzmani(korisnik);

            var lista = _komentariRepo.PronadjiSve()
                .Where(k => KomentarNaMojSmestaj(k, moji))
                .ToList();

            if (samoNeprihvaceni.GetValueOrDefault(false))
                lista = lista.Where(k => !k.Prihvacen).ToList();

            ViewBag.SamoNeprihvaceni = samoNeprihvaceni;
            return View(lista.OrderByDescending(k => k.KomentarId).ToList());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Prihvati(int id)
        {
            var korisnik = Trenutni();
            var moji = MojiAranzmani(korisnik);

            var komentar = _komentariRepo.PronadjiSve().FirstOrDefault(k => k.KomentarId == id);
            if (komentar == null) return HttpNotFound();
            if (!KomentarNaMojSmestaj(komentar, moji))
                return new HttpUnauthorizedResult("Nemaš dozvolu.");

            komentar.Prihvacen = true;
            _komentariRepo.Azuriraj(komentar);

            TempData["OK"] = "✅ Komentar je prihvaćen i sada je vidljiv svima.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Odbij(int id)
        {
            var korisnik = Trenutni();
            var moji = MojiAranzmani(korisnik);

            var komentar = _komentariRepo.PronadjiSve().FirstOrDefault(k => k.KomentarId == id);
            if (komentar == null) return HttpNotFound();
            if (!KomentarNaMojSmestaj(komentar, moji))
                return new HttpUnauthorizedResult("Nemaš dozvolu.");

            komentar.Prihvacen = false;
            _komentariRepo.Obrisi(id);

            TempData["OK"] = "❌ Komentar je odbijen i trajno uklonjen.";
            return RedirectToAction("Index");
        }
    }
}
