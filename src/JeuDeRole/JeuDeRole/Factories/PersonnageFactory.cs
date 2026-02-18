using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.ValueObjects;

namespace JeuDeRole.Factories;

/// <summary>
/// Implémentation de la fabrique de personnages (Héros).
/// Centralise la création des héros en fonction de leur classe (Guerrier, Mage, etc.).
/// Définit les statistiques de départ, les résistances et les compétences initiales pour chaque classe.
/// </summary>
public class PersonnageFactory : IPersonnageFactory
{
    /// <summary>
    /// Crée un nouveau héros avec le nom et la classe spécifiés.
    /// Initialise ses stats, ses résistances et ses compétences selon la configuration de la classe.
    /// </summary>
    /// <param name="nom">Le nom du héros.</param>
    /// <param name="classe">La classe choisie (ex: Guerrier, Mage).</param>
    /// <returns>Une instance de Heros prête à l'emploi.</returns>
    public Heros CreerHeros(string nom, ClasseHeros classe)
    {
        var stats = ObtenirStatsParClasse(classe);
        var heros = new Heros(nom, classe, stats);
        heros.Resistances = ObtenirResistancesParClasse(classe);

        foreach (var competence in ObtenirCompetencesParClasse(classe))
        {
            heros.AjouterCompetence(competence);
        }

        return heros;
    }

    /// <summary>
    /// Renvoie les statistiques de base pour chaque classe.
    /// C'est ici qu'on équilibre les PV, le Mana, la Force, etc. entre les classes.
    /// </summary>
    private static Stats ObtenirStatsParClasse(ClasseHeros classe) => classe switch
    {
        ClasseHeros.Guerrier => new Stats(
            pointsDeVieMax: 120, pointsDeManaMax: 20,
            force: 18, intelligence: 6, agilite: 10,
            defense: 15, resistanceMagique: 8),

        ClasseHeros.Mage => new Stats(
            pointsDeVieMax: 70, pointsDeManaMax: 80,
            force: 5, intelligence: 20, agilite: 12,
            defense: 6, resistanceMagique: 16),

        ClasseHeros.Voleur => new Stats(
            pointsDeVieMax: 85, pointsDeManaMax: 30,
            force: 12, intelligence: 10, agilite: 20,
            defense: 9, resistanceMagique: 10),

        ClasseHeros.Clerc => new Stats(
            pointsDeVieMax: 90, pointsDeManaMax: 60,
            force: 8, intelligence: 16, agilite: 10,
            defense: 10, resistanceMagique: 14),

        ClasseHeros.Paladin => new Stats(
            pointsDeVieMax: 130, pointsDeManaMax: 40,
            force: 14, intelligence: 12, agilite: 8,
            defense: 18, resistanceMagique: 12),

        ClasseHeros.Necromancien => new Stats(
            pointsDeVieMax: 75, pointsDeManaMax: 70,
            force: 6, intelligence: 22, agilite: 10,
            defense: 7, resistanceMagique: 14),

        ClasseHeros.Assassin => new Stats(
            pointsDeVieMax: 80, pointsDeManaMax: 35,
            force: 16, intelligence: 8, agilite: 24,
            defense: 8, resistanceMagique: 8),

        ClasseHeros.Druide => new Stats(
            pointsDeVieMax: 95, pointsDeManaMax: 55,
            force: 10, intelligence: 16, agilite: 12,
            defense: 11, resistanceMagique: 13),

        _ => throw new ArgumentOutOfRangeException(nameof(classe))
    };

    /// <summary>
    /// Définit les résistances et faiblesses élémentaires de chaque classe.
    /// > 1.0 = Faiblesse (reçoit plus de dégâts).
    /// < 1.0 = Résistance (reçoit moins de dégâts).
    /// </summary>
    private static Dictionary<Element, double> ObtenirResistancesParClasse(ClasseHeros classe) => classe switch
    {
        ClasseHeros.Guerrier => new()
        {
            [Element.Feu] = 0.9, [Element.Foudre] = 1.2
        },
        ClasseHeros.Mage => new()
        {
            [Element.Feu] = 0.5, [Element.Glace] = 0.5, [Element.Tenebres] = 1.5
        },
        ClasseHeros.Voleur => new()
        {
            [Element.Tenebres] = 0.5, [Element.Lumiere] = 1.5
        },
        ClasseHeros.Clerc => new()
        {
            [Element.Lumiere] = 0.3, [Element.Tenebres] = 0.5, [Element.Feu] = 1.4
        },
        ClasseHeros.Paladin => new()
        {
            [Element.Lumiere] = 0.3, [Element.Tenebres] = 0.5, [Element.Feu] = 0.8
        },
        ClasseHeros.Necromancien => new()
        {
            [Element.Tenebres] = 0.2, [Element.Lumiere] = 2.0, [Element.Glace] = 0.7
        },
        ClasseHeros.Assassin => new()
        {
            [Element.Tenebres] = 0.5, [Element.Lumiere] = 1.5, [Element.Foudre] = 0.8
        },
        ClasseHeros.Druide => new()
        {
            [Element.Feu] = 0.7, [Element.Glace] = 0.7, [Element.Foudre] = 0.7, [Element.Lumiere] = 0.8
        },
        _ => new()
    };

    /// <summary>
    /// Liste des compétences (attaques physiques) et sorts (magie) de départ.
    /// </summary>
    private static List<Competence> ObtenirCompetencesParClasse(ClasseHeros classe) => classe switch
    {
        ClasseHeros.Guerrier => new List<Competence>
        {
            new("Attaque", 0, 10, TypeDegat.Physique, CibleType.UnEnnemi),
            new("Coup Puissant", 5, 20, TypeDegat.Physique, CibleType.UnEnnemi),
            new("Coup Critique", 10, 30, TypeDegat.Physique, CibleType.UnEnnemi),
            new("Tourbillon", 15, 22, TypeDegat.Physique, CibleType.TousLesEnnemis, niveauRequis: 3),
            new("Cri de Guerre", 8, 0, TypeDegat.Physique, CibleType.Soi, StatutEffet.BuffAttaque, dureeEffet: 3, niveauRequis: 5),
        },

        ClasseHeros.Mage => new List<Competence>
        {
            new("Attaque", 0, 5, TypeDegat.Physique, CibleType.UnEnnemi),
            new Sort("Boule de Feu", 15, 25, TypeDegat.Magique, CibleType.UnEnnemi, "Évocation", StatutEffet.Brulure, dureeEffet: 3, element: Element.Feu),
            new Sort("Blizzard", 25, 18, TypeDegat.Magique, CibleType.TousLesEnnemis, "Évocation", StatutEffet.Gel, dureeEffet: 1, element: Element.Glace),
            new Sort("Éclair", 10, 20, TypeDegat.Magique, CibleType.UnEnnemi, "Évocation", StatutEffet.Paralysie, dureeEffet: 1, element: Element.Foudre),
            new Sort("Sommeil", 12, 0, TypeDegat.Magique, CibleType.UnEnnemi, "Enchantement", StatutEffet.Sommeil, dureeEffet: 2, niveauRequis: 3),
            new Sort("Météore", 35, 40, TypeDegat.Magique, CibleType.TousLesEnnemis, "Évocation", niveauRequis: 5, element: Element.Feu),
        },

        ClasseHeros.Voleur => new List<Competence>
        {
            new("Attaque", 0, 8, TypeDegat.Physique, CibleType.UnEnnemi),
            new("Attaque Sournoise", 5, 22, TypeDegat.Physique, CibleType.UnEnnemi, element: Element.Tenebres),
            new("Lancer de Poison", 8, 12, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.Poison, dureeEffet: 4),
            new("Lame de l'Ombre", 12, 28, TypeDegat.Physique, CibleType.UnEnnemi, niveauRequis: 3, element: Element.Tenebres),
            new("Affaiblissement", 10, 8, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.DebuffDefense, dureeEffet: 3, niveauRequis: 4),
        },

        ClasseHeros.Clerc => new List<Competence>
        {
            new("Attaque", 0, 6, TypeDegat.Physique, CibleType.UnEnnemi),
            new Sort("Soin", 10, 25, TypeDegat.Magique, CibleType.UnAllie, "Restauration"),
            new Sort("Châtiment Sacré", 12, 18, TypeDegat.Magique, CibleType.UnEnnemi, "Évocation Sacrée", element: Element.Lumiere),
            new Sort("Purification", 8, 0, TypeDegat.Magique, CibleType.UnAllie, "Restauration"),
            new Sort("Grand Soin", 25, 50, TypeDegat.Magique, CibleType.UnAllie, "Restauration", niveauRequis: 3),
            new Sort("Lumière Sacrée", 20, 15, TypeDegat.Magique, CibleType.TousLesEnnemis, "Évocation Sacrée", niveauRequis: 5, element: Element.Lumiere),
        },

        ClasseHeros.Paladin => new List<Competence>
        {
            new("Attaque", 0, 8, TypeDegat.Physique, CibleType.UnEnnemi),
            new Sort("Frappe Sacrée", 8, 18, TypeDegat.Magique, CibleType.UnEnnemi, "Évocation Sacrée", element: Element.Lumiere),
            new Sort("Soin de Lumière", 12, 22, TypeDegat.Magique, CibleType.UnAllie, "Restauration"),
            new Sort("Bouclier Divin", 10, 0, TypeDegat.Magique, CibleType.Soi, "Abjuration", StatutEffet.BuffAttaque, dureeEffet: 3),
            new Sort("Jugement Sacré", 15, 25, TypeDegat.Magique, CibleType.UnEnnemi, "Évocation Sacrée", niveauRequis: 3, element: Element.Lumiere),
            new Sort("Aura Sacrée", 25, 30, TypeDegat.Magique, CibleType.TousLesEnnemis, "Évocation Sacrée", niveauRequis: 5, element: Element.Lumiere),
        },

        ClasseHeros.Necromancien => new List<Competence>
        {
            new("Attaque", 0, 5, TypeDegat.Physique, CibleType.UnEnnemi),
            new Sort("Drain de Vie", 10, 18, TypeDegat.Magique, CibleType.UnEnnemi, "Nécromancie", element: Element.Tenebres),
            new Sort("Invoquer Squelette", 15, 22, TypeDegat.Magique, CibleType.UnEnnemi, "Invocation", element: Element.Tenebres),
            new Sort("Malédiction", 12, 8, TypeDegat.Magique, CibleType.UnEnnemi, "Nécromancie", StatutEffet.DebuffDefense, dureeEffet: 3, element: Element.Tenebres),
            new Sort("Armée des Morts", 28, 20, TypeDegat.Magique, CibleType.TousLesEnnemis, "Nécromancie", StatutEffet.Poison, dureeEffet: 3, niveauRequis: 3, element: Element.Tenebres),
            new Sort("Emprise Mortelle", 22, 35, TypeDegat.Magique, CibleType.UnEnnemi, "Nécromancie", StatutEffet.Sommeil, dureeEffet: 2, niveauRequis: 5, element: Element.Tenebres),
        },

        ClasseHeros.Assassin => new List<Competence>
        {
            new("Attaque", 0, 10, TypeDegat.Physique, CibleType.UnEnnemi),
            new("Lame Empoisonnée", 6, 16, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.Poison, dureeEffet: 4),
            new("Frappe Fatale", 10, 28, TypeDegat.Physique, CibleType.UnEnnemi, element: Element.Tenebres),
            new("Éventration", 8, 14, TypeDegat.Physique, CibleType.UnEnnemi, StatutEffet.DebuffDefense, dureeEffet: 3),
            new("Tempête de Lames", 15, 20, TypeDegat.Physique, CibleType.TousLesEnnemis, niveauRequis: 3),
            new("Exécution", 22, 42, TypeDegat.Physique, CibleType.UnEnnemi, niveauRequis: 5, element: Element.Tenebres),
        },

        ClasseHeros.Druide => new List<Competence>
        {
            new("Attaque", 0, 7, TypeDegat.Physique, CibleType.UnEnnemi),
            new Sort("Épines Naturelles", 8, 16, TypeDegat.Magique, CibleType.UnEnnemi, "Nature", element: Element.Glace),
            new Sort("Régénération", 12, 25, TypeDegat.Magique, CibleType.UnAllie, "Restauration"),
            new Sort("Bouclier de Ronces", 10, 0, TypeDegat.Magique, CibleType.Soi, "Nature", StatutEffet.BuffAttaque, dureeEffet: 3),
            new Sort("Tempête Élémentaire", 22, 22, TypeDegat.Magique, CibleType.TousLesEnnemis, "Évocation", StatutEffet.Paralysie, dureeEffet: 1, niveauRequis: 3, element: Element.Foudre),
            new Sort("Fureur Animale", 20, 32, TypeDegat.Magique, CibleType.TousLesEnnemis, "Transmutation", niveauRequis: 5, element: Element.Feu),
        },

        _ => new List<Competence>()
    };
}
