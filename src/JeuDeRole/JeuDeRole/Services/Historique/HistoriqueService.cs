using JeuDeRole.Domain.Models;
using JeuDeRole.Services.Interfaces;

namespace JeuDeRole.Services.Historique;

/// <summary>
/// Service gérant l'historique des combats et les statistiques de la partie.
/// Enregistre chaque résultat de combat pour permettre l'analyse ou l'affichage ultérieur.
/// </summary>
public class HistoriqueService : IHistoriqueService
{
    // Liste interne stockant l'historique des combats en mémoire
    private readonly List<ResultatCombat> _historique = new();

    /// <summary>
    /// Ajoute un nouveau résultat de combat à l'historique.
    /// </summary>
    /// <param name="resultat">Le résultat du combat terminé (victoire/défaite, tours, etc.).</param>
    public void AjouterResultat(ResultatCombat resultat)
    {
        _historique.Add(resultat);
    }

    /// <summary>
    /// Retourne une copie de la liste complète de l'historique pour éviter toute modification externe accidentelle.
    /// </summary>
    public List<ResultatCombat> ObtenirHistorique() => new(_historique);

    /// <summary>
    /// Restaure l'historique depuis une source externe (ex: chargement de sauvegarde).
    /// </summary>
    /// <param name="historique">La liste des combats à restaurer.</param>
    public void Restaurer(List<ResultatCombat> historique)
    {
        _historique.Clear();
        _historique.AddRange(historique);
    }

    /// <summary>
    /// Calcule le nombre total de victoires des héros.
    /// </summary>
    public int TotalVictoires => _historique.Count(r => r.VictoireHeros);

    /// <summary>
    /// Calcule le nombre total de défaites (ou fuites/morts).
    /// </summary>
    public int TotalDefaites => _historique.Count(r => !r.VictoireHeros);

    /// <summary>
    /// Retourne le nombre total de combats enregistrés.
    /// </summary>
    public int TotalCombats => _historique.Count;
}
