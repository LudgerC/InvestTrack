# InvestTrack

## Omschrijving

InvestTrack is een desktopapplicatie ontwikkeld met **C# (.NET WPF)** voor het beheren en analyseren van trading-activiteiten.
De applicatie richt zich op zowel **traders** als **admins**, waarbij gebruikers accounts kunnen aanmaken, stortingen en opnames kunnen bijhouden, trades kunnen registreren en favoriet markeren, en symbolen kunnen beheren zoals Forex-paren, metalen, indexen en crypto-assets.

De applicatie ondersteunt gebruikersrollen:

* **Trader**: kan eigen accounts beheren, trades registreren en favorieten bekijken.
* **Admin**: kan alle gebruikers, accounts, trades en symbolen beheren.

De opslag gebeurt lokaal via een **SQLite database**.

---

## Technische Uitleg

### Technologieën

* **.NET (WPF Desktop)**
* **Entity Framework Core (SQLite)**
* **ASP.NET Core Identity** (voor gebruikersbeheer & rollen)
* **MVVM-achtige structuur** (Views + Models + gedeeltelijke scheiding van logica)
* **ASP.NET Core MVC / Web API**
* **.NET MAUI** voor de mobiele applicatie
* **HttpClient & JSON** voor communicatie tussen Mobile en Web API

### Architectuur

| Laag                | Beschrijving                                                                                                                   |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| **Model**           | Bevat database-entiteiten zoals `Account`, `Trade`, `Transaction`, `Symbol`, `FavoriteTrade`, en identiteit `ApplicationUser`. |
| **Data**            | `InvestTrackDbContext` definieert tabellen, relaties, soft delete filters en seed-data voor symbolen.                          |
| **Identity**        | ASP.NET Identity configureert gebruikersregistratie, login en user roles (`Admin`, `Trader`).                                  |
| **Views (WPF)**     | Schermen voor login, registratie, dashboards en bewerkingsvensters.                                                            |
| **Web**             | ASP.NET Core MVC/Web API met controllers, services, views en autorisatie.                                                      |
| **Mobile (MAUI)**   | Mobiele applicatie met Views, ViewModels, services, lokale SQLite-opslag en API-communicatie.                                  |
| **Business Logica** | Uitgevoerd in services, ViewModels en waar nodig in code-behind van dashboards en dialoogvensters.                             |

---

## Database

* Database type: **SQLite**
* Entity Framework Core wordt gebruikt voor de databasecommunicatie.
* Automatische migratie bij opstart (`Database.MigrateAsync()`).
* De database bevat onder andere:

  * Users / `ApplicationUser`
  * Accounts
  * Trades
  * Symbols
  * Transactions
  * FavoriteTrades
  * ASP.NET Identity-tabellen

De gebruikers zijn gekoppeld aan hun eigen accounts via `Account.UserId`, die verwijst naar `ApplicationUser`.

---

## Belangrijke Functionaliteiten

### Trader Dashboard

* Accounts bekijken, toevoegen en verwijderen.
* **Storten/Opnemen** (balance updates op bestaande accounts).
* Trades registreren (met berekening van winst/verlies op account).
* Trades verwijderen (saldo wordt gecorrigeerd).
* Markeer trades als **favoriet**.
* Favorieten-tab met filter.
* Symbolen-tab met categorie filter (Forex, Metals, Crypto, Index, etc.)

### Admin Dashboard

* Overzicht van alle gebruikers.
* Accounts beheren (inclusief soft delete).
* Trades bekijken en verwijderen.
* Symbolen toevoegen en verwijderen.

### Authenticatie & Autorisatie

* Login en registratie schermen.
* Nieuwe gebruikers krijgen automatisch de rol **Trader**.
* Admins kunnen volledig beheer uitvoeren.

---

# Web Applicatie

Naast de desktopapplicatie bevat het project een **ASP.NET Core Web-applicatie**. Deze applicatie biedt zowel webpagina's als een REST API die door de mobiele applicatie wordt gebruikt.

### Web-projectstructuur

Het Web-project is opgebouwd met onder andere:

* `Controllers` – MVC- en API-controllers.
* `Services` – services voor de verschillende functionaliteiten.
* `Views` – Razor Views voor de webinterface.
* `wwwroot` – statische bestanden zoals CSS, JavaScript en andere webresources.
* `Program.cs` – configuratie en opstart van de applicatie.
* `Startup.cs` – configuratie van services en middleware.

### Web API

De API is opgesplitst in drie groepen:

* **AuthApi** – authenticatie en login.
* **TraderApi** – functionaliteiten voor traders.
* **AdminApi** – functionaliteiten voor administrators.

De API gebruikt REST-endpoints en JSON om gegevens uit te wisselen met de mobiele applicatie.

De mobiele applicatie communiceert met deze API via `ApiService.cs`, waarbij `HttpClient`, `async` en `await` worden gebruikt.

### CRUD-functionaliteiten

Voor de belangrijkste entiteiten zijn CRUD-bewerkingen voorzien:

* **Accounts** – aanmaken, bekijken, aanpassen en verwijderen.
* **Trades** – aanmaken, bekijken, aanpassen en verwijderen.
* **Symbols** – aanmaken, bekijken, aanpassen en verwijderen.

Voor bepaalde entiteiten wordt **soft delete** gebruikt via de eigenschap `IsDeleted`, zodat gegevens niet onmiddellijk fysiek uit de database worden verwijderd.

### Identity Framework

ASP.NET Core Identity wordt gebruikt voor gebruikersbeheer, authenticatie en rollen.

De standaard `ApplicationUser` werd uitgebreid met onder andere:

* `FullName`
* `IsDeleted`
* `Accounts` als navigatieproperty

Er worden twee rollen gebruikt:

* `Admin`
* `Trader`

Nieuwe gebruikers krijgen standaard de rol **Trader**. Admin-gebruikers kunnen gebruikers bekijken, aanmaken met een bepaalde rol en verwijderen.

### Seeding

Bij het opstarten van de Web-applicatie wordt basisdata voorzien.

De seeding bevat onder andere:

* De rollen `Admin` en `Trader`.
* Twee standaardgebruikers:

  * `admin@investtrack.local`
  * `trader@investtrack.local`
* Een reeks standaard trading-symbolen.

De symbolen worden via `OnModelCreating` voorzien en de nodige Identity-data wordt tijdens de applicatie-opstart geconfigureerd.

### Autorisatie

Gebruikers krijgen alleen toegang tot de pagina's en informatie die relevant zijn voor hun rol.

Dit wordt gerealiseerd met bijvoorbeeld:

```csharp
[Authorize(Roles = "Admin")]
```

en

```csharp
[Authorize(Roles = "Trader")]
```

Daarnaast wordt het menu in `_Layout.cshtml` dynamisch aangepast op basis van de rol van de ingelogde gebruiker.

Hierdoor krijgen traders en admins verschillende opties in de webinterface.

### Gebruikers en eigen databankgegevens

Een `Account` is gekoppeld aan een `ApplicationUser` via `UserId`.

Hierdoor kan de applicatie bepalen welke accounts en tradinggegevens bij welke gebruiker horen. De relatie wordt ook gebruikt voor navigatie tussen de gebruiker en zijn accounts.

### Middleware

De Web-applicatie gebruikt onder andere de volgende middleware:

1. Localization
2. Static Files
3. Routing
4. Authentication
5. Authorization

De middleware wordt in de juiste volgorde uitgevoerd zodat authenticatie en autorisatie correct werken.

### Selectieveld en AJAX

Op de Symbolen-pagina wordt een `<select>` gebruikt om symbolen op basis van hun categorie te filteren, bijvoorbeeld Forex, Metals, Crypto en Index.

De filtering gebeurt dynamisch via AJAX, zodat de pagina niet volledig opnieuw geladen hoeft te worden.

---

# Mobile Applicatie (MAUI)

Naast de desktop- en webapplicatie bevat het project een mobiele applicatie ontwikkeld met **.NET MAUI**. De applicatie is gericht op gebruik op een standaard Android-smartphone.

### Mobile-projectstructuur

Het MAUI-project bevat onder andere:

* `Views` – XAML-pagina's voor de gebruikersinterface.
* `ViewModels` – logica volgens het MVVM-principe.
* `Services` – gedeelde services zoals API- en databasefunctionaliteiten.
* `Converters` – herbruikbare value converters.
* `Resources` – afbeeldingen, stijlen, fonts en andere resources.
* `Platforms` – platform-specifieke configuratie.
* `AppShell` – navigatie binnen de mobiele applicatie.
* `MauiProgram.cs` – configuratie van Dependency Injection en applicatieservices.

### Lokale database

De mobiele applicatie gebruikt een lokale **SQLite-database via Entity Framework Core**.

De lokale database bevat dezelfde belangrijke entiteiten als de serverdatabase, zoals:

* Users
* Accounts
* Trades
* Symbols
* Transactions
* FavoriteTrades

Hierdoor kan bepaalde functionaliteit ook offline worden gebruikt.

### API-communicatie

De communicatie met de Web API gebeurt via `ApiService.cs`.

De service gebruikt:

* `HttpClient`
* `async/await`
* JSON
* `System.Text.Json`

De API is georganiseerd rond drie groepen:

* **Auth** – authenticatie.
* **Trader** – traderfunctionaliteiten.
* **Admin** – adminfunctionaliteiten.

De mobiele applicatie maakt gebruik van ongeveer 8 trader-endpoints en 7 admin-endpoints.

Voor Android wordt rekening gehouden met het verschil tussen `localhost` en de Android emulator. De emulator gebruikt `10.0.2.2` om naar de lokale computer te communiceren.

### CRUD-functionaliteiten

De mobiele applicatie ondersteunt CRUD-gerelateerde functionaliteiten voor de belangrijkste entiteiten.

**Accounts**

* Account toevoegen.
* Accounts bekijken.
* Geld storten.
* Account verwijderen.

**Trades**

* Trade toevoegen.
* Trades bekijken.
* Trade als favoriet markeren.
* Trade verwijderen.

**Symbols**

* Symbolen bekijken.
* Symbolen toevoegen.
* Symbolen verwijderen.

### Selectievelden

Bij het toevoegen van een trade gebruikt `AddTradePage` twee `Picker`-elementen:

* Een Picker om het trading-account te kiezen.
* Een Picker om het handelssymbool te kiezen.

De gekozen waarden worden via databinding aan de ViewModel gekoppeld.

### Aanmeldingsprocedure

De mobiele applicatie gebruikt de Web API voor de authenticatie.

De gebruiker meldt zich aan via de API, waarna de nodige gegevens lokaal worden opgeslagen via `Preferences`.

Bij een volgende sessie wordt de opgeslagen informatie gebruikt om de gebruiker automatisch opnieuw te authenticeren zonder dat de gebruiker telkens opnieuw handmatig moet aanmelden.

Wanneer de API niet beschikbaar is, is er een offline fallback waarbij lokaal opgeslagen gebruikersgegevens en `PasswordHasher` worden gebruikt.

### Lokale seeding

Bij de eerste opstart kan de lokale database worden voorzien van basisgegevens via:

`DatabaseService.SeedDefaultUsers()`

Hierbij worden de nodige rollen en standaardgebruikers, zoals een Trader en Admin, lokaal aangemaakt.

Dit zorgt ervoor dat de mobiele applicatie ook over basisgegevens beschikt wanneer er geen verbinding met de Web API is.

### Asynchrone communicatie

De communicatie tussen de mobiele applicatie en de Web API gebeurt volledig asynchroon.

De services gebruiken `async/await` voor API- en databasebewerkingen.

Wanneer wijzigingen aan de gebruikersinterface nodig zijn, wordt onder andere `MainThread.InvokeOnMainThreadAsync()` gebruikt om de UI-thread correct aan te spreken.

### Android gebruikersinterface

De mobiele interface is ontworpen voor gebruik op een standaard Android-smartphone.

Er wordt onder andere gebruikgemaakt van:

* `ScrollView`
* `CollectionView`
* `Shell`-navigatie
* `Frame`-kaarten
* XAML layouts

De applicatie wordt getest met een Android emulator vanaf ongeveer **API 33**.

### Programmeercultuur

Het Mobile-project maakt gebruik van het **MVVM-principe** om de gebruikersinterface en logica van elkaar te scheiden.

Daarnaast wordt gebruikgemaakt van:

* Dependency Injection via `MauiProgram.cs`.
* Herbruikbare services.
* Herbruikbare converters.
* Consistente naamgeving.
* Gedeelde logica waar mogelijk.
* ViewModels voor de presentatie- en applicatielogica.

### XAML Binding

De mobiele interface maakt uitgebreid gebruik van XAML databinding.

Voorbeelden hiervan zijn:

* `{Binding}`
* `StringFormat`
* `DataTrigger`
* `RelativeSource`
* `IsVisible`
* `ICommand`

Hierdoor wordt de UI gekoppeld aan de ViewModels zonder dat voor iedere UI-wijziging handmatig code-behind nodig is.

---

## Bronnen

| Bron                                       | Toepassing                                                |
| ------------------------------------------ | --------------------------------------------------------- |
| **ChatGPT**                                | Hulp bij analyse & opstellen van code en structuur.       |
| **Canvas oefening**                        | Gebruikt als basis voor het aanmaken van view-structuren. |
| **Microsoft Docs - Entity Framework Core** | Database migraties, DbContext configuratie.               |
| **Microsoft Docs - WPF**                   | UI componenten & Window interacties.                      |
| **Microsoft Docs - ASP.NET Identity**      | Gebruikersbeheer & rollen.                                |
| **Microsoft Docs - ASP.NET Core**          | Web API, middleware, autorisatie en MVC.                  |
| **Microsoft Docs - .NET MAUI**             | Mobile UI, XAML, MVVM en applicatiestructuur.             |

---

## Project Starten

1. Zorg dat .NET geïnstalleerd is (versie 6+).
2. Clone de repository.
3. Start het gewenste project:

   * `InvestTrack.Desktop` voor de desktopapplicatie.
   * `InvestTrack.Web` voor de webapplicatie.
   * `InvestTrack.Mobile` voor de mobiele applicatie.
4. De applicaties maken/gebruiken automatisch de benodigde SQLite-database.
5. Log in met:

   * **Admin:** `admin@investtrack.local` – Wachtwoord: `Admin#12345`
   * **Trader:** `trader@investtrack.local` – Wachtwoord: `Trader#12345`

---

## Mogelijke Uitbreidingen (optioneel)

* Rapportage van totale winst/verlies over tijd.
* Grafieken voor accountgroei.
* API koppeling met real-time marktdata.
* Exporteer gegevens naar CSV / Excel.

---

## Bronnen

* **Canvas (oefening)** – Gebruikt als referentie voor UI-indeling en component-structuur. [¹]
* **ChatGPT** – Ingezet voor assistentie bij projectuitleg en README opmaak. [²]

[¹]: https://canvas.ehb.be/courses/45808
[²]: https://chatgpt.com/g/g-p-690dee8168e081918d5efc455e8a87cf-c/project
