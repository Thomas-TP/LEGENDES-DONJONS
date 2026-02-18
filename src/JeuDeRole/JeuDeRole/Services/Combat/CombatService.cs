using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.Models;
using JeuDeRole.Logging;
using JeuDeRole.Services.Interfaces; // Interface du service
using JeuDeRole.Strategies.Degats; // Stratégies de calcul

namespace JeuDeRole.Services.Combat;

/// <summary>
/// Service central gérant la logique des combats au tour par tour.
/// Orchestre les actions des héros et des monstres, calcule les dégâts, et gère les effets de statut.
/// C'est le "Moteur de Combat" du jeu.
/// </summary>
public class CombatService : ICombatService
{
    // Dépendances injectées pour gérer l'affichage, les calculs et l'inventaire
    private readonly ICombatLogger _logger;
    private readonly ICalculDegats _calculPhysique;
    private readonly ICalculDegats _calculMagique;
    private readonly IInventaireService _inventaireService;
    private readonly Random _random = new();

    // Stats de combat accumulées pour le rapport de fin de combat
    private int _totalDegats;
    private int _totalSoins;
    private Dictionary<string, int> _degatsParHeros = new();

    /// <summary>
    /// Callback pour obtenir l'action choisie par le joueur.
    /// Nécessaire car le service est indépendant de l'UI (Console, Web, Unity...).
    /// L'UI s'abonne à ce délégué pour fournir les choix du joueur.
    /// </summary>
    public Func<Heros, List<ICombattant>, List<ICombattant>, Domain.Entities.Inventaire, ActionCombat>? DemanderActionJoueur { get; set; }

    /// <summary>
    /// Initialise le service de combat avec ses dépendances (Logger, Calculateur de dégâts, Inventaire).
    /// </summary>
    public CombatService(ICombatLogger logger, ICalculDegats calculPhysique,
                         ICalculDegats calculMagique, IInventaireService inventaireService)
    {
        _logger = logger;
        _calculPhysique = calculPhysique;
        _calculMagique = calculMagique;
        _inventaireService = inventaireService;
    }

    /// <summary>
    /// Lance une boucle de combat complète jusqu'à la victoire ou la défaite.
    /// Retourne un résumé détaillé du combat (statistiques, expérience, etc.).
    /// </summary>
    /// <param name="heros">Liste des héros participants</param>
    /// <param name="monstres">Liste des monstres adverses</param>
    /// <param name="inventaire">Inventaire du groupe pour l'utilisation d'objets</param>
    public ResultatCombat LancerCombat(List<Heros> heros, List<Monstre> monstres,
                                       Domain.Entities.Inventaire inventaire)
    {
        // Initialisation des compteurs pour ce combat spécifique
        _totalDegats = 0;
        _totalSoins = 0;
        _degatsParHeros = new Dictionary<string, int>();
        foreach (var h in heros)
            _degatsParHeros[h.Nom] = 0; // Initialise les compteurs à 0
        int tour = 0;

        // Boucle principale du combat : tant qu'il reste des survivants dans les deux camps
        while (HerosVivants(heros) && MonstresVivants(monstres))
        {
            tour++;
            _logger.LogDebutTour(tour); // Affiche "--- Tour X ---"

            // Exécution d'un tour de jeu complet (Actions de tous les participants)
            ExecuterTour(heros, monstres, inventaire);

            // Appliquer effets de statut (poison, brûlure, etc.) en fin de tour
            // On traite une liste unifiée de tous les combattants
            var tousCombattants = heros.Cast<ICombattant>().Concat(monstres).ToList();
            AppliquerEffetsStatut(tousCombattants);

            // Mettre à jour les durées d'effets et nettoyer les effets expirés
            foreach (var c in tousCombattants.Where(c => c.EstVivant))
                c.MettreAJourEffets();

            // Vérifier changement de phase des boss (mécanique spécifique aux Boss)
            foreach (var monstre in monstres.OfType<Boss>().Where(b => b.EstVivant))
            {
                if (monstre.VerifierChangementPhase())
                    _logger.LogPhaseChangement(monstre.Nom, monstre.PhaseActuelle, monstre.GetNomPhase());
            }
        }

        bool victoire = HerosVivants(heros);
        _logger.LogFinCombat(victoire);

        // Gestion de l'expérience en fin de combat uniquement si victoire
        int xpTotal = 0;
        if (victoire)
        {
            // Somme de l'XP de tous les monstres vaincus
            xpTotal = monstres.Sum(m => m.ExperienceDonnee);
            // Distribution de l'XP à tous les héros vivants
            foreach (var h in heros.Where(h => h.EstVivant))
            {
                bool levelUp = h.GagnerExperience(xpTotal);
                _logger.LogExperience(h.Nom, xpTotal, h.Niveau, levelUp);
            }
        }

        // Construction du rapport de combat final
        return new ResultatCombat
        {
            VictoireHeros = victoire,
            TotalDegatsInfliges = _totalDegats,
            TotalSoinsProdigues = _totalSoins,
            NombreTours = tour,
            ExperienceGagnee = xpTotal,
            HerosParticipants = heros.Select(h => h.Nom).ToList(),
            MonstresAffrontes = monstres.Select(m => m.Nom).ToList(),
            DegatsParHeros = new Dictionary<string, int>(_degatsParHeros)
        };
    }

    /// <summary>
    /// Gère l'ordre d'action et l'exécution des actions pour tous les combattants lors d'un tour.
    /// </summary>
    private void ExecuterTour(List<Heros> heros, List<Monstre> monstres,
                              Domain.Entities.Inventaire inventaire)
    {
        // Initiative dynamique : recalcul à chaque tour avec composante aléatoire
        // (Agilité + 1d6) pour éviter que l'ordre soit figé tout le combat
        var tousLesCombattants = CalculerOrdre(
            heros.Cast<ICombattant>().Concat(monstres).ToList());

        foreach (var combattant in tousLesCombattants)
        {
            // Vérifier si le combattant est toujours vivant avant d'agir (il a pu mourir avant son tour)
            if (!combattant.EstVivant) continue;

            // Gestion des statuts empêchant l'action (CC: Crowd Control)
            if (combattant.PossedeEffet(StatutEffet.Paralysie))
            {
                _logger.LogAction($"{combattant.Nom} est paralysé et ne peut pas agir !");
                continue;
            }
            if (combattant.PossedeEffet(StatutEffet.Sommeil))
            {
                _logger.LogAction($"{combattant.Nom} est endormi et ne peut pas agir !");
                continue; // Réveil au prochain dégât (géré dans SubirDegats normalement, ou fin de tour)
            }
            if (combattant.PossedeEffet(StatutEffet.Gel))
            {
                _logger.LogAction($"{combattant.Nom} est gelé et ne peut pas agir !");
                continue;
            }

            // Délégation de l'action selon le type de combattant (Joueur ou IA)
            if (combattant is Heros heros1)
                ExecuterActionHeros(heros1, heros, monstres, inventaire);
            else if (combattant is Monstre monstre)
                ExecuterActionMonstre(monstre, heros);
        }
    }

    /// <summary>
    /// Traite le tour d'un héros en demandant une action au joueur via le délégué.
    /// </summary>
    private void ExecuterActionHeros(Heros heros, List<Heros> equipe,
                                     List<Monstre> monstres, Domain.Entities.Inventaire inventaire)
    {
        var ennemisVivants = monstres.Where(m => m.EstVivant).Cast<ICombattant>().ToList();
        var alliesVivants = equipe.Where(h => h.EstVivant).Cast<ICombattant>().ToList();

        if (ennemisVivants.Count == 0) return; // Combat terminé, plus d'ennemis

        ActionCombat action;
        if (DemanderActionJoueur != null)
            // Appel vers l'interface utilisateur (Console, WPF...) pour choix interactif
            action = DemanderActionJoueur(heros, ennemisVivants, alliesVivants, inventaire);
        else
            // Action par défaut (IA basique) si pas d'UI connectée (ex: tests)
            action = ActionCombat.Defendre(heros);

        TraiterAction(action, inventaire);
    }

    /// <summary>
    /// Traite le tour d'un monstre en utilisant son IA interne.
    /// </summary>
    private void ExecuterActionMonstre(Monstre monstre, List<Heros> heros)
    {
        var ciblesVivantes = heros.Where(h => h.EstVivant).Cast<ICombattant>().ToList();
        if (ciblesVivantes.Count == 0) return; // Plus de héros à attaquer

        _logger.LogDebutActionMonstre(monstre.Nom); // "Le monstre X se prépare..."

        // L'IA décide de l'action (Attaque simple, Compétence spéciale, Soin...)
        var action = monstre.StrategieIA.ChoisirAction(monstre, ciblesVivantes);

        // Logging spécifique pour les compétences
        if (action.Competence != null && action.Cibles.Count > 0)
        {
            bool aoE = action.Cibles.Count > 1; // Area of Effect check
            string nomCible = aoE ? "tous les héros" : action.Cibles[0].Nom;
            _logger.LogActionMonstre(monstre.Nom, action.Competence.Nom, nomCible, aoE);
        }

        // Exécution de l'action choisie
        TraiterAction(action, null); // Pas d'inventaire pour les monstres
    }

    /// <summary>
    /// Exécute concrètement l'action de combat choisie (Attaque, Défense, Objet, Compétence).
    /// </summary>
    private void TraiterAction(ActionCombat action, Domain.Entities.Inventaire? inventaire)
    {
        // 1. Cas : Défense
        if (action.EstDefense)
        {
            _logger.LogDefense(action.Source.Nom);
            // Note : La logique de réduction de dégâts est gérée dans SubirDegats (si IsDefending=true)
            // ou via un statut temporaire ajouté ici si on voulait complexifier.
            return;
        }

        // 2. Cas : Utilisation d'objet
        if (action.Objet != null && inventaire != null)
        {
            var cible = action.Cibles.First();
            _inventaireService.UtiliserObjet(action.Objet.Objet, cible, inventaire);
            return;
        }

        // 3. Cas : Compétence (Attaque ou Soin)
        if (action.Competence == null) return; // Sécurité

        var competence = action.Competence;

        // Si compétence de soin, buff ou purification
        if (competence.Cible == CibleType.UnAllie || competence.Cible == CibleType.Soi)
        {
            TraiterSoin(action);
            return;
        }

        // Gestion du coût en Mana
        if (competence.CoutMana > 0)
        {
            if (action.Source.PointsDeMana < competence.CoutMana)
            {
                _logger.LogAction($"{action.Source.Nom} n'a pas assez de mana pour {competence.Nom} !");
                return; // Tour perdu
            }
            action.Source.ConsommerMana(competence.CoutMana);
        }

        _logger.LogAction($"{action.Source.Nom} utilise {competence.Nom} !");

        // Application de la compétence sur chaque cible (multi-cibles possible)
        foreach (var cible in action.Cibles)
        {
            if (!cible.EstVivant) continue;

            // Esquive basée sur l'Agilité : Max 15% + (Agilité * 0.4)
            // ex: 10 Agi => 4% esquive + 5% base ? Non, ici scaling pur.
            double chanceEsquive = Math.Min(15, cible.StatsActuelles.Agilite * 0.4);
            if (_random.Next(100) < chanceEsquive)
            {
                _logger.LogEsquive(cible); // "X esquive l'attaque !"
                continue;
            }

            // Sélection du calculateur de dégâts approprié (Physique vs Magique)
            // Utilise le pattern Strategy
            var calculateur = competence.TypeDegat == TypeDegat.Physique
                ? _calculPhysique
                : _calculMagique;

            int degats = calculateur.Calculer(action.Source, cible, competence);

            // Coup critique basé sur l'agilité : Base 5% + Agilité * 0.3
            double chanceCrit = 5 + action.Source.StatsActuelles.Agilite * 0.3;
            bool critique = _random.Next(100) < chanceCrit;
            if (critique)
                degats = (int)(degats * 1.5); // Dégâts x1.5 en critique

            // Application des faiblesses/résistances élémentaires (Feu sur Eau, etc.)
            double multiplicateur = cible.GetResistance(competence.Element);
            _logger.LogElement(competence.Element, multiplicateur); // Affiche "C'est super efficace !"

            // Application finale des dégâts aux PV
            cible.SubirDegats(degats);
            _totalDegats += degats;

            // Tracking pour statistiques
            if (_degatsParHeros.ContainsKey(action.Source.Nom))
                _degatsParHeros[action.Source.Nom] += degats;

            // Logs
            if (critique)
                _logger.LogCritique(action.Source, cible, degats);
            else
                _logger.LogDegats(action.Source, cible, degats);

            // Application de l'effet secondaire (statut) si présent (ex: Poison, Gel)
            if (competence.EffetSecondaire != StatutEffet.Aucun && cible.EstVivant)
            {
                // Calcul de la puissance de l'effet si c'est un buff/debuff
                // Pour un debuff stat simple, on divise la puissance par 3 arbitrairement
                var effet = new EffetActif(competence.EffetSecondaire, competence.DureeEffet,
                    competence.EffetSecondaire is StatutEffet.BuffAttaque or StatutEffet.DebuffDefense
                        ? competence.Puissance / 3 : 0);
                
                cible.AjouterEffet(effet);
                _logger.LogStatut(cible, competence.EffetSecondaire);
            }

            if (!cible.EstVivant)
                _logger.LogMort(cible);
        }
    }

    /// <summary>
    /// Gère spécifiquement les compétences de soin et de purification.
    /// </summary>
    private void TraiterSoin(ActionCombat action)
    {
        var competence = action.Competence!;

        // Vérification Mana pour les soins aussi
        if (competence.CoutMana > 0)
        {
            if (action.Source.PointsDeMana < competence.CoutMana)
            {
                _logger.LogAction($"{action.Source.Nom} n'a pas assez de mana pour {competence.Nom} !");
                return;
            }
            action.Source.ConsommerMana(competence.CoutMana);
        }

        foreach (var cible in action.Cibles)
        {
            // Purification (Puissance 0 conventionnelle pour 'Esuna'/'Purify')
            if (competence.Puissance == 0) // Convention : Puissance 0 = Status Heal
            {
                cible.AppliquerStatut(StatutEffet.Aucun);
                // Retire tous les effets négatifs majeurs
                cible.EffetsActifs.RemoveAll(e => e.Statut is StatutEffet.Poison
                    or StatutEffet.Paralysie or StatutEffet.Sommeil
                    or StatutEffet.Brulure or StatutEffet.Gel or StatutEffet.DebuffDefense);
                _logger.LogAction($"{action.Source.Nom} purifie {cible.Nom} !");
                continue;
            }

            // Soin avec scaling sur l'Intelligence du lanceur
            int soin = competence.Puissance + action.Source.StatsActuelles.Intelligence / 2;
            cible.Soigner(soin);
            _totalSoins += soin;
            _logger.LogSoin(action.Source, cible, soin);
        }
    }

    /// <summary>
    /// Applique les dégâts périodiques (DoT) comme le Poison ou la Brûlure en fin de tour.
    /// </summary>
    private void AppliquerEffetsStatut(List<ICombattant> combattants)
    {
        foreach (var combattant in combattants)
        {
            if (!combattant.EstVivant) continue;

            if (combattant.PossedeEffet(StatutEffet.Poison))
            {
                // Poison : 10% des PV Max par tour
                int degatPoison = Math.Max(1, combattant.StatsActuelles.PointsDeVieMax / 10);
                combattant.SubirDegats(degatPoison);
                _logger.LogAction($"🧪 {combattant.Nom} subit {degatPoison} dégâts de poison ! (PV: {combattant.PointsDeVie})");
                if (!combattant.EstVivant) _logger.LogMort(combattant);
            }

            if (combattant.PossedeEffet(StatutEffet.Brulure))
            {
                // Brûlure : 12.5% des PV Max (légèrement plus fort que le poison)
                int degatBrulure = Math.Max(1, combattant.StatsActuelles.PointsDeVieMax / 8);
                combattant.SubirDegats(degatBrulure);
                _logger.LogAction($"🔥 {combattant.Nom} subit {degatBrulure} dégâts de brûlure ! (PV: {combattant.PointsDeVie})");
                if (!combattant.EstVivant) _logger.LogMort(combattant);
            }
        }
    }

    /// <summary>
    /// Détermine l'ordre de passage pour le tour en cours.
    /// </summary>
    private List<ICombattant> CalculerOrdre(List<ICombattant> combattants)
    {
        // Initiative = Agilité + 1d6 (0 à 5 inclus dans Random.Next(0, 6))
        // Plus l'agilité est haute, plus on joue tôt. La part d'aléatoire évite le déterminisme absolu.
        return combattants
            .Where(c => c.EstVivant)
            .OrderByDescending(c => c.StatsActuelles.Agilite + _random.Next(0, 6))
            .ToList();
    }

    // Helpers pour vérifier l'état des équipes
    private static bool HerosVivants(List<Heros> heros) => heros.Any(h => h.EstVivant);
    private static bool MonstresVivants(List<Monstre> monstres) => monstres.Any(m => m.EstVivant);
}
