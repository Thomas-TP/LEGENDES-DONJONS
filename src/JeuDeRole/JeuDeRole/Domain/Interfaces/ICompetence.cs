using JeuDeRole.Domain.Enums;

namespace JeuDeRole.Domain.Interfaces;

public interface ICompetence
{
    string Nom { get; }
    int CoutMana { get; }
    int Puissance { get; }
    TypeDegat TypeDegat { get; }
    CibleType Cible { get; }
    StatutEffet EffetSecondaire { get; }
    int DureeEffet { get; }
    int NiveauRequis { get; }
    Element Element { get; }
}
