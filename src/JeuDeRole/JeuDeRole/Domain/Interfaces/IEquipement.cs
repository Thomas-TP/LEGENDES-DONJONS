using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.ValueObjects;

namespace JeuDeRole.Domain.Interfaces;

public interface IEquipement
{
    string Nom { get; }
    TypeEquipement Type { get; }
    Stats BonusStats { get; }
}
