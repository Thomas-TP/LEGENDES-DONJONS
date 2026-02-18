using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Repositories.Interfaces;

namespace JeuDeRole.Repositories.InMemory;

public class MemoireObjetRepository : IObjetRepository
{
    public List<ObjetConsommable> ChargerTous()
    {
        return new List<ObjetConsommable>
        {
            new("Potion de soin", "Restaure 30 PV", 5,
                cible => cible.Soigner(30)),
            new("Grande potion de soin", "Restaure 60 PV", 3,
                cible => cible.Soigner(60)),
            new("Potion de mana", "Restaure 20 PM", 5,
                cible => cible.RestaurerMana(20)),
            new("Grande potion de mana", "Restaure 40 PM", 3,
                cible => cible.RestaurerMana(40)),
            new("Antidote", "Soigne le poison", 3,
                cible => cible.AppliquerStatut(StatutEffet.Aucun)),
        };
    }
}
