using JeuDeRole.Domain.Enums;

namespace JeuDeRole.Domain.Models;

public class EffetActif
{
    public StatutEffet Statut { get; }
    public int ToursRestants { get; private set; }
    public int BonusValeur { get; }

    public EffetActif(StatutEffet statut, int duree, int bonusValeur = 0)
    {
        Statut = statut;
        ToursRestants = duree;
        BonusValeur = bonusValeur;
    }

    public bool EstActif => ToursRestants > 0;

    public void DecrementerTour()
    {
        if (ToursRestants > 0)
            ToursRestants--;
    }
}
