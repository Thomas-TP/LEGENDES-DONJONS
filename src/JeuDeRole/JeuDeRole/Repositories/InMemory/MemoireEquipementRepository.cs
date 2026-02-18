using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.ValueObjects;
using JeuDeRole.Repositories.Interfaces;

namespace JeuDeRole.Repositories.InMemory;

public class MemoireEquipementRepository : IEquipementRepository
{
    private readonly List<Equipement> _equipements = new()
    {
        new Equipement("Épée en fer", TypeEquipement.Arme,
            new Stats(0, 0, 5, 0, 0, 0, 0)),
        new Equipement("Bâton magique", TypeEquipement.Arme,
            new Stats(0, 10, 0, 6, 0, 0, 2)),
        new Equipement("Dague d'ombre", TypeEquipement.Arme,
            new Stats(0, 0, 3, 0, 4, 0, 0)),
        new Equipement("Masse sacrée", TypeEquipement.Arme,
            new Stats(0, 5, 4, 3, 0, 0, 0)),
        new Equipement("Armure de plates", TypeEquipement.Armure,
            new Stats(10, 0, 0, 0, -2, 8, 2)),
        new Equipement("Robe enchantée", TypeEquipement.Armure,
            new Stats(5, 15, 0, 3, 0, 2, 6)),
        new Equipement("Armure de cuir", TypeEquipement.Armure,
            new Stats(5, 0, 0, 0, 2, 4, 2)),
        new Equipement("Anneau de force", TypeEquipement.Accessoire,
            new Stats(0, 0, 3, 0, 0, 0, 0)),
        new Equipement("Amulette de sagesse", TypeEquipement.Accessoire,
            new Stats(0, 10, 0, 4, 0, 0, 3)),
        new Equipement("Bottes de vitesse", TypeEquipement.Accessoire,
            new Stats(0, 0, 0, 0, 5, 0, 0)),
    };

    public List<Equipement> ChargerTous()
    {
        return new List<Equipement>(_equipements);
    }

    public Equipement? ChargerParNom(string nom)
    {
        return _equipements.FirstOrDefault(e => e.Nom == nom);
    }
}
