using JeuDeRole.Domain.Models;

namespace JeuDeRole.Services.Interfaces;

/// <summary>
/// Service gérant l'historique de toutes les batailles jouées durant la session.
/// Permet de garder une trace des victoires, défaites et scores cumulés.
/// </summary>
public interface IHistoriqueService
{
    /// <summary>
    /// Ajoute le résultat d'un combat terminé à l'historique et met à jour les stats globales (`TotalVictoires` etc.).
    /// </summary>
    void AjouterResultat(ResultatCombat resultat);

    /// <summary>
    /// Retourne la liste complète de tous les résultats de combats stockés.
    /// </summary>
    List<ResultatCombat> ObtenirHistorique();

    /// <summary>
    /// Restaure l'historique depuis une sauvegarde pour reprendre la session sans perdre les stats.
    /// </summary>
    void Restaurer(List<ResultatCombat> historique);

    int TotalVictoires { get; }
    int TotalDefaites { get; }
    int TotalCombats { get; }
}
