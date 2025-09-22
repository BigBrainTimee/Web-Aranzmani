using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAranzmani.Models
{
    public class RezervacijaPrikaz
    {
        public int RezervacijaId { get; set; }
        public string AranzmanNaziv { get; set; } = string.Empty;
        public string SmestajNaziv { get; set; } = string.Empty;
        public int BrojGostiju { get; set; }
        public double Cena { get; set; }
        public StatusRezervacije Status { get; set; }
    }
}
