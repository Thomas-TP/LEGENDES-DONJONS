namespace JeuDeRole.Domain.Models;

public class ResultatCombat
{
    public bool VictoireHeros { get; init; }
    public int TotalDegatsInfliges { get; init; }
    public int TotalSoinsProdigues { get; init; }
    public int NombreTours { get; init; }
    public int ExperienceGagnee { get; init; }
    public DateTime Date { get; init; } = DateTime.Now;
    public List<string> HerosParticipants { get; init; } = new();
    public List<string> MonstresAffrontes { get; init; } = new();
    public Dictionary<string, int> DegatsParHeros { get; init; } = new();
}
