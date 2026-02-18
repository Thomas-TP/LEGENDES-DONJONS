using JeuDeRole.Services.Interfaces;
// Alias pour éviter la confusion avec le namespace ou une classe interne potentielle
using SuccesDto = JeuDeRole.Services.Interfaces.Succes;

namespace JeuDeRole.Services.Succes;

/// <summary>
/// Service gérant les réalisations (Achievements) du joueur.
/// Débloque des trophées en fonction des statistiques de la partie.
/// </summary>
public class SuccesService : ISuccesService
{
    private readonly List<SuccesDto> _succes;
    
    // Liste tampon des succès débloqués lors de la dernière action
    private readonly List<SuccesDto> _nouveaux = new();

    /// <summary>
    /// Initialise la liste de tous les succès possibles dans le jeu.
    /// </summary>
    public SuccesService()
    {
        _succes = new List<SuccesDto>
        {
            // Succès de combat (Kills)
            new() { Id = "first_blood", Nom = "Premier Sang", Description = "Tuer un monstre pour la première fois", Icone = "🗡" },
            new() { Id = "kill_10", Nom = "Chasseur", Description = "Tuer 10 monstres", Icone = "🏹" },
            new() { Id = "kill_50", Nom = "Exterminateur", Description = "Tuer 50 monstres", Icone = "💀" },
            new() { Id = "kill_100", Nom = "Légende", Description = "Tuer 100 monstres", Icone = "👑" },
            
            // Succès de Boss
            new() { Id = "first_boss", Nom = "Tueur de Boss", Description = "Vaincre un boss", Icone = "🐉" },
            new() { Id = "boss_3", Nom = "Fléau des Boss", Description = "Vaincre 3 boss", Icone = "⚔" },
            
            // Succès de performance
            new() { Id = "no_death", Nom = "Invincible", Description = "Gagner un combat sans aucune mort", Icone = "🛡" },
            new() { Id = "solo", Nom = "Solitaire", Description = "Gagner avec un seul héros vivant", Icone = "🎯" },
            
            // Succès de leveling
            new() { Id = "level_5", Nom = "Vétéran", Description = "Atteindre le niveau 5", Icone = "⭐" },
            new() { Id = "level_10", Nom = "Maître", Description = "Atteindre le niveau 10", Icone = "🌟" },
            
            // Succès de modes de jeu
            new() { Id = "arena_5", Nom = "Gladiateur", Description = "Atteindre la vague 5 en arène", Icone = "🏛" },
            new() { Id = "arena_10", Nom = "Champion", Description = "Atteindre la vague 10 en arène", Icone = "🏆" },
            new() { Id = "donjon_5", Nom = "Explorateur", Description = "Atteindre l'étage 5 d'un donjon", Icone = "🗺" },
            
            // Succès généraux
            new() { Id = "win_3", Nom = "Gagnant", Description = "Remporter 3 victoires", Icone = "✌" },
            new() { Id = "win_10", Nom = "Conquérant", Description = "Remporter 10 victoires", Icone = "🎖" },
        };
    }

    /// <summary>
    /// Vérifie si de nouveaux succès sont débloqués en fonction du contexte de jeu actuel.
    /// Les succès nouvellement obtenus sont ajoutés à la liste _nouveaux.
    /// </summary>
    /// <param name="ctx">Objet contenant les compteurs et états nécessaires à la validation.</param>
    public void Verifier(ContexteSucces ctx)
    {
        _nouveaux.Clear();

        // Vérification en masse de toutes les conditions
        TenterDebloquer("first_blood", ctx.TotalKills >= 1);
        TenterDebloquer("kill_10", ctx.TotalKills >= 10);
        TenterDebloquer("kill_50", ctx.TotalKills >= 50);
        TenterDebloquer("kill_100", ctx.TotalKills >= 100);
        
        TenterDebloquer("first_boss", ctx.BossVaincus >= 1);
        TenterDebloquer("boss_3", ctx.BossVaincus >= 3);
        
        TenterDebloquer("no_death", ctx.VictoireSansMort);
        TenterDebloquer("solo", ctx.VictoireSoloHeros);
        
        TenterDebloquer("level_5", ctx.NiveauMaxAtteint >= 5);
        TenterDebloquer("level_10", ctx.NiveauMaxAtteint >= 10);
        
        TenterDebloquer("arena_5", ctx.VaguesArene >= 5);
        TenterDebloquer("arena_10", ctx.VaguesArene >= 10);
        TenterDebloquer("donjon_5", ctx.DonjonsProfondeur >= 5);
        
        TenterDebloquer("win_3", ctx.TotalVictoires >= 3);
        TenterDebloquer("win_10", ctx.TotalVictoires >= 10);
    }

    /// <summary>
    /// Tente de débloquer un succès spécifique si la condition est vraie.
    /// </summary>
    private void TenterDebloquer(string id, bool condition)
    {
        var succes = _succes.FirstOrDefault(s => s.Id == id);
        // Si succès existe, n'est pas encore débloqué, et condition remplie
        if (succes != null && !succes.Debloque && condition)
        {
            succes.Debloque = true;
            succes.DateDeblocage = DateTime.Now;
            _nouveaux.Add(succes);
        }
    }

    public List<SuccesDto> ObtenirTous() => new(_succes);
    public List<SuccesDto> ObtenirDebloques() => _succes.Where(s => s.Debloque).ToList();
    
    /// <summary>
    /// Retourne la liste des succès débloqués lors de la dernière vérification.
    /// Permet d'afficher des notifications (pop-ups) au joueur.
    /// </summary>
    public List<SuccesDto> NouveauxSucces() => new(_nouveaux);

    /// <summary>
    /// Restaure l'état des succès depuis une sauvegarde.
    /// </summary>
    public void Restaurer(List<SuccesSauvegarde> sauvegardes)
    {
        foreach (var s in sauvegardes)
        {
            var succes = _succes.FirstOrDefault(x => x.Id == s.Id);
            if (succes != null)
            {
                succes.Debloque = true;
                succes.DateDeblocage = s.DateDeblocage;
            }
        }
    }
}
