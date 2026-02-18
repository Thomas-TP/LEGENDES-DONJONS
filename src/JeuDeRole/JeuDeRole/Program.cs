using JeuDeRole.Factories;
using JeuDeRole.Logging;
using JeuDeRole.Repositories.InMemory;
using JeuDeRole.Repositories.Interfaces;
using JeuDeRole.Services.Bestiaire;
using JeuDeRole.Services.Boutique;
using JeuDeRole.Services.Combat;
using JeuDeRole.Services.Dialogue;
using JeuDeRole.Services.Donjon;
using JeuDeRole.Services.Evenements;
using JeuDeRole.Services.Historique;
using JeuDeRole.Services.Interfaces;
using JeuDeRole.Services.Inventaire;
using JeuDeRole.Services.Quetes;
using JeuDeRole.Services.Sauvegarde;
using JeuDeRole.Services.Succes;
using JeuDeRole.Strategies.Degats;
using JeuDeRole.UI;

/// <summary>
/// Point d'entrée de l'application "Jeu de Rôle".
/// Ce fichier agit comme le "Composition Root" où toutes les dépendances
/// sont instanciées et injectées (Dependency Injection) avant de lancer l'interface utilisateur.
/// </summary>

// ═══════════════════════════════════════════════
//  Composition Root — Injection de Dépendances
// ═══════════════════════════════════════════════

// Logger (Observer pattern) : Gère l'affichage des logs de combat
ICombatLogger logger = new ConsoleLogger();

// Strategies (Strategy pattern) : Algorithmes de calcul de dégâts interchangeables
ICalculDegats calculPhysique = new CalculDegatsPhysiques();
ICalculDegats calculMagique = new CalculDegatsMagiques();

// Factories (Factory pattern) : Création centralisée des entités
IPersonnageFactory personnageFactory = new PersonnageFactory();
IMonstreFactory monstreFactory = new MonstreFactory();

// Repositories (Repository pattern) : Accès aux données (équipements, objets)
IEquipementRepository equipementRepo = new MemoireEquipementRepository();
IObjetRepository objetRepo = new MemoireObjetRepository();

// Services : Logique métier de chaque module
IInventaireService inventaireService = new InventaireService(logger);
IHistoriqueService historiqueService = new HistoriqueService();
ISauvegardeService sauvegardeService = new SauvegardeService();
IBestiaireService bestiaireService = new BestiaireService();
ISuccesService succesService = new SuccesService();
IEvenementService evenementService = new EvenementService();
IDonjonService donjonService = new DonjonService();
IDialogueService dialogueService = new DialogueService();
IBoutiqueService boutiqueService = new BoutiqueService();
IQueteService queteService = new QueteService();
var combatService = new CombatService(logger, calculPhysique, calculMagique, inventaireService);

// UI : Interface utilisateur console (Spectre.Console)
// Injection de tous les services nécessaires au fonctionnement du jeu
var ui = new CombatUI(combatService, personnageFactory, monstreFactory, equipementRepo, objetRepo,
                      historiqueService, sauvegardeService, bestiaireService, succesService,
                      evenementService, donjonService, dialogueService,
                      boutiqueService, queteService);

// Lancement de la boucle principale du jeu
ui.AfficherMenuPrincipal();
