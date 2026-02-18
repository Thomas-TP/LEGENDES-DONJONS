using JeuDeRole.Domain.Interfaces;

namespace JeuDeRole.Domain.Entities;

/// <summary>
/// Représente un objet utilisable (Potion, Antidote, etc.) stocké dans l'inventaire.
/// Possède un effet immédiat lorsqu'il est consommé.
/// </summary>
public class ObjetConsommable : IObjetUtilisable
{
    public string Nom { get; }
    public string Description { get; }
    
    // Nombre d'exemplaires disponibles dans l'inventaire
    public int Quantite { get; private set; }

    // Action déléguée définissant l'effet de l'objet sur une cible
    private readonly Action<ICombattant> _effet;

    /// <summary>
    /// Crée un nouvel objet consommable.
    /// </summary>
    /// <param name="nom">Nom de l'objet</param>
    /// <param name="description">Texte explicatif pour l'UI</param>
    /// <param name="quantite">Stock initial</param>
    /// <param name="effet">Fonction anonyme ou méthode appliquée à l'utilisation</param>
    public ObjetConsommable(string nom, string description, int quantite, Action<ICombattant> effet)
    {
        Nom = nom;
        Description = description;
        Quantite = quantite;
        _effet = effet;
    }

    /// <summary>
    /// Utilise une unité de l'objet sur la cible, si disponible.
    /// Décrémente la quantité.
    /// </summary>
    public void Utiliser(ICombattant cible)
    {
        if (Quantite <= 0) return;
        _effet(cible);
        Quantite--;
    }

    /// <summary>
    /// Indique si l'objet peut encore être utilisé.
    /// </summary>
    public bool EstDisponible => Quantite > 0;
}
