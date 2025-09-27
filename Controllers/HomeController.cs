using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebAranzmani.Models;
using WebAranzmani.Repositories;

namespace WebAranzmani.Controllers
{
    public class HomeController : Controller
    {
        private readonly AranzmaniRepozitorijum _aranzmaniRepo = new AranzmaniRepozitorijum();
        private readonly KomentariRepozitorijum _komentariRepo = new KomentariRepozitorijum();
        private readonly SmestajiRepozitorijum _smestajiRepo = new SmestajiRepozitorijum();
        private readonly JediniceRepozitorijum _jediniceRepo = new JediniceRepozitorijum();

        // INDEX - pretraga + filtriranje + sortiranje
        public ActionResult Index(
            string naziv,
            string tipPrevoza,
            string tipAranzmana,
            DateTime? datumOd,
            DateTime? datumDo,
            DateTime? krajOd,
            DateTime? krajDo,
            string sort
        )
        {
            var aranzmani = _aranzmaniRepo.PronadjiSve();

            // Pretraga po nazivu
            if (!string.IsNullOrWhiteSpace(naziv))
                aranzmani = aranzmani
                    .Where(a => a.Naziv.IndexOf(naziv, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

            // Filtriranje po prevozu
            Prevoz prevoz;
            if (!string.IsNullOrWhiteSpace(tipPrevoza) &&
                Enum.TryParse(tipPrevoza, true, out prevoz))
            {
                aranzmani = aranzmani
                    .Where(a => a.VrstaPrevoza == prevoz)
                    .ToList();
            }

            // Filtriranje po paketu
            TipPaketa paket;
            if (!string.IsNullOrWhiteSpace(tipAranzmana) &&
                Enum.TryParse(tipAranzmana, true, out paket))
            {
                aranzmani = aranzmani
                    .Where(a => a.Paket == paket)
                    .ToList();
            }

            // Filtriranje po datumima
            if (datumOd.HasValue)
                aranzmani = aranzmani
                    .Where(a => a.DatumPocetka >= datumOd.Value)
                    .ToList();

            if (datumDo.HasValue)
                aranzmani = aranzmani
                    .Where(a => a.DatumPocetka <= datumDo.Value)
                    .ToList();

            if (krajOd.HasValue)
                aranzmani = aranzmani
                    .Where(a => a.DatumZavrsetka >= krajOd.Value)
                    .ToList();

            if (krajDo.HasValue)
                aranzmani = aranzmani
                    .Where(a => a.DatumZavrsetka <= krajDo.Value)
                    .ToList();

            // Sortiranje
            switch (sort)
            {
                case "nazivAsc":
                    aranzmani = aranzmani.OrderBy(a => a.Naziv).ToList();
                    break;
                case "nazivDesc":
                    aranzmani = aranzmani.OrderByDescending(a => a.Naziv).ToList();
                    break;
                case "pocetakAsc":
                    aranzmani = aranzmani.OrderBy(a => a.DatumPocetka).ToList();
                    break;
                case "pocetakDesc":
                    aranzmani = aranzmani.OrderByDescending(a => a.DatumPocetka).ToList();
                    break;
                case "krajAsc":
                    aranzmani = aranzmani.OrderBy(a => a.DatumZavrsetka).ToList();
                    break;
                case "krajDesc":
                    aranzmani = aranzmani.OrderByDescending(a => a.DatumZavrsetka).ToList();
                    break;
                default:
                    aranzmani = aranzmani.OrderBy(a => a.Sifra).ToList();
                    break;
            }

            return View(aranzmani);
        }

        // ABOUT
        public ActionResult About()
        {
            ViewBag.Message = "Ova aplikacija služi za pregled, filtriranje i rezervaciju aranžmana.";
            return View();
        }

        // CONTACT
        public ActionResult Contact()
        {
            ViewBag.Message = "Kontakt strana aplikacije.";
            return View();
        }

        // DETALJI - pojedinačan aranžman sa smeštajima i komentarima
        public ActionResult Detalji(string naziv)
        {
            if (string.IsNullOrWhiteSpace(naziv))
                return RedirectToAction("Index");

            var aranzman = _aranzmaniRepo.PronadjiSve()
                .FirstOrDefault(a => a.Naziv.Equals(naziv, StringComparison.OrdinalIgnoreCase));

            if (aranzman == null)
                return HttpNotFound();

            var smestaji = new List<SmestajInfo>();

            if (aranzman.ListaSmestaja != null)
            {
                foreach (var smId in aranzman.ListaSmestaja)
                {
                    var smestaj = _smestajiRepo.PronadjiPoId(smId);
                    if (smestaj == null) continue;

                    // ✅ Napuni jedinice preko liste ID-jeva
                    smestaj.SmestajneJedinice = smestaj.Jedinice
                        .Select(jid => _jediniceRepo.PronadjiPoId(jid))
                        .Where(j => j != null)
                        .ToList();

                    smestaji.Add(smestaj);
                }
            }

            ViewBag.Smestaji = smestaji;
            return View(aranzman);
        }
    }
}
