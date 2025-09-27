using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAranzmani.Models;
using WebAranzmani.Repositories;

namespace WebAranzmani.Controllers
{
    public class VodicJedinicaController : Controller
    {
        private readonly JediniceRepozitorijum _jediniceRepo = new JediniceRepozitorijum();
        private readonly SmestajiRepozitorijum _smestajiRepo = new SmestajiRepozitorijum();
        private readonly AranzmaniRepozitorijum _aranzmaniRepo = new AranzmaniRepozitorijum();
        private readonly RezervazcijeRepozitorijum _rezervacijeRepo = new RezervazcijeRepozitorijum();


        private KorisnikInfo Curr()
        {
            var k = Session["Korisnik"] as KorisnikInfo;
            if (k == null || k.Uloga != Uloga.Menadzer)
                throw new HttpException(401, "❌ Samo menadžer može pristupiti ovde.");
            return k;
        }

        private bool SmestajPripadaMeni(int smestajId, KorisnikInfo korisnik)
        {
            var aranzmani = _aranzmaniRepo.PronadjiSve()
                .Where(a => a.Menadzer == korisnik.KorisnickoIme && !a.Obrisano)
                .ToList();

            return aranzmani.Any(a => a.ListaSmestaja != null && a.ListaSmestaja.Contains(smestajId));
        }
        public ActionResult Index(int smestajId)
        {
            var korisnik = Curr();
            var smestaj = _smestajiRepo.PronadjiPoId(smestajId);
            if (smestaj == null) return HttpNotFound();
            if (!SmestajPripadaMeni(smestajId, korisnik))
                return new HttpUnauthorizedResult("❌ Možeš pristupiti samo jedinicama svojih smeštaja.");

            var jedinice = _jediniceRepo.PronadjiSve()
                                        .Where(j => !j.Obrisana && j.SmestajId == smestajId)
                                        .OrderBy(j => j.JedinicaId)
                                        .ToList();

            ViewBag.Smestaj = smestaj;
            return View(jedinice);
        }

        public ActionResult Kreiraj(int smestajId)
        {
            var korisnik = Curr();
            var smestaj = _smestajiRepo.PronadjiPoId(smestajId);
            if (smestaj == null) return HttpNotFound();
            if (!SmestajPripadaMeni(smestajId, korisnik))
                return new HttpUnauthorizedResult("❌ Možeš dodavati jedinice samo u svoje smeštaje.");

            ViewBag.Smestaj = smestaj;

            return View(new SmestajnaJedinicaInfo
            {
                SmestajId = smestajId,
                Status = StatusJedinice.Slobodna,
                Ljubimci = false,
                Cena = 0
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Kreiraj(SmestajnaJedinicaInfo form)
        {
            var korisnik = Curr();
            if (!SmestajPripadaMeni(form.SmestajId, korisnik))
                return new HttpUnauthorizedResult("❌ Možeš dodavati jedinice samo u svoje smeštaje.");
            var lista = _jediniceRepo.PronadjiSve();
            form.JedinicaId = lista.Any() ? lista.Max(j => j.JedinicaId) + 1 : 1;
            form.Obrisana = false;

            lista.Add(form);
            _jediniceRepo.SacuvajSve(lista);

            var smestaj = _smestajiRepo.PronadjiPoId(form.SmestajId);
            if (smestaj == null) return HttpNotFound();

            if (smestaj.Jedinice == null)
                smestaj.Jedinice = new System.Collections.Generic.List<int>();

            if (!smestaj.Jedinice.Contains(form.JedinicaId))
                smestaj.Jedinice.Add(form.JedinicaId);

            _smestajiRepo.Azuriraj(smestaj);

            TempData["OK"] = "✅ Jedinica je uspešno dodata.";
            return RedirectToAction("Index", new { smestajId = form.SmestajId });
        }

        public ActionResult Izmeni(int id)
        {
            var korisnik = Curr();
            var j = _jediniceRepo.PronadjiPoId(id);
            if (j == null) return HttpNotFound();
            if (!SmestajPripadaMeni(j.SmestajId, korisnik))
                return new HttpUnauthorizedResult("❌ Možeš menjati samo jedinice svojih smeštaja.");

            ViewBag.Smestaj = _smestajiRepo.PronadjiPoId(j.SmestajId);
            return View(j);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Izmeni(SmestajnaJedinicaInfo form)
        {
            var korisnik = Curr();
            var postojeca = _jediniceRepo.PronadjiPoId(form.JedinicaId);
            if (postojeca == null) return HttpNotFound();
            if (!SmestajPripadaMeni(postojeca.SmestajId, korisnik))
                return new HttpUnauthorizedResult("❌ Možeš menjati samo jedinice svojih smeštaja.");

            bool blok = _rezervacijeRepo.PronadjiSve().Any(r =>
                r.SmestajnaJedinicaId == postojeca.JedinicaId &&
                r.Status == StatusRezervacije.Aktivna &&
                _aranzmaniRepo.PronadjiPoId(r.AranzmanId) != null &&
                _aranzmaniRepo.PronadjiPoId(r.AranzmanId).DatumPocetka > DateTime.Now
            );

            if (blok && form.BrojGostiju != postojeca.BrojGostiju)
            {
                TempData["ERR"] = "❌ Nije dozvoljena izmena broja gostiju (aktivna rezervacija u budućem aranžmanu).";
                return RedirectToAction("Izmeni", new { id = form.JedinicaId });
            }

            form.SmestajId = postojeca.SmestajId;
            form.Obrisana = postojeca.Obrisana;

            _jediniceRepo.Azuriraj(form);

            TempData["OK"] = "✏️ Jedinica je uspešno izmenjena.";
            return RedirectToAction("Index", new { smestajId = form.SmestajId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Obrisi(int id)
        {
            var korisnik = Curr();
            var j = _jediniceRepo.PronadjiPoId(id);
            if (j == null) return HttpNotFound();
            if (!SmestajPripadaMeni(j.SmestajId, korisnik))
                return new HttpUnauthorizedResult("❌ Možeš brisati samo jedinice svojih smeštaja.");

            bool blok = _rezervacijeRepo.PronadjiSve().Any(r =>
                r.SmestajnaJedinicaId == j.JedinicaId &&
                r.Status == StatusRezervacije.Aktivna &&
                _aranzmaniRepo.PronadjiPoId(r.AranzmanId) != null &&
                _aranzmaniRepo.PronadjiPoId(r.AranzmanId).DatumPocetka > DateTime.Now
            );

            if (blok)
            {
                TempData["ERR"] = "❌ Brisanje nije dozvoljeno: postoji aktivna rezervacija.";
                return RedirectToAction("Index", new { smestajId = j.SmestajId });
            }

            j.Obrisana = true;
            _jediniceRepo.Azuriraj(j);

            var smestaj = _smestajiRepo.PronadjiPoId(j.SmestajId);
            if (smestaj != null && smestaj.Jedinice != null && smestaj.Jedinice.Contains(j.JedinicaId))
            {
                smestaj.Jedinice.Remove(j.JedinicaId);
                _smestajiRepo.Azuriraj(smestaj);
            }


            TempData["OK"] = "🗑️ Jedinica je logički obrisana.";
            return RedirectToAction("Index", new { smestajId = j.SmestajId });
        }
    }
}
