using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using WebAranzmani.Service;

namespace WebAranzmani.Models
{
    public enum TipPaketa { nocenje_sa_doruckom, polupansion, pun_pansion, all_inclusive, najam_apartmana }
    public enum Prevoz { autobus, avion, autobus_avion, individualan, ostalo }
    public enum LokacijaPutovanja { grad, drzava, regija }

    public class AranzmanInfo
    {
        public int Sifra { get; set; }
        public string Naziv { get; set; } = "";
        public TipPaketa Paket { get; set; }
        public Prevoz VrstaPrevoza { get; set; }

        public LokacijaPutovanja VrstaLokacije { get; set; }

        [JsonConverter(typeof(DatumKonverter))]
        public DateTime DatumPocetka { get; set; }

        [JsonConverter(typeof(DatumKonverter))]
        public DateTime DatumZavrsetka { get; set; }

        public int BrojPutnika { get; set; }
        public string Opis { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Poster { get; set; } = string.Empty;

        [JsonProperty("ListaSmestaja")]
        public List<int> ListaSmestaja { get; set; } = new List<int>();

        public List<SmestajInfo> Smestaji { get; set; } = new List<SmestajInfo>();

        // Logičko brisanje
        public bool Obrisano { get; set; }

        // NOVO: Menadžer koji je kreirao aranžman
        public string Menadzer { get; set; } = string.Empty;

        public AranzmanInfo() { }

        public AranzmanInfo(int sifra, string naziv, TipPaketa paket, Prevoz prevoz,
                           LokacijaPutovanja lokacija, DateTime pocetak, DateTime kraj,
                           int brojPutnika, string opis, string plan, string poster,
                           List<int> smestaji, string menadzer, bool obrisano = false)
        {
            Sifra = sifra;
            Naziv = naziv;
            Paket = paket;
            VrstaPrevoza = prevoz;
            VrstaLokacije = lokacija;
            DatumPocetka = pocetak;
            DatumZavrsetka = kraj;
            BrojPutnika = brojPutnika;
            Opis = opis;
            Plan = plan;
            Poster = poster;
            ListaSmestaja = smestaji ?? new List<int>();
            Menadzer = menadzer;
            Obrisano = obrisano;
        }

        public override string ToString()
        {
            return $"{Naziv} ({DatumPocetka:dd.MM.yyyy} - {DatumZavrsetka:dd.MM.yyyy})";
        }
    }
}
