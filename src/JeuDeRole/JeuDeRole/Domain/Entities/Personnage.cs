using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.Models;
using JeuDeRole.Domain.ValueObjects;

namespace JeuDeRole.Domain.Entities;

/// <summary>
/// Classe de base abstraite représentant tout combattant dans le jeu (Héros ou Monstre).
/// Gère les statistiques, les points de vie/mana, les effets de statut et les compétences.
/// </summary>
public abstract class Personnage : ICombattant
{
    // Propriétés de base
    public string Nom { get; }
    public int PointsDeVie { get; protected set; }
    public int PointsDeMana { get; protected set; }
    public Stats StatsBase { get; protected set; }
    public int Niveau { get; protected set; }
    public int Experience { get; protected set; }
    
    // Calcul de l'expérience requise pour le prochain niveau (formule linéaire simple)
    public int ExperiencePourProchainNiveau => Niveau * 100;

    // Gestion des statuts et effets
    public StatutEffet StatutActuel { get; protected set; }
    public bool EstVivant => PointsDeVie > 0;
    public List<EffetActif> EffetsActifs { get; } = new();
    
    // Résistances aux éléments (Dictionary: Element -> Multiplicateur de dégâts)
    // 1.0 = normal, 0.5 = résistant, 2.0 = vulnérable
    public Dictionary<Element, double> Resistances { get; set; } = new();

    // Liste interne des compétences
    private readonly List<ICompetence> _competences = new();

    /// <summary>
    /// Constructeur protégé initialisant les stats de base.
    /// </summary>
    /// <param name="nom">Nom du personnage</param>
    /// <param name="statsBase">Statistiques initiales</param>
    /// <param name="niveau">Niveau de départ (par défaut 1)</param>
    protected Personnage(string nom, Stats statsBase, int niveau = 1)
    {
        Nom = nom;
        StatsBase = statsBase;
        Niveau = niveau;
        Experience = 0;
        // Initialisation à pleine santé/mana
        PointsDeVie = statsBase.PointsDeVieMax;
        PointsDeMana = statsBase.PointsDeManaMax;
        StatutActuel = StatutEffet.Aucun;
    }

    /// <summary>
    /// Récupère le multiplicateur de résistance pour un élément donné.
    /// Retourne 1.0 par défaut si aucune résistance spécifique n'est définie.
    /// </summary>
    public double GetResistance(Element element)
    {
        return Resistances.GetValueOrDefault(element, 1.0);
    }

    /// <summary>
    /// Calcule les statistiques actuelles en prenant en compte les effets actifs (buffs/debuffs).
    /// </summary>
    public virtual Stats StatsActuelles
    {
        get
        {
            var stats = StatsBase;
            foreach (var effet in EffetsActifs.Where(e => e.EstActif))
            {
                if (effet.Statut == StatutEffet.BuffAttaque)
                    stats = new Stats(stats.PointsDeVieMax, stats.PointsDeManaMax,
                        stats.Force + effet.BonusValeur, stats.Intelligence + effet.BonusValeur,
                        stats.Agilite, stats.Defense, stats.ResistanceMagique);
                else if (effet.Statut == StatutEffet.DebuffDefense)
                    stats = new Stats(stats.PointsDeVieMax, stats.PointsDeManaMax,
                        stats.Force, stats.Intelligence, stats.Agilite,
                        Math.Max(0, stats.Defense - effet.BonusValeur),
                        Math.Max(0, stats.ResistanceMagique - effet.BonusValeur));
            }
            return stats;
        }
    }

    public List<ICompetence> GetCompetences() => new(_competences);

    public void AjouterCompetence(ICompetence competence)
    {
        _competences.Add(competence);
    }

    /// <summary>
    /// Applique des dégâts au personnage.
    /// Assure que les PV ne tombent pas en dessous de 0.
    /// </summary>
    public void SubirDegats(int montant)
    {
        int degatsEffectifs = Math.Max(0, montant);
        PointsDeVie = Math.Max(0, PointsDeVie - degatsEffectifs);
    }

    /// <summary>
    /// Soigne le personnage.
    /// Assure que les PV ne dépassent pas le maximum autorisé.
    /// </summary>
    public void Soigner(int montant)
    {
        int soinEffectif = Math.Max(0, montant);
        PointsDeVie = Math.Min(StatsActuelles.PointsDeVieMax, PointsDeVie + soinEffectif);
    }

    /// <summary>
    /// Consomme du mana pour une action.
    /// </summary>
    public void ConsommerMana(int montant)
    {
        int coutEffectif = Math.Max(0, montant);
        PointsDeMana = Math.Max(0, PointsDeMana - coutEffectif);
    }

    /// <summary>
    /// Restaure du mana.
    /// Assure que les PM ne dépassent pas le maximum autorisé.
    /// </summary>
    public void RestaurerMana(int montant)
    {
        int manaEffectif = Math.Max(0, montant);
        PointsDeMana = Math.Min(StatsActuelles.PointsDeManaMax, PointsDeMana + manaEffectif);
    }

    public void AppliquerStatut(StatutEffet statut)
    {
        StatutActuel = statut;
    }

    /// <summary>
    /// Ajoute un effet temporaire (durée limitée).
    /// Gère le remplacement des effets identiques et la mise à jour du statut principal.
    /// </summary>
    public void AjouterEffet(EffetActif effet)
    {
        // Remplacer un effet du même type s'il existe pour éviter l'accumulation infinie
        EffetsActifs.RemoveAll(e => e.Statut == effet.Statut);
        EffetsActifs.Add(effet);

        // Mettre à jour le statut principal si c'est un effet négatif majeur
        if (effet.Statut is StatutEffet.Poison or StatutEffet.Paralysie
            or StatutEffet.Sommeil or StatutEffet.Brulure or StatutEffet.Gel)
        {
            StatutActuel = effet.Statut;
        }
    }

    /// <summary>
    /// Met à jour les durées des effets actifs (à appeler à chaque tour).
    /// </summary>
    public void MettreAJourEffets()
    {
        foreach (var effet in EffetsActifs)
            effet.DecrementerTour();

        // Suppression des effets expirés
        EffetsActifs.RemoveAll(e => !e.EstActif);

        // Remettre statut à Aucun si plus d'effet négatif actif
        if (!EffetsActifs.Any(e => e.Statut is StatutEffet.Poison or StatutEffet.Paralysie
            or StatutEffet.Sommeil or StatutEffet.Brulure or StatutEffet.Gel))
        {
            StatutActuel = StatutEffet.Aucun;
        }
    }

    public bool PossedeEffet(StatutEffet statut)
    {
        return EffetsActifs.Any(e => e.Statut == statut && e.EstActif);
    }
}
