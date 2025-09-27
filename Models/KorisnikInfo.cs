using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebAranzmani.Service;

namespace WebAranzmani.Models
{
    public enum Uloga { Admin = 0, Menadzer = 1, Turista = 2 }


    public class KorisnikInfo
    {
        [Required]
        public string KorisnickoIme { get; set; } = string.Empty;

        public bool Blokiran { get; set; }


        [Required, MinLength(5)]
        public string Lozinka { get; set; } = string.Empty;

        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string Pol { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public Uloga Uloga { get; set; }


        [JsonConverter(typeof(DatumKonverter))]
        public DateTime DatumRodjenja { get; set; }

        public Uloga Rola { get; set; }

        public List<RezervacijaInfo> Rezervacije { get; set; } = new List<RezervacijaInfo>();
        public List<int> KreiraniAranzmani { get; set; } = new List<int>();

        public KorisnikInfo() { }

        public KorisnikInfo(string korisnickoIme, string lozinka, string ime, string prezime,
                            string pol, string email, DateTime rodjenje,
                            Uloga rola, List<RezervacijaInfo> rezervacije, List<int> aranzmani)
        {
            KorisnickoIme = korisnickoIme;
            Lozinka = lozinka;
            Ime = ime;
            Prezime = prezime;
            Pol = pol;
            Email = email;
            DatumRodjenja = rodjenje;
            Rola = rola;
            Rezervacije = rezervacije ?? new List<RezervacijaInfo>();
            KreiraniAranzmani = aranzmani ?? new List<int>();
        }

        public override string ToString()
        {
            return $"{Ime} {Prezime} ({Rola})";
        }
    }
}
