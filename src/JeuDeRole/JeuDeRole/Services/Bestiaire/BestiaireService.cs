using JeuDeRole.Domain.Enums;
using JeuDeRole.Services.Interfaces;

namespace JeuDeRole.Services.Bestiaire;

/// <summary>
/// Implémentation du service bestiaire.
/// Utilise un dictionnaire pour stocker les monstres rencontrés.
/// </summary>
public class BestiaireService : IBestiaireService
{
    // Dictionnaire interne pour un accès rapide par nom
    private readonly Dictionary<string, EntreeBestiaire> _bestiaire = new();

    /// <summary>
    /// Enregistre ou met à jour les infos d'un monstre.
    /// Ne remplace pas si le monstre existe déjà (on garde la date de première rencontre).
    /// </summary>
    public void EnregistrerMonstre(string nom, int pvMax, int force, int defense,
                                   Dictionary<Element, double> faiblesses, int xp)
    {
        if (!_bestiaire.ContainsKey(nom))
        {
            _bestiaire[nom] = new EntreeBestiaire
            {
                Nom = nom,PvMax = pvMax, Force = force, Defense = defense,
                Faiblesses = faiblesses, Xp = xp
            };
        }
    }

    /// <summary>
    /// Ajoute +1 au compteur de kill pour ce monstre.
    /// </summary>
    public void EnregistrerKill(string nom)
    {
        if (_bestiaire.TryGetValue(nom, out var entree))
            entree.NombreKills++;
    }

    public List<EntreeBestiaire> ObtenirBestiaire() => _bestiaire.Values.ToList();

    public EntreeBestiaire? Obtenir(string nom) =>
        _bestiaire.TryGetValue(nom, out var e) ? e : null;

    public int TotalKills => _bestiaire.Values.Sum(e => e.NombreKills);

    public void Restaurer(List<EntreeBestiaire> entrees)
    {
        _bestiaire.Clear();
        foreach (var e in entrees)
            _bestiaire[e.Nom] = e;
    }
}
