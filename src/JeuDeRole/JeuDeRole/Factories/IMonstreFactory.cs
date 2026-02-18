using JeuDeRole.Domain.Entities;

namespace JeuDeRole.Factories;

/// <summary>
/// Interface pour la création de monstres et de boss.
/// Permet d'abstraire la logique de génération des ennemis (Pattern Factory).
/// </summary>
public interface IMonstreFactory
{
    /// <summary>
    /// Crée un monstre spécifique à partir de son nom/type.
    /// </summary>
    Monstre CreerMonstre(string type);

    /// <summary>
    /// Génère un groupe de monstres aléatoires.
    /// </summary>
    /// <param name="nombre">Nombre de monstres à générer (généralement entre 1 et 4).</param>
    List<Monstre> GenererGroupeAleatoire(int nombre);

    /// <summary>
    /// Crée un boss spécifique avec ses phases et compétences.
    /// </summary>
    Boss CreerBoss(string type);
}
