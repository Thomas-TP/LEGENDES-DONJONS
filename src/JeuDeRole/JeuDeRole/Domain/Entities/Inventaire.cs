namespace JeuDeRole.Domain.Entities;

/// <summary>
/// Gère la collection d'objets consommables possédés par le groupe de héros.
/// Simule un sac commun pour tous les personnages.
/// </summary>
public class Inventaire
{
    // Liste interne des stacks d'objets
    private readonly List<ObjetConsommable> _objets = new();

    /// <summary>
    /// Ajoute un nouvel objet (ou pile d'objets) à l'inventaire.
    /// </summary>
    public void Ajouter(ObjetConsommable objet)
    {
        // Pourrait être amélioré pour fusionner les stacks existants du même objet
        _objets.Add(objet);
    }

    /// <summary>
    /// Retire complètement un objet de l'inventaire.
    /// </summary>
    public bool Retirer(ObjetConsommable objet)
    {
        return _objets.Remove(objet);
    }

    /// <summary>
    /// Retourne la liste des objets ayant au moins une charge disponible.
    /// </summary>
    public List<ObjetConsommable> ListerObjets()
    {
        return _objets.Where(o => o.EstDisponible).ToList();
    }
}
