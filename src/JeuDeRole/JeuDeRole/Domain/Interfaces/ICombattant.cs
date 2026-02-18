using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Models;
using JeuDeRole.Domain.ValueObjects;

namespace JeuDeRole.Domain.Interfaces;

public interface ICombattant
{
    string Nom { get; }
    int PointsDeVie { get; }
    int PointsDeMana { get; }
    Stats StatsActuelles { get; }
    bool EstVivant { get; }
    StatutEffet StatutActuel { get; }
    List<EffetActif> EffetsActifs { get; }
    Dictionary<Element, double> Resistances { get; }
    List<ICompetence> GetCompetences();
    void SubirDegats(int montant);
    void Soigner(int montant);
    void ConsommerMana(int montant);
    void RestaurerMana(int montant);
    void AppliquerStatut(StatutEffet statut);
    void AjouterEffet(EffetActif effet);
    void MettreAJourEffets();
    bool PossedeEffet(StatutEffet statut);
    double GetResistance(Element element);
}
