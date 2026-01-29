# Web aplikacija – Turistička agencija

Ovaj projekat predstavlja **web aplikaciju za rad turističke agencije**, razvijenu korišćenjem **ASP.NET MVC** tehnologije.  
Aplikacija omogućava korisnicima pregled i rezervaciju putnih aranžmana, dok administratori imaju potpunu kontrolu nad sadržajem i rezervacijama.

---

## 🚀 Funkcionalnosti

### Gost / Registrovani korisnik
- Pregled dostupnih putnih aranžmana
- Detaljan prikaz destinacija i ponuda
- Registracija i prijava korisnika
- Rezervacija putnih aranžmana
- Otkazivanje postojećih rezervacija
- Pregled sopstvenih rezervacija

### Administrator
- Prijava sa administratorskim privilegijama
- Dodavanje, izmena i brisanje aranžmana
- Upravljanje korisnicima
- Pregled i upravljanje rezervacijama
- Upravljanje destinacijama, vodičima i dodatnim uslugama

---

## 🛠 Korišćene tehnologije

- **ASP.NET MVC**
- **C#**
- **Entity Framework**
- **Razor Views**
- **HTML, CSS, JavaScript**
- **Bootstrap**
- **SQL Server**

---

## 🏗 Struktura projekta

- `Controllers` – Obrada zahteva i poslovna logika
- `Models` – Modeli i entiteti baze podataka
- `Views` – Razor prikazi korisničkog interfejsa
- `Services` – Servisni sloj i poslovna logika
- `App_Start` – Konfiguracija aplikacije
- `Content` / `Scripts` – Statički resursi (CSS, JS)

---

## 🔐 Autentifikacija i uloge

Aplikacija koristi sistem uloga:
- **Gost/Korisnik** – Pregled i rezervacija aranžmana
- **Administrator** – Potpuna kontrola sistema

---

## ⚙️ Pokretanje projekta

1. Klonirati repozitorijum:
   ```bash
   git clone https://github.com/BigBrainTimee/Web-Aranzmani.git
