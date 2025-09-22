using System.Linq;
using System.Web.Mvc;
using WebAranzmani.Models;
using WebAranzmani.Repositories;

namespace WebAranzmani.Controllers
{
    public class SmestajController : Controller
    {
        private readonly SmestajiRepozitorijum _smestajiRepo = new SmestajiRepozitorijum();
        private readonly KomentariRepozitorijum _komentariRepo = new KomentariRepozitorijum();

        public ActionResult DetaljiJedinice(int jedinicaId)
        {
            var jedinica = _smestajiRepo.PronadjiSve()
                .SelectMany(s => s.SmestajneJedinice)   // uzimamo sve jedinice iz smeštaja
                .FirstOrDefault(j => j.JedinicaId == jedinicaId);

            if (jedinica == null)
                return HttpNotFound();

            // renderuje se partial view _SmestajnaJedinica.cshtml sa modelom SmestajnaJedinicaInfo
            return PartialView("_SmestajnaJedinica", jedinica);
        }

        public ActionResult Detalji(int id)
        {
            var smestaj = _smestajiRepo.PronadjiPoId(id);
            if (smestaj == null) return HttpNotFound();

            // dopuni svaku jedinicu sa komentarima
            foreach (var jedinica in smestaj.SmestajneJedinice)
            {
                jedinica.Komentari = _komentariRepo
                    .PronadjiSve()
                    .Where(k => k.SmestajNaziv == smestaj.Naziv && k.Prihvacen)
                    .ToList();
            }

            return View(smestaj);
        }
    }
}
