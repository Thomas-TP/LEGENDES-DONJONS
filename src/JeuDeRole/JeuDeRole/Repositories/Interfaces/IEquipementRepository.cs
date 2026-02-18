using JeuDeRole.Domain.Entities;

namespace JeuDeRole.Repositories.Interfaces;

public interface IEquipementRepository
{
    List<Equipement> ChargerTous();
    Equipement? ChargerParNom(string nom);
}
