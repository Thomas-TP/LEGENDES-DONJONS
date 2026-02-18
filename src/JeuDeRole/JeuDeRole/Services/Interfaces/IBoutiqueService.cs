using JeuDeRole.Domain.Entities;

namespace JeuDeRole.Services.Interfaces;

/// <summary>
/// Service gérant la boutique du jeu (achat et vente).
/// Propose des équipements et des consommables aux joueurs.
/// Gère aussi le porte-monnaie (Or).
/// </summary>
public interface IBoutiqueService
{
    /// <summary>
    /// Récupère la liste des équipements (armes, armures) en vente.
    /// </summary>
    List<ArticleBoutique> ObtenirEquipements();

    /// <summary>
    /// Récupère la liste des objets consommables (potions...) en vente.
    /// </summary>
    List<ArticleBoutique> ObtenirObjets();

    /// <summary>
    /// Tente d'acheter un article.
    /// Vérifie si le joueur a assez d'or.
    /// Retourne true si succès, false sinon.
    /// </summary>
    bool Acheter(ArticleBoutique article, int quantite = 1);

    /// <summary>
    /// Vend un équipement du joueur à la boutique pour un prix réduit (généralement 50%).
    /// Retourne le montant d'or gagné.
    /// </summary>
    int VendreEquipement(Equipement equipement);

    /// <summary>
    /// Or actuellement possédé par le joueur.
    /// </summary>
    int Or { get; }

    /// <summary>
    /// Ajoute ou retire de l'or au joueur (montant peut être négatif, mais préférer Acheter).
    /// </summary>
    void AjouterOr(int montant);

    /// <summary>
    /// Restaure l'or du joueur depuis une sauvegarde.
    /// </summary>
    void Restaurer(int or);
}

/// <summary>
/// Représente un article achetable en boutique.
/// </summary>
public class ArticleBoutique
{
    public string Nom { get; init; } = "";
    public string Description { get; init; } = "";
    public int Prix { get; init; }
    public string Categorie { get; init; } = "";
}
