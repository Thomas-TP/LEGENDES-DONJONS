using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Logging;

namespace JeuDeRole.Web.Services;

public class WebCombatLogger : ICombatLogger
{
    private readonly List<CombatLogEntry> _logs = new();
    private readonly object _lock = new();

    public void LogAction(string message) => Add("action", message);
    public void LogDegats(ICombattant source, ICombattant cible, int degats)
        => Add("degats", $"{source.Nom} inflige {degats} dégâts à {cible.Nom} ! (PV: {cible.PointsDeVie})");
    public void LogSoin(ICombattant source, ICombattant cible, int montant)
        => Add("soin", $"{source.Nom} soigne {cible.Nom} de {montant} PV ! (PV: {cible.PointsDeVie})");
    public void LogMort(ICombattant combattant)
        => Add("mort", $"{combattant.Nom} est vaincu !");
    public void LogStatut(ICombattant cible, StatutEffet statut)
        => Add("statut", $"{cible.Nom} est affecté par {statut} !");
    public void LogDebutTour(int numeroTour)
        => Add("tour", $"--- Tour {numeroTour} ---");
    public void LogFinCombat(bool victoireHeros)
        => Add("fin", victoireHeros ? "Victoire des héros !" : "Défaite...");
    public void LogExperience(string nomHeros, int xp, int niveauActuel, bool levelUp)
        => Add("xp", levelUp ? $"{nomHeros} gagne {xp} XP et monte au niveau {niveauActuel} !" : $"{nomHeros} gagne {xp} XP (Niv.{niveauActuel})");
    public void LogPhaseChangement(string nomBoss, int phase, string nomPhase)
        => Add("phase", $"{nomBoss} entre en {nomPhase} !");
    public void LogActionMonstre(string nomMonstre, string nomCompetence, string nomCible, bool estAoE)
        => Add("monstre", estAoE ? $"{nomMonstre} utilise {nomCompetence} sur tous !" : $"{nomMonstre} utilise {nomCompetence} sur {nomCible} !");
    public void LogDebutActionMonstre(string nomMonstre)
        => Add("monstre", $"Au tour de {nomMonstre}...");
    public void LogDefense(string nomCombattant)
        => Add("defense", $"{nomCombattant} se met en défense !");
    public void LogCritique(ICombattant source, ICombattant cible, int degats)
        => Add("critique", $"CRITIQUE ! {source.Nom} inflige {degats} dégâts à {cible.Nom} ! (PV: {cible.PointsDeVie})");
    public void LogEsquive(ICombattant cible)
        => Add("esquive", $"{cible.Nom} esquive l'attaque !");
    public void LogElement(Element element, double multiplicateur)
    {
        if (Math.Abs(multiplicateur - 1.0) > 0.01)
            Add("element", multiplicateur > 1.0 ? $"C'est super efficace ! ({element} x{multiplicateur:F1})" : $"Ce n'est pas très efficace... ({element} x{multiplicateur:F1})");
    }

    private void Add(string type, string message)
    {
        lock (_lock) { _logs.Add(new CombatLogEntry(type, message)); }
    }

    public List<CombatLogEntry> Flush()
    {
        lock (_lock)
        {
            var copy = new List<CombatLogEntry>(_logs);
            _logs.Clear();
            return copy;
        }
    }
}

public record CombatLogEntry(string Type, string Message);
