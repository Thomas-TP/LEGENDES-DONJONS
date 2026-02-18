using System.Diagnostics;
using JeuDeRole.Factories;
using JeuDeRole.Logging;
using JeuDeRole.Repositories.InMemory;
using JeuDeRole.Repositories.Interfaces;
using JeuDeRole.Services.Bestiaire;
using JeuDeRole.Services.Boutique;
using JeuDeRole.Services.Combat;
using JeuDeRole.Services.Dialogue;
using JeuDeRole.Services.Donjon;
using JeuDeRole.Services.Evenements;
using JeuDeRole.Services.Historique;
using JeuDeRole.Services.Interfaces;
using JeuDeRole.Services.Inventaire;
using JeuDeRole.Services.Quetes;
using JeuDeRole.Services.Sauvegarde;
using JeuDeRole.Services.Succes;
using JeuDeRole.Strategies.Degats;
using JeuDeRole.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<WebCombatLogger>();
builder.Services.AddSingleton<ICombatLogger>(sp => sp.GetRequiredService<WebCombatLogger>());
builder.Services.AddSingleton<IPersonnageFactory, PersonnageFactory>();
builder.Services.AddSingleton<IMonstreFactory, MonstreFactory>();
builder.Services.AddSingleton<IEquipementRepository, MemoireEquipementRepository>();
builder.Services.AddSingleton<IObjetRepository, MemoireObjetRepository>();
builder.Services.AddSingleton<IHistoriqueService, HistoriqueService>();
builder.Services.AddSingleton<ISauvegardeService, SauvegardeService>();
builder.Services.AddSingleton<IBestiaireService, BestiaireService>();
builder.Services.AddSingleton<ISuccesService, SuccesService>();
builder.Services.AddSingleton<IEvenementService, EvenementService>();
builder.Services.AddSingleton<IDonjonService, DonjonService>();
builder.Services.AddSingleton<IDialogueService, DialogueService>();
builder.Services.AddSingleton<IBoutiqueService, BoutiqueService>();
builder.Services.AddSingleton<IQueteService, QueteService>();
builder.Services.AddSingleton<CombatService>(sp =>
{
    var logger = sp.GetRequiredService<ICombatLogger>();
    var invService = new InventaireService(logger);
    return new CombatService(logger, new CalculDegatsPhysiques(), new CalculDegatsMagiques(), invService);
});
builder.Services.AddSingleton<GameSessionService>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();
app.Services.GetRequiredService<GameSessionService>();

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/state", (GameSessionService gs) => gs.GetFullState());
app.MapPost("/api/difficulty", (GameSessionService gs, DifficultyRequest r) => { gs.SetDifficulty(r.Difficulty); return gs.GetFullState(); });
app.MapPost("/api/save", (GameSessionService gs) => { gs.Save(); return Results.Ok(); });
app.MapPost("/api/load", (GameSessionService gs) => { gs.Load(); return gs.GetFullState(); });
app.MapGet("/api/classes", (GameSessionService gs) => gs.GetClasses());
app.MapGet("/api/equipment", (GameSessionService gs) => gs.GetEquipment());
app.MapPost("/api/team/create", (GameSessionService gs, CreateTeamRequest r) => { gs.CreateTeam(r.Heroes); return gs.GetFullState(); });
app.MapPost("/api/team/equip", (GameSessionService gs, EquipRequest r) => { gs.EquipHero(r.HeroIndex, r.Slot, r.ItemName); return gs.GetFullState(); });
app.MapPost("/api/combat/start", (GameSessionService gs) => gs.StartCombat());
app.MapPost("/api/combat/boss", (GameSessionService gs, BossRequest r) => gs.StartBossCombat(r.BossType));
app.MapPost("/api/combat/action", (GameSessionService gs, CombatActionRequest r) => gs.SubmitAction(r));
app.MapGet("/api/combat/state", (GameSessionService gs) => gs.GetCombatState());
app.MapPost("/api/combat/abandon", (GameSessionService gs) => gs.AbandonCombat());
app.MapPost("/api/arena/start", (GameSessionService gs) => gs.StartArena());
app.MapPost("/api/arena/action", (GameSessionService gs, CombatActionRequest r) => gs.SubmitAction(r));
app.MapPost("/api/arena/rest", (GameSessionService gs, RestRequest r) => gs.ArenaRest(r.Choice));
app.MapPost("/api/dungeon/start", (GameSessionService gs, DungeonRequest r) => gs.StartDungeon(r.Depth));
app.MapPost("/api/dungeon/action", (GameSessionService gs, CombatActionRequest r) => gs.SubmitAction(r));
app.MapPost("/api/dungeon/proceed", (GameSessionService gs) => gs.DungeonProceed());
app.MapPost("/api/dungeon/event", (GameSessionService gs, EventChoiceRequest r) => gs.DungeonEventChoice(r.Choice));
app.MapGet("/api/shop/items", (GameSessionService gs) => gs.GetShopItems());
app.MapPost("/api/shop/buy", (GameSessionService gs, BuyRequest r) => { gs.BuyItem(r.Name, r.Category, r.Quantity, r.HeroIndex); return gs.GetFullState(); });
app.MapPost("/api/shop/sell", (GameSessionService gs, SellRequest r) => { gs.SellEquipment(r.HeroIndex, r.Slot); return gs.GetFullState(); });
app.MapGet("/api/bestiary", (GameSessionService gs) => gs.GetBestiary());
app.MapGet("/api/achievements", (GameSessionService gs) => gs.GetAchievements());
app.MapGet("/api/history", (GameSessionService gs) => gs.GetHistory());
app.MapGet("/api/quests", (GameSessionService gs) => gs.GetQuests());

app.MapFallback(async ctx =>
{
    var indexPath = Path.Combine(app.Environment.WebRootPath, "index.html");
    if (File.Exists(indexPath))
    {
        ctx.Response.ContentType = "text/html";
        await ctx.Response.SendFileAsync(indexPath);
    }
    else ctx.Response.StatusCode = 404;
});

var url = "http://localhost:5100";
Console.WriteLine($"RPG Combat Web — {url}");
try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
app.Run(url);

public record DifficultyRequest(string Difficulty);
public record CreateTeamRequest(List<HeroCreation> Heroes);
public record HeroCreation(string Name, string ClassName);
public record EquipRequest(int HeroIndex, string Slot, string ItemName);
public record BossRequest(string BossType);
public record CombatActionRequest(string Type, int CompetenceIndex = -1, int TargetIndex = -1, int ObjectIndex = -1);
public record RestRequest(int Choice);
public record DungeonRequest(int Depth);
public record EventChoiceRequest(string Choice);
public record BuyRequest(string Name, string Category, int Quantity = 1, int HeroIndex = -1);
public record SellRequest(int HeroIndex, string Slot);
