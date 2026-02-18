using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;

namespace JeuDeRole.Logging;

public interface ICombatLogger
{
    void LogAction(string message);
    void LogDegats(ICombattant source, ICombattant cible, int degats);
    void LogSoin(ICombattant source, ICombattant cible, int montant);
    void LogMort(ICombattant combattant);
    void LogStatut(ICombattant cible, StatutEffet statut);
    void LogDebutTour(int numeroTour);
    void LogFinCombat(bool victoireHeros);
    void LogExperience(string nomHeros, int xp, int niveauActuel, bool levelUp);
    void LogPhaseChangement(string nomBoss, int phase, string nomPhase);
    void LogActionMonstre(string nomMonstre, string nomCompetence, string nomCible, bool estAoE);
    void LogDebutActionMonstre(string nomMonstre);
    void LogDefense(string nomCombattant);
    void LogCritique(ICombattant source, ICombattant cible, int degats);
    void LogEsquive(ICombattant cible);
    void LogElement(Element element, double multiplicateur);
}
