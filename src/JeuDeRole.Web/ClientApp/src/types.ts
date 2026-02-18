export interface GameState {
  hasTeam: boolean;
  difficulty: string;
  gold: number;
  team: Hero[];
  bossesDefeated: string[];
  arenaMaxWaves: number;
  dungeonMaxDepth: number;
  totalWins: number;
  totalLosses: number;
  inventory: Item[];
  hasSave: boolean;
  allBossTypes: string[];
  allBossesDefeated: boolean;
}

export interface Hero {
  nom: string;
  classe: string;
  niveau: number;
  pv: number;
  pvMax: number;
  pm: number;
  pmMax: number;
  xp: number;
  xpNext: number;
  stats: Stats;
  arme: Equipment | null;
  armure: Equipment | null;
  accessoire: Equipment | null;
  competences: Competence[];
  effets: StatusEffect[];
  estVivant: boolean;
}

export interface Stats {
  pvMax: number; pmMax: number;
  force: number; intelligence: number;
  agilite: number; defense: number; resMag: number;
}

export interface Equipment {
  nom: string; type: string; bonus: Stats;
}

export interface Competence {
  nom: string; cout: number; puissance: number;
  typeDegat: string; cible: string; element: string;
  effet: string; niveauRequis: number;
}

export interface StatusEffect {
  statut: string; tours: number;
}

export interface Item {
  nom: string; description: string; quantite: number;
}

export interface Fighter {
  nom: string; pv: number; pvMax: number;
  pm: number; pmMax: number;
  stats: Stats; estVivant: boolean;
  statut: string; effets: StatusEffect[];
  competences: Competence[];
  isBoss: boolean; phase: number; totalPhases: number;
}

export interface CombatState {
  active: boolean; finished: boolean;
  victory?: boolean;
  waitingForAction?: boolean;
  activeHero?: Hero;
  enemies?: Fighter[];
  allies?: Fighter[];
  inventory?: Item[];
  logs: LogEntry[];
  team?: Hero[];
  gold?: number;
  result?: CombatResult;
}

export interface CombatResult {
  totalDamage: number; totalHealing: number;
  turns: number; xp: number;
  damagePerHero: Record<string, number>;
  monstersDefeated: string[];
}

export interface LogEntry { type: string; message: string; }

export interface ClassInfo { name: string; description: string; }

export interface ShopData {
  gold: number;
  equipment: ShopItem[];
  items: ShopItem[];
}

export interface ShopItem {
  nom: string; description: string; prix: number; categorie: string;
}

export interface BestiaryEntry {
  nom: string; pvMax: number; force: number; defense: number;
  xp: number; nombreKills: number;
  faiblesses: { element: string; mult: number }[];
}

export interface Achievement {
  nom: string; description: string; icone: string;
  debloque: boolean; dateDeblocage: string | null;
}

export interface HistoryData {
  wins: number; losses: number; total: number;
  combats: HistoryCombat[];
}

export interface HistoryCombat {
  victoireHeros: boolean; nombreTours: number;
  totalDegatsInfliges: number; totalSoinsProdigues: number;
  experienceGagnee: number; date: string;
  herosParticipants: string[]; monstresAffrontes: string[];
  degatsParHeros: Record<string, number>;
}

export interface QuestData {
  active: Quest[]; completed: Quest[]; all: Quest[];
}

export interface Quest {
  id: string; nom: string; description: string;
  objectif: string; icone: string;
  recompenseOr: number; recompenseXp: number;
  terminee: boolean; dateCompletion: string | null;
}

export interface DungeonRoom {
  etage: number; type: string; nom: string; visitee: boolean;
}
