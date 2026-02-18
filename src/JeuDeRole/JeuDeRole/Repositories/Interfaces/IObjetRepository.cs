using JeuDeRole.Domain.Entities;

namespace JeuDeRole.Repositories.Interfaces;

public interface IObjetRepository
{
    List<ObjetConsommable> ChargerTous();
}
