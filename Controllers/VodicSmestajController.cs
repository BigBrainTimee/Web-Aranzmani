using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebAranzmani.Models;
using WebAranzmani.Repositories;

namespace WebAranzmani.Controllers
{
    public class VodicSmestajController : Controller
    {
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

        private HashSet<int> MyAranzmanIds(KorisnikInfo u)
        {
            var set = new HashSet<int>();
            if (u.KreiraniAranzmani != null)
            {
                foreach (var id in u.KreiraniAranzmani)
                    set.Add(id);
            }
            return set;
        }

        private bool SmestajPripadaMeni(int smestajId, HashSet<int> myArIds)
        {
            foreach (var a in _aranzmaniRepo.PronadjiSve())
            {
                if (!myArIds.Contains(a.Sifra)) continue;
                if (a.ListaSmestaja != null && a.ListaSmestaja.Contains(smestajId))
                    return true;
            }
            return false;
        }

        public ActionResult Index(string q = "", bool? samoMoji = true, bool? prikaziObrisane = false, string sort = "")
        {
            var user = Curr();
            var myArIds = MyAranzmanIds(user);

            var list = _smestajiRepo.PronadjiSve();
            if (!prikaziObrisane.GetValueOrDefault(false))
                list = list.Where(s => !s.Obrisano).ToList();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qq = q.Trim();
                list = list.Where(s => (s.Naziv ?? "").IndexOf(qq, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            if (samoMoji.GetValueOrDefault(true))
                list = list.Where(s => SmestajPripadaMeni(s.SmestajId, myArIds)).ToList();

            switch (sort)
            {
                case "nazivAsc": list = list.OrderBy(s => s.Naziv).ToList(); break;
                case "nazivDesc": list = list.OrderByDescending(s => s.Naziv).ToList(); break;
                default: list = list.OrderBy(s => s.SmestajId).ToList(); break;
            }

            ViewBag.Q = q;
            ViewBag.SamoMoji = samoMoji;
            ViewBag.PrikaziObrisane = prikaziObrisane;
            ViewBag.Sort = sort;

            return View(list);
        }

        public ActionResult Kreiraj()
        {
            var user = Curr();
            var myIds = MyAranzmanIds(user);
            var mojiAr = _aranzmaniRepo.PronadjiSve().Where(a => myIds.Contains(a.Sifra) && !a.Obrisano).ToList();
            ViewBag.MojiAranzmani = new SelectList(mojiAr, "Sifra", "Naziv");
            return View(new SmestajInfo());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Kreiraj(SmestajInfo form, int aranzmanId)
        {
            var user = Curr();
            var myIds = MyAranzmanIds(user);
            if (!myIds.Contains(aranzmanId))
                return new HttpUnauthorizedResult("❌ Smeštaj možeš dodati samo u svoje aranžmane.");

            var all = _smestajiRepo.PronadjiSve();
            form.SmestajId = all.Any() ? all.Max(s => s.SmestajId) + 1 : 1;
            form.Obrisano = false;

            all.Add(form);
            _smestajiRepo.SacuvajSve(all);

            var a = _aranzmaniRepo.PronadjiPoId(aranzmanId);
            if (a.ListaSmestaja == null) a.ListaSmestaja = new List<int>();
            if (!a.ListaSmestaja.Contains(form.SmestajId)) a.ListaSmestaja.Add(form.SmestajId);
            _aranzmaniRepo.Azuriraj(a);

            TempData["OK"] = "✅ Smeštaj je uspešno dodat.";
            return RedirectToAction("Index");
        }

        public ActionResult Izmeni(int id)
        {
            var user = Curr();
            var myIds = MyAranzmanIds(user);
            var s = _smestajiRepo.PronadjiPoId(id);
            if (s == null) return HttpNotFound();
            if (!SmestajPripadaMeni(id, myIds))
                return new HttpUnauthorizedResult("❌ Možeš uređivati samo smeštaj u okviru svojih aranžmana.");

            return View(s);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Izmeni(SmestajInfo form)
        {
            var user = Curr();
            var myIds = MyAranzmanIds(user);
            if (!SmestajPripadaMeni(form.SmestajId, myIds))
                return new HttpUnauthorizedResult("❌ Možeš uređivati samo smeštaj u okviru svojih aranžmana.");

            var stari = _smestajiRepo.PronadjiPoId(form.SmestajId);
            if (stari == null) return HttpNotFound();

            form.Obrisano = stari.Obrisano;
            _smestajiRepo.Azuriraj(form);

            TempData["OK"] = "✏️ Smeštaj je ažuriran.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Obrisi(int id)
        {
            var user = Curr();
            var myIds = MyAranzmanIds(user);

            var s = _smestajiRepo.PronadjiPoId(id);
            if (s == null) return HttpNotFound();

            // kreiramo repo lokalno (kao kod tvog druga)
            var jedinicaRepo = new JediniceRepozitorijum();
            var idsJedinica = jedinicaRepo.PronadjiSve()
                                          .Where(j => j.SmestajId == id && !j.Obrisana)
                                          .Select(j => j.JedinicaId)
                                          .ToList();

            var imaRez = _rezervacijeRepo.PronadjiSve()
                            .Any(r => idsJedinica.Contains(r.SmestajnaJedinicaId));

            if (imaRez)
            {
                TempData["ERR"] = "❌ Brisanje nije dozvoljeno: postoji rezervacija za ovaj smeštaj.";
                return RedirectToAction("Index");
            }

            var today = DateTime.Today;
            var imaBuduciAranzman = _aranzmaniRepo.PronadjiSve().Any(a =>
                !a.Obrisano &&
                a.ListaSmestaja != null &&
                a.ListaSmestaja.Contains(id) &&
                a.DatumPocetka >= today
            );

            if (imaBuduciAranzman)
            {
                TempData["ERR"] = "❌ Brisanje nije dozvoljeno: postoji budući aranžman sa ovim smeštajem.";
                return RedirectToAction("Index");
            }

            s.Obrisano = true;
            _smestajiRepo.Azuriraj(s);

            TempData["OK"] = "🗑️ Smeštaj je logički obrisan.";
            return RedirectToAction("Index");
        }
    }
}
