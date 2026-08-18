# Projectvoorstel: InvestTrack Mobile

## Naam van de initiatiefnemer
[Je naam invullen]

## Werknaam van het project
InvestTrack Mobile

## Korte omschrijving van het project
InvestTrack Mobile is de mobiele uitbreiding van de bestaande InvestTrack WPF-applicatie. De app biedt traders een overzichtelijk dashboard waarop zij hun accounts, huidige saldo's, en openstaande trades kunnen raadplegen vanaf hun smartphone. De app haalt de benodigde gegevens op via een ASP.NET Core API en slaat relevante data (zoals accounts, trades en symbolen) lokaal op in een SQLite database. Hierdoor is de belangrijkste functionaliteit ook off-line beschikbaar.

## Korte motivatie voor het uitvoeren van het project
In de eerdere modules (.NET Frameworks en .NET Advanced) is een robuuste desktop applicatie (WPF) en bijbehorende backend (ASP.NET Core API) ontwikkeld. Tegenwoordig verwachten gebruikers echter dat ze hun financiële data en trades altijd en overal kunnen raadplegen. Een mobiele applicatie is hierom de logische volgende stap. Door dit project te kiezen, kan ik verder bouwen op de bestaande architectuur (hergebruik van de Class Library modellen en de API) en me volledig focussen op de unieke aspecten van .NET MAUI, waaronder data-synchronisatie voor off-line gebruik en mobiele UI-patronen.

## Uitgebreidere omschrijving en overzicht van pages
De mobiele applicatie is in de eerste iteratie vooral gericht op de 'Trader' rol (read-only of beperkte interactie) om zo de scope binnen de voorziene 100 uren te houden. 

### User Stories
- **US1:** Als trader wil ik kunnen inloggen in de app, zodat mijn gegevens veilig afgeschermd zijn.
- **US2:** Als trader wil ik een dashboard zien met mijn totale balans en totale winst/verlies, zodat ik in één oogopslag weet hoe ik ervoor sta.
- **US3:** Als trader wil ik een lijst van mijn accounts en bijbehorende trades bekijken.
- **US4:** Als trader wil ik de app kunnen openen en mijn laatste data kunnen inzien, zelfs als ik tijdelijk geen internetverbinding heb (off-line modus via SQLite).
- **US5:** Als trader wil ik een lijst met beschikbare trading symbolen kunnen inzien.

### Overzicht van te realiseren Pages
1. **LoginPage:**
   - Authenticatiescherm waar de gebruiker e-mail en wachtwoord ingeeft.
   - Communiceert met de API voor validatie.

2. **TraderDashboardPage (Main Tab):**
   - Toont samenvattende statistieken (Total Balance, Profit/Loss, aantal trades).
   - Toont een lijst van de accounts.
   - Toont recente trades en hun prestaties.
   - Deze pagina haalt data op van de API en slaat dit lokaal op in de SQLite database (`InvestTrackDbContext`). Bij gebrek aan internetverbinding wordt de lokale data getoond.

3. **SymbolsPage (Tab):**
   - Toont een overzicht van alle beschikbare assets en symbolen (Forex, Crypto, etc.).
   - Ook deze data wordt gesynchroniseerd naar de lokale database voor offline inzage.

## Technische architectuur
- **Frontend:** .NET MAUI
- **Backend API:** ASP.NET Core API (reeds gerealiseerd)
- **Shared Models:** Class Library (`InvestTrack.Model`)
- **Lokale Opslag:** Entity Framework Core met SQLite (off-line cache)
- **Data flow:** Bij het openen van het dashboard probeert de app data te fetchen via `ApiService`. Indien succesvol, overschrijft/update dit de lokale data in SQLite via `DatabaseService`. Indien de API faalt (bv. geen netwerk), wordt de laatst bewaarde data uit de lokale SQLite database geladen, en krijgt de gebruiker hiervan een melding.
