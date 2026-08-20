using System.Collections.Generic;

namespace InvestTrack.Web.Services
{
    public class LanguageService
    {
        private readonly Dictionary<string, Dictionary<string, string>> _translations = new()
        {
            ["nl"] = new Dictionary<string, string>
            {
                // Navigation
                ["Home"] = "Home",
                ["TraderDashboard"] = "Trader Dashboard",
                ["AdminPanel"] = "Admin Panel",
                ["Login"] = "Inloggen",
                ["Register"] = "Registreren",
                ["Logout"] = "Uitloggen",
                ["Symbols"] = "Symbolen",
                ["Language"] = "Taal",

                // Home Page
                ["Welcome"] = "Welkom bij InvestTrack",
                ["Tagline"] = "Hét alles-in-één platform voor het realtime beheren en opvolgen van uw handelsaccounts, investeringsportfolio en marktposities.",
                ["GoToAdmin"] = "Naar Admin Beheer",
                ["MyDashboard"] = "Mijn Dashboard",
                ["CreateAccount"] = "Account Aanmaken",
                ["Feature1Title"] = "Account- & Saldobeheer",
                ["Feature1Desc"] = "Maak meerdere handelsaccounts aan in verschillende valuta's en beheer eenvoudig stortingen en opnames.",
                ["Feature2Title"] = "Realtime Posities & Trades",
                ["Feature2Desc"] = "Voer orders in, volg winst en verlies (P/L) op en markeer favoriete posities voor directe inzichten.",
                ["Feature3Title"] = "Multi-Platform & Offline",
                ["Feature3Desc"] = "Volledige integratie over Web, Desktop en Mobile met automatische offline synchronisatie.",

                // Symbols Page
                ["Category"] = "Categorie",
                ["AllCategories"] = "Alle Categorieën",
                ["Code"] = "Code",
                ["Name"] = "Naam",
                ["NoSymbolsFound"] = "Geen symbolen gevonden.",

                // Trader Dashboard
                ["TotalBalance"] = "Totaal Saldo",
                ["TotalPL"] = "Totaal P/L",
                ["TotalTrades"] = "Aantal Trades",
                ["Accounts"] = "Accounts",
                ["Trades"] = "Trades & Posities",
                ["Favorites"] = "Favorieten",
                ["NewAccount"] = "Nieuw Account",
                ["NewTrade"] = "Nieuwe Trade",
                ["Deposit"] = "Storten",
                ["Withdraw"] = "Opnemen",
                ["Balance"] = "Saldo",
                ["Currency"] = "Valuta",
                ["Lots"] = "Lots",
                ["ProfitLoss"] = "Winst / Verlies",
                ["Action"] = "Actie",
                ["Cancel"] = "Annuleren",

                // Admin Panel
                ["AdminControlPanel"] = "Admin Control Panel",
                ["AdminDesc"] = "Systeembeheer van gebruikers, accounts, trades en markt-symbolen.",
                ["CreateUser"] = "Gebruiker Aanmaken",
                ["AddSymbol"] = "Symbool Toevoegen",
                ["Users"] = "Gebruikers",
                ["RegisteredUsers"] = "Geregistreerde Gebruikers",
                ["SystemAccounts"] = "Alle Systeem Accounts",
                ["SystemTrades"] = "Alle Systeem Trades",
                ["SystemSymbols"] = "Beheer Markt-Symbolen",
                ["UserEmail"] = "Gebruikersnaam / E-mail",
                ["FullName"] = "Volledige Naam",
                ["Role"] = "Rol",
                ["Actions"] = "Acties",
                ["Delete"] = "Verwijderen",
                ["AccountName"] = "Accountnaam",
                ["OwnerEmail"] = "E-mail Eigenaar",
                ["Password"] = "Wachtwoord"
            },
            ["en"] = new Dictionary<string, string>
            {
                // Navigation
                ["Home"] = "Home",
                ["TraderDashboard"] = "Trader Dashboard",
                ["AdminPanel"] = "Admin Panel",
                ["Login"] = "Log in",
                ["Register"] = "Register",
                ["Logout"] = "Log out",
                ["Symbols"] = "Symbols",
                ["Language"] = "Language",

                // Home Page
                ["Welcome"] = "Welcome to InvestTrack",
                ["Tagline"] = "The all-in-one platform for real-time tracking of your trading accounts, investment portfolio, and market positions.",
                ["GoToAdmin"] = "Go to Admin Panel",
                ["MyDashboard"] = "My Dashboard",
                ["CreateAccount"] = "Create Account",
                ["Feature1Title"] = "Account & Balance Management",
                ["Feature1Desc"] = "Create multiple trading accounts in various currencies and easily manage deposits and withdrawals.",
                ["Feature2Title"] = "Real-Time Positions & Trades",
                ["Feature2Desc"] = "Enter orders, track profit and loss (P/L), and bookmark favorite positions for instant insights.",
                ["Feature3Title"] = "Multi-Platform & Offline",
                ["Feature3Desc"] = "Full integration across Web, Desktop, and Mobile with automatic offline synchronization.",

                // Symbols Page
                ["Category"] = "Category",
                ["AllCategories"] = "All Categories",
                ["Code"] = "Code",
                ["Name"] = "Name",
                ["NoSymbolsFound"] = "No symbols found.",

                // Trader Dashboard
                ["TotalBalance"] = "Total Balance",
                ["TotalPL"] = "Total P/L",
                ["TotalTrades"] = "Total Trades",
                ["Accounts"] = "Accounts",
                ["Trades"] = "Trades & Positions",
                ["Favorites"] = "Favorites",
                ["NewAccount"] = "New Account",
                ["NewTrade"] = "New Trade",
                ["Deposit"] = "Deposit",
                ["Withdraw"] = "Withdraw",
                ["Balance"] = "Balance",
                ["Currency"] = "Currency",
                ["Lots"] = "Lots",
                ["ProfitLoss"] = "Profit / Loss",
                ["Action"] = "Action",
                ["Cancel"] = "Cancel",

                // Admin Panel
                ["AdminControlPanel"] = "Admin Control Panel",
                ["AdminDesc"] = "System management of users, accounts, trades, and market symbols.",
                ["CreateUser"] = "Create User",
                ["AddSymbol"] = "Add Symbol",
                ["Users"] = "Users",
                ["RegisteredUsers"] = "Registered Users",
                ["SystemAccounts"] = "All System Accounts",
                ["SystemTrades"] = "All System Trades",
                ["SystemSymbols"] = "Manage Market Symbols",
                ["UserEmail"] = "Username / Email",
                ["FullName"] = "Full Name",
                ["Role"] = "Role",
                ["Actions"] = "Actions",
                ["Delete"] = "Delete",
                ["AccountName"] = "Account Name",
                ["OwnerEmail"] = "Owner Email",
                ["Password"] = "Password"
            },
            ["fr"] = new Dictionary<string, string>
            {
                // Navigation
                ["Home"] = "Accueil",
                ["TraderDashboard"] = "Tableau de bord",
                ["AdminPanel"] = "Panneau Admin",
                ["Login"] = "Connexion",
                ["Register"] = "S'inscrire",
                ["Logout"] = "Déconnexion",
                ["Symbols"] = "Symboles",
                ["Language"] = "Langue",

                // Home Page
                ["Welcome"] = "Bienvenue sur InvestTrack",
                ["Tagline"] = "La plateforme tout-en-un pour le suivi en temps réel de vos comptes de trading, portefeuille d'investissement et positions du marché.",
                ["GoToAdmin"] = "Aller au panneau d'administration",
                ["MyDashboard"] = "Mon tableau de bord",
                ["CreateAccount"] = "Créer un compte",
                ["Feature1Title"] = "Gestion des comptes et des soldes",
                ["Feature1Desc"] = "Créez plusieurs comptes de trading dans différentes devises et gérez facilement les dépôts et retraits.",
                ["Feature2Title"] = "Positions et transactions en temps réel",
                ["Feature2Desc"] = "Passez des ordres, suivez vos gains et pertes (P/L) et marquez vos positions préférées pour des informations instantanées.",
                ["Feature3Title"] = "Multi-plateforme et hors ligne",
                ["Feature3Desc"] = "Intégration complète sur Web, Desktop et Mobile avec synchronisation hors ligne automatique.",

                // Symbols Page
                ["Category"] = "Catégorie",
                ["AllCategories"] = "Toutes les catégories",
                ["Code"] = "Code",
                ["Name"] = "Nom",
                ["NoSymbolsFound"] = "Aucun symbole trouvé.",

                // Trader Dashboard
                ["TotalBalance"] = "Solde Total",
                ["TotalPL"] = "P/L Total",
                ["TotalTrades"] = "Nombre de Trades",
                ["Accounts"] = "Comptes",
                ["Trades"] = "Trades & Positions",
                ["Favorites"] = "Favoris",
                ["NewAccount"] = "Nouveau Compte",
                ["NewTrade"] = "Nouveau Trade",
                ["Deposit"] = "Déposer",
                ["Withdraw"] = "Retirer",
                ["Balance"] = "Solde",
                ["Currency"] = "Devise",
                ["Lots"] = "Lots",
                ["ProfitLoss"] = "Profit / Perte",
                ["Action"] = "Action",
                ["Cancel"] = "Annuler",

                // Admin Panel
                ["AdminControlPanel"] = "Panneau de Contrôle Admin",
                ["AdminDesc"] = "Gestion système des utilisateurs, comptes, trades et symboles du marché.",
                ["CreateUser"] = "Créer un Utilisateur",
                ["AddSymbol"] = "Ajouter un Symbole",
                ["Users"] = "Utilisateurs",
                ["RegisteredUsers"] = "Utilisateurs Enregistrés",
                ["SystemAccounts"] = "Tous les Comptes Système",
                ["SystemTrades"] = "Toutes les Transactions Système",
                ["SystemSymbols"] = "Gérer les Symboles",
                ["UserEmail"] = "Nom d'utilisateur / Email",
                ["FullName"] = "Nom Complet",
                ["Role"] = "Rôle",
                ["Actions"] = "Actions",
                ["Delete"] = "Supprimer",
                ["AccountName"] = "Nom du Compte",
                ["OwnerEmail"] = "Email du Propriétaire",
                ["Password"] = "Mot de passe"
            }
        };

        public string Get(string key, string culture)
        {
            if (string.IsNullOrEmpty(culture)) culture = "nl-BE";
            var lang = culture.ToLower();
            
            string code = "nl";
            if (lang.StartsWith("fr")) code = "fr";
            else if (lang.StartsWith("en")) code = "en";

            if (_translations.TryGetValue(code, out var dict) && dict.TryGetValue(key, out var translation))
            {
                return translation;
            }

            return key;
        }
    }
}
