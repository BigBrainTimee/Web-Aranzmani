using System;
using System.Linq;
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

        public ActionResult Index(int smestajId)
        {
            var smestaj = _smestajiRepo.PronadjiPoId(smestajId);
            if (smestaj == null) return HttpNotFound();

            var jedinice = _jediniceRepo.PronadjiSve()
                                        .Where(j => !j.Obrisana && j.SmestajId == smestajId)
                                        .OrderBy(j => j.JedinicaId)
                                        .ToList();

            ViewBag.Smestaj = smestaj;
            return View(jedinice);
        }

        public ActionResult Kreiraj(int smestajId)
        {
            var smestaj = _smestajiRepo.PronadjiPoId(smestajId);
            if (smestaj == null) return HttpNotFound();

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
            var j = _jediniceRepo.PronadjiPoId(id);
            if (j == null) return HttpNotFound();

            ViewBag.Smestaj = _smestajiRepo.PronadjiPoId(j.SmestajId);
            return View(j);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Izmeni(SmestajnaJedinicaInfo form)
        {
            var postojeca = _jediniceRepo.PronadjiPoId(form.JedinicaId);
            if (postojeca == null) return HttpNotFound();

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
            var j = _jediniceRepo.PronadjiPoId(id);
            if (j == null) return HttpNotFound();

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
