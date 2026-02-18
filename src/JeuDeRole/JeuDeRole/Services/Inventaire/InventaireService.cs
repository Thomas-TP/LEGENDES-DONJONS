using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Logging;
using JeuDeRole.Services.Interfaces; // Ajout de l'using pour l'interface

namespace JeuDeRole.Services.Inventaire;

/// <summary>
/// Service de gestion de l'inventaire et de l'utilisation des objets.
/// Coordonne l'effet des consommables sur les personnages et journalise les actions.
/// </summary>
public class InventaireService : IInventaireService
{
    private readonly ICombatLogger _logger;

    public InventaireService(ICombatLogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Applique l'effet d'un objet consommable sur une cible spécifique.
    /// Traite également le décrément du stock et l'affichage du résultat.
    /// </summary>
    /// <param name="objet">L'objet à consommer (potion, parchemin...).</param>
    /// <param name="cible">Le combattant qui reçoit l'effet.</param>
    /// <param name="inventaire">L'inventaire source (pour vérification - même si la logique est dans l'objet).</param>
    public void UtiliserObjet(ObjetConsommable objet, ICombattant cible, Domain.Entities.Inventaire inventaire)
    {
        // Vérification de sécurité
        if (!objet.EstDisponible)
        {
            _logger.LogAction($"{objet.Nom} n'est plus disponible !");
            return;
        }

        // Capture des stats avant utilisation pour calculer le gain réel
        int pvAvant = cible.PointsDeVie;
        int pmAvant = cible.PointsDeMana;

        // Déclenche l'effet de l'objet (soin, mana, buff...)
        objet.Utiliser(cible);

        // Calcul des différences pour le feedback utilisateur
        int pvGagnes = cible.PointsDeVie - pvAvant;
        int pmGagnes = cible.PointsDeMana - pmAvant;

        // Message contextuel selon l'effet principal (PV ou PM)
        if (pvGagnes > 0)
            _logger.LogAction($"{cible.Nom} utilise {objet.Nom} et récupère {pvGagnes} PV !");
        else if (pmGagnes > 0)
            _logger.LogAction($"{cible.Nom} utilise {objet.Nom} et récupère {pmGagnes} PM !");
        else
            _logger.LogAction($"{cible.Nom} utilise {objet.Nom} !");
    }
}
