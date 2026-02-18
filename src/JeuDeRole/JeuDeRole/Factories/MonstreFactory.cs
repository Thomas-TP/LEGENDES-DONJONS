using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.ValueObjects;
using JeuDeRole.Strategies.IA;

namespace JeuDeRole.Factories;

/// <summary>
/// Implémentation de la fabrique de monstres.
/// Contient les configurations (stats, compétences, résistances) de tous les ennemis du jeu.
/// Agit comme un "Bestiaire" codé en dur.
/// </summary>
public class MonstreFactory : IMonstreFactory
{
    private readonly Random _random;

    // Liste des types de monstres "mob" disponibles pour la génération aléatoire
    private static readonly string[] TypesMonstres = { "Gobelin", "Squelette", "Loup", "Orc", "Spectre", "Golem de Pierre", "Serpent de Feu", "Minotaure", "Harpie" };

    public MonstreFactory(Random? random = null)
    {
        _random = random ?? new Random();
    }

    /// <summary>
    /// Crée un monstre en récupérant sa configuration.
    /// Assigne aussi les compétences et les résistances élémentaires.
    /// </summary>
    public Monstre CreerMonstre(string type)
    {
        var (stats, xp, strategieIA, competences, resistances) = ObtenirConfigMonstre(type);
        var monstre = new Monstre(type, stats, xp, strategieIA);
        monstre.Resistances = resistances;

        foreach (var competence in competences)
        {
            monstre.AjouterCompetence(competence);
        }

        return monstre;
    }

    /// <summary>
    /// Génère un groupe de monstres pour une rencontre.
    /// Limite le nombre de monstres entre 1 et 4 pour l'équilibrage.
    /// </summary>
    public List<Monstre> GenererGroupeAleatoire(int nombre)
    {
        nombre = Math.Clamp(nombre, 1, 4);
        var monstres = new List<Monstre>();

        for (int i = 0; i < nombre; i++)
        {
            var type = TypesMonstres[_random.Next(TypesMonstres.Length)];
            monstres.Add(CreerMonstre(type));
        }

        return monstres;
    }

    /// <summary>
    /// Configuration statique des monstres (Tuples C# pour simplifier l'écriture).
    /// Définit : Stats de base, XP donnée, Stratégie IA, Compétences, Résistances.
    /// </summary>
    private (Stats stats, int xp, IStrategieIA ia, List<Competence> competences, Dictionary<Element, double> resistances) ObtenirConfigMonstre(string type) => type switch
    {
        "Gobelin" => (
            new Stats(70, 25, 14, 10, 18, 10, 5),
            35,
            new IAAleatoire(_random), // IA bête
            new List<Competence>
            {
                new("Morsure", 0, 12, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Coup de dague", 4, 18, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Bombe incendiaire", 6, 14, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Brulure, dureeEffet: 2, element: Element.Feu),
            },
            new Dictionary<Element, double> { [Element.Feu] = 1.5, [Element.Lumiere] = 1.3 } // Faible au Feu
        ),

        "Squelette" => (
            new Stats(85, 20, 16, 8, 10, 14, 5),
            40,
            new IAAleatoire(_random),
            new List<Competence>
            {
                new("Coup d'épée rouillée", 0, 14, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Charge osseuse", 4, 18, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.Paralysie, dureeEffet: 1),
                new("Toucher glacial", 5, 16, TypeDegat.Magique, CibleType.UnEnnemi, StatutEffet.Gel, dureeEffet: 1, element: Element.Glace),
            },
            new Dictionary<Element, double> { [Element.Lumiere] = 2.0, [Element.Tenebres] = 0.3, [Element.Glace] = 0.5 }
        ),

        "Loup" => (
            new Stats(65, 0, 20, 5, 22, 8, 5),
            30,
            new IACiblee(), // IA qui focus le faible
            new List<Competence>
            {
                new("Morsure féroce", 0, 14, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Griffe déchirante", 0, 10, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.Poison, dureeEffet: 3),
                new("Hurlement", 0, 8, TypeDegat.Physique, CibleType.TousLesEnnemis, StatutEffet.DebuffDefense, dureeEffet: 2),
            },
            new Dictionary<Element, double> { [Element.Feu] = 1.5, [Element.Glace] = 1.3 }
        ),

        "Orc" => (
            new Stats(140, 25, 22, 6, 10, 16, 8),
            55,
            new IACiblee(),
            new List<Competence>
            {
                new("Coup de massue", 0, 18, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Frappe brutale", 5, 24, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Cri de guerre", 4, 12, TypeDegat.Physique, CibleType.TousLesEnnemis, StatutEffet.DebuffDefense, dureeEffet: 2),
                new("Charge dévastatrice", 8, 26, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.Paralysie, dureeEffet: 1),
            },
            new Dictionary<Element, double> { [Element.Foudre] = 1.5, [Element.Lumiere] = 1.3 }
        ),

        "Dragon" => (
            new Stats(300, 80, 28, 20, 12, 24, 18),
            120,
            new IACiblee(),
            new List<Competence>
            {
                new("Griffe draconique", 0, 20, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Souffle de feu", 14, 28, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Brulure, dureeEffet: 3, element: Element.Feu),
                new("Morsure venimeuse", 10, 22, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.Poison, dureeEffet: 4),
                new("Souffle glacé", 16, 26, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Gel, dureeEffet: 1, element: Element.Glace),
                new("Queue foudroyante", 12, 26, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Paralysie, dureeEffet: 1, element: Element.Foudre),
            },
            new Dictionary<Element, double> { [Element.Feu] = 0.2, [Element.Glace] = 1.8, [Element.Foudre] = 0.5 }
        ),

        "Spectre" => (
            new Stats(70, 60, 10, 18, 18, 6, 18),
            45,
            new IACiblee(),
            new List<Competence>
            {
                new("Toucher spectral", 0, 14, TypeDegat.Magique, CibleType.UnEnnemi, element: Element.Tenebres),
                new("Drain d'âme", 8, 18, TypeDegat.Magique, CibleType.UnEnnemi, StatutEffet.Sommeil, dureeEffet: 1, element: Element.Tenebres),
                new("Lamentation", 10, 12, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.DebuffDefense, dureeEffet: 2, element: Element.Tenebres),
            },
            new Dictionary<Element, double> { [Element.Tenebres] = 0.2, [Element.Lumiere] = 2.5, [Element.Feu] = 0.8 }
        ),

        "Golem de Pierre" => (
            new Stats(200, 15, 20, 6, 6, 30, 12),
            60,
            new IAAleatoire(_random),
            new List<Competence>
            {
                new("Poing de pierre", 0, 22, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Séisme", 4, 16, TypeDegat.Physique, CibleType.TousLesEnnemis, StatutEffet.Paralysie, dureeEffet: 1),
                new("Lancer de rocher", 3, 20, TypeDegat.Physique, CibleType.UnEnnemi),
            },
            new Dictionary<Element, double> { [Element.Foudre] = 0.5, [Element.Feu] = 0.7, [Element.Glace] = 1.5, [Element.Lumiere] = 1.3 }
        ),

        "Serpent de Feu" => (
            new Stats(90, 45, 14, 16, 20, 10, 12),
            50,
            new IACiblee(),
            new List<Competence>
            {
                new("Croc venimeux", 0, 14, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.Poison, dureeEffet: 3),
                new("Crachat de flammes", 6, 20, TypeDegat.Magique, CibleType.UnEnnemi, StatutEffet.Brulure, dureeEffet: 2, element: Element.Feu),
                new("Constriction brûlante", 8, 16, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.Paralysie, dureeEffet: 1, element: Element.Feu),
            },
            new Dictionary<Element, double> { [Element.Feu] = 0.2, [Element.Glace] = 2.0, [Element.Foudre] = 1.3 }
        ),

        "Minotaure" => (
            new Stats(140, 20, 24, 8, 10, 18, 8),
            55,
            new IACiblee(),
            new List<Competence>
            {
                new("Coup de cornes", 0, 18, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Charge furieuse", 5, 24, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.Paralysie, dureeEffet: 1),
                new("Piétinement", 6, 16, TypeDegat.Physique, CibleType.TousLesEnnemis),
            },
            new Dictionary<Element, double> { [Element.Feu] = 1.3, [Element.Foudre] = 1.5, [Element.Lumiere] = 1.3 }
        ),

        "Harpie" => (
            new Stats(80, 35, 14, 14, 26, 8, 10),
            40,
            new IACiblee(),
            new List<Competence>
            {
                new("Griffes acérées", 0, 14, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Cri perçant", 5, 10, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Paralysie, dureeEffet: 1),
                new("Piqué aérien", 6, 20, TypeDegat.Physique, CibleType.UnEnnemi, element: Element.Foudre),
                new("Vent tranchant", 8, 14, TypeDegat.Magique, CibleType.TousLesEnnemis, element: Element.Foudre),
            },
            new Dictionary<Element, double> { [Element.Foudre] = 0.5, [Element.Glace] = 1.8, [Element.Feu] = 1.3 }
        ),

        _ => throw new ArgumentException($"Type de monstre inconnu : {type}", nameof(type))
    };

    /// <summary>
    /// Crée un boss avec ses configurations complèxes par phase.
    /// </summary>
    public Boss CreerBoss(string type) => type switch
    {
        "Liche" => CreerBossLiche(),
        "DragonAncien" => CreerBossDragonAncien(),
        "GolemCristal" => CreerBossGolemCristal(),
        "Hydre" => CreerBossHydre(),
        "SeigneurDemon" => CreerBossSeigneurDemon(),
        _ => throw new ArgumentException($"Type de boss inconnu : {type}", nameof(type))
    };

    private Boss CreerBossLiche()
    {
        var statsParPhase = new Dictionary<int, Stats>
        {
            [1] = new Stats(500, 150, 14, 30, 12, 18, 25),
            [2] = new Stats(500, 200, 16, 38, 14, 15, 30),
        };
        var competencesParPhase = new Dictionary<int, List<Competence>>
        {
            [1] = new()
            {
                new("Drain de vie", 12, 28, TypeDegat.Magique, CibleType.UnEnnemi, element: Element.Tenebres),
                new("Rayon de mort", 22, 38, TypeDegat.Magique, CibleType.UnEnnemi, StatutEffet.Poison, dureeEffet: 4, element: Element.Tenebres),
                new("Vague de ténèbres", 28, 26, TypeDegat.Magique, CibleType.TousLesEnnemis, element: Element.Tenebres),
                new("Malédiction", 15, 5, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.DebuffDefense, dureeEffet: 3, element: Element.Tenebres),
            },
            [2] = new()
            {
                new("Sommeil éternel", 18, 0, TypeDegat.Magique, CibleType.UnEnnemi, StatutEffet.Sommeil, dureeEffet: 3, element: Element.Tenebres),
                new("Apocalypse nécrotique", 45, 52, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Poison, dureeEffet: 3, element: Element.Tenebres),
                new("Mort subite", 30, 60, TypeDegat.Magique, CibleType.UnEnnemi, element: Element.Tenebres),
            },
        };
        var iaParPhase = new Dictionary<int, IStrategieIA>
        {
            [1] = new IACiblee(),
            [2] = new IACiblee(),
        };
        var boss = new Boss("Liche Ancienne", statsParPhase, competencesParPhase, iaParPhase, 300);
        boss.Resistances = new Dictionary<Element, double>
        {
            [Element.Tenebres] = 0.1, [Element.Lumiere] = 2.0,
            [Element.Glace] = 0.5, [Element.Feu] = 0.7
        };
        return boss;
    }

    private Boss CreerBossDragonAncien()
    {
        var statsParPhase = new Dictionary<int, Stats>
        {
            [1] = new Stats(800, 120, 30, 26, 8, 26, 22),
            [2] = new Stats(800, 160, 36, 32, 10, 24, 26),
            [3] = new Stats(800, 200, 42, 38, 12, 22, 30),
        };
        var competencesParPhase = new Dictionary<int, List<Competence>>
        {
            [1] = new()
            {
                new("Griffe titanesque", 0, 28, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Souffle infernal", 22, 42, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Brulure, dureeEffet: 3, element: Element.Feu),
            },
            [2] = new()
            {
                new("Frappe de queue", 12, 32, TypeDegat.Physique, CibleType.TousLesEnnemis),
                new("Rugissement terrifiant", 15, 8, TypeDegat.Physique, CibleType.TousLesEnnemis, StatutEffet.DebuffDefense, dureeEffet: 2),
                new("Orage draconique", 25, 36, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Paralysie, dureeEffet: 1, element: Element.Foudre),
            },
            [3] = new()
            {
                new("Cataclysme draconique", 45, 58, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Brulure, dureeEffet: 3, element: Element.Feu),
                new("Morsure fatale", 0, 50, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.Poison, dureeEffet: 5),
                new("Souffle du néant", 35, 45, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Gel, dureeEffet: 1, element: Element.Glace),
            },
        };
        var iaParPhase = new Dictionary<int, IStrategieIA>
        {
            [1] = new IAAleatoire(_random),
            [2] = new IACiblee(),
            [3] = new IACiblee(),
        };
        var boss = new Boss("Dragon Ancien", statsParPhase, competencesParPhase, iaParPhase, 600);
        boss.Resistances = new Dictionary<Element, double>
        {
            [Element.Feu] = 0.1, [Element.Glace] = 1.8,
            [Element.Foudre] = 0.3, [Element.Lumiere] = 1.3
        };
        return boss;
    }

    private Boss CreerBossGolemCristal()
    {
        var statsParPhase = new Dictionary<int, Stats>
        {
            [1] = new Stats(600, 30, 28, 10, 4, 35, 20),
            [2] = new Stats(600, 50, 35, 16, 6, 30, 25),
        };
        var competencesParPhase = new Dictionary<int, List<Competence>>
        {
            [1] = new()
            {
                new("Poing cristallin", 0, 26, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Éclats de cristal", 8, 20, TypeDegat.Physique, CibleType.TousLesEnnemis),
                new("Mur de pierre", 5, 0, TypeDegat.Physique, CibleType.Soi, StatutEffet.BuffAttaque, dureeEffet: 2),
                new("Séisme dévastateur", 12, 30, TypeDegat.Physique, CibleType.TousLesEnnemis, StatutEffet.Paralysie, dureeEffet: 1),
            },
            [2] = new()
            {
                new("Charge cristalline", 10, 40, TypeDegat.Physique, CibleType.UnEnnemi),
                new("Explosion de cristaux", 20, 35, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.DebuffDefense, dureeEffet: 2, element: Element.Lumiere),
                new("Emprisonnement de pierre", 15, 28, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.Paralysie, dureeEffet: 2),
            },
        };
        var iaParPhase = new Dictionary<int, IStrategieIA>
        {
            [1] = new IAAleatoire(_random),
            [2] = new IACiblee(),
        };
        var boss = new Boss("Golem de Cristal", statsParPhase, competencesParPhase, iaParPhase, 400);
        boss.Resistances = new Dictionary<Element, double>
        {
            [Element.Foudre] = 0.3, [Element.Feu] = 0.7,
            [Element.Glace] = 1.5, [Element.Lumiere] = 1.5
        };
        return boss;
    }

    private Boss CreerBossHydre()
    {
        var statsParPhase = new Dictionary<int, Stats>
        {
            [1] = new Stats(550, 80, 22, 18, 10, 20, 16),
            [2] = new Stats(550, 120, 28, 24, 12, 18, 20),
            [3] = new Stats(550, 160, 34, 30, 14, 16, 24),
        };
        var competencesParPhase = new Dictionary<int, List<Competence>>
        {
            [1] = new()
            {
                new("Triple morsure", 0, 22, TypeDegat.Physique, CibleType.TousLesEnnemis, StatutEffet.Poison, dureeEffet: 3),
                new("Crachat acide", 10, 26, TypeDegat.Magique, CibleType.UnEnnemi, StatutEffet.DebuffDefense, dureeEffet: 3),
                new("Queue fouettante", 5, 18, TypeDegat.Physique, CibleType.TousLesEnnemis),
            },
            [2] = new()
            {
                new("Régénération", 15, 30, TypeDegat.Magique, CibleType.Soi),
                new("Souffle toxique", 18, 24, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Poison, dureeEffet: 4),
                new("Morsure dévorante", 8, 35, TypeDegat.Physique, CibleType.UnEnnemi),
            },
            [3] = new()
            {
                new("Tempête de morsures", 20, 30, TypeDegat.Physique, CibleType.TousLesEnnemis, StatutEffet.Poison, dureeEffet: 4),
                new("Constriction fatale", 15, 45, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.Paralysie, dureeEffet: 2),
                new("Bain d'acide", 25, 38, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.DebuffDefense, dureeEffet: 3),
            },
        };
        var iaParPhase = new Dictionary<int, IStrategieIA>
        {
            [1] = new IAAleatoire(_random),
            [2] = new IAAleatoire(_random),
            [3] = new IACiblee(),
        };
        var boss = new Boss("Hydre Venimeuse", statsParPhase, competencesParPhase, iaParPhase, 450);
        boss.Resistances = new Dictionary<Element, double>
        {
            [Element.Feu] = 1.8, [Element.Glace] = 0.5,
            [Element.Lumiere] = 1.3, [Element.Tenebres] = 0.7
        };
        return boss;
    }

    private Boss CreerBossSeigneurDemon()
    {
        var statsParPhase = new Dictionary<int, Stats>
        {
            [1] = new Stats(700, 180, 26, 32, 14, 22, 28),
            [2] = new Stats(700, 220, 32, 40, 16, 20, 32),
            [3] = new Stats(700, 260, 40, 48, 18, 18, 36),
        };
        var competencesParPhase = new Dictionary<int, List<Competence>>
        {
            [1] = new()
            {
                new("Griffes démoniaques", 0, 24, TypeDegat.Physique, CibleType.UnEnnemi, element: Element.Tenebres),
                new("Flamme infernale", 18, 32, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Brulure, dureeEffet: 3, element: Element.Feu),
                new("Malédiction démoniaque", 15, 8, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.DebuffDefense, dureeEffet: 3, element: Element.Tenebres),
                new("Pacte de sang", 12, 0, TypeDegat.Magique, CibleType.Soi, StatutEffet.BuffAttaque, dureeEffet: 3),
            },
            [2] = new()
            {
                new("Chaos infernal", 25, 38, TypeDegat.Magique, CibleType.TousLesEnnemis, element: Element.Feu),
                new("Emprise des ténèbres", 20, 42, TypeDegat.Magique, CibleType.UnEnnemi, StatutEffet.Sommeil, dureeEffet: 2, element: Element.Tenebres),
                new("Drain démoniaque", 15, 30, TypeDegat.Magique, CibleType.UnEnnemi, element: Element.Tenebres),
            },
            [3] = new()
            {
                new("Apocalypse", 40, 55, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.Brulure, dureeEffet: 3, element: Element.Feu),
                new("Jugement des Enfers", 30, 60, TypeDegat.Magique, CibleType.UnEnnemi, element: Element.Tenebres),
                new("Porte des Enfers", 35, 45, TypeDegat.Magique, CibleType.TousLesEnnemis, StatutEffet.DebuffDefense, dureeEffet: 4, element: Element.Tenebres),
            },
        };
        var iaParPhase = new Dictionary<int, IStrategieIA>
        {
            [1] = new IAAleatoire(_random),
            [2] = new IACiblee(),
            [3] = new IACiblee(),
        };
        var boss = new Boss("Seigneur Démon", statsParPhase, competencesParPhase, iaParPhase, 700);
        boss.Resistances = new Dictionary<Element, double>
        {
            [Element.Tenebres] = 0.1, [Element.Lumiere] = 2.0,
            [Element.Feu] = 0.2, [Element.Glace] = 1.3, [Element.Foudre] = 0.8
        };
        return boss;
    }
}
