using JeuDeRole.Domain.Entities;
using JeuDeRole.Domain.Enums;
using JeuDeRole.Domain.Interfaces;
using JeuDeRole.Domain.Models;
using JeuDeRole.Factories;
using JeuDeRole.Repositories.Interfaces;
using JeuDeRole.Services.Combat;
using JeuDeRole.Services.Interfaces;

namespace JeuDeRole.Web.Services;

public class GameSessionService
{
    private readonly IPersonnageFactory _personnageFactory;
    private readonly IMonstreFactory _monstreFactory;
    private readonly IEquipementRepository _equipementRepo;
    private readonly IObjetRepository _objetRepo;
    private readonly CombatService _combatService;
    private readonly IHistoriqueService _historiqueService;
    private readonly ISauvegardeService _sauvegardeService;
    private readonly IBestiaireService _bestiaireService;
    private readonly ISuccesService _succesService;
    private readonly IEvenementService _evenementService;
    private readonly IDonjonService _donjonService;
    private readonly IDialogueService _dialogueService;
    private readonly IBoutiqueService _boutiqueService;
    private readonly IQueteService _queteService;
    private readonly WebCombatLogger _logger;

    // Game state
    private List<Heros> _equipe = new();
    private Inventaire _inventaire = new();
    private Difficulte _difficulte = Difficulte.Normal;
    private readonly List<string> _bossVaincusNoms = new();
    private int _vaguesAreneMax;
    private int _donjonProfondeurMax;
    private readonly string _savePath;

    // Combat state
    private CombatSessionState? _combatState;
    private readonly object _combatLock = new();

    public GameSessionService(
        IPersonnageFactory personnageFactory, IMonstreFactory monstreFactory,
        IEquipementRepository equipementRepo, IObjetRepository objetRepo,
        CombatService combatService, IHistoriqueService historiqueService,
        ISauvegardeService sauvegardeService, IBestiaireService bestiaireService,
        ISuccesService succesService, IEvenementService evenementService,
        IDonjonService donjonService, IDialogueService dialogueService,
        IBoutiqueService boutiqueService, IQueteService queteService,
        WebCombatLogger logger)
    {
        _personnageFactory = personnageFactory;
        _monstreFactory = monstreFactory;
        _equipementRepo = equipementRepo;
        _objetRepo = objetRepo;
        _combatService = combatService;
        _historiqueService = historiqueService;
        _sauvegardeService = sauvegardeService;
        _bestiaireService = bestiaireService;
        _succesService = succesService;
        _evenementService = evenementService;
        _donjonService = donjonService;
        _dialogueService = dialogueService;
        _boutiqueService = boutiqueService;
        _queteService = queteService;
        _logger = logger;
        _savePath = Path.Combine(AppContext.BaseDirectory, "sauvegarde_web.json");

        _combatService.DemanderActionJoueur = OnDemanderAction;
        InitInventaire();
    }

    private void InitInventaire()
    {
        _inventaire = new Inventaire();
        foreach (var obj in _objetRepo.ChargerTous())
            _inventaire.Ajouter(obj);
    }

    private void HealTeam()
    {
        foreach (var h in _equipe)
        {
            h.Soigner(h.StatsActuelles.PointsDeVieMax);
            h.RestaurerMana(h.StatsActuelles.PointsDeManaMax);
            h.EffetsActifs.Clear();
            h.AppliquerStatut(StatutEffet.Aucun);
        }
    }

    // ───────────────────── State ─────────────────────
    public object GetFullState()
    {
        return new
        {
            hasTeam = _equipe.Count > 0,
            difficulty = _difficulte.ToString(),
            gold = _boutiqueService.Or,
            team = _equipe.Select(MapHero).ToList(),
            bossesDefeated = _bossVaincusNoms.ToList(),
            arenaMaxWaves = _vaguesAreneMax,
            dungeonMaxDepth = _donjonProfondeurMax,
            totalWins = _historiqueService.TotalVictoires,
            totalLosses = _historiqueService.TotalDefaites,
            inventory = _inventaire.ListerObjets().Select(o => new { o.Nom, o.Description, o.Quantite }).ToList(),
            hasSave = _sauvegardeService.SauvegardeExiste(_savePath),
            allBossTypes = new[] { "Liche", "DragonAncien", "GolemCristal", "Hydre", "SeigneurDemon" },
            allBossesDefeated = _bossVaincusNoms.Count >= 5
        };
    }

    private static object MapHero(Heros h) => new
    {
        nom = h.Nom,
        classe = h.Classe.ToString(),
        niveau = h.Niveau,
        pv = h.PointsDeVie,
        pvMax = h.StatsActuelles.PointsDeVieMax,
        pm = h.PointsDeMana,
        pmMax = h.StatsActuelles.PointsDeManaMax,
        xp = h.Experience,
        xpNext = h.ExperiencePourProchainNiveau,
        stats = MapStats(h.StatsActuelles),
        arme = h.Arme != null ? new { h.Arme.Nom, type = h.Arme.Type.ToString(), bonus = MapStats(h.Arme.BonusStats) } : null,
        armure = h.Armure != null ? new { h.Armure.Nom, type = h.Armure.Type.ToString(), bonus = MapStats(h.Armure.BonusStats) } : null,
        accessoire = h.Accessoire != null ? new { h.Accessoire.Nom, type = h.Accessoire.Type.ToString(), bonus = MapStats(h.Accessoire.BonusStats) } : null,
        competences = h.GetCompetences().Where(c => c.NiveauRequis <= h.Niveau).Select(MapCompetence).ToList(),
        effets = h.EffetsActifs.Select(e => new { statut = e.Statut.ToString(), tours = e.ToursRestants }).ToList(),
        estVivant = h.EstVivant
    };

    private static object MapStats(JeuDeRole.Domain.ValueObjects.Stats s) => new
    {
        pvMax = s.PointsDeVieMax, pmMax = s.PointsDeManaMax,
        force = s.Force, intelligence = s.Intelligence,
        agilite = s.Agilite, defense = s.Defense, resMag = s.ResistanceMagique
    };

    private static object MapCompetence(ICompetence c) => new
    {
        nom = c.Nom, cout = c.CoutMana, puissance = c.Puissance,
        typeDegat = c.TypeDegat.ToString(), cible = c.Cible.ToString(),
        element = c.Element.ToString(), effet = c.EffetSecondaire.ToString(),
        niveauRequis = c.NiveauRequis
    };

    private static object MapFighter(ICombattant c) => new
    {
        nom = c.Nom, pv = c.PointsDeVie, pvMax = c.StatsActuelles.PointsDeVieMax,
        pm = c.PointsDeMana, pmMax = c.StatsActuelles.PointsDeManaMax,
        stats = MapStats(c.StatsActuelles), estVivant = c.EstVivant,
        statut = c.StatutActuel.ToString(),
        effets = c.EffetsActifs.Select(e => new { statut = e.Statut.ToString(), tours = e.ToursRestants }).ToList(),
        competences = c.GetCompetences().Select(MapCompetence).ToList(),
        isBoss = c is Boss,
        phase = c is Boss b ? b.PhaseActuelle : 0,
        totalPhases = c is Boss b2 ? b2.NombrePhases : 0
    };

    // ───────────────────── Difficulty ─────────────────────
    public void SetDifficulty(string difficulty)
    {
        if (Enum.TryParse<Difficulte>(difficulty, true, out var d))
            _difficulte = d;
    }

    // ───────────────────── Classes ─────────────────────
    public object GetClasses()
    {
        var descs = new Dictionary<string, string>
        {
            ["Guerrier"] = "Tank puissant, haute défense et PV",
            ["Mage"] = "Dégâts magiques AoE, sorts élémentaires",
            ["Voleur"] = "Rapide et agile, poisons et critiques",
            ["Clerc"] = "Soins et purification, lumière sacrée",
            ["Paladin"] = "Tank/healer hybride, dégâts sacrés",
            ["Necromancien"] = "Magie noire, drains de vie et malédictions",
            ["Assassin"] = "Ultra rapide, dégâts single-target massifs",
            ["Druide"] = "Polyvalent, soins et magie élémentaire"
        };
        return Enum.GetValues<ClasseHeros>().Select(c => new { name = c.ToString(), description = descs.GetValueOrDefault(c.ToString(), "") });
    }

    // ───────────────────── Equipment ─────────────────────
    public object GetEquipment()
    {
        return _equipementRepo.ChargerTous().Select(e => new
        {
            nom = e.Nom, type = e.Type.ToString(), bonus = MapStats(e.BonusStats)
        });
    }

    // ───────────────────── Team ─────────────────────
    public void CreateTeam(List<HeroCreation> heroes)
    {
        _equipe.Clear();
        foreach (var h in heroes)
        {
            if (Enum.TryParse<ClasseHeros>(h.ClassName, true, out var classe))
                _equipe.Add(_personnageFactory.CreerHeros(h.Name, classe));
        }
        InitInventaire();
    }

    public void EquipHero(int heroIndex, string slot, string itemName)
    {
        if (heroIndex < 0 || heroIndex >= _equipe.Count) return;
        var eq = _equipementRepo.ChargerParNom(itemName);
        if (eq == null) return;
        _equipe[heroIndex].Equiper(eq);
    }

    // ───────────────────── Combat Bridge ─────────────────────
    private ActionCombat OnDemanderAction(Heros hero, List<ICombattant> ennemis, List<ICombattant> allies, Inventaire inventaire)
    {
        lock (_combatLock)
        {
            if (_combatState == null) return ActionCombat.Defendre(hero);
            _combatState.WaitingForAction = true;
            _combatState.ActiveHero = hero;
            _combatState.Enemies = ennemis;
            _combatState.Allies = allies;
            _combatState.Logs.AddRange(_logger.Flush());
        }

        _combatState!.ActionReady.Reset();
        _combatState.ActionReady.Wait();

        lock (_combatLock)
        {
            _combatState.WaitingForAction = false;
            if (_combatState.Abandoned)
            {
                // Kill all heroes to end combat immediately as a loss
                foreach (var h in _equipe.Where(h => h.EstVivant))
                    h.SubirDegats(h.PointsDeVie + 9999);
                return ActionCombat.Defendre(hero);
            }
            var action = _combatState.PendingAction ?? ActionCombat.Defendre(hero);
            _combatState.PendingAction = null;
            return action;
        }
    }

    public object StartCombat()
    {
        HealTeam();
        var monstres = _monstreFactory.GenererGroupeAleatoire(Random.Shared.Next(2, 5));
        ApplyDifficulty(monstres);
        return RunCombat(monstres, "combat");
    }

    public object StartBossCombat(string bossType)
    {
        HealTeam();
        var boss = _monstreFactory.CreerBoss(bossType);
        ApplyDifficulty(new List<Monstre> { boss });
        var dialogue = _dialogueService.ObtenirDialogueBoss(bossType);
        var state = RunCombat(new List<Monstre> { boss }, "boss");
        return new { dialogue, combat = state };
    }

    private object RunCombat(List<Monstre> monstres, string mode)
    {
        foreach (var m in monstres)
        {
            _bestiaireService.EnregistrerMonstre(m.Nom, m.StatsActuelles.PointsDeVieMax,
                m.StatsActuelles.Force, m.StatsActuelles.Defense, m.Resistances, m.ExperienceDonnee);
        }

        _logger.Flush();
        var session = new CombatSessionState { Mode = mode, Monstres = monstres };
        lock (_combatLock) { _combatState = session; }

        session.CombatTask = Task.Run(() =>
        {
            try
            {
                session.Result = _combatService.LancerCombat(_equipe, monstres, _inventaire);
            }
            catch (Exception ex)
            {
                session.Error = ex.Message;
            }
        });

        // Wait a bit for combat to start and potentially reach first player turn
        SpinWait.SpinUntil(() => session.WaitingForAction || session.CombatTask.IsCompleted, 5000);

        return BuildCombatResponse(session);
    }

    public object SubmitAction(CombatActionRequest request)
    {
        lock (_combatLock)
        {
            if (_combatState == null || !_combatState.WaitingForAction)
                return new { error = "No action expected" };
        }

        var hero = _combatState!.ActiveHero!;
        var ennemis = _combatState.Enemies!;
        var allies = _combatState.Allies!;
        ActionCombat action;

        switch (request.Type?.ToLower())
        {
            case "attack":
            case "attaquer":
                var comps = hero.GetCompetences().Where(c => c.NiveauRequis <= hero.Niveau).ToList();
                if (request.CompetenceIndex < 0 || request.CompetenceIndex >= comps.Count)
                    action = ActionCombat.Defendre(hero);
                else
                {
                    var comp = comps[request.CompetenceIndex];
                    List<ICombattant> cibles;
                    if (comp.Cible == CibleType.TousLesEnnemis)
                        cibles = ennemis.Where(e => e.EstVivant).ToList();
                    else if (comp.Cible == CibleType.Soi)
                        cibles = new List<ICombattant> { hero };
                    else if (comp.Cible == CibleType.UnAllie)
                    {
                        var idx = Math.Clamp(request.TargetIndex, 0, allies.Count - 1);
                        cibles = new List<ICombattant> { allies[idx] };
                    }
                    else
                    {
                        var idx = Math.Clamp(request.TargetIndex, 0, ennemis.Count - 1);
                        var cible = ennemis[idx];
                        if (!cible.EstVivant)
                            cible = ennemis.FirstOrDefault(e => e.EstVivant) ?? ennemis[0];
                        cibles = new List<ICombattant> { cible };
                    }
                    action = ActionCombat.Attaquer(hero, comp, cibles);
                }
                break;

            case "defend":
            case "defendre":
                action = ActionCombat.Defendre(hero);
                break;

            case "item":
            case "objet":
                var objets = _inventaire.ListerObjets();
                if (request.ObjectIndex >= 0 && request.ObjectIndex < objets.Count)
                {
                    var obj = objets[request.ObjectIndex];
                    ICombattant cible;
                    if (request.TargetIndex >= 0 && request.TargetIndex < allies.Count)
                        cible = allies[request.TargetIndex];
                    else
                        cible = hero;
                    action = ActionCombat.UtiliserObjet(hero, obj, cible);
                }
                else
                    action = ActionCombat.Defendre(hero);
                break;

            default:
                action = ActionCombat.Defendre(hero);
                break;
        }

        lock (_combatLock) { _combatState!.PendingAction = action; }
        _combatState!.ActionReady.Set();

        // Wait for combat to process and reach next decision point
        SpinWait.SpinUntil(() => _combatState.WaitingForAction || _combatState.CombatTask!.IsCompleted, 5000);

        return BuildCombatResponse(_combatState);
    }

    public object GetCombatState()
    {
        lock (_combatLock)
        {
            if (_combatState == null) return new { active = false, finished = false };
            return BuildCombatResponse(_combatState);
        }
    }

    public object AbandonCombat()
    {
        lock (_combatLock)
        {
            if (_combatState == null) return new { active = false, finished = true, victory = false, abandoned = true };
            _combatState.Abandoned = true;
        }
        _combatState!.ActionReady.Set();
        // Wait for combat to finish
        SpinWait.SpinUntil(() => _combatState?.CombatTask?.IsCompleted == true, 5000);
        lock (_combatLock)
        {
            // Clean up arena/dungeon state too
            _arenaState = null;
            _dungeonState = null;
            if (_combatState != null)
            {
                var resp = BuildCombatResponse(_combatState);
                _combatState = null;
                return resp;
            }
            return new { active = false, finished = true, victory = false, abandoned = true };
        }
    }

    private object BuildCombatResponse(CombatSessionState session)
    {
        lock (_combatLock)
        {
            session.Logs.AddRange(_logger.Flush());
            var logs = session.Logs.ToList();

            if (session.CombatTask?.IsCompleted == true && (session.Result != null || session.Error != null))
            {
                if (!session.ResultProcessed && session.Result != null)
                {
                    ProcessCombatResult(session.Result, session.Monstres);
                    session.ResultProcessed = true;
                }
                var result = session.Result;
                session.Logs.Clear();
                return new
                {
                    active = false, finished = true,
                    victory = result?.VictoireHeros ?? false,
                    result = result != null ? new
                    {
                        totalDamage = result.TotalDegatsInfliges,
                        totalHealing = result.TotalSoinsProdigues,
                        turns = result.NombreTours,
                        xp = result.ExperienceGagnee,
                        damagePerHero = result.DegatsParHeros,
                        monstersDefeated = result.MonstresAffrontes
                    } : null,
                    logs,
                    team = _equipe.Select(MapHero).ToList(),
                    gold = _boutiqueService.Or
                };
            }

            session.Logs.Clear();
            return new
            {
                active = true, finished = false,
                waitingForAction = session.WaitingForAction,
                activeHero = session.ActiveHero != null ? MapHero(session.ActiveHero) : null,
                enemies = session.Enemies?.Select(MapFighter).ToList(),
                allies = session.Allies?.Select(MapFighter).ToList(),
                inventory = _inventaire.ListerObjets().Select(o => new { o.Nom, o.Description, o.Quantite }).ToList(),
                logs,
                team = _equipe.Select(MapHero).ToList()
            };
        }
    }

    private void ProcessCombatResult(ResultatCombat result, List<Monstre> monstres)
    {
        _historiqueService.AjouterResultat(result);
        if (result.VictoireHeros)
        {
            int orGagne = monstres.Sum(m => m.ExperienceDonnee / 2);
            _boutiqueService.AjouterOr(orGagne);

            foreach (var m in monstres.Where(m => !m.EstVivant))
                _bestiaireService.EnregistrerKill(m.Nom);

            foreach (var m in monstres.OfType<Boss>())
                if (!_bossVaincusNoms.Contains(m.Nom))
                    _bossVaincusNoms.Add(m.Nom);
        }

        CheckAchievementsAndQuests();
    }

    private void CheckAchievementsAndQuests()
    {
        var niveauMax = _equipe.Count > 0 ? _equipe.Max(h => h.Niveau) : 1;
        _succesService.Verifier(new ContexteSucces
        {
            TotalKills = _bestiaireService.TotalKills,
            TotalVictoires = _historiqueService.TotalVictoires,
            TotalDefaites = _historiqueService.TotalDefaites,
            BossVaincus = _bossVaincusNoms.Count,
            NiveauMaxAtteint = niveauMax,
            VaguesArene = _vaguesAreneMax,
            DonjonsProfondeur = _donjonProfondeurMax
        });
        _queteService.Verifier(new ContexteQuete
        {
            TotalKills = _bestiaireService.TotalKills,
            TotalVictoires = _historiqueService.TotalVictoires,
            BossVaincus = _bossVaincusNoms.Count,
            NiveauMaxAtteint = niveauMax,
            VaguesArene = _vaguesAreneMax,
            DonjonProfondeur = _donjonProfondeurMax,
            DragonAncienVaincu = _bossVaincusNoms.Contains("Dragon Ancien"),
            LicheVaincue = _bossVaincusNoms.Contains("Liche Ancienne"),
            GolemVaincu = _bossVaincusNoms.Contains("Golem de Cristal"),
            HydreVaincue = _bossVaincusNoms.Contains("Hydre Venimeuse"),
            DemonVaincu = _bossVaincusNoms.Contains("Seigneur Démon")
        });
    }

    // ───────────────────── Arena ─────────────────────
    private ArenaState? _arenaState;

    public object StartArena()
    {
        HealTeam();
        _arenaState = new ArenaState { Wave = 1 };
        var intro = _dialogueService.ObtenirIntroArene();
        var monstres = _monstreFactory.GenererGroupeAleatoire(3);
        ApplyDifficulty(monstres);
        _arenaState.CurrentMonsters = monstres;
        var combatResult = RunCombat(monstres, "arena");
        return new { intro, wave = _arenaState.Wave, combat = combatResult };
    }

    public object ArenaRest(int choice)
    {
        if (_arenaState == null) return new { error = "No arena active" };

        // Heal team between waves
        foreach (var h in _equipe.Where(h => h.EstVivant))
        {
            h.Soigner(h.StatsActuelles.PointsDeVieMax / 3);
            h.RestaurerMana(h.StatsActuelles.PointsDeManaMax / 3);
        }

        _arenaState.Wave++;
        if (_arenaState.Wave > _vaguesAreneMax)
            _vaguesAreneMax = _arenaState.Wave;

        double scaling = 1.0 + (_arenaState.Wave - 1) * 0.20;
        int count = Math.Min(5, 2 + _arenaState.Wave / 3);
        var monstres = _monstreFactory.GenererGroupeAleatoire(count);
        ApplyDifficulty(monstres);
        foreach (var m in monstres) m.AppliquerScaling(scaling);
        _arenaState.CurrentMonsters = monstres;

        var combatResult = RunCombat(monstres, "arena");
        return new { wave = _arenaState.Wave, combat = combatResult };
    }

    // ───────────────────── Dungeon ─────────────────────
    private DungeonState? _dungeonState;

    public object StartDungeon(int depth)
    {
        var donjon = _donjonService.GenererDonjon(depth);
        _dungeonState = new DungeonState { Donjon = donjon, CurrentFloor = 0 };
        var intro = _dialogueService.ObtenirIntroDonjon(depth);
        return new { intro, floors = donjon.Salles.Select(MapRoom).ToList(), dungeon = DungeonProceedInternal() };
    }

    public object DungeonProceed()
    {
        return DungeonProceedInternal();
    }

    private object DungeonProceedInternal()
    {
        if (_dungeonState == null) return new { error = "No dungeon active" };
        var donjon = _dungeonState.Donjon;

        if (_dungeonState.CurrentFloor >= donjon.Salles.Count)
        {
            if (_dungeonState.CurrentFloor > _donjonProfondeurMax)
                _donjonProfondeurMax = _dungeonState.CurrentFloor;
            CheckAchievementsAndQuests();
            _dungeonState = null;
            return new { finished = true, victory = true };
        }

        var salle = donjon.Salles[_dungeonState.CurrentFloor];
        salle.Visitee = true;
        var narration = _dialogueService.ObtenirNarrationEtage(salle.Etage, salle.Type);

        switch (salle.Type)
        {
            case TypeSalle.Combat:
            case TypeSalle.MiniBoss:
                double scaling = 1.0 + _dungeonState.CurrentFloor * 0.15;
                List<Monstre> monstres;
                if (salle.Type == TypeSalle.MiniBoss)
                    monstres = new List<Monstre> { _monstreFactory.CreerMonstre("Orc") };
                else
                    monstres = _monstreFactory.GenererGroupeAleatoire(Random.Shared.Next(2, 4));
                ApplyDifficulty(monstres);
                foreach (var m in monstres) m.AppliquerScaling(scaling);
                _dungeonState.CurrentFloor++;
                var combat = RunCombat(monstres, "dungeon");
                return new { finished = false, type = salle.Type.ToString(), narration, floor = salle.Etage, combat, floors = donjon.Salles.Select(MapRoom).ToList() };

            case TypeSalle.BossFinal:
                var boss = _monstreFactory.CreerBoss("Liche");
                double bossScale = 1.0 + _dungeonState.CurrentFloor * 0.05;
                boss.AppliquerScaling(bossScale);
                ApplyDifficulty(new List<Monstre> { boss });
                _dungeonState.CurrentFloor++;
                var bossCombat = RunCombat(new List<Monstre> { boss }, "dungeon");
                return new { finished = false, type = "BossFinal", narration, floor = salle.Etage, combat = bossCombat, floors = donjon.Salles.Select(MapRoom).ToList() };

            case TypeSalle.Repos:
                foreach (var h in _equipe.Where(h => h.EstVivant))
                {
                    h.Soigner(h.StatsActuelles.PointsDeVieMax / 2);
                    h.RestaurerMana(h.StatsActuelles.PointsDeManaMax / 2);
                }
                _dungeonState.CurrentFloor++;
                return new { finished = false, type = "Repos", narration, floor = salle.Etage, team = _equipe.Select(MapHero).ToList(), floors = donjon.Salles.Select(MapRoom).ToList() };

            case TypeSalle.Evenement:
                var evt = _evenementService.GenererEvenement();
                _dungeonState.PendingEvent = evt;
                return new
                {
                    finished = false, type = "Evenement", narration, floor = salle.Etage,
                    eventType = evt.Type.ToString(), eventName = evt.Nom, eventDesc = evt.Description,
                    floors = donjon.Salles.Select(MapRoom).ToList()
                };

            default:
                _dungeonState.CurrentFloor++;
                return new { finished = false, type = "Unknown", narration, floor = salle.Etage, floors = donjon.Salles.Select(MapRoom).ToList() };
        }
    }

    public object DungeonEventChoice(string choice)
    {
        if (_dungeonState?.PendingEvent == null) return new { error = "No event" };
        var evt = _dungeonState.PendingEvent;
        _dungeonState.PendingEvent = null;
        _dungeonState.CurrentFloor++;

        var message = "";
        switch (evt.Type)
        {
            case TypeEvenement.Coffre:
                int or = Random.Shared.Next(20, 60);
                _boutiqueService.AjouterOr(or);
                message = $"Vous trouvez {or} pièces d'or !";
                break;
            case TypeEvenement.Piege:
                foreach (var h in _equipe.Where(h => h.EstVivant))
                    h.SubirDegats(Random.Shared.Next(5, 15));
                message = "Un piège s'active ! Votre équipe subit des dégâts.";
                break;
            case TypeEvenement.Fontaine:
                foreach (var h in _equipe.Where(h => h.EstVivant))
                {
                    h.Soigner(30);
                    h.RestaurerMana(15);
                }
                message = "Une fontaine magique restaure votre équipe !";
                break;
            case TypeEvenement.Sanctuaire:
                foreach (var h in _equipe.Where(h => h.EstVivant))
                {
                    h.EffetsActifs.Clear();
                    h.AppliquerStatut(StatutEffet.Aucun);
                }
                message = "Le sanctuaire purifie tous les effets négatifs !";
                break;
            case TypeEvenement.Embuscade:
                var monstres = _monstreFactory.GenererGroupeAleatoire(2);
                ApplyDifficulty(monstres);
                _dungeonState.CurrentFloor--; // Will be re-incremented after combat
                var combat = RunCombat(monstres, "dungeon");
                _dungeonState.CurrentFloor++;
                return new { message = "Embuscade !", combat, team = _equipe.Select(MapHero).ToList() };
            default:
                message = evt.Description;
                break;
        }

        return new { message, team = _equipe.Select(MapHero).ToList(), gold = _boutiqueService.Or };
    }

    private static object MapRoom(SalleDonjon s) => new
    {
        etage = s.Etage, type = s.Type.ToString(), nom = s.Nom, visitee = s.Visitee
    };

    // ───────────────────── Shop ─────────────────────
    public object GetShopItems()
    {
        return new
        {
            gold = _boutiqueService.Or,
            equipment = _boutiqueService.ObtenirEquipements().Select(a => new { a.Nom, a.Description, a.Prix, a.Categorie }),
            items = _boutiqueService.ObtenirObjets().Select(a => new { a.Nom, a.Description, a.Prix, a.Categorie })
        };
    }

    public void BuyItem(string name, string category, int quantity, int heroIndex)
    {
        var allItems = category == "Objet"
            ? _boutiqueService.ObtenirObjets()
            : _boutiqueService.ObtenirEquipements();

        var article = allItems.FirstOrDefault(a => a.Nom == name);
        if (article == null) return;

        if (_boutiqueService.Acheter(article, quantity))
        {
            if (category == "Objet")
            {
                var objDef = _objetRepo.ChargerTous().FirstOrDefault(o => o.Nom == name);
                if (objDef != null)
                {
                    for (int i = 0; i < quantity; i++)
                    {
                        var newObj = new ObjetConsommable(objDef.Nom, objDef.Description, 1,
                            cible =>
                            {
                                if (name.Contains("soin", StringComparison.OrdinalIgnoreCase))
                                    cible.Soigner(name.Contains("Grande") ? 60 : 30);
                                else if (name.Contains("mana", StringComparison.OrdinalIgnoreCase))
                                    cible.RestaurerMana(name.Contains("Grande") ? 40 : 20);
                                else if (name.Contains("Antidote", StringComparison.OrdinalIgnoreCase))
                                    cible.AppliquerStatut(StatutEffet.Aucun);
                                else if (name.Contains("puissance", StringComparison.OrdinalIgnoreCase))
                                    cible.AjouterEffet(new EffetActif(StatutEffet.BuffAttaque, 3, 5));
                            });
                        _inventaire.Ajouter(newObj);
                    }
                }
            }
            else if (heroIndex >= 0 && heroIndex < _equipe.Count)
            {
                var eq = _equipementRepo.ChargerParNom(name);
                if (eq != null) _equipe[heroIndex].Equiper(eq);
            }
        }
    }

    public void SellEquipment(int heroIndex, string slot)
    {
        if (heroIndex < 0 || heroIndex >= _equipe.Count) return;
        var hero = _equipe[heroIndex];
        Equipement? eq = slot.ToLower() switch
        {
            "arme" => hero.Arme as Equipement,
            "armure" => hero.Armure as Equipement,
            "accessoire" => hero.Accessoire as Equipement,
            _ => null
        };
        if (eq != null)
            _boutiqueService.VendreEquipement(eq);
    }

    // ───────────────────── Bestiary / Achievements / History / Quests ─────────────────────
    public object GetBestiary() => _bestiaireService.ObtenirBestiaire().Select(b => new
    {
        b.Nom, b.PvMax, b.Force, b.Defense, b.Xp, b.NombreKills,
        faiblesses = b.Faiblesses.Select(kv => new { element = kv.Key.ToString(), mult = kv.Value })
    });

    public object GetAchievements() => _succesService.ObtenirTous().Select(s => new
    {
        s.Nom, s.Description, s.Icone, s.Debloque, s.DateDeblocage
    });

    public object GetHistory() => new
    {
        wins = _historiqueService.TotalVictoires,
        losses = _historiqueService.TotalDefaites,
        total = _historiqueService.TotalCombats,
        combats = _historiqueService.ObtenirHistorique().TakeLast(20).Reverse().Select(r => new
        {
            r.VictoireHeros, r.NombreTours, r.TotalDegatsInfliges, r.TotalSoinsProdigues,
            r.ExperienceGagnee, r.Date, r.HerosParticipants, r.MonstresAffrontes, r.DegatsParHeros
        })
    };

    public object GetQuests()
    {
        return new
        {
            active = _queteService.ObtenirActives().Select(MapQuest),
            completed = _queteService.ObtenirTerminees().Select(MapQuest),
            all = _queteService.ObtenirToutes().Select(MapQuest)
        };
    }

    private static object MapQuest(Quete q) => new
    {
        q.Id, q.Nom, q.Description, q.Objectif, q.Icone,
        q.RecompenseOr, q.RecompenseXp, q.Terminee, q.DateCompletion
    };

    // ───────────────────── Save / Load ─────────────────────
    public void Save()
    {
        var donnees = new DonneesSauvegarde
        {
            Heros = _equipe.Select(h => new HerosSauvegarde
            {
                Nom = h.Nom, Classe = h.Classe.ToString(),
                Niveau = h.Niveau, Experience = h.Experience,
                PvMax = h.StatsActuelles.PointsDeVieMax, PmMax = h.StatsActuelles.PointsDeManaMax,
                Force = h.StatsActuelles.Force, Intelligence = h.StatsActuelles.Intelligence,
                Agilite = h.StatsActuelles.Agilite, Defense = h.StatsActuelles.Defense,
                ResistanceMagique = h.StatsActuelles.ResistanceMagique
            }).ToList(),
            Historique = _historiqueService.ObtenirHistorique(),
            Bestiaire = _bestiaireService.ObtenirBestiaire(),
            Succes = _succesService.ObtenirDebloques().Select(s => new SuccesSauvegarde { Id = s.Id, DateDeblocage = s.DateDeblocage }).ToList(),
            TotalVictoires = _historiqueService.TotalVictoires,
            TotalDefaites = _historiqueService.TotalDefaites,
            BossVaincus = _bossVaincusNoms.Count,
            VaguesAreneMax = _vaguesAreneMax,
            DonjonProfondeurMax = _donjonProfondeurMax,
            Or = _boutiqueService.Or,
            Quetes = _queteService.ObtenirTerminees().Select(q => new QueteSauvegarde { Id = q.Id, DateCompletion = q.DateCompletion }).ToList(),
            BossVaincusNoms = _bossVaincusNoms.ToList(),
            Difficulte = _difficulte.ToString()
        };
        _sauvegardeService.Sauvegarder(donnees, _savePath);
    }

    public void Load()
    {
        var donnees = _sauvegardeService.Charger(_savePath);
        if (donnees == null) return;

        _equipe.Clear();
        foreach (var hs in donnees.Heros)
        {
            if (!Enum.TryParse<ClasseHeros>(hs.Classe, out var classe)) continue;
            var hero = _personnageFactory.CreerHeros(hs.Nom, classe);
            for (int i = 1; i < hs.Niveau; i++)
                hero.GagnerExperience(hero.ExperiencePourProchainNiveau);
            hero.GagnerExperience(hs.Experience);
            _equipe.Add(hero);
        }

        _historiqueService.Restaurer(donnees.Historique);
        _bestiaireService.Restaurer(donnees.Bestiaire);
        _succesService.Restaurer(donnees.Succes);
        _queteService.Restaurer(donnees.Quetes);
        _boutiqueService.Restaurer(donnees.Or);
        _bossVaincusNoms.Clear();
        _bossVaincusNoms.AddRange(donnees.BossVaincusNoms);
        _vaguesAreneMax = donnees.VaguesAreneMax;
        _donjonProfondeurMax = donnees.DonjonProfondeurMax;
        if (Enum.TryParse<Difficulte>(donnees.Difficulte, out var diff))
            _difficulte = diff;

        InitInventaire();
    }

    private void ApplyDifficulty(List<Monstre> monstres)
    {
        var (multStats, multXP) = _difficulte switch
        {
            Difficulte.Facile => (0.8, 1.2),
            Difficulte.Normal => (1.2, 1.1),
            Difficulte.Difficile => (1.6, 1.4),
            Difficulte.Cauchemar => (2.2, 2.0),
            _ => (1.2, 1.1)
        };
        foreach (var m in monstres)
            m.AppliquerDifficulte(multStats, multXP);
    }

    // ───────────────────── Inner State Classes ─────────────────────
    private class CombatSessionState
    {
        public string Mode { get; set; } = "";
        public List<Monstre> Monstres { get; set; } = new();
        public Task? CombatTask { get; set; }
        public ResultatCombat? Result { get; set; }
        public string? Error { get; set; }
        public bool WaitingForAction { get; set; }
        public Heros? ActiveHero { get; set; }
        public List<ICombattant>? Enemies { get; set; }
        public List<ICombattant>? Allies { get; set; }
        public ActionCombat? PendingAction { get; set; }
        public ManualResetEventSlim ActionReady { get; } = new(false);
        public List<CombatLogEntry> Logs { get; } = new();
        public bool Abandoned { get; set; }
        public bool ResultProcessed { get; set; }
    }

    private class ArenaState
    {
        public int Wave { get; set; }
        public List<Monstre> CurrentMonsters { get; set; } = new();
    }

    private class DungeonState
    {
        public JeuDeRole.Services.Interfaces.Donjon Donjon { get; set; } = null!;
        public int CurrentFloor { get; set; }
        public EvenementAleatoire? PendingEvent { get; set; }
    }
}
