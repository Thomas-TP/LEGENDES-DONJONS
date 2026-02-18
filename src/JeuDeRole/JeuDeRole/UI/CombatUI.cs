using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.Models;
using JeuDeRole.Domain.ValueObjects;
using JeuDeRole.Factories;
using JeuDeRole.Repositories.Interfaces;
using JeuDeRole.Services.Combat;
using JeuDeRole.Services.Donjon;
using JeuDeRole.Services.Interfaces;
using Spectre.Console;

namespace JeuDeRole.UI;

/// <summary>
/// Interface utilisateur principale du système de combat et de gestion du jeu.
/// Gère l'affichage, les menus, les interactions utilisateur et le flux principal de l'application.
/// Utilise la bibliothèque Spectre.Console pour un rendu riche en console.
/// </summary>
public class CombatUI
{
    private readonly CombatService _combatService;
    private readonly IPersonnageFactory _personnageFactory;
    private readonly IMonstreFactory _monstreFactory;
    private readonly IEquipementRepository _equipementRepo;
    private readonly IObjetRepository _objetRepo;
    private readonly IHistoriqueService _historiqueService;
    private readonly ISauvegardeService _sauvegardeService;
    private readonly IBestiaireService _bestiaireService;
    private readonly ISuccesService _succesService;
    private readonly IEvenementService _evenementService;
    private readonly IDonjonService _donjonService;
    private readonly IDialogueService _dialogueService;
    private readonly IBoutiqueService _boutiqueService;
    private readonly IQueteService _queteService;

    private const string CheminSauvegarde = "sauvegarde.json";
    
    // État de la session actuelle
    private List<Heros> _equipeActuelle = new();
    private int _totalVictoires;
    private int _totalDefaites;
    private int _bossVaincus;
    private int _vaguesAreneMax;
    private Difficulte _difficulte = Difficulte.Normal;
    private Inventaire _inventaireActuel = new();
    private int _donjonProfondeurMax;
    private readonly HashSet<string> _bossVaincusNoms = new();

    /// <summary>
    /// Constructeur avec injection de toutes les dépendances nécessaires aux différents sous-systèmes.
    /// </summary>
    public CombatUI(CombatService combatService, IPersonnageFactory personnageFactory,
                    IMonstreFactory monstreFactory, IEquipementRepository equipementRepo,
                    IObjetRepository objetRepo, IHistoriqueService historiqueService,
                    ISauvegardeService sauvegardeService, IBestiaireService bestiaireService,
                    ISuccesService succesService, IEvenementService evenementService,
                    IDonjonService donjonService, IDialogueService dialogueService,
                    IBoutiqueService boutiqueService, IQueteService queteService)
    {
        _combatService = combatService;
        _personnageFactory = personnageFactory;
        _monstreFactory = monstreFactory;
        _equipementRepo = equipementRepo;
        _objetRepo = objetRepo;
        _historiqueService = historiqueService;
        _sauvegardeService = sauvegardeService;
        _bestiaireService = bestiaireService;
        _succesService = succesService;
        _evenementService = evenementService;
        _donjonService = donjonService;
        _dialogueService = dialogueService;
        _boutiqueService = boutiqueService;
        _queteService = queteService;

        // Configuration du délégué pour l'action du joueur dans le service de combat
        _combatService.DemanderActionJoueur = DemanderActionJoueur;
    }

    /// <summary>
    /// Point d'entrée de l'interface utilisateur. Affiche le menu principal et gère la boucle principale du jeu.
    /// </summary>
    public void AfficherMenuPrincipal()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        // Chargement automatique de la sauvegarde au démarrage si elle existe
        ChargerAutomatique();

        while (true)
        {
            AnsiConsole.Clear();
            AfficherTitre();

            // Construction et affichage de la barre de statut (HUD)
            var statusTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Expand();
            statusTable.AddColumn(new TableColumn("[grey]Difficulté[/]").Centered());
            statusTable.AddColumn(new TableColumn("[gold1]Or[/]").Centered());
            statusTable.AddColumn(new TableColumn("[green]Victoires[/]").Centered());
            statusTable.AddColumn(new TableColumn("[red]Défaites[/]").Centered());
            statusTable.AddColumn(new TableColumn("[cyan]Équipe[/]").Centered());
            string diffEmoji2 = _difficulte switch { Difficulte.Facile => "🟢", Difficulte.Normal => "🟡", Difficulte.Difficile => "🔴", Difficulte.Cauchemar => "💀", _ => "" };
            string equipeStr = _equipeActuelle.Count > 0 ? string.Join(", ", _equipeActuelle.Select(h => $"{AsciiArt.ObtenirIconeClasse(h.Classe)}{Markup.Escape(h.Nom)}")) : "[grey]Aucune[/]";
            statusTable.AddRow(
                $"{diffEmoji2} {_difficulte}",
                $"[gold1]{_boutiqueService.Or} 💰[/]",
                $"[green]{_totalVictoires}[/]",
                $"[red]{_totalDefaites}[/]",
                equipeStr);
            AnsiConsole.Write(statusTable);
            AnsiConsole.WriteLine();

            // Menu de sélection principal
            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Que voulez-vous faire ?[/]")
                    .PageSize(20)
                    .AddChoices(new[]
                    {
                        "1. Créer une équipe",
                        "2. Combattre",
                        "3. Combat rapide (équipe auto)",
                        "4. Combat Boss",
                        "5. Mode Arène infinie",
                        "6. Donjon procédural",
                        "7. Boutique / Marchand",
                        "8. Gestion d'équipe",
                        "9. Quêtes",
                        "A. Bestiaire",
                        "B. Succès / Trophées",
                        "C. Historique des combats",
                        "D. Sauvegarder",
                        "E. Charger une sauvegarde",
                        "F. Difficulté",
                        "Q. Quitter"
                    }));

            // Routage vers les différentes fonctionnalités
            switch (choix[0..2].Trim())
            {
                case "1.": CreerEquipeMenu(); break;
                case "2.": LancerCombatRapideEquipe(); break;
                case "3.": LancerCombatRapide(); break;
                case "4.": LancerCombatBoss(); break;
                case "5.": LancerArene(); break;
                case "6.": LancerDonjon(); break;
                case "7.": AfficherBoutique(); break;
                case "8.": GererEquipe(); break;
                case "9.": AfficherQuetes(); break;
                case "A.": AfficherBestiaire(); break;
                case "B.": AfficherSucces(); break;
                case "C.": AfficherHistorique(); break;
                case "D.": Sauvegarder(); break;
                case "E.": Charger(); break;
                case "F.": ChoisirDifficulte(); break;
                case "Q.":
                    // Sauvegarde automatique avant de quitter
                    Sauvegarder(silencieux: true);
                    AnsiConsole.MarkupLine("[grey]Sauvegarde effectuée. Au revoir ![/]");
                    return;
            }
        }
    }

    /// <summary>
    /// Affiche le titre du jeu en ASCII art.
    /// </summary>
    private void AfficherTitre()
    {
        AnsiConsole.Write(
            new FigletText("RPG Combat")
                .Color(Color.Gold1)
                .Centered());

        AnsiConsole.Write(
            new Panel("[bold yellow]⚔  Système de Combat  ⚔[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Gold1)
                .Padding(2, 0)
                .Expand());
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Gère le flux de création d'une nouvelle équipe de héros.
    /// </summary>
    private void CreerEquipeMenu()
    {
        AnsiConsole.Clear();
        var heros = CreerEquipe();
        EquiperHeros(heros); // Propose d'équiper les héros créés
        _equipeActuelle = heros;

        // Résumé visuel de l'équipe créée
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Equipe Prete !").Color(Color.Green).Centered());
        AnsiConsole.Write(new Rule("[green bold]✓ Votre équipe est prête au combat ![/]").RuleStyle("green"));
        AnsiConsole.WriteLine();


        foreach (var h in heros)
        {
            var s = h.StatsActuelles;
            string couleur = AsciiArt.ObtenirCouleurClasse(h.Classe);
            string icone = AsciiArt.ObtenirIconeClasse(h.Classe);
            string armeStr = h.Arme != null ? $"⚔ {Markup.Escape(h.Arme.Nom)}" : "[grey]Aucune arme[/]";
            string armureStr = h.Armure != null ? $"🛡 {Markup.Escape(h.Armure.Nom)}" : "[grey]Aucune armure[/]";
            string accStr = h.Accessoire != null ? $"💍 {Markup.Escape(h.Accessoire.Nom)}" : "[grey]Aucun[/]";

            var panel = new Panel(
                $"[{couleur} bold]{icone} {Markup.Escape(h.Nom)}  —  {h.Classe}[/]\n\n" +
                $"[green]♥ PV: {s.PointsDeVieMax}[/]  [blue]✦ PM: {s.PointsDeManaMax}[/]\n" +
                $"[red]FOR: {s.Force}[/]  [blue]INT: {s.Intelligence}[/]  [yellow]DEF: {s.Defense}[/]  [cyan]AGI: {s.Agilite}[/]  [mediumpurple2]RES: {s.ResistanceMagique}[/]\n\n" +
                $"{armeStr}  |  {armureStr}  |  {accStr}")
                .Border(BoxBorder.Heavy)
                .BorderColor(Color.Green)
                .Padding(1, 0)
                .Expand();
            AnsiConsole.Write(panel);
        }

        AnsiConsole.WriteLine();
        Sauvegarder(silencieux: true);
        AttendreTouche();
    }

    /// <summary>
    /// Lance un combat rapide avec l'équipe actuelle contre un groupe de monstres aléatoires.
    /// </summary>
    private void LancerCombatRapideEquipe()
    {
        AnsiConsole.Clear();

        if (_equipeActuelle.Count == 0 || !_equipeActuelle.Any(h => h.EstVivant))
        {
            AnsiConsole.MarkupLine("[red]Vous devez d'abord créer une équipe (option 1) ![/]");
            AttendreTouche();
            return;
        }

        // Restaurer PV/PM avant le combat pour un départ frais
        foreach (var h in _equipeActuelle.Where(h => h.EstVivant))
        {
            h.Soigner(h.StatsActuelles.PointsDeVieMax);
            h.RestaurerMana(h.StatsActuelles.PointsDeManaMax);
        }

        AfficherEquipe(_equipeActuelle);

        var random = new Random();
        int nbMonstres = random.Next(1, 5);
        var monstres = _monstreFactory.GenererGroupeAleatoire(nbMonstres);
        AppliquerDifficulte(monstres);

        EnregistrerMonstres(monstres);
        AfficherEnnemis(monstres.Cast<Monstre>().ToList());
        AttendreTouche("Appuyez sur une touche pour commencer le combat...");

        InitialiserInventaire();
        var resultat = _combatService.LancerCombat(_equipeActuelle, monstres, _inventaireActuel);
        _historiqueService.AjouterResultat(resultat);
        TraiterResultat(resultat, monstres);
        AfficherResultat(resultat);
    }

    /// <summary>
    /// Lance un combat rapide avec une équipe prédéfinie (pour test rapide ou démo).
    /// </summary>
    private void LancerCombatRapide()
    {
        AnsiConsole.Clear();
        // Création d'une équipe "type" équilibrée
        var heros = new List<Heros>
        {
            _personnageFactory.CreerHeros("Arthas", ClasseHeros.Guerrier),
            _personnageFactory.CreerHeros("Gandalf", ClasseHeros.Mage),
            _personnageFactory.CreerHeros("Shadow", ClasseHeros.Voleur),
            _personnageFactory.CreerHeros("Elara", ClasseHeros.Clerc),
        };

        var equipements = _equipementRepo.ChargerTous();
        heros[0].Equiper(equipements.First(e => e.Nom == "Épée en fer"));
        heros[0].Equiper(equipements.First(e => e.Nom == "Armure de plates"));
        heros[1].Equiper(equipements.First(e => e.Nom == "Bâton magique"));
        heros[1].Equiper(equipements.First(e => e.Nom == "Robe enchantée"));
        heros[2].Equiper(equipements.First(e => e.Nom == "Dague d'ombre"));
        heros[2].Equiper(equipements.First(e => e.Nom == "Armure de cuir"));
        heros[3].Equiper(equipements.First(e => e.Nom == "Masse sacrée"));

        _equipeActuelle = heros;

        AfficherEquipe(heros);

        var random = new Random();
        var monstres = _monstreFactory.GenererGroupeAleatoire(random.Next(2, 5));
        AppliquerDifficulte(monstres);

        EnregistrerMonstres(monstres);
        AfficherEnnemis(monstres);
        AttendreTouche("Appuyez sur une touche pour commencer...");

        InitialiserInventaire();
        var resultat = _combatService.LancerCombat(heros, monstres, _inventaireActuel);
        _historiqueService.AjouterResultat(resultat);
        TraiterResultat(resultat, monstres);
        AfficherResultat(resultat);
    }

    /// <summary>
    /// Lance un combat de boss spécifique sélectionné par le joueur.
    /// </summary>
    private void LancerCombatBoss()
    {
        AnsiConsole.Clear();

        if (_equipeActuelle.Count == 0 || !_equipeActuelle.Any(h => h.EstVivant))
        {
            AnsiConsole.MarkupLine("[red]Vous devez d'abord créer une équipe (option 1 ou 2) ![/]");
            AttendreTouche();
            return;
        }

        var choixBoss = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[red bold]Choisissez votre Boss :[/]")
                .AddChoices(
                    "1. Liche Ancienne (2 phases)",
                    "2. Dragon Ancien (3 phases)",
                    "3. Golem de Cristal (2 phases)",
                    "4. Hydre Venimeuse (3 phases)",
                    "5. Seigneur Démon (3 phases)"));

        string typeBoss = choixBoss[0] switch
        {
            '1' => "Liche",
            '2' => "DragonAncien",
            '3' => "GolemCristal",
            '4' => "Hydre",
            '5' => "SeigneurDemon",
            _ => "Liche"
        };
        var boss = _monstreFactory.CreerBoss(typeBoss);
        AppliquerDifficulte(new List<Monstre> { boss });

        // Affichage du dialogue d'introduction du boss
        var dialogue = _dialogueService.ObtenirDialogueBoss(typeBoss);
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel($"[italic red]{Markup.Escape(dialogue)}[/]")
            .Header($"[bold red]{Markup.Escape(boss.Nom)}[/]")
            .Border(BoxBorder.Double)
            .BorderColor(Color.DarkRed)
            .Padding(1, 1));
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine($"[red]{Markup.Escape(AsciiArt.BanniereBoss)}[/]");
        AnsiConsole.Write(new Rule($"[red bold]⚠ {Markup.Escape(boss.Nom)} ⚠[/]").RuleStyle("red"));
        AfficherBarreDeVie(boss.Nom, boss.PointsDeVie, boss.StatsActuelles.PointsDeVieMax, Color.Red);
        AnsiConsole.MarkupLine($"  [grey]Phases: {boss.NombrePhases} | XP: {boss.ExperienceDonnee}[/]");
        AnsiConsole.WriteLine();

        AttendreTouche("Appuyez sur une touche pour affronter le boss...");

        // Restaurer HP/MP des héros avant le boss pour être "fair"
        foreach (var h in _equipeActuelle)
        {
            h.Soigner(h.StatsActuelles.PointsDeVieMax);
            h.RestaurerMana(h.StatsActuelles.PointsDeManaMax);
        }

        InitialiserInventaire();
        var resultat = _combatService.LancerCombat(_equipeActuelle, new List<Monstre> { boss }, _inventaireActuel);
        _historiqueService.AjouterResultat(resultat);
        TraiterResultat(resultat, new List<Monstre> { boss });
        AfficherResultat(resultat);
    }

    /// <summary>
    /// Permet au joueur de composer son équipe manuellement.
    /// </summary>
    private List<Heros> CreerEquipe()
    {
        AnsiConsole.Write(new FigletText("Nouvelle Equipe").Color(Color.Gold1).Centered());
        AnsiConsole.Write(new Panel(
            "[bold gold1]Formez votre groupe d'aventuriers ![/]\n" +
            "[grey]Choisissez le nombre de héros, leur nom et leur classe.[/]")
            .Border(BoxBorder.Double)
            .BorderColor(Color.Gold1)
            .Padding(1, 0)
            .Expand());
        AnsiConsole.WriteLine();

        int nombre = AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .Title("[yellow bold]Combien de héros dans votre équipe ?[/]")
                .AddChoices(2, 3, 4)
                .UseConverter(n => n switch
                {
                    2 => "⚔ Duo  (2 héros)",
                    3 => "⚔ Trio (3 héros)",
                    4 => "⚔ Escouade complète (4 héros)",
                    _ => n.ToString()
                }));

        var heros = new List<Heros>();
        var classes = Enum.GetValues<ClasseHeros>();

        for (int i = 0; i < nombre; i++)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText($"Heros {i + 1}").Color(Color.Cyan1).Centered());
            AnsiConsole.Write(new Rule($"[cyan bold]Héros {i + 1} / {nombre}[/]").RuleStyle("cyan"));
            AnsiConsole.WriteLine();

            string nom = AnsiConsole.Prompt(
                new TextPrompt<string>("[yellow]Nom du héros :[/]")
                    .PromptStyle("bold white"));

            // Aperçu des classes dans un tableau
            var classTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title("[grey bold]Classes disponibles[/]")
                .Expand();
            classTable.AddColumn(new TableColumn("[white]Classe[/]").Centered());
            classTable.AddColumn(new TableColumn("[white]Spécialité[/]").Centered());
            classTable.AddColumn(new TableColumn("[white]Force[/]").Centered());

            var classDescriptions = new Dictionary<ClasseHeros, (string spec, string force)>
            {
                [ClasseHeros.Guerrier] = ("Mêlée / Tank", "PV et FOR élevés"),
                [ClasseHeros.Mage] = ("Magie offensive", "INT et PM élevés"),
                [ClasseHeros.Voleur] = ("Dégâts / Esquive", "AGI et critiques"),
                [ClasseHeros.Clerc] = ("Soins / Support", "Soins puissants"),
                [ClasseHeros.Paladin] = ("Tank / Soins", "DEF et soins"),
                [ClasseHeros.Necromancien] = ("Magie noire", "Drain et poison"),
                [ClasseHeros.Assassin] = ("Dégâts purs", "Critiques massifs"),
                [ClasseHeros.Druide] = ("Magie nature", "Soins + dégâts"),
            };
            foreach (var c in classes)
            {
                var (spec, force) = classDescriptions.GetValueOrDefault(c, ("-", "-"));
                string couleur = AsciiArt.ObtenirCouleurClasse(c);
                string icone = AsciiArt.ObtenirIconeClasse(c);
                classTable.AddRow(
                    $"[{couleur}]{icone} {c}[/]",
                    $"[grey]{spec}[/]",
                    $"[grey]{force}[/]");
            }
            AnsiConsole.Write(classTable);
            AnsiConsole.WriteLine();

            var classe = AnsiConsole.Prompt(
                new SelectionPrompt<ClasseHeros>()
                    .Title($"[yellow bold]Classe de {Markup.Escape(nom)} :[/]")
                    .AddChoices(classes)
                    .UseConverter(c =>
                    {
                        string icone = AsciiArt.ObtenirIconeClasse(c);
                        var (spec, _) = classDescriptions.GetValueOrDefault(c, ("-", "-"));
                        return $"{icone}  {c}  —  {spec}";
                    }));

            var hero = _personnageFactory.CreerHeros(nom, classe);
            heros.Add(hero);

            // Afficher le héros créé avec son ASCII art
            string couleurCls = AsciiArt.ObtenirCouleurClasse(hero.Classe);
            string iconeCls = AsciiArt.ObtenirIconeClasse(hero.Classe);
            var s = hero.StatsActuelles;
            var heroPanel = new Panel(
                $"[{couleurCls}]{Markup.Escape(AsciiArt.ObtenirClasse(hero.Classe))}[/]\n\n" +
                $"[{couleurCls} bold]{iconeCls} {Markup.Escape(hero.Nom)}  —  {hero.Classe}[/]\n" +
                $"[green]♥ PV: {s.PointsDeVieMax}[/]  [blue]✦ PM: {s.PointsDeManaMax}[/]  " +
                $"[red]FOR: {s.Force}[/]  [blue]INT: {s.Intelligence}[/]  [yellow]DEF: {s.Defense}[/]  [cyan]AGI: {s.Agilite}[/]")
                .Header($"[bold {couleurCls}]✓ HÉROS CRÉÉ[/]")
                .Border(BoxBorder.Heavy)
                .BorderColor(Color.Green)
                .Padding(1, 0)
                .Expand();
            AnsiConsole.Write(heroPanel);
            AttendreTouche();
        }

        return heros;
    }

    /// <summary>
    /// Propose un menu pour permettre au joueur de gérer l'équipement de ses héros (initialisation).
    /// </summary>
    private void EquiperHeros(List<Heros> heros)
    {
        var equipements = _equipementRepo.ChargerTous();

        foreach (var hero in heros)
        {
            AnsiConsole.Clear();
            string couleur = AsciiArt.ObtenirCouleurClasse(hero.Classe);
            string icone = AsciiArt.ObtenirIconeClasse(hero.Classe);

            AnsiConsole.Write(new FigletText("Equipement").Color(Color.Gold1).Centered());
            AnsiConsole.Write(new Rule($"[{couleur} bold]{icone} Équipement de {Markup.Escape(hero.Nom)} ({hero.Classe})[/]").RuleStyle(couleur));
            AnsiConsole.WriteLine();

            // Tableau des équipements disponibles
            var armes = equipements.Where(e => e.Type == TypeEquipement.Arme).ToList();
            var armures = equipements.Where(e => e.Type == TypeEquipement.Armure).ToList();
            var accessoires = equipements.Where(e => e.Type == TypeEquipement.Accessoire).ToList();

            var invTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Grey)
                .Title("[grey bold]Équipements disponibles[/]")
                .Expand();
            invTable.AddColumn(new TableColumn("[red]⚔ Armes[/]"));
            invTable.AddColumn(new TableColumn("[blue]🛡 Armures[/]"));
            invTable.AddColumn(new TableColumn("[gold1]💍 Accessoires[/]"));

            int maxRows = Math.Max(armes.Count, Math.Max(armures.Count, accessoires.Count));
            for (int r = 0; r < maxRows; r++)
            {
                invTable.AddRow(
                    r < armes.Count ? $"{Markup.Escape(armes[r].Nom)} [grey]({FormatBonus(armes[r])})[/]" : "",
                    r < armures.Count ? $"{Markup.Escape(armures[r].Nom)} [grey]({FormatBonus(armures[r])})[/]" : "",
                    r < accessoires.Count ? $"{Markup.Escape(accessoires[r].Nom)} [grey]({FormatBonus(accessoires[r])})[/]" : "");
            }
            AnsiConsole.Write(invTable);
            AnsiConsole.WriteLine();

            EquiperSlot(hero, equipements, TypeEquipement.Arme, "⚔ Arme");
            EquiperSlot(hero, equipements, TypeEquipement.Armure, "🛡 Armure");
            EquiperSlot(hero, equipements, TypeEquipement.Accessoire, "💍 Accessoire");

            // Récap de l'équipement choisi
            string armeStr = hero.Arme != null ? $"[red]⚔ {Markup.Escape(hero.Arme.Nom)}[/]" : "[grey]—[/]";
            string armureStr = hero.Armure != null ? $"[blue]🛡 {Markup.Escape(hero.Armure.Nom)}[/]" : "[grey]—[/]";
            string accStr = hero.Accessoire != null ? $"[gold1]💍 {Markup.Escape(hero.Accessoire.Nom)}[/]" : "[grey]—[/]";

            AnsiConsole.Write(new Panel(
                $"[{couleur} bold]{icone} {Markup.Escape(hero.Nom)}[/]\n" +
                $"{armeStr}  |  {armureStr}  |  {accStr}")
                .Header($"[green bold]✓ Équipé[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Padding(1, 0)
                .Expand());
            AttendreTouche();
        }
    }

    /// <summary>
    /// Gère la sélection et l'équipement d'un objet pour un slot donné (Arme, Armure, Accessoire).
    /// </summary>
    /// <param name="hero">Le héros à équiper.</param>
    /// <param name="equipements">La liste des équipements disponibles.</param>
    /// <param name="type">Le type d'équipement visé.</param>
    /// <param name="nomSlot">Le nom affiché du slot.</param>
    private static void EquiperSlot(Heros hero, List<Equipement> equipements,
                                     TypeEquipement type, string nomSlot)
    {
        var items = equipements.Where(e => e.Type == type).ToList();
        var choix = new List<string> { "(Aucun)" };
        choix.AddRange(items.Select(e => $"{e.Nom} ({FormatBonus(e)})"));

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow bold]{nomSlot} :[/]")
                .AddChoices(choix));

        if (selection != "(Aucun)")
        {
            var nom = selection.Split(" (")[0];
            var equip = items.First(e => e.Nom == nom);
            hero.Equiper(equip);
        }
    }

    /// <summary>
    /// Formate les bonus de statistiques d'un équipement en chaîne de caractères.
    /// </summary>
    /// <param name="e">L'équipement.</param>
    /// <returns>Une chaîne décrivant les bonus (ex: "FOR +5, DEF +2").</returns>
    private static string FormatBonus(Equipement e)
    {
        var parts = new List<string>();
        if (e.BonusStats.Force != 0) parts.Add($"FOR {e.BonusStats.Force:+#;-#}");
        if (e.BonusStats.Intelligence != 0) parts.Add($"INT {e.BonusStats.Intelligence:+#;-#}");
        if (e.BonusStats.Defense != 0) parts.Add($"DEF {e.BonusStats.Defense:+#;-#}");
        if (e.BonusStats.Agilite != 0) parts.Add($"AGI {e.BonusStats.Agilite:+#;-#}");
        if (e.BonusStats.PointsDeVieMax != 0) parts.Add($"PV {e.BonusStats.PointsDeVieMax:+#;-#}");
        if (e.BonusStats.PointsDeManaMax != 0) parts.Add($"PM {e.BonusStats.PointsDeManaMax:+#;-#}");
        if (e.BonusStats.ResistanceMagique != 0) parts.Add($"RES {e.BonusStats.ResistanceMagique:+#;-#}");
        return string.Join(", ", parts);
    }

    /// <summary>
    /// Demande au joueur de choisir une action pour le tour d'un héros.
    /// </summary>
    /// <param name="heros">Le héros dont c'est le tour.</param>
    /// <param name="ennemis">La liste des ennemis vivants.</param>
    /// <param name="allies">La liste des alliés vivants.</param>
    /// <param name="inventaire">L'inventaire de l'équipe.</param>
    /// <returns>L'action de combat choisie.</returns>
    public ActionCombat DemanderActionJoueur(Heros heros, List<ICombattant> ennemis,
                                             List<ICombattant> allies,
                                             Domain.Entities.Inventaire inventaire)
    {
        // Récapitulatif de tour avec barres de vie
        AfficherRecapTour(allies, ennemis);

        AnsiConsole.Write(new Rule($"[yellow bold]Tour de {Markup.Escape(heros.Nom)} ({heros.Classe} Nv.{heros.Niveau})[/]").RuleStyle("yellow"));
        AfficherBarreDeVie("PV", heros.PointsDeVie, heros.StatsActuelles.PointsDeVieMax, Color.Green);
        AfficherBarreDeVie("PM", heros.PointsDeMana, heros.StatsActuelles.PointsDeManaMax, Color.Blue);

        if (heros.EffetsActifs.Any())
        {
            var effets = string.Join(", ", heros.EffetsActifs.Select(e => $"{e.Statut}({e.ToursRestants}t)"));
            AnsiConsole.MarkupLine($"  [grey]Effets: {Markup.Escape(effets)}[/]");
        }

        var choix = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Action :[/]")
                .AddChoices("1. Compétence", "2. Objet", "3. Défendre"));

        return choix[0] switch
        {
            '1' => ChoisirCompetence(heros, ennemis, allies),
            '2' => ChoisirObjet(heros, allies, inventaire),
            _ => ActionCombat.Defendre(heros)
        };
    }

    /// <summary>
    /// Affiche un tableau récapitulatif de l'état de tous les combattants (alliés et ennemis).
    /// </summary>
    private void AfficherRecapTour(List<ICombattant> allies, List<ICombattant> ennemis)
    {
        AnsiConsole.Write(new Rule("[cyan bold]⚔ État du combat ⚔[/]").RuleStyle("cyan"));

        var table = new Table()
            .Border(TableBorder.Heavy)
            .BorderColor(Color.Cyan1)
            .Title("[cyan bold]═══ CHAMP DE BATAILLE ═══[/]")
            .Expand();

        table.AddColumn(new TableColumn("[green bold]Combattant[/]").Centered());
        table.AddColumn(new TableColumn("[green]♥ PV[/]").Centered());
        table.AddColumn(new TableColumn("[blue]✦ PM[/]").Centered());
        table.AddColumn(new TableColumn("[grey]Statut[/]").Centered());

        foreach (var a in allies)
        {
            string statut = a.EffetsActifs.Any()
                ? string.Join(", ", a.EffetsActifs.Select(e => FormatStatut(e.Statut)))
                : "[green]OK[/]";

            string pvColor = GetPvColor(a.PointsDeVie, a.StatsActuelles.PointsDeVieMax);
            int barW = LayoutHelper.LargeurBarreCourte;
            string pvBar = CreerBarreTexte(a.PointsDeVie, a.StatsActuelles.PointsDeVieMax, barW);

            string icone = a is Heros hero ? AsciiArt.ObtenirIconeClasse(hero.Classe) + " " : "";
            table.AddRow(
                a.EstVivant ? $"[white]{icone}{Markup.Escape(a.Nom)}[/]" : $"[strikethrough grey]💀 {Markup.Escape(a.Nom)}[/]",
                $"[{pvColor}]{Markup.Escape(pvBar)} {a.PointsDeVie}/{a.StatsActuelles.PointsDeVieMax}[/]",
                $"[blue]{a.PointsDeMana}/{a.StatsActuelles.PointsDeManaMax}[/]",
                statut);
        }

        table.AddEmptyRow();
        table.AddRow("[red bold]── ENNEMIS ──[/]", "", "", "");

        foreach (var e in ennemis)
        {
            string pvColor = GetPvColor(e.PointsDeVie, e.StatsActuelles.PointsDeVieMax);
            int barW2 = LayoutHelper.LargeurBarreCourte;
            string pvBar = CreerBarreTexte(e.PointsDeVie, e.StatsActuelles.PointsDeVieMax, barW2);
            string statut = e.EffetsActifs.Any()
                ? string.Join(", ", e.EffetsActifs.Select(ef => FormatStatut(ef.Statut)))
                : "";

            string nom = e is Boss boss
                ? $"[bold red]☠ {Markup.Escape(e.Nom)} ({boss.GetNomPhase()})[/]"
                : $"[red]{Markup.Escape(e.Nom)}[/]";

            table.AddRow(
                nom,
                $"[{pvColor}]{Markup.Escape(pvBar)} {e.PointsDeVie}/{e.StatsActuelles.PointsDeVieMax}[/]",
                $"[blue]{e.PointsDeMana}/{e.StatsActuelles.PointsDeManaMax}[/]",
                statut);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Crée une barre de progression textuelle (ex: "████░░").
    /// </summary>
    private static string CreerBarreTexte(int actuel, int max, int largeur)
    {
        if (max == 0) return new string('░', largeur);
        int rempli = (int)Math.Round((double)actuel / max * largeur);
        rempli = Math.Clamp(rempli, 0, largeur);
        return new string('█', rempli) + new string('░', largeur - rempli);
    }

    /// <summary>
    /// Retourne une couleur (green, yellow, red) en fonction du pourcentage de PV restants.
    /// </summary>
    private static string GetPvColor(int actuel, int max)
    {
        if (max == 0) return "grey";
        double ratio = (double)actuel / max;
        if (ratio > 0.6) return "green";
        if (ratio > 0.3) return "yellow";
        return "red";
    }

    /// <summary>
    /// Retourne une représentation formatée d'un statut.
    /// </summary>
    private static string FormatStatut(StatutEffet statut) => statut switch
    {
        StatutEffet.Poison => "[green]🧪 Poison[/]",
        StatutEffet.Brulure => "[orangered1]🔥 Brûlure[/]",
        StatutEffet.Gel => "[aqua]❄ Gel[/]",
        StatutEffet.Paralysie => "[yellow]⚡ Paralysie[/]",
        StatutEffet.Sommeil => "[mediumpurple2]💤 Sommeil[/]",
        StatutEffet.BuffAttaque => "[gold1]⬆ Buff ATK[/]",
        StatutEffet.DebuffDefense => "[grey]⬇ Debuff DEF[/]",
        _ => ""
    };

    /// <summary>
    /// Affiche une barre de vie colorée avec label.
    /// </summary>
    private static void AfficherBarreDeVie(string label, int actuel, int max, Color couleur)
    {
        if (max == 0) return;
        double ratio = Math.Clamp((double)actuel / max, 0, 1);
        int largeur = LayoutHelper.LargeurBarre;
        int rempli = (int)Math.Round(ratio * largeur);

        string barre = new string('█', rempli) + new string('░', largeur - rempli);
        int pourcent = (int)Math.Round(ratio * 100);
        string pvColor = ratio > 0.6 ? "green" : ratio > 0.3 ? "yellow" : "red";
        string icone = label == "PV" ? "♥" : label == "PM" ? "✦" : label == "XP" ? "★" : "●";

        AnsiConsole.MarkupLine($"  [{pvColor}]{Markup.Escape($"{icone} {label}: [{barre}] {actuel}/{max} ({pourcent}%)")}[/]");
    }

    /// <summary>
    /// Affiche un menu permettant de sélectionner une compétence à utiliser.
    /// </summary>
    /// <param name="heros">Le héros qui lance la compétence.</param>
    /// <param name="ennemis">Liste des ennemis potentiels.</param>
    /// <param name="allies">Liste des alliés potentiels.</param>
    /// <returns>L'action de combat "Attaquer" avec la compétence choisie.</returns>
    private ActionCombat ChoisirCompetence(Heros heros, List<ICombattant> ennemis,
                                            List<ICombattant> allies)
    {
        var competences = heros.GetCompetences()
            .Where(c => c.NiveauRequis <= heros.Niveau)
            .ToList();

        var noms = competences.Select(c =>
        {
            string cout = c.CoutMana > 0 ? $" (PM: {c.CoutMana})" : " (gratuit)";
            string type = c.TypeDegat == TypeDegat.Physique ? "[red]PHY[/]" : "[blue]MAG[/]";
            
            // Calcul de la puissance estimée (Base + Skill Power)
            int statBase = c.TypeDegat == TypeDegat.Physique ? heros.StatsActuelles.Force : heros.StatsActuelles.Intelligence;
            int totalPuissance = statBase + c.Puissance;
            
            string effet = c.EffetSecondaire != StatutEffet.Aucun ? $" ({c.EffetSecondaire})" : "";
            string dispo = c.CoutMana > heros.PointsDeMana ? " [grey](pas assez de PM)[/]" : "";
            string elem = c.Element != Element.Neutre ? $" {AsciiArt.ObtenirEmoji(c.Element)}" : "";
            return $"{Markup.Escape(c.Nom)} {type} Atk:{totalPuissance} (P:{c.Puissance}){cout}{Markup.Escape(effet)}{elem}{dispo}";
        }).ToList();

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Compétence :[/]")
                .AddChoices(noms));

        int idx = noms.IndexOf(selection);
        var competence = competences[idx];

        if (competence.Cible == CibleType.UnAllie || competence.Cible == CibleType.Soi)
        {
            var cible = ChoisirCible(allies, "allié");
            return ActionCombat.Attaquer(heros, competence, new List<ICombattant> { cible });
        }
        else if (competence.Cible == CibleType.TousLesEnnemis)
        {
            return ActionCombat.Attaquer(heros, competence, ennemis);
        }
        else
        {
            var cible = ChoisirCible(ennemis, "ennemi");
            return ActionCombat.Attaquer(heros, competence, new List<ICombattant> { cible });
        }
    }

    /// <summary>
    /// Affiche un menu permettant de sélectionner un objet de l'inventaire à utiliser.
    /// </summary>
    /// <param name="heros">Le héros qui utilise l'objet.</param>
    /// <param name="allies">Liste des alliés (cibles potentielles pour soins/buffs).</param>
    /// <param name="inventaire">L'inventaire contenant les objets.</param>
    /// <returns>L'action de combat "UtiliserObjet".</returns>
    private static ActionCombat ChoisirObjet(Heros heros, List<ICombattant> allies,
                                      Domain.Entities.Inventaire inventaire)
    {
        var objets = inventaire.ListerObjets();
        if (objets.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Aucun objet disponible ! Défense automatique.[/]");
            return ActionCombat.Defendre(heros);
        }

        var noms = objets.Select(o => $"{o.Nom} (x{o.Quantite}) - {o.Description}").ToList();

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Objet :[/]")
                .AddChoices(noms));

        int idx = noms.IndexOf(selection);
        var objet = objets[idx];

        var cible = ChoisirCible(allies, "allié");
        return ActionCombat.UtiliserObjet(heros, objet, cible);
    }

    /// <summary>
    /// Permet de sélectionner une cible parmi une liste de combattants.
    /// </summary>
    /// <param name="cibles">Liste des cibles possibles.</param>
    /// <param name="typeCible">Texte décrivant le type de cible (ennemi/allié).</param>
    /// <returns>Le combattant choisi.</returns>
    private static ICombattant ChoisirCible(List<ICombattant> cibles, string typeCible)
    {
        if (cibles.Count == 1) return cibles[0];

        int barW = LayoutHelper.LargeurBarreCourte;
        var noms = cibles.Select(c =>
        {
            string pvColor = GetPvColor(c.PointsDeVie, c.StatsActuelles.PointsDeVieMax);
            string barre = CreerBarreTexte(c.PointsDeVie, c.StatsActuelles.PointsDeVieMax, barW);
            return $"{Markup.Escape(c.Nom)} [{pvColor}]{Markup.Escape(barre)}[/] ♥{c.PointsDeVie}/{c.StatsActuelles.PointsDeVieMax}";
        }).ToList();

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]Choisir un {typeCible} :[/]")
                .AddChoices(noms));

        int idx = noms.IndexOf(selection);
        return cibles[idx];
    }

    /// <summary>
    /// Affiche les statistiques détaillées à la fin d'un combat (victoire ou défaite).
    /// </summary>
    /// <param name="resultat">L'objet contenant les résultats du combat.</param>
    public void AfficherResultat(ResultatCombat resultat)
    {
        AnsiConsole.WriteLine();

        // Banner
        if (resultat.VictoireHeros)
            AnsiConsole.MarkupLine($"[gold1]{Markup.Escape(AsciiArt.BanniereVictoire)}[/]");
        else
            AnsiConsole.MarkupLine($"[darkred]{Markup.Escape(AsciiArt.BanniereDefaite)}[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Double)
            .BorderColor(resultat.VictoireHeros ? Color.Gold1 : Color.DarkRed)
            .Title(resultat.VictoireHeros
                ? "[bold gold1]🏆 STATISTIQUES DE LA VICTOIRE[/]"
                : "[bold darkred]💀 STATISTIQUES DE LA DÉFAITE[/]")
            .Expand();

        table.AddColumn("[white]Statistique[/]");
        table.AddColumn("[white]Valeur[/]");

        table.AddRow("Résultat", resultat.VictoireHeros ? "[green]VICTOIRE[/]" : "[red]DÉFAITE[/]");
        table.AddRow("Tours joués", resultat.NombreTours.ToString());
        table.AddRow("Dégâts totaux", $"[red]{resultat.TotalDegatsInfliges}[/]");
        table.AddRow("Soins totaux", $"[green]{resultat.TotalSoinsProdigues}[/]");
        table.AddRow("XP gagnée", $"[gold1]{resultat.ExperienceGagnee}[/]");

        AnsiConsole.Write(table);

        // Dégâts par héros
        if (resultat.DegatsParHeros.Any(d => d.Value > 0))
        {
            AnsiConsole.WriteLine();
            var chart = new BarChart()
                .Width(LayoutHelper.LargeurChart)
                .Label("[white bold]⚔ Dégâts par héros[/]");

            foreach (var (nom, degats) in resultat.DegatsParHeros.OrderByDescending(d => d.Value))
            {
                chart.AddItem(nom, degats, Color.Red);
            }

            AnsiConsole.Write(chart);
        }

        // Afficher XP des héros vivants
        if (resultat.VictoireHeros && _equipeActuelle.Any())
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[gold1 bold]★ Progression des héros ★[/]").RuleStyle("gold1"));
            foreach (var h in _equipeActuelle.Where(h => h.EstVivant))
            {
                string couleur = AsciiArt.ObtenirCouleurClasse(h.Classe);
                string icone = AsciiArt.ObtenirIconeClasse(h.Classe);
                AnsiConsole.MarkupLine($"  [{couleur}]{icone} {Markup.Escape(h.Nom)}[/] Nv.[gold1]{h.Niveau}[/] — XP: {h.Experience}/{h.ExperiencePourProchainNiveau}");
                AfficherBarreDeVie("XP", h.Experience, h.ExperiencePourProchainNiveau, Color.Gold1);
            }
        }

        AttendreTouche();
    }

    /// <summary>
    /// Affiche l'historique complet des combats passés.
    /// </summary>
    private void AfficherHistorique()
    {
        AnsiConsole.Clear();
        var historique = _historiqueService.ObtenirHistorique();

        if (historique.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Aucun combat dans l'historique.[/]");
            AttendreTouche();
            return;
        }

        // Stats globales
        AnsiConsole.Write(new FigletText("Historique").Color(Color.Cyan1).Centered());
        AnsiConsole.Write(new Rule("[cyan bold]📊 Statistiques Globales[/]").RuleStyle("cyan"));
        AnsiConsole.WriteLine();

        var statsTable = new Table()
            .Border(TableBorder.Heavy)
            .BorderColor(Color.Cyan1)
            .Title("[cyan bold]═══ STATISTIQUES ═══[/]")
            .Expand();

        statsTable.AddColumn("[white]Stat[/]");
        statsTable.AddColumn("[white]Valeur[/]");

        statsTable.AddRow("Total combats", _historiqueService.TotalCombats.ToString());
        statsTable.AddRow("Victoires", $"[green]{_historiqueService.TotalVictoires}[/]");
        statsTable.AddRow("Défaites", $"[red]{_historiqueService.TotalDefaites}[/]");
        statsTable.AddRow("Taux de victoire",
            $"[gold1]{(_historiqueService.TotalCombats > 0 ? _historiqueService.TotalVictoires * 100 / _historiqueService.TotalCombats : 0)}%[/]");
        statsTable.AddRow("Total dégâts infligés",
            $"[red]{historique.Sum(r => r.TotalDegatsInfliges)}[/]");
        statsTable.AddRow("Total soins prodigués",
            $"[green]{historique.Sum(r => r.TotalSoinsProdigues)}[/]");
        statsTable.AddRow("Total XP gagnée",
            $"[gold1]{historique.Sum(r => r.ExperienceGagnee)}[/]");

        AnsiConsole.Write(statsTable);
        AnsiConsole.WriteLine();

        // Détail des combats
        var detailTable = new Table()
            .Border(TableBorder.Heavy)
            .BorderColor(Color.Cyan1)
            .Title("[cyan bold]═══ HISTORIQUE DÉTAILLÉ ═══[/]")
            .Expand();

        detailTable.AddColumn("#");
        detailTable.AddColumn("Date");
        detailTable.AddColumn("Résultat");
        detailTable.AddColumn("Tours");
        detailTable.AddColumn("Dégâts");
        detailTable.AddColumn("Soins");
        detailTable.AddColumn("XP");
        detailTable.AddColumn("Ennemis");

        for (int i = 0; i < historique.Count; i++)
        {
            var r = historique[i];
            detailTable.AddRow(
                (i + 1).ToString(),
                r.Date.ToString("HH:mm:ss"),
                r.VictoireHeros ? "[green]Victoire[/]" : "[red]Défaite[/]",
                r.NombreTours.ToString(),
                r.TotalDegatsInfliges.ToString(),
                r.TotalSoinsProdigues.ToString(),
                r.ExperienceGagnee.ToString(),
                Markup.Escape(string.Join(", ", r.MonstresAffrontes))
            );
        }

        AnsiConsole.Write(detailTable);
        AttendreTouche();
    }

    /// <summary>
    /// Sauvegarde l'état complet de la partie (héros, progression, succès, etc.) dans un fichier JSON.
    /// </summary>
    /// <param name="silencieux">Si vrai, n'affiche pas de message de confirmation à l'écran.</param>
    private void Sauvegarder(bool silencieux = false)
    {
        if (_equipeActuelle.Count == 0)
        {
            if (!silencieux)
            {
                AnsiConsole.MarkupLine("[red]Aucune équipe à sauvegarder ![/]");
                AttendreTouche();
            }
            return;
        }

        var donnees = new DonneesSauvegarde
        {
            Heros = _equipeActuelle.Select(h => new HerosSauvegarde
            {
                Nom = h.Nom,
                Classe = h.Classe.ToString(),
                Niveau = h.Niveau,
                Experience = h.Experience,
                PvMax = h.StatsBase.PointsDeVieMax,
                PmMax = h.StatsBase.PointsDeManaMax,
                Force = h.StatsBase.Force,
                Intelligence = h.StatsBase.Intelligence,
                Agilite = h.StatsBase.Agilite,
                Defense = h.StatsBase.Defense,
                ResistanceMagique = h.StatsBase.ResistanceMagique
            }).ToList(),
            Historique = _historiqueService.ObtenirHistorique(),
            Bestiaire = _bestiaireService.ObtenirBestiaire(),
            Succes = _succesService.ObtenirDebloques().Select(s => new SuccesSauvegarde
            {
                Id = s.Id,
                DateDeblocage = s.DateDeblocage
            }).ToList(),
            TotalVictoires = _totalVictoires,
            TotalDefaites = _totalDefaites,
            BossVaincus = _bossVaincus,
            VaguesAreneMax = _vaguesAreneMax,
            DonjonProfondeurMax = _donjonProfondeurMax,
            Or = _boutiqueService.Or,
            Quetes = _queteService.ObtenirTerminees().Select(q => new QueteSauvegarde
            {
                Id = q.Id,
                DateCompletion = q.DateCompletion
            }).ToList(),
            BossVaincusNoms = _bossVaincusNoms.ToList(),
            Difficulte = _difficulte.ToString()
        };

        _sauvegardeService.Sauvegarder(donnees, CheminSauvegarde);

        if (!silencieux)
        {
            AnsiConsole.MarkupLine("[green]✓ Sauvegarde effectuée ![/]");
            AttendreTouche();
        }
    }

    /// <summary>
    /// Charge une partie sauvegardée depuis le fichier JSON.
    /// </summary>
    private void Charger()
    {
        if (!_sauvegardeService.SauvegardeExiste(CheminSauvegarde))
        {
            AnsiConsole.MarkupLine("[red]Aucune sauvegarde trouvée.[/]");
            AttendreTouche();
            return;
        }

        var donnees = _sauvegardeService.Charger(CheminSauvegarde);
        if (donnees == null)
        {
            AnsiConsole.MarkupLine("[red]Erreur lors du chargement.[/]");
            AttendreTouche();
            return;
        }

        _equipeActuelle = new List<Heros>();
        foreach (var hs in donnees.Heros)
        {
            var classe = Enum.Parse<ClasseHeros>(hs.Classe);
            var stats = new Domain.ValueObjects.Stats(hs.PvMax, hs.PmMax, hs.Force,
                hs.Intelligence, hs.Agilite, hs.Defense, hs.ResistanceMagique);
            var hero = new Heros(hs.Nom, classe, stats, hs.Niveau);

            // Ajouter les compétences de la factory pour cette classe
            var tempHero = _personnageFactory.CreerHeros(hs.Nom, classe);
            foreach (var comp in tempHero.GetCompetences())
                hero.AjouterCompetence(comp);
            hero.Resistances = tempHero.Resistances;

            // Restaurer l'XP
            if (hs.Experience > 0)
                hero.GagnerExperience(hs.Experience);

            _equipeActuelle.Add(hero);
        }

        // Restaurer l'historique
        _historiqueService.Restaurer(donnees.Historique);

        // Restaurer le bestiaire
        _bestiaireService.Restaurer(donnees.Bestiaire);

        // Restaurer les succès
        _succesService.Restaurer(donnees.Succes);

        // Restaurer les stats de progression
        _totalVictoires = donnees.TotalVictoires;
        _totalDefaites = donnees.TotalDefaites;
        _bossVaincus = donnees.BossVaincus;
        _vaguesAreneMax = donnees.VaguesAreneMax;
        _donjonProfondeurMax = donnees.DonjonProfondeurMax;
        _boutiqueService.Restaurer(donnees.Or);
        _queteService.Restaurer(donnees.Quetes);
        _bossVaincusNoms.Clear();
        foreach (var nom in donnees.BossVaincusNoms)
            _bossVaincusNoms.Add(nom);
        if (Enum.TryParse<Difficulte>(donnees.Difficulte, out var diff))
            _difficulte = diff;

        AnsiConsole.MarkupLine($"[green]✓ Sauvegarde chargée ({donnees.Heros.Count} héros) ![/]");
        AnsiConsole.MarkupLine($"[grey]Date de sauvegarde : {donnees.DateSauvegarde:g}[/]");
        AnsiConsole.MarkupLine($"[grey]Difficulté: {_difficulte} | Or: {_boutiqueService.Or} | Victoires: {_totalVictoires} | Boss vaincus: {_bossVaincus} | Bestiaire: {donnees.Bestiaire.Count} monstres | Succès: {donnees.Succes.Count}[/]");
        AfficherEquipe(_equipeActuelle);
        AttendreTouche();
    }

    private static void AfficherEquipe(List<Heros> heros)
    {
        AnsiConsole.Write(new Rule("[green bold]⚔ Votre Équipe ⚔[/]").RuleStyle("green"));

        var table = new Table()
            .Border(TableBorder.Heavy)
            .BorderColor(Color.Green)
            .Title("[green bold]═══ HÉROS ═══[/]")
            .Expand();

        table.AddColumn(new TableColumn("[white bold]Héros[/]").Centered());
        table.AddColumn(new TableColumn("[yellow]Classe[/]").Centered());
        table.AddColumn(new TableColumn("[gold1]Nv[/]").Centered());
        table.AddColumn(new TableColumn("[green]♥ PV[/]").Centered());
        table.AddColumn(new TableColumn("[blue]✦ PM[/]").Centered());
        table.AddColumn(new TableColumn("[red]FOR[/]").Centered());
        table.AddColumn(new TableColumn("[blue]INT[/]").Centered());
        table.AddColumn(new TableColumn("[yellow]DEF[/]").Centered());
        table.AddColumn(new TableColumn("[cyan]AGI[/]").Centered());

        foreach (var h in heros)
        {
            var s = h.StatsActuelles;
            string couleurClasse = AsciiArt.ObtenirCouleurClasse(h.Classe);
            string icone = AsciiArt.ObtenirIconeClasse(h.Classe);
            string pvColor = GetPvColor(h.PointsDeVie, s.PointsDeVieMax);
            table.AddRow(
                $"[{couleurClasse}]{icone} {Markup.Escape(h.Nom)}[/]",
                $"[{couleurClasse}]{h.Classe}[/]",
                $"[gold1]{h.Niveau}[/]",
                $"[{pvColor}]{h.PointsDeVie}/{s.PointsDeVieMax}[/]",
                $"[blue]{h.PointsDeMana}/{s.PointsDeManaMax}[/]",
                $"[red]{s.Force}[/]",
                $"[blue]{s.Intelligence}[/]",
                $"[yellow]{s.Defense}[/]",
                $"[cyan]{s.Agilite}[/]");
        }

        AnsiConsole.Write(table);
    }

    private static void AfficherEnnemis(List<Monstre> monstres)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[red bold]☠ Ennemis ☠[/]").RuleStyle("red"));
        foreach (var m in monstres)
        {
            string type = m is Boss boss ? $"💀 BOSS ({boss.NombrePhases} phases)" : "Monstre";

            // Afficher ASCII art dans un panel coloré
            var art = AsciiArt.Obtenir(m.Nom);
            string pvColor = m is Boss ? "darkred" : "red";
            var boxBorder = m is Boss ? BoxBorder.Double : BoxBorder.Rounded;

            var resStr = "";
            if (m.Resistances.Count > 0)
            {
                resStr = "\n" + string.Join(" ", m.Resistances.Select(r =>
                {
                    string emoji = AsciiArt.ObtenirEmoji(r.Key);
                    string couleur = AsciiArt.ObtenirCouleur(r.Key);
                    return $"[{couleur}]{emoji}{r.Key}(x{r.Value:F1})[/]";
                }));
            }

            var panel = new Panel(
                $"[{pvColor}]{Markup.Escape(art)}[/]\n\n" +
                $"[grey]{type} — ♥ PV: {m.PointsDeVie} | ★ XP: {m.ExperienceDonnee}[/]{resStr}")
                .Header($"[bold {pvColor}]{Markup.Escape(m.Nom)}[/]")
                .Border(boxBorder)
                .BorderColor(m is Boss ? Color.DarkRed : Color.Red)
                .Padding(1, 0);
            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }
    }

    private void EnregistrerMonstres(List<Monstre> monstres)
    {
        foreach (var m in monstres)
        {
            _bestiaireService.EnregistrerMonstre(m.Nom, m.StatsActuelles.PointsDeVieMax,
                m.StatsActuelles.Force, m.StatsActuelles.Defense, m.Resistances, m.ExperienceDonnee);
        }
    }

    private void TraiterResultat(ResultatCombat resultat, List<Monstre> monstres)
    {
        if (resultat.VictoireHeros)
        {
            _totalVictoires++;
            int orGagne = 0;
            foreach (var m in monstres.Where(m => !m.EstVivant))
            {
                _bestiaireService.EnregistrerKill(m.Nom);
                orGagne += 10 + m.ExperienceDonnee / 5;
                if (m is Boss boss)
                {
                    _bossVaincus++;
                    _bossVaincusNoms.Add(boss.Nom switch
                    {
                        "Liche Ancienne" => "Liche",
                        "Dragon Ancien" => "DragonAncien",
                        "Golem de Cristal" => "GolemCristal",
                        "Hydre Venimeuse" => "Hydre",
                        "Seigneur Démon" => "SeigneurDemon",
                        _ => boss.Nom
                    });
                    orGagne += 100;
                }
            }
            _boutiqueService.AjouterOr(orGagne);
            AnsiConsole.MarkupLine($"[gold1]+{orGagne} pièces d'or ! (Total: {_boutiqueService.Or})[/]");
        }
        else
        {
            _totalDefaites++;
            AfficherGameOver();
        }

        VerifierSucces();
        VerifierQuetes();

        // Sauvegarde automatique
        Sauvegarder(silencieux: true);
    }

    private void VerifierSucces()
    {
        var ctx = new ContexteSucces
        {
            TotalKills = _bestiaireService.TotalKills,
            TotalVictoires = _totalVictoires,
            TotalDefaites = _totalDefaites,
            BossVaincus = _bossVaincus,
            NiveauMaxAtteint = _equipeActuelle.Any() ? _equipeActuelle.Max(h => h.Niveau) : 0,
            VaguesArene = _vaguesAreneMax,
            DonjonsProfondeur = _donjonProfondeurMax,
            VictoireSansMort = _equipeActuelle.All(h => h.EstVivant),
            VictoireSoloHeros = _equipeActuelle.Count(h => h.EstVivant) == 1,
        };

        _succesService.Verifier(ctx);

        foreach (var s in _succesService.NouveauxSucces())
        {
            SoundService.Succes();
            AnsiConsole.WriteLine();
            var panel = new Panel($"[bold gold1]{s.Icone} SUCCÈS DÉBLOQUÉ : {Markup.Escape(s.Nom)}[/]\n[grey]{Markup.Escape(s.Description)}[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Gold1)
                .Padding(1, 0);
            AnsiConsole.Write(panel);
        }
    }

    // ═══ BOUTIQUE / MARCHAND ═══

    private void AfficherBoutique()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Boutique").Color(Color.Gold1).Centered());
        AnsiConsole.MarkupLine($"[gold1]{Markup.Escape(AsciiArt.BanniereBoutique)}[/]");
        AnsiConsole.WriteLine();

        while (true)
        {
            AnsiConsole.Write(new Rule($"[gold1]Or : {_boutiqueService.Or} pièces[/]").RuleStyle("gold1"));

            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Que souhaitez-vous ?[/]")
                    .AddChoices(
                        "1. Acheter un équipement",
                        "2. Acheter des consommables",
                        "3. Vendre un équipement",
                        "4. Retour"));

            switch (choix[0])
            {
                case '1': AcheterEquipement(); break;
                case '2': AcheterConsommable(); break;
                case '3': VendreEquipement(); break;
                case '4': return;
            }
        }
    }

    private void AcheterEquipement()
    {
        var articles = _boutiqueService.ObtenirEquipements();
        var noms = articles.Select(a => $"{a.Nom} — {a.Description} ({a.Prix} or)").ToList();
        noms.Add("Retour");

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]Or : {_boutiqueService.Or} | Acheter un équipement :[/]")
                .PageSize(20)
                .AddChoices(noms));

        if (selection == "Retour") return;

        int idx = noms.IndexOf(selection);
        var article = articles[idx];

        if (_boutiqueService.Acheter(article))
        {
            // Créer l'équipement et l'ajouter au héros sélectionné
            var equipement = CreerEquipementDepuisArticle(article);
            if (equipement != null && _equipeActuelle.Count > 0)
            {
                var heroNoms = _equipeActuelle.Select(h => $"{h.Nom} ({h.Classe})").ToList();
                heroNoms.Add("Stocker (personne)");

                var choixHero = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Équiper sur quel héros ?[/]")
                        .AddChoices(heroNoms));

                if (choixHero != "Stocker (personne)")
                {
                    int hIdx = heroNoms.IndexOf(choixHero);
                    _equipeActuelle[hIdx].Equiper(equipement);
                    AnsiConsole.MarkupLine($"[green]✓ {Markup.Escape(article.Nom)} équipé sur {Markup.Escape(_equipeActuelle[hIdx].Nom)} ![/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[green]✓ {Markup.Escape(article.Nom)} acheté ![/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine($"[green]✓ {Markup.Escape(article.Nom)} acheté ![/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Pas assez d'or ![/]");
        }
    }

    private void AcheterConsommable()
    {
        var articles = _boutiqueService.ObtenirObjets();
        var noms = articles.Select(a => $"{a.Nom} — {a.Description} ({a.Prix} or)").ToList();
        noms.Add("Retour");

        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[yellow]Or : {_boutiqueService.Or} | Acheter des consommables :[/]")
                .AddChoices(noms));

        if (selection == "Retour") return;

        int idx = noms.IndexOf(selection);
        var article = articles[idx];

        int quantite = AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .Title($"[yellow]Combien ? (prix unitaire: {article.Prix} or)[/]")
                .AddChoices(1, 2, 3, 5));

        if (_boutiqueService.Acheter(article, quantite))
        {
            var objet = CreerObjetDepuisArticle(article, quantite);
            if (objet != null)
                _inventaireActuel.Ajouter(objet);
            AnsiConsole.MarkupLine($"[green]✓ {quantite}x {Markup.Escape(article.Nom)} acheté(s) ![/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Pas assez d'or ! (coût: {article.Prix * quantite})[/]");
        }
    }

    private void VendreEquipement()
    {
        if (_equipeActuelle.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Aucune équipe. Rien à vendre.[/]");
            return;
        }

        var items = new List<string>();
        var equipements = new List<(Heros hero, IEquipement equip, string slot)>();

        foreach (var h in _equipeActuelle)
        {
            if (h.Arme != null) { items.Add($"{h.Nom} → Arme: {h.Arme.Nom}"); equipements.Add((h, h.Arme, "Arme")); }
            if (h.Armure != null) { items.Add($"{h.Nom} → Armure: {h.Armure.Nom}"); equipements.Add((h, h.Armure, "Armure")); }
            if (h.Accessoire != null) { items.Add($"{h.Nom} → Accessoire: {h.Accessoire.Nom}"); equipements.Add((h, h.Accessoire, "Accessoire")); }
        }

        if (items.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Aucun équipement à vendre.[/]");
            return;
        }

        items.Add("Retour");
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Vendre quel équipement ?[/]")
                .AddChoices(items));

        if (selection == "Retour") return;

        int idx = items.IndexOf(selection);
        var (hero, equip, _) = equipements[idx];
        var equipAsEquipement = equip as Equipement;
        if (equipAsEquipement != null)
        {
            int prix = _boutiqueService.VendreEquipement(equipAsEquipement);
            // Retirer l'équipement du héros
            hero.Equiper(new Equipement("(vide)", equip.Type, Stats.Zero));
            AnsiConsole.MarkupLine($"[green]✓ {Markup.Escape(equip.Nom)} vendu pour {prix} or ![/]");
        }
    }

    private static Equipement? CreerEquipementDepuisArticle(ArticleBoutique article)
    {
        var type = article.Categorie switch
        {
            "Arme" => TypeEquipement.Arme,
            "Armure" => TypeEquipement.Armure,
            "Accessoire" => TypeEquipement.Accessoire,
            _ => TypeEquipement.Accessoire
        };

        // Parse bonus from known items
        var stats = article.Nom switch
        {
            "Épée en fer" => new Stats(0, 0, 5, 0, 0, 0, 0),
            "Bâton magique" => new Stats(0, 10, 0, 6, 0, 0, 2),
            "Dague d'ombre" => new Stats(0, 0, 3, 0, 4, 0, 0),
            "Masse sacrée" => new Stats(0, 5, 4, 3, 0, 0, 0),
            "Épée de flammes" => new Stats(0, 0, 8, 2, 0, 0, 0),
            "Arc elfique" => new Stats(0, 0, 4, 2, 6, 0, 0),
            "Faux maudite" => new Stats(0, 0, 10, 0, 0, 0, -2),
            "Armure de plates" => new Stats(10, 0, 0, 0, -2, 8, 2),
            "Robe enchantée" => new Stats(5, 15, 0, 3, 0, 2, 6),
            "Armure de cuir" => new Stats(5, 0, 0, 0, 2, 4, 2),
            "Armure de mithril" => new Stats(20, 0, 0, 0, 0, 12, 4),
            "Cape d'invisibilité" => new Stats(0, 0, 0, 0, 8, 3, 0),
            "Anneau de force" => new Stats(0, 0, 3, 0, 0, 0, 0),
            "Amulette de sagesse" => new Stats(0, 10, 0, 4, 0, 0, 3),
            "Bottes de vitesse" => new Stats(0, 0, 0, 0, 5, 0, 0),
            "Collier de vie" => new Stats(30, 0, 0, 0, 0, 2, 0),
            "Talisman élémentaire" => new Stats(0, 0, 0, 2, 0, 0, 8),
            _ => Stats.Zero
        };

        return new Equipement(article.Nom, type, stats);
    }

    private static ObjetConsommable? CreerObjetDepuisArticle(ArticleBoutique article, int quantite)
    {
        return article.Nom switch
        {
            "Potion de soin" => new ObjetConsommable("Potion de soin", "Restaure 30 PV", quantite, c => c.Soigner(30)),
            "Grande potion de soin" => new ObjetConsommable("Grande potion de soin", "Restaure 60 PV", quantite, c => c.Soigner(60)),
            "Potion de mana" => new ObjetConsommable("Potion de mana", "Restaure 20 PM", quantite, c => c.RestaurerMana(20)),
            "Grande potion de mana" => new ObjetConsommable("Grande potion de mana", "Restaure 40 PM", quantite, c => c.RestaurerMana(40)),
            "Antidote" => new ObjetConsommable("Antidote", "Soigne le poison", quantite, c => c.AppliquerStatut(StatutEffet.Aucun)),
            "Élixir de puissance" => new ObjetConsommable("Élixir de puissance", "Buff ATK temporaire", quantite, c => c.AjouterEffet(new EffetActif(StatutEffet.BuffAttaque, 3, 5))),
            _ => null
        };
    }

    // ═══ GESTION D'ÉQUIPE ═══

    private void GererEquipe()
    {
        AnsiConsole.Clear();

        if (_equipeActuelle.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]Aucune équipe créée. Utilisez l'option 1 ou 2 d'abord.[/]");
            AttendreTouche();
            return;
        }

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Equipe").Color(Color.Cyan1).Centered());

            // Tableau détaillé
            var table = new Table()
                .Border(TableBorder.Heavy)
                .BorderColor(Color.Cyan1)
                .Title("[cyan bold]═══ GESTION D'ÉQUIPE ═══[/]")
                .Expand();

            table.AddColumn(new TableColumn("[white bold]Héros[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]Classe[/]").Centered());
            table.AddColumn(new TableColumn("[gold1]Nv[/]").Centered());
            table.AddColumn(new TableColumn("[green]♥ PV[/]").Centered());
            table.AddColumn(new TableColumn("[blue]✦ PM[/]").Centered());
            table.AddColumn(new TableColumn("[red]FOR[/]").Centered());
            table.AddColumn(new TableColumn("[blue]INT[/]").Centered());
            table.AddColumn(new TableColumn("[yellow]DEF[/]").Centered());
            table.AddColumn(new TableColumn("[cyan]AGI[/]").Centered());
            table.AddColumn(new TableColumn("[mediumpurple2]RES[/]").Centered());
            table.AddColumn(new TableColumn("[grey]Arme[/]").Centered());
            table.AddColumn(new TableColumn("[grey]Armure[/]").Centered());
            table.AddColumn(new TableColumn("[grey]Acc.[/]").Centered());

            foreach (var h in _equipeActuelle)
            {
                var s = h.StatsActuelles;
                string couleurClasse = AsciiArt.ObtenirCouleurClasse(h.Classe);
                string icone = AsciiArt.ObtenirIconeClasse(h.Classe);
                string pvColor = GetPvColor(h.PointsDeVie, s.PointsDeVieMax);
                table.AddRow(
                    $"[{couleurClasse}]{icone} {Markup.Escape(h.Nom)}[/]",
                    $"[{couleurClasse}]{h.Classe}[/]",
                    $"[gold1]{h.Niveau}[/]",
                    $"[{pvColor}]{h.PointsDeVie}/{s.PointsDeVieMax}[/]",
                    $"[blue]{h.PointsDeMana}/{s.PointsDeManaMax}[/]",
                    $"[red]{s.Force}[/]",
                    $"[blue]{s.Intelligence}[/]",
                    $"[yellow]{s.Defense}[/]",
                    $"[cyan]{s.Agilite}[/]",
                    $"[mediumpurple2]{s.ResistanceMagique}[/]",
                    h.Arme != null ? Markup.Escape(h.Arme.Nom) : "[grey]—[/]",
                    h.Armure != null ? Markup.Escape(h.Armure.Nom) : "[grey]—[/]",
                    h.Accessoire != null ? Markup.Escape(h.Accessoire.Nom) : "[grey]—[/]"
                );
            }

            AnsiConsole.Write(table);

            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Actions :[/]")
                    .AddChoices(
                        "1. Voir les compétences d'un héros",
                        "2. Changer l'équipement d'un héros",
                        "3. Retour"));

            switch (choix[0])
            {
                case '1':
                    VoirCompetences();
                    break;
                case '2':
                    ChangerEquipementHeros();
                    break;
                case '3':
                    return;
            }
        }
    }

    private void VoirCompetences()
    {
        var heroNoms = _equipeActuelle.Select(h => $"{h.Nom} ({h.Classe} Nv.{h.Niveau})").ToList();
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Quel héros ?[/]")
                .AddChoices(heroNoms));

        int idx = heroNoms.IndexOf(selection);
        var hero = _equipeActuelle[idx];

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Yellow)
            .Title($"[yellow bold]Compétences de {Markup.Escape(hero.Nom)}[/]");

        table.AddColumn("Compétence");
        table.AddColumn("Type");
        table.AddColumn("Puissance");
        table.AddColumn("Coût PM");
        table.AddColumn("Cible");
        table.AddColumn("Élément");
        table.AddColumn("Effet");
        table.AddColumn("Nv requis");

        foreach (var c in hero.GetCompetences())
        {
            string typeStr = c.TypeDegat == TypeDegat.Physique ? "[red]PHY[/]" : "[blue]MAG[/]";
            string elem = c.Element != Element.Neutre ? $"{AsciiArt.ObtenirEmoji(c.Element)} {c.Element}" : "—";
            string effet = c.EffetSecondaire != StatutEffet.Aucun ? c.EffetSecondaire.ToString() : "—";
            string dispo = c.NiveauRequis <= hero.Niveau ? "" : " [grey](verrouillé)[/]";

            table.AddRow(
                $"{Markup.Escape(c.Nom)}{dispo}",
                typeStr,
                c.Puissance.ToString(),
                c.CoutMana > 0 ? c.CoutMana.ToString() : "—",
                c.Cible.ToString(),
                elem,
                effet,
                c.NiveauRequis.ToString()
            );
        }

        AnsiConsole.Write(table);
        AttendreTouche();
    }

    private void ChangerEquipementHeros()
    {
        var heroNoms = _equipeActuelle.Select(h => $"{h.Nom} ({h.Classe})").ToList();
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Équiper quel héros ?[/]")
                .AddChoices(heroNoms));

        int idx = heroNoms.IndexOf(selection);
        var hero = _equipeActuelle[idx];

        var equipements = _equipementRepo.ChargerTous();
        EquiperSlot(hero, equipements, TypeEquipement.Arme, "Arme");
        EquiperSlot(hero, equipements, TypeEquipement.Armure, "Armure");
        EquiperSlot(hero, equipements, TypeEquipement.Accessoire, "Accessoire");

        AnsiConsole.MarkupLine($"[green]✓ Équipement de {Markup.Escape(hero.Nom)} mis à jour ![/]");
        AttendreTouche();
    }

    // ═══ QUÊTES ═══

    private void AfficherQuetes()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Quetes").Color(Color.Yellow).Centered());

        var toutes = _queteService.ObtenirToutes();
        var actives = _queteService.ObtenirActives();
        var terminees = _queteService.ObtenirTerminees();

        AnsiConsole.Write(new Rule($"[yellow bold]📜 {terminees.Count}/{toutes.Count} quêtes terminées[/]").RuleStyle("yellow"));
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Heavy)
            .BorderColor(Color.Yellow)
            .Title("[yellow bold]═══ JOURNAL DE QUÊTES ═══[/]")
            .Expand();

        table.AddColumn("");
        table.AddColumn("Quête");
        table.AddColumn("Description");
        table.AddColumn("Objectif");
        table.AddColumn("Récompenses");
        table.AddColumn("Statut");

        foreach (var q in toutes)
        {
            string statut = q.Terminee
                ? $"[green]✓ {q.DateCompletion:HH:mm}[/]"
                : "[grey]En cours...[/]";
            string couleur = q.Terminee ? "green" : "white";
            string recompenses = $"[gold1]{q.RecompenseOr} or[/]";
            if (q.RecompenseXp > 0)
                recompenses += $" + [cyan]{q.RecompenseXp} XP[/]";

            table.AddRow(
                q.Icone,
                $"[{couleur}]{Markup.Escape(q.Nom)}[/]",
                $"[{couleur}]{Markup.Escape(q.Description)}[/]",
                Markup.Escape(q.Objectif),
                recompenses,
                statut
            );
        }

        AnsiConsole.Write(table);
        AttendreTouche();
    }

    private void VerifierQuetes()
    {
        var ctx = new ContexteQuete
        {
            TotalKills = _bestiaireService.TotalKills,
            TotalVictoires = _totalVictoires,
            BossVaincus = _bossVaincus,
            NiveauMaxAtteint = _equipeActuelle.Any() ? _equipeActuelle.Max(h => h.Niveau) : 0,
            VaguesArene = _vaguesAreneMax,
            DonjonProfondeur = _donjonProfondeurMax,
            DragonAncienVaincu = _bossVaincusNoms.Contains("DragonAncien"),
            LicheVaincue = _bossVaincusNoms.Contains("Liche"),
            GolemVaincu = _bossVaincusNoms.Contains("GolemCristal"),
            HydreVaincue = _bossVaincusNoms.Contains("Hydre"),
            DemonVaincu = _bossVaincusNoms.Contains("SeigneurDemon"),
        };

        _queteService.Verifier(ctx);

        foreach (var q in _queteService.NouvellesQuetesTerminees())
        {
            SoundService.Succes();
            _boutiqueService.AjouterOr(q.RecompenseOr);
            if (q.RecompenseXp > 0)
            {
                foreach (var h in _equipeActuelle.Where(h => h.EstVivant))
                    h.GagnerExperience(q.RecompenseXp);
            }

            AnsiConsole.WriteLine();
            var panel = new Panel($"[bold yellow]{q.Icone} QUÊTE TERMINÉE : {Markup.Escape(q.Nom)}[/]\n" +
                                  $"[grey]{Markup.Escape(q.Description)}[/]\n" +
                                  $"[gold1]+{q.RecompenseOr} or[/]" +
                                  (q.RecompenseXp > 0 ? $" [cyan]+{q.RecompenseXp} XP[/]" : ""))
                .Border(BoxBorder.Double)
                .BorderColor(Color.Yellow)
                .Padding(1, 0);
            AnsiConsole.Write(panel);
        }

        // Vérifier si TOUS les boss sont vaincus → victoire finale
        if (_bossVaincusNoms.Count >= 5)
        {
            AfficherVictoireFinale();
        }
    }

    // ═══ GAME OVER / VICTOIRE ═══

    private static void AfficherGameOver()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[darkred]{Markup.Escape(AsciiArt.BanniereGameOver)}[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[darkred]{Markup.Escape(AsciiArt.BanniereDefaite)}[/]");
        AnsiConsole.Write(new Panel(
            $"[darkred]{Markup.Escape(AsciiArt.Crane)}[/]\n\n" +
            "[bold darkred]Votre équipe a été vaincue...[/]\n\n" +
            "[grey]Les ténèbres engloutissent vos héros.\n" +
            "Mais tout espoir n'est pas perdu.\n" +
            "Relevez-vous et combattez à nouveau ![/]")
            .Border(BoxBorder.Double)
            .BorderColor(Color.DarkRed)
            .Padding(2, 1));
    }

    private void AfficherVictoireFinale()
    {
        AnsiConsole.Clear();
        SoundService.Victoire();
        AnsiConsole.Write(new FigletText("VICTOIRE !").Color(Color.Gold1).Centered());
        AnsiConsole.MarkupLine($"[gold1]{Markup.Escape(AsciiArt.BanniereVictoire)}[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[gold1]{Markup.Escape(AsciiArt.Couronne)}[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel(
            "[bold gold1]FÉLICITATIONS ! Vous avez vaincu tous les boss ![/]\n\n" +
            "[white]La Liche Ancienne ............ ✓\n" +
            "Le Dragon Ancien ............. ✓\n" +
            "Le Golem de Cristal .......... ✓\n" +
            "L'Hydre Venimeuse ............ ✓\n" +
            "Le Seigneur Démon ............ ✓[/]\n\n" +
            "[italic gold1]Le monde est sauvé grâce à votre courage !\n" +
            "Votre légende restera gravée dans les annales.\n" +
            "Vous êtes de véritables héros ![/]")
            .Border(BoxBorder.Double)
            .BorderColor(Color.Gold1)
            .Padding(2, 1));

        // Stats finales
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Gold1)
            .Title("[gold1 bold]Statistiques finales[/]");

        table.AddColumn("[white]Stat[/]");
        table.AddColumn("[white]Valeur[/]");

        table.AddRow("Victoires", $"[green]{_totalVictoires}[/]");
        table.AddRow("Défaites", $"[red]{_totalDefaites}[/]");
        table.AddRow("Boss vaincus", $"[gold1]{_bossVaincus}[/]");
        table.AddRow("Or accumulé", $"[gold1]{_boutiqueService.Or}[/]");
        table.AddRow("Quêtes terminées", $"[yellow]{_queteService.ObtenirTerminees().Count}[/]");
        table.AddRow("Vagues d'arène max", $"[red]{_vaguesAreneMax}[/]");
        table.AddRow("Profondeur donjon max", $"[grey]{_donjonProfondeurMax}[/]");

        if (_equipeActuelle.Any())
        {
            table.AddRow("Niveau max héros", $"[gold1]{_equipeActuelle.Max(h => h.Niveau)}[/]");
        }

        AnsiConsole.Write(table);
        AttendreTouche();
    }

    // ═══ MODE ARÈNE INFINIE ═══

    /// <summary>
    /// Lance le mode Arène Infinie où le joueur affronte des vagues d'ennemis de plus en plus forts.
    /// </summary>
    private void LancerArene()
    {
        AnsiConsole.Clear();

        if (_equipeActuelle.Count == 0 || !_equipeActuelle.Any(h => h.EstVivant))
        {
            AnsiConsole.MarkupLine("[yellow]Création automatique d'une équipe pour l'arène...[/]");
            _equipeActuelle = new List<Heros>
            {
                _personnageFactory.CreerHeros("Arthas", ClasseHeros.Guerrier),
                _personnageFactory.CreerHeros("Gandalf", ClasseHeros.Mage),
                _personnageFactory.CreerHeros("Shadow", ClasseHeros.Voleur),
                _personnageFactory.CreerHeros("Elara", ClasseHeros.Clerc),
            };

            var equipements = _equipementRepo.ChargerTous();
            _equipeActuelle[0].Equiper(equipements.First(e => e.Nom == "Épée en fer"));
            _equipeActuelle[0].Equiper(equipements.First(e => e.Nom == "Armure de plates"));
            _equipeActuelle[1].Equiper(equipements.First(e => e.Nom == "Bâton magique"));
            _equipeActuelle[1].Equiper(equipements.First(e => e.Nom == "Robe enchantée"));
            _equipeActuelle[2].Equiper(equipements.First(e => e.Nom == "Dague d'ombre"));
            _equipeActuelle[2].Equiper(equipements.First(e => e.Nom == "Armure de cuir"));
            _equipeActuelle[3].Equiper(equipements.First(e => e.Nom == "Masse sacrée"));
        }

        AnsiConsole.Write(new FigletText("ARENE").Color(Color.Red).Centered());
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(AsciiArt.BanniereArene)}[/]");
        AnsiConsole.Write(new Rule("[red bold]MODE ARÈNE INFINIE[/]").RuleStyle("red"));
        var introArene = _dialogueService.ObtenirIntroArene();
        AnsiConsole.Write(new Panel($"[italic yellow]{Markup.Escape(introArene)}[/]")
            .Border(BoxBorder.Rounded).BorderColor(Color.Yellow).Padding(1, 0));
        AnsiConsole.MarkupLine("[grey]Entre chaque vague : repos et boutique.[/]");
        AttendreTouche();

        InitialiserInventaire();
        int vague = 0;
        var random = new Random();

        while (_equipeActuelle.Any(h => h.EstVivant))
        {
            vague++;
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[red bold]⚔ VAGUE {vague} ⚔[/]").RuleStyle("red"));
            AnsiConsole.MarkupLine($"[grey]Difficulté croissante : +{vague * 10}% stats ennemies[/]");
            AnsiConsole.WriteLine();

            // Nombre de monstres croissant
            int nbMonstres = Math.Min(4, 1 + vague / 2);
            var monstres = _monstreFactory.GenererGroupeAleatoire(nbMonstres);
            AppliquerDifficulte(monstres);

            // Scaling progressif : +10% stats par vague
            foreach (var m in monstres)
            {
                m.AppliquerScaling(1.0 + vague * 0.1);
            }

            EnregistrerMonstres(monstres);
            AfficherEnnemis(monstres);
            AttendreTouche($"Vague {vague} — Appuyez sur une touche...");

            var resultat = _combatService.LancerCombat(_equipeActuelle, monstres, _inventaireActuel);
            _historiqueService.AjouterResultat(resultat);

            if (!resultat.VictoireHeros)
            {
                _totalDefaites++;
                AnsiConsole.Write(new Rule($"[darkred]ARÈNE TERMINÉE — Vague {vague}[/]").RuleStyle("darkred"));
                break;
            }

            _totalVictoires++;
            foreach (var m in monstres.Where(m => !m.EstVivant))
                _bestiaireService.EnregistrerKill(m.Nom);

            if (vague > _vaguesAreneMax) _vaguesAreneMax = vague;
            VerifierSucces();

            AfficherResultat(resultat);

            // Boutique entre les vagues
            AfficherBoutiqueArene(vague);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel($"[bold gold1]🏛 ARÈNE — Score final : Vague {vague}[/]\n[grey]Kills totaux: {_bestiaireService.TotalKills}[/]")
            .Border(BoxBorder.Double).BorderColor(Color.Gold1).Padding(2, 1));
        Sauvegarder(silencieux: true);
        AttendreTouche();
    }

    /// <summary>
    /// Propose un menu de gestion entre deux vagues d'arène (soin, repos).
    /// </summary>
    /// <param name="vague">Le numéro de la vague qui vient d'être terminée.</param>
    private void AfficherBoutiqueArene(int vague)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[gold1]🏪 BOUTIQUE D'ARÈNE[/]").RuleStyle("gold1"));

        var choix = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[yellow]Que faire entre les vagues ?[/]")
                .AddChoices(
                    "1. Repos (restaure 50% PV/PM)",
                    "2. Soins complets (restaure 100% PV/PM, +1 vague de difficulté)",
                    "3. Continuer sans repos"));

        switch (choix[0])
        {
            case '1':
                foreach (var h in _equipeActuelle.Where(h => h.EstVivant))
                {
                    h.Soigner(h.StatsActuelles.PointsDeVieMax / 2);
                    h.RestaurerMana(h.StatsActuelles.PointsDeManaMax / 2);
                }
                AnsiConsole.MarkupLine("[green]♥ L'équipe se repose... 50% PV/PM restaurés.[/]");
                break;
            case '2':
                foreach (var h in _equipeActuelle.Where(h => h.EstVivant))
                {
                    h.Soigner(h.StatsActuelles.PointsDeVieMax);
                    h.RestaurerMana(h.StatsActuelles.PointsDeManaMax);
                }
                AnsiConsole.MarkupLine("[green]✚ Soins complets ! (prochaine vague renforcée)[/]");
                break;
            case '3':
                AnsiConsole.MarkupLine("[grey]Pas de repos. En avant ![/]");
                break;
        }

        AttendreTouche();
    }

    // ═══ DONJON PROCÉDURAL ═══

    /// <summary>
    /// Lance le mode Donjon Procédural, où l'équipe explore une série de salles générées dynamiquement.
    /// </summary>
    private void LancerDonjon()
    {
        AnsiConsole.Clear();

        if (_equipeActuelle.Count == 0 || !_equipeActuelle.Any(h => h.EstVivant))
        {
            AnsiConsole.MarkupLine("[red]Vous devez d'abord créer une équipe (option 1 ou 2) ![/]");
            AttendreTouche();
            return;
        }

        int profondeur = AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .Title("[yellow]Profondeur du donjon :[/]")
                .AddChoices(5, 8, 10, 15));

        var donjon = _donjonService.GenererDonjon(profondeur);

        AnsiConsole.Write(new FigletText("DONJON").Color(Color.Grey).Centered());
        AnsiConsole.MarkupLine($"[grey]{Markup.Escape(AsciiArt.BanniereDonjon)}[/]");
        AnsiConsole.Write(new Rule("[grey bold]DONJON PROCÉDURAL[/]").RuleStyle("grey"));

        // Narration d'introduction
        var introDonjon = _dialogueService.ObtenirIntroDonjon(profondeur);
        AnsiConsole.Write(new Panel($"[italic grey]{Markup.Escape(introDonjon)}[/]")
            .Border(BoxBorder.Rounded).BorderColor(Color.Grey).Padding(1, 0));
        AnsiConsole.WriteLine();

        // Restaurer HP/PM
        foreach (var h in _equipeActuelle.Where(h => h.EstVivant))
        {
            h.Soigner(h.StatsActuelles.PointsDeVieMax);
            h.RestaurerMana(h.StatsActuelles.PointsDeManaMax);
        }

        InitialiserInventaire();

        foreach (var salle in donjon.Salles)
        {
            AnsiConsole.Clear();

            if (salle.Etage > _donjonProfondeurMax)
                _donjonProfondeurMax = salle.Etage;

            AnsiConsole.MarkupLine($"[grey]{Markup.Escape(DonjonService.GenererCarte(donjon.Salles, salle.Etage))}[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[cyan bold]Étage {salle.Etage}/{donjon.ProfondeurMax} — {Markup.Escape(salle.Nom)}[/]").RuleStyle("cyan"));

            // Narration de l'étage
            var narration = _dialogueService.ObtenirNarrationEtage(salle.Etage, salle.Type);
            AnsiConsole.Write(new Panel($"[italic]{Markup.Escape(narration)}[/]")
                .Border(BoxBorder.Rounded).BorderColor(Color.Grey).Padding(1, 0));
            AnsiConsole.WriteLine();

            switch (salle.Type)
            {
                case TypeSalle.Combat:
                    if (!TraiterSalleCombat(salle.Etage, donjon.ProfondeurMax))
                    {
                        AfficherFinDonjon(salle.Etage, donjon.ProfondeurMax);
                        return;
                    }
                    break;

                case TypeSalle.MiniBoss:
                    AnsiConsole.MarkupLine("[red bold]☠ Un mini-boss vous attend ![/]");
                    if (!TraiterSalleCombat(salle.Etage, donjon.ProfondeurMax, miniBoss: true))
                    {
                        AfficherFinDonjon(salle.Etage, donjon.ProfondeurMax);
                        return;
                    }
                    break;

                case TypeSalle.BossFinal:
                    AnsiConsole.MarkupLine("[red bold]💀 LE BOSS FINAL VOUS ATTEND ![/]");
                    var bossTypes = new[] { "Liche", "DragonAncien", "GolemCristal", "Hydre", "SeigneurDemon" };
                    var bossType = bossTypes[new Random().Next(bossTypes.Length)];
                    var boss = _monstreFactory.CreerBoss(bossType);
                    AppliquerDifficulte(new List<Monstre> { boss });

                    // Dialogue du boss de donjon
                    var dialogueDonjon = _dialogueService.ObtenirDialogueBoss(bossType);
                    AnsiConsole.Write(new Panel($"[italic red]{Markup.Escape(dialogueDonjon)}[/]")
                        .Header($"[bold red]{Markup.Escape(boss.Nom)}[/]")
                        .Border(BoxBorder.Double)
                        .BorderColor(Color.DarkRed)
                        .Padding(1, 1));

                    EnregistrerMonstres(new List<Monstre> { boss });
                    AfficherEnnemis(new List<Monstre> { boss });
                    AttendreTouche();

                    var res = _combatService.LancerCombat(_equipeActuelle, new List<Monstre> { boss }, _inventaireActuel);
                    _historiqueService.AjouterResultat(res);
                    TraiterResultat(res, new List<Monstre> { boss });

                    if (!res.VictoireHeros)
                    {
                        AfficherFinDonjon(salle.Etage, donjon.ProfondeurMax);
                        return;
                    }
                    AfficherResultat(res);
                    break;

                case TypeSalle.Evenement:
                    TraiterEvenement();
                    break;

                case TypeSalle.Repos:
                    AnsiConsole.MarkupLine("[green]♥ Salle de repos. Votre équipe récupère.[/]");
                    foreach (var h in _equipeActuelle.Where(h => h.EstVivant))
                    {
                        h.Soigner(h.StatsActuelles.PointsDeVieMax / 3);
                        h.RestaurerMana(h.StatsActuelles.PointsDeManaMax / 3);
                    }
                    AnsiConsole.MarkupLine("[green]33% PV/PM restaurés.[/]");
                    AttendreTouche();
                    break;
            }

            salle.Visitee = true;

            if (!_equipeActuelle.Any(h => h.EstVivant))
            {
                AfficherFinDonjon(salle.Etage, donjon.ProfondeurMax);
                return;
            }
        }

        // Donjon terminé avec succès
        AnsiConsole.Clear();
        SoundService.Victoire();
        AnsiConsole.Write(new Panel($"[bold gold1]🏆 DONJON TERMINÉ ! ({donjon.ProfondeurMax} étages)[/]")
            .Border(BoxBorder.Double).BorderColor(Color.Gold1).Padding(2, 1));
        VerifierSucces();
        Sauvegarder(silencieux: true);
        AttendreTouche();
    }

    /// <summary>
    /// Gère une salle de combat standard ou de mini-boss dans le donjon.
    /// </summary>
    private bool TraiterSalleCombat(int etage, int profondeurMax, bool miniBoss = false)
    {
        var random = new Random();
        int nbMonstres = miniBoss ? Math.Min(4, 2 + etage / 3) : Math.Min(4, 1 + etage / 3);
        var monstres = _monstreFactory.GenererGroupeAleatoire(nbMonstres);
        AppliquerDifficulte(monstres);

        EnregistrerMonstres(monstres);
        AfficherEnnemis(monstres);
        AttendreTouche();

        var resultat = _combatService.LancerCombat(_equipeActuelle, monstres, _inventaireActuel);
        _historiqueService.AjouterResultat(resultat);
        TraiterResultat(resultat, monstres);
        AfficherResultat(resultat);

        return resultat.VictoireHeros;
    }

    /// <summary>
    /// Gère un événement aléatoire (coffre, piège, fontaine, etc.).
    /// </summary>
    private void TraiterEvenement()
    {
        var evt = _evenementService.GenererEvenement();

        AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(evt.AsciiArt)}[/]");
        AnsiConsole.Write(new Panel($"[bold yellow]{Markup.Escape(evt.Nom)}[/]\n{Markup.Escape(evt.Description)}")
            .Border(BoxBorder.Rounded).BorderColor(Color.Yellow));

        switch (evt.Type)
        {
            case TypeEvenement.Coffre:
                _inventaireActuel.Ajouter(new ObjetConsommable("Potion de soin", "Restaure 30 PV", 3, cible => cible.Soigner(30)));
                _inventaireActuel.Ajouter(new ObjetConsommable("Potion de mana", "Restaure 20 PM", 2, cible => cible.RestaurerMana(20)));
                AnsiConsole.MarkupLine("[green]Vous trouvez 3 potions de soin et 2 potions de mana ![/]");
                break;

            case TypeEvenement.Piege:
                int degats = 15;
                foreach (var h in _equipeActuelle.Where(h => h.EstVivant))
                    h.SubirDegats(degats);
                AnsiConsole.MarkupLine($"[red]Chaque héros subit {degats} dégâts ![/]");
                break;

            case TypeEvenement.Fontaine:
                foreach (var h in _equipeActuelle.Where(h => h.EstVivant))
                {
                    h.Soigner(h.StatsActuelles.PointsDeVieMax / 2);
                    h.RestaurerMana(h.StatsActuelles.PointsDeManaMax / 2);
                }
                AnsiConsole.MarkupLine("[green]50% PV/PM restaurés ![/]");
                break;

            case TypeEvenement.Sanctuaire:
                foreach (var h in _equipeActuelle.Where(h => h.EstVivant))
                {
                    h.Soigner(h.StatsActuelles.PointsDeVieMax / 4);
                    h.RestaurerMana(h.StatsActuelles.PointsDeManaMax / 4);
                    h.AjouterEffet(new EffetActif(StatutEffet.BuffAttaque, 3, 5));
                }
                AnsiConsole.MarkupLine("[gold1]Une aura divine renforce votre équipe ! (+5 ATK pendant 3 tours, 25% PV/PM restaurés)[/]");
                break;

            case TypeEvenement.Marchand:
                AnsiConsole.MarkupLine("[yellow]Le marchand vous offre des soins ![/]");
                foreach (var h in _equipeActuelle.Where(h => h.EstVivant))
                    h.Soigner(h.StatsActuelles.PointsDeVieMax / 4);
                break;

            case TypeEvenement.Embuscade:
                AnsiConsole.MarkupLine("[red]Embuscade ! Combat immédiat ![/]");
                var monstres = _monstreFactory.GenererGroupeAleatoire(new Random().Next(2, 4));
                AppliquerDifficulte(monstres);
                EnregistrerMonstres(monstres);
                var resultat = _combatService.LancerCombat(_equipeActuelle, monstres, _inventaireActuel);
                _historiqueService.AjouterResultat(resultat);
                TraiterResultat(resultat, monstres);
                AfficherResultat(resultat);
                return;
        }

        AttendreTouche();
    }

    /// <summary>
    /// Affiche l'écran de fin de donjon (échec).
    /// </summary>
    /// <param name="etageAtteint">L'étage atteint lors de la défaite.</param>
    /// <param name="profondeurMax">La profondeur totale visée.</param>
    private static void AfficherFinDonjon(int etageAtteint, int profondeurMax)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel($"[darkred]💀 Donjon échoué à l'étage {etageAtteint}/{profondeurMax}[/]")
            .Border(BoxBorder.Double).BorderColor(Color.DarkRed).Padding(2, 1));
        AttendreTouche();
    }

    // ═══ BESTIAIRE ═══

    /// <summary>
    /// Affiche le bestiaire des monstres rencontrés.
    /// </summary>
    private void AfficherBestiaire()
    {
        AnsiConsole.Clear();
        var bestiaire = _bestiaireService.ObtenirBestiaire();

        if (bestiaire.Count == 0)
        {
            AnsiConsole.MarkupLine("[grey]Bestiaire vide. Combattez des monstres pour les découvrir ![/]");
            AttendreTouche();
            return;
        }

        AnsiConsole.Write(new FigletText("Bestiaire").Color(Color.Red).Centered());
        AnsiConsole.Write(new Rule($"[red bold]📖 Monstres rencontrés : {bestiaire.Count}[/]").RuleStyle("red"));
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Heavy)
            .BorderColor(Color.Red)
            .Title("[red bold]═══ BESTIAIRE ═══[/]")
            .Expand();

        table.AddColumn("Monstre");
        table.AddColumn("PV");
        table.AddColumn("FOR");
        table.AddColumn("DEF");
        table.AddColumn("XP");
        table.AddColumn("Kills");
        table.AddColumn("Faiblesses");

        foreach (var e in bestiaire)
        {
            var faiblesses = string.Join(" ", e.Faiblesses
                .Where(f => f.Value > 1.0)
                .Select(f => $"{AsciiArt.ObtenirEmoji(f.Key)}x{f.Value:F1}"));

            table.AddRow(
                $"[red]{Markup.Escape(e.Nom)}[/]",
                e.PvMax.ToString(),
                e.Force.ToString(),
                e.Defense.ToString(),
                $"[gold1]{e.Xp}[/]",
                $"[grey]{e.NombreKills}[/]",
                faiblesses
            );
        }

        AnsiConsole.Write(table);

        // Afficher ASCII art du monstre sélectionné
        if (bestiaire.Count > 0)
        {
            var choix = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[grey]Voir le détail d'un monstre :[/]")
                    .AddChoices(bestiaire.Select(e => e.Nom).Append("Retour").ToArray()));

            if (choix != "Retour")
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(AsciiArt.Obtenir(choix))}[/]");
                AttendreTouche();
            }
        }
        else
        {
            AttendreTouche();
        }
    }

    // ═══ SUCCÈS / TROPHÉES ═══

    /// <summary>
    /// Affiche les succès débloqués et verrouillés.
    /// </summary>
    private void AfficherSucces()
    {
        AnsiConsole.Clear();
        var tous = _succesService.ObtenirTous();
        var debloques = _succesService.ObtenirDebloques();

        AnsiConsole.Write(new FigletText("Succes").Color(Color.Gold1).Centered());
        AnsiConsole.Write(new Rule($"[gold1 bold]🏆 {debloques.Count}/{tous.Count} succès débloqués[/]").RuleStyle("gold1"));
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.Heavy)
            .BorderColor(Color.Gold1)
            .Title("[gold1 bold]═══ SUCCÈS ET TROPHÉES ═══[/]")
            .Expand();

        table.AddColumn("");
        table.AddColumn("Succès");
        table.AddColumn("Description");
        table.AddColumn("Statut");

        foreach (var s in tous)
        {
            string statut = s.Debloque
                ? $"[green]✓ {s.DateDeblocage:HH:mm}[/]"
                : "[grey]🔒 Verrouillé[/]";
            string couleur = s.Debloque ? "gold1" : "grey";

            table.AddRow(
                s.Icone,
                $"[{couleur}]{Markup.Escape(s.Nom)}[/]",
                $"[{couleur}]{Markup.Escape(s.Description)}[/]",
                statut
            );
        }

        AnsiConsole.Write(table);
        AttendreTouche();
    }

    /// <summary>
    /// Initialise un inventaire de base pour les héros.
    /// </summary>
    private void InitialiserInventaire()
    {
        _inventaireActuel = new Inventaire();
        foreach (var objet in _objetRepo.ChargerTous())
            _inventaireActuel.Ajouter(objet);
    }

    /// <summary>
    /// Affiche un message et attend que l'utilisateur appuie sur une touche.
    /// </summary>
    /// <param name="message">Message personnalisé (optionnel).</param>
    private static void AttendreTouche(string? message = null)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[grey italic]{message ?? "▶ Appuyez sur une touche pour continuer..."}[/]");
        Console.ReadKey(true);
    }

    // ═══ DIFFICULTÉ ═══

    /// <summary>
    /// Tente de charger automatiquement la sauvegarde par défaut si elle existe.
    /// </summary>
    private void ChargerAutomatique()
    {
        if (!_sauvegardeService.SauvegardeExiste(CheminSauvegarde))
            return;

        var donnees = _sauvegardeService.Charger(CheminSauvegarde);
        if (donnees == null)
            return;

        _equipeActuelle = new List<Heros>();
        foreach (var hs in donnees.Heros)
        {
            var classe = Enum.Parse<ClasseHeros>(hs.Classe);
            var stats = new Domain.ValueObjects.Stats(hs.PvMax, hs.PmMax, hs.Force,
                hs.Intelligence, hs.Agilite, hs.Defense, hs.ResistanceMagique);
            var hero = new Heros(hs.Nom, classe, stats, hs.Niveau);

            var tempHero = _personnageFactory.CreerHeros(hs.Nom, classe);
            foreach (var comp in tempHero.GetCompetences())
                hero.AjouterCompetence(comp);
            hero.Resistances = tempHero.Resistances;

            if (hs.Experience > 0)
                hero.GagnerExperience(hs.Experience);

            _equipeActuelle.Add(hero);
        }

        _historiqueService.Restaurer(donnees.Historique);
        _bestiaireService.Restaurer(donnees.Bestiaire);
        _succesService.Restaurer(donnees.Succes);
        _totalVictoires = donnees.TotalVictoires;
        _totalDefaites = donnees.TotalDefaites;
        _bossVaincus = donnees.BossVaincus;
        _vaguesAreneMax = donnees.VaguesAreneMax;
        _donjonProfondeurMax = donnees.DonjonProfondeurMax;
        _boutiqueService.Restaurer(donnees.Or);
        _queteService.Restaurer(donnees.Quetes);
        _bossVaincusNoms.Clear();
        foreach (var nom in donnees.BossVaincusNoms)
            _bossVaincusNoms.Add(nom);
        if (Enum.TryParse<Difficulte>(donnees.Difficulte, out var diff))
            _difficulte = diff;
    }

    /// <summary>
    /// Permet au joueur de choisir le niveau de difficulté.
    /// </summary>
    private void ChoisirDifficulte()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[yellow bold]DIFFICULTÉ[/]").RuleStyle("yellow"));

        var labels = new Dictionary<Difficulte, string>
        {
            [Difficulte.Facile] = "🟢 Facile — Monstres x0.8, XP x1.2 (Idéal pour débutants)",
            [Difficulte.Normal] = "🟡 Normal — Équilibré (L'expérience prévue)",
            [Difficulte.Difficile] = "🔴 Difficile — Monstres x1.3, XP x1.3 (Pour stratèges)",
            [Difficulte.Cauchemar] = "💀 Cauchemar — Monstres x1.6, XP x1.8 (Risque mortel)"
        };

        var choix = AnsiConsole.Prompt(
            new SelectionPrompt<Difficulte>()
                .Title($"[yellow]Difficulté actuelle : {_difficulte}[/]")
                .AddChoices(Enum.GetValues<Difficulte>())
                .UseConverter(d => labels[d]));

        _difficulte = choix;
        Sauvegarder(silencieux: true);
        AnsiConsole.MarkupLine($"[green]✓ Difficulté changée : {_difficulte}[/]");
        AttendreTouche();
    }

    /// <summary>
    /// Affiche le niveau de difficulté actuel.
    /// </summary>
    private void AfficherDifficulte()
    {
        string diffEmoji = _difficulte switch
        {
            Difficulte.Facile => "🟢",
            Difficulte.Normal => "🟡",
            Difficulte.Difficile => "🔴",
            Difficulte.Cauchemar => "💀",
            _ => ""
        };
        AnsiConsole.MarkupLine($"[grey]Difficulté : {diffEmoji} {_difficulte}[/]");
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Obtient les multiplicateurs de stats et d'XP selon la difficulté.
    /// </summary>
    /// <returns>Tuple (multiplicateurStats, multiplicateurXP).</returns>
    private (double stats, double xp) ObtenirMultiplicateurs() => _difficulte switch
    {
        Difficulte.Facile => (0.8, 1.2),      // Légèrement plus facile, XP modéré
        Difficulte.Normal => (1.0, 1.0),      // Standard
        Difficulte.Difficile => (1.3, 1.3),   // Challenge (+30%), Récompense (+30%)
        Difficulte.Cauchemar => (1.6, 1.8),   // Hardcore (+60%), Récompense (+80%)
        _ => (1.0, 1.0)
    };

    /// <summary>
    /// Applique les multiplicateurs de difficulté à une liste de monstres.
    /// </summary>
    /// <param name="monstres">La liste des monstres à ajuster.</param>
    private void AppliquerDifficulte(List<Monstre> monstres)
    {
        var (multStats, multXP) = ObtenirMultiplicateurs();
        foreach (var m in monstres)
            m.AppliquerDifficulte(multStats, multXP);
    }
}
