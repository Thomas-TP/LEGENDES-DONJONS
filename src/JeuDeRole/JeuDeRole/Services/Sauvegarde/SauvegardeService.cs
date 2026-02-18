using System.Text.Json;
using JeuDeRole.Services.Interfaces;

namespace JeuDeRole.Services.Sauvegarde;

/// <summary>
/// Service de persistance des données du jeu.
/// Gère la sérialisation et désérialisation de l'état du jeu en JSON.
/// </summary>
public class SauvegardeService : ISauvegardeService
{
    // Configuration JSON pour garantir la lisibilité et la compatibilité
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true, // Pour que le fichier soit lisible par un humain
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Convention standard JS/JSON
    };

    /// <summary>
    /// Sauvegarde l'état complet du jeu dans un fichier JSON.
    /// Crée automatiquement les répertoires nécessaires si absents.
    /// </summary>
    /// <param name="donnees">L'objet racine contenant tout l'état de la partie.</param>
    /// <param name="chemin">Le chemin complet du fichier de destination.</param>
    public void Sauvegarder(DonneesSauvegarde donnees, string chemin)
    {
        // Horodatage automatique
        donnees.DateSauvegarde = DateTime.Now;

        string json = JsonSerializer.Serialize(donnees, Options);
        
        // Sécurisation de la création du dossier
        string? directory = Path.GetDirectoryName(chemin);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
            
        File.WriteAllText(chemin, json);
    }

    /// <summary>
    /// Charge une partie depuis un fichier JSON existant.
    /// </summary>
    /// <returns>L'objet de sauvegarde ou null si le fichier n'existe pas.</returns>
    public DonneesSauvegarde? Charger(string chemin)
    {
        if (!File.Exists(chemin))
            return null;

        string json = File.ReadAllText(chemin);
        return JsonSerializer.Deserialize<DonneesSauvegarde>(json, Options);
    }

    /// <summary>
    /// Vérifie simplement la présence d'un fichier de sauvegarde à l'emplacement donné.
    /// </summary>
    public bool SauvegardeExiste(string chemin)
    {
        return File.Exists(chemin);
    }
}
