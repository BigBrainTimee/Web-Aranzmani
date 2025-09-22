using System.Collections.Generic;
using System.Linq;
using WebAranzmani.Models;
using WebAranzmani.Repositories;

namespace WebAranzmani.Service
{
    public class KorisniciServise
    {
        private readonly KorisniciRepozitorijum _korisniciRepo = new KorisniciRepozitorijum();

        public KorisnikInfo Login(string korisnickoIme, string lozinka)
        {
            var k = _korisniciRepo.PronadjiPoKorisnickomImenu(korisnickoIme);
            return (k != null && k.Lozinka == lozinka) ? k : null;
        }

        public bool Registruj(KorisnikInfo novi)
        {
            if (_korisniciRepo.PronadjiPoKorisnickomImenu(novi.KorisnickoIme) != null)
                return false;

            novi.Uloga = Uloga.Turista;
            var svi = _korisniciRepo.PronadjiSve();
            svi.Add(novi);
            _korisniciRepo.SacuvajSve(svi);
            return true;
        }

        public void AzurirajProfil(KorisnikInfo izmenjen)
        {
            _korisniciRepo.Azuriraj(izmenjen);
        }

        public List<KorisnikInfo> PronadjiSve()
        {
            return _korisniciRepo.PronadjiSve();
        }

        public void Azuriraj(KorisnikInfo korisnik)
        {
            _korisniciRepo.Azuriraj(korisnik);
        }

        public void Obrisi(string korisnickoIme)
        {
            var svi = _korisniciRepo.PronadjiSve();
            var target = svi.FirstOrDefault(k => k.KorisnickoIme == korisnickoIme);

            if (target != null && target.Uloga != Uloga.Admin)
            {
                svi.Remove(target);
                _korisniciRepo.SacuvajSve(svi);
            }
        }
    }
}
