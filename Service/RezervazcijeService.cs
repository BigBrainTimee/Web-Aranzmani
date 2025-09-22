using System;
using System.Collections.Generic;
using System.Linq;
using WebAranzmani.Models;
using WebAranzmani.Repositories;

namespace WebAranzmani.Service
{
    public class RezervazcijeService
    {
        private readonly RezervazcijeRepozitorijum _rezRepo = new RezervazcijeRepozitorijum();
        private readonly AranzmaniRepozitorijum _arrRepo = new AranzmaniRepozitorijum();
        private readonly SmestajiRepozitorijum _smRepo = new SmestajiRepozitorijum();

        public List<RezervacijaInfo> VratiSve() => _rezRepo.PronadjiSve();

        public RezervacijaInfo PronadjiPoId(int id) => _rezRepo.PronadjiPoId(id);

        public List<RezervacijaInfo> VratiPoTuristi(string korisnickoIme)
        {
            return VratiSve().Where(r => r.Turista == korisnickoIme).ToList();
        }

        public bool DodajRezervaciju(string turista, AranzmanInfo aranzman, int jedinicaId)
        {
            var smestaj = _smRepo.PronadjiPoId(aranzman.ListaSmestaja.FirstOrDefault());
            if (smestaj == null) return false;

            var jedinica = smestaj.SmestajneJedinice.FirstOrDefault(j => j.JedinicaId == jedinicaId);
            if (jedinica == null || jedinica.Status != StatusJedinice.Slobodna)
                return false;

            int noviId = _rezRepo.PronadjiSve().Any() ? _rezRepo.PronadjiSve().Max(r => r.RezervacijaId) + 1 : 1;

            var nova = new RezervacijaInfo
            {
                RezervacijaId = noviId,
                Turista = turista,
                Status = StatusRezervacije.Aktivna,
                AranzmanId = aranzman.Sifra,
                SmestajnaJedinicaId = jedinica.JedinicaId
            };

            jedinica.Status = StatusJedinice.Zauzeta;

            _rezRepo.Dodaj(nova);
            _smRepo.Azuriraj(smestaj);
            return true;
        }

        public bool Otkazi(int id)
        {
            var rezervacija = _rezRepo.PronadjiPoId(id);
            if (rezervacija == null) return false;

            var aranzman = _arrRepo.PronadjiPoId(rezervacija.AranzmanId);
            if (aranzman == null) return false;

            if (DateTime.Now > aranzman.DatumZavrsetka) return false;

            rezervacija.Status = StatusRezervacije.Otkazana;

            foreach (var smId in aranzman.ListaSmestaja)
            {
                var sm = _smRepo.PronadjiPoId(smId);
                if (sm != null)
                {
                    var jedinica = sm.SmestajneJedinice
                        .FirstOrDefault(j => j.JedinicaId == rezervacija.SmestajnaJedinicaId);
                    if (jedinica != null) jedinica.Status = StatusJedinice.Slobodna;

                    _smRepo.Azuriraj(sm);
                }
            }

            _rezRepo.Azuriraj(rezervacija);
            return true;
        }

        public List<RezervacijaInfo> Pretrazi(string korisnickoIme, string id = "", string naziv = "", string status = "")
        {
            var query = VratiPoTuristi(korisnickoIme).AsEnumerable();

            if (!string.IsNullOrWhiteSpace(id) && int.TryParse(id, out var rezId))
                query = query.Where(r => r.RezervacijaId == rezId);

            if (!string.IsNullOrWhiteSpace(naziv))
            {
                query = query.Where(r =>
                {
                    var arr = _arrRepo.PronadjiPoId(r.AranzmanId);
                    return arr != null && arr.Naziv.IndexOf(naziv, StringComparison.OrdinalIgnoreCase) >= 0;
                });
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase));

            return query.ToList();
        }

        public List<RezervacijaInfo> Sortiraj(List<RezervacijaInfo> lista, string kriterijum)
        {
            if (kriterijum == "nazivAsc")
            {
                return lista.OrderBy(r => _arrRepo.PronadjiPoId(r.AranzmanId)?.Naziv).ToList();
            }
            else if (kriterijum == "nazivDesc")
            {
                return lista.OrderByDescending(r => _arrRepo.PronadjiPoId(r.AranzmanId)?.Naziv).ToList();
            }
            else
            {
                return lista;
            }
        }

    }
}
