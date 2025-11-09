# InvestTrack

## Omschrijving

InvestTrack is een desktopapplicatie ontwikkeld met **C# (.NET WPF)** voor het beheren en analyseren van trading-activiteiten.  
De applicatie richt zich op zowel **traders** als **admins**, waarbij gebruikers accounts kunnen aanmaken, stortingen en opnames kunnen bijhouden, trades kunnen registreren en favoriet markeren, en symbolen kunnen beheren zoals Forex-paren, metalen, indexen en crypto-assets.

De applicatie ondersteunt gebruikersrollen:
- **Trader**: kan eigen accounts beheren, trades registreren en favorieten bekijken.
- **Admin**: kan alle gebruikers, accounts, trades en symbolen beheren.

De opslag gebeurt lokaal via een **SQLite database**.

---

## Technische Uitleg

### Technologieën
- **.NET (WPF Desktop)**
- **Entity Framework Core (SQLite)**
- **ASP.NET Core Identity** (voor gebruikersbeheer & rollen)
- **MVVM-achtige structuur** (Views + Models + gedeeltelijke scheiding van logica)

### Architectuur

| Laag | Beschrijving |
|------|--------------|
| **Model** | Bevat database-entiteiten zoals `Account`, `Trade`, `Transaction`, `Symbol`, `FavoriteTrade`, en identiteit `ApplicationUser`. |
| **Data** | `InvestTrackDbContext` definieert tabellen, relaties, soft delete filters en seed-data voor symbolen. |
| **Identity** | ASP.NET Identity configureert gebruikersregistratie, login en user roles (`Admin`, `Trader`). |
| **Views (WPF)** | Schermen voor login, registratie, dashboards en bewerkingsvensters. |
| **Business Logica** | Uitgevoerd in code-behind van dashboards en dialoogvensters. |

### Database

- Database type: **SQLite**
- Automatische migratie bij opstart (`Database.MigrateAsync()`).
- Voorzien van **seed-data**:
  - 2 standaard gebruikers: `admin@investtrack.local` & `trader@investtrack.local`
  - Een set populaire trading-symbolen (Forex, Metals, Crypto, Indexes).

### Belangrijke Functionaliteiten

#### Trader Dashboard
- Accounts bekijken, toevoegen en verwijderen.
- **Storten/Opnemen** (balance updates op bestaande accounts).
- Trades registreren (met berekening van winst/verlies op account).
- Trades verwijderen (saldo wordt gecorrigeerd).
- Markeer trades als **favoriet**.
- Favorieten-tab met filter.
- Symbolen-tab met categorie filter (Forex, Metals, Crypto, Index, etc.)

#### Admin Dashboard
- Overzicht van alle gebruikers.
- Accounts beheren (inclusief soft delete).
- Trades bekijken en verwijderen.
- Symbolen toevoegen en verwijderen.

#### Authenticatie & Autorisatie
- Login en registratie schermen.
- Nieuwe gebruikers krijgen automatisch de rol **Trader**.
- Admins kunnen volledig beheer uitvoeren.

---

## Bronnen

| Bron | Toepassing |
|------|------------|
| **ChatGPT** | Hulp bij analyse & opstellen van code en structuur. |
| **Canvas oefening** | Gebruikt als basis voor het aanmaken van view-structuren. |
| **Microsoft Docs - Entity Framework Core** | Database migraties, DbContext configuratie. |
| **Microsoft Docs - WPF** | UI componenten & Window interacties. |
| **Microsoft Docs - ASP.NET Identity** | Gebruikersbeheer & rollen. |

---

## Project Starten

1. Zorg dat .NET geïnstalleerd is (versie 6+).
2. Clone de repository.
3. Start het project vanaf `InvestTrack.Desktop`.
4. Applicatie maakt automatisch de database (`investtrack.db`) aan.
5. Log in met:
   - **Admin:** `admin@investtrack.local` – Wachtwoord: `Admin#12345`
   - **Trader:** `trader@investtrack.local` – Wachtwoord: `Trader#12345`

---

## Mogelijke Uitbreidingen (optioneel)
- Rapportage van totale winst/verlies over tijd.
- Grafieken voor accountgroei.
- API koppeling met real-time marktdata.
- Exporteer gegevens naar CSV / Excel.

---

## Bronnen

- **Canvas (oefening)** – Gebruikt als referentie voor UI-indeling en component-structuur. [¹]
- **ChatGPT** – Ingezet voor assistentie bij projectuitleg en README opmaak. [²]

[¹]: https://canvas.ehb.be/courses/45808
[²]: https://chatgpt.com/g/g-p-690dee8168e081918d5efc455e8a87cf-c/project
