using JeuDeRole.Services.Interfaces;

namespace JeuDeRole.Services.Quetes;

/// <summary>
/// Service de gestion des quêtes et objectifs.
/// Suit la progression du joueur et débloque les récompenses lorsque les conditions sont remplies.
/// </summary>
public class QueteService : IQueteService
{
    private readonly List<Quete> _quetes;
    
    // Liste temporaire pour stocker les quêtes validées lors de la dernière vérification
    private readonly List<Quete> _nouvellesTerminees = new();

    /// <summary>
    /// Initialise le catalogue de toutes les quêtes disponibles dans le jeu.
    /// </summary>
    public QueteService()
    {
        _quetes = new List<Quete>
        {
            // Quêtes de base
            new() { Id = "q_premier_combat", Nom = "Premiers pas", Description = "Remportez votre premier combat", Objectif = "1 victoire", Icone = "⚔", RecompenseOr = 50, RecompenseXp = 20 },
            new() { Id = "q_victoires10", Nom = "Conquérant", Description = "Remportez 10 combats", Objectif = "10 victoires", Icone = "🎖", RecompenseOr = 200, RecompenseXp = 100 },
            
            // Quêtes de chasse (Monster Hunter style)
            new() { Id = "q_chasseur", Nom = "Le Chasseur", Description = "Éliminez 10 monstres", Objectif = "10 kills", Icone = "🏹", RecompenseOr = 100, RecompenseXp = 50 },
            new() { Id = "q_exterminateur", Nom = "L'Exterminateur", Description = "Éliminez 50 monstres", Objectif = "50 kills", Icone = "💀", RecompenseOr = 300, RecompenseXp = 150 },
            
            // Quêtes de Boss (Cibles prioritaires)
            new() { Id = "q_dragon", Nom = "Fléau des Dragons", Description = "Vaincre le Dragon Ancien", Objectif = "Tuer le Dragon Ancien", Icone = "🐉", RecompenseOr = 500, RecompenseXp = 300 },
            new() { Id = "q_liche", Nom = "Repos Éternel", Description = "Vaincre la Liche Ancienne", Objectif = "Tuer la Liche", Icone = "💀", RecompenseOr = 400, RecompenseXp = 250 },
            new() { Id = "q_golem", Nom = "Brise-Pierre", Description = "Vaincre le Golem de Cristal", Objectif = "Tuer le Golem", Icone = "🪨", RecompenseOr = 450, RecompenseXp = 280 },
            new() { Id = "q_hydre", Nom = "Coupeur de Têtes", Description = "Vaincre l'Hydre Venimeuse", Objectif = "Tuer l'Hydre", Icone = "🐍", RecompenseOr = 450, RecompenseXp = 280 },
            new() { Id = "q_demon", Nom = "Purificateur", Description = "Vaincre le Seigneur Démon", Objectif = "Tuer le Démon", Icone = "😈", RecompenseOr = 600, RecompenseXp = 350 },
            
            // Quêtes de progression (Mode infini/Arène)
            new() { Id = "q_arene5", Nom = "Gladiateur", Description = "Atteindre la vague 5 en arène", Objectif = "Vague 5", Icone = "🏛", RecompenseOr = 200, RecompenseXp = 100 },
            new() { Id = "q_arene10", Nom = "Champion de l'Arène", Description = "Atteindre la vague 10 en arène", Objectif = "Vague 10", Icone = "🏆", RecompenseOr = 500, RecompenseXp = 250 },
            new() { Id = "q_donjon", Nom = "Explorateur des Profondeurs", Description = "Atteindre l'étage 10 d'un donjon", Objectif = "Étage 10", Icone = "🗺", RecompenseOr = 350, RecompenseXp = 200 },
            
            // Quêtes de leveling
            new() { Id = "q_niveau5", Nom = "Vétéran", Description = "Atteindre le niveau 5 avec un héros", Objectif = "Niveau 5", Icone = "⭐", RecompenseOr = 150, RecompenseXp = 0 },
            new() { Id = "q_niveau10", Nom = "Maître Héros", Description = "Atteindre le niveau 10 avec un héros", Objectif = "Niveau 10", Icone = "🌟", RecompenseOr = 400, RecompenseXp = 0 },
            
            // Quête Ultime
            new() { Id = "q_tous_boss", Nom = "Tueur de Légendes", Description = "Vaincre les 5 boss du jeu", Objectif = "5 boss vaincus", Icone = "👑", RecompenseOr = 1000, RecompenseXp = 500 },
        };
    }

    /// <summary>
    /// Vérifie si des quêtes sont complétées en fonction du contexte de jeu actuel.
    /// Les quêtes nouvellement terminées sont stockées dans _nouvellesTerminees.
    /// </summary>
    /// <param name="ctx">Objet contenant toutes les statistiques actuelles du joueur.</param>
    public void Verifier(ContexteQuete ctx)
    {
        _nouvellesTerminees.Clear();

        // Vérification conditionnelle pour chaque quête
        // Note: La logique est centralisée ici pour éviter de disperser la validation partout.
        TenterTerminer("q_premier_combat", ctx.TotalVictoires >= 1);
        TenterTerminer("q_chasseur", ctx.TotalKills >= 10);
        TenterTerminer("q_exterminateur", ctx.TotalKills >= 50);
        
        // Boss
        TenterTerminer("q_dragon", ctx.DragonAncienVaincu);
        TenterTerminer("q_liche", ctx.LicheVaincue);
        TenterTerminer("q_golem", ctx.GolemVaincu);
        TenterTerminer("q_hydre", ctx.HydreVaincue);
        TenterTerminer("q_demon", ctx.DemonVaincu);
        
        // Challenges
        TenterTerminer("q_arene5", ctx.VaguesArene >= 5);
        TenterTerminer("q_arene10", ctx.VaguesArene >= 10);
        TenterTerminer("q_donjon", ctx.DonjonProfondeur >= 10);
        
        // Progression
        TenterTerminer("q_niveau5", ctx.NiveauMaxAtteint >= 5);
        TenterTerminer("q_niveau10", ctx.NiveauMaxAtteint >= 10);
        TenterTerminer("q_tous_boss", ctx.BossVaincus >= 5);
        TenterTerminer("q_victoires10", ctx.TotalVictoires >= 10);
    }

    /// <summary>
    /// Valide une quête spécifique si la condition est remplie et qu'elle n'est pas déjà terminée.
    /// </summary>
    private void TenterTerminer(string id, bool condition)
    {
        var quete = _quetes.FirstOrDefault(q => q.Id == id);
        // Si la quête existe, n'est pas finie, et que la condition est vraie => Validation
        if (quete != null && !quete.Terminee && condition)
        {
            quete.Terminee = true;
            quete.DateCompletion = DateTime.Now;
            _nouvellesTerminees.Add(quete);
        }
    }

    public List<Quete> ObtenirToutes() => new(_quetes);
    public List<Quete> ObtenirActives() => _quetes.Where(q => !q.Terminee).ToList();
    public List<Quete> ObtenirTerminees() => _quetes.Where(q => q.Terminee).ToList();
    
    /// <summary>
    /// Retourne la liste des quêtes complétées lors de la dernière vérification.
    /// Utile pour afficher des notifications "Quête accomplie !" à l'UI.
    /// </summary>
    public List<Quete> NouvellesQuetesTerminees() => new(_nouvellesTerminees);

    /// <summary>
    /// Restaure l'état des quêtes depuis une sauvegarde.
    /// Marque les quêtes sauvegardées comme terminées.
    /// </summary>
    public void Restaurer(List<QueteSauvegarde> sauvegardes)
    {
        foreach (var s in sauvegardes)
        {
            var quete = _quetes.FirstOrDefault(q => q.Id == s.Id);
            if (quete != null)
            {
                quete.Terminee = true;
                quete.DateCompletion = s.DateCompletion;
            }
        }
    }
}
