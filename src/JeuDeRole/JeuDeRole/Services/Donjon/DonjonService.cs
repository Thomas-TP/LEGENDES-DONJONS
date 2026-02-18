using JeuDeRole.Services.Interfaces;

namespace JeuDeRole.Services.Donjon;

/// <summary>
/// Implémentation du générateur de donjon.
/// Crée une suite de salles avec une difficulté croissante et des événements variés.
/// </summary>
public class DonjonService : IDonjonService
{
    private readonly Random _random = new();

    /// <summary>
    /// Génère la structure complète du donjon pour une session.
    /// Alterne entre combats, repos et événements, avec des boss aux points clés.
    /// </summary>
    public Interfaces.Donjon GenererDonjon(int profondeurMax)
    {
        var salles = new List<SalleDonjon>();

        for (int etage = 1; etage <= profondeurMax; etage++)
        {
            var type = DeterminerTypeSalle(etage, profondeurMax);
            
            // Création de la salle
            salles.Add(new SalleDonjon
            {
                Etage = etage,
                Type = type,
                Nom = GenererNomSalle(type, etage)
            });
        }

        return new Interfaces.Donjon
        {
            Salles = salles,
            ProfondeurMax = profondeurMax
        };
    }

    /// <summary>
    /// Algorithme simple de détermination du type de salle.
    /// Force certains types à des étages précis (Boss final, mi-parcours, repos).
    /// </summary>
    private TypeSalle DeterminerTypeSalle(int etage, int max)
    {
        // Le dernier étage est toujours le Boss Final
        if (etage == max) return TypeSalle.BossFinal;
        
        // Mi-parcours : Mini-Boss
        if (etage == max / 2) return TypeSalle.MiniBoss;
        
        // Tous les 3 étages : Repos garanti
        if (etage % 3 == 0) return TypeSalle.Repos;

        // Sinon : 60% Combat, 40% Événement
        int roll = _random.Next(100);
        if (roll < 60) return TypeSalle.Combat;
        return TypeSalle.Evenement;
    }

    private static string GenererNomSalle(TypeSalle type, int etage) => type switch
    {
        TypeSalle.Combat => $"Salle de combat (Étage {etage})",
        TypeSalle.Evenement => $"Salle mystérieuse (Étage {etage})",
        TypeSalle.Repos => $"Salle de repos (Étage {etage})",
        TypeSalle.MiniBoss => $"Antre du Mini-Boss (Étage {etage})",
        TypeSalle.BossFinal => $"Salle du Boss Final (Étage {etage})",
        _ => $"Étage {etage}"
    };

    /// <summary>
    /// Génère une représentation ASCII de la carte du donjon pour l'interface utilisateur.
    /// Affiche l'étage actuel, les salles visitées et les types de salles (si révélés).
    /// </summary>
    public static string GenererCarte(List<SalleDonjon> salles, int etageActuel)
    {
        var lignes = new List<string>();
        lignes.Add("╔══════════════════════════╗");
        lignes.Add("║    CARTE DU DONJON       ║");
        lignes.Add("╠══════════════════════════╣");

        foreach (var salle in salles)
        {
            // Icône représentative du type de salle
            string icone = salle.Type switch
            {
                TypeSalle.Combat => "⚔",
                TypeSalle.Evenement => "?",
                TypeSalle.Repos => "♥",
                TypeSalle.MiniBoss => "☠",
                TypeSalle.BossFinal => "💀",
                _ => " "
            };

            // Indicateur de position du joueur
            string marqueur = salle.Etage == etageActuel ? "→" : " ";
            // Indicateur d'état (visité, actuel, non visité)
            string etat = salle.Visitee ? "✓" : (salle.Etage == etageActuel ? "●" : "○");

            // Formatage de la ligne
            lignes.Add($"║ {marqueur} {etat} Étage {salle.Etage,2} [{icone}] {(salle.Visitee || salle.Etage == etageActuel ? salle.Nom.PadRight(10)[..10] : "??????????"),10} ║");
        }

        lignes.Add("╚══════════════════════════╝");
        return string.Join('\n', lignes);
    }
}
