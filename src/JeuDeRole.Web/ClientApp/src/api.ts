import type { GameState, ClassInfo, Equipment, CombatState, ShopData, BestiaryEntry, Achievement, HistoryData, QuestData } from './types';

const BASE = '';

async function api<T>(path: string, method = 'GET', body?: unknown): Promise<T> {
  const opts: RequestInit = { method, headers: { 'Content-Type': 'application/json' } };
  if (body !== undefined) opts.body = JSON.stringify(body);
  const res = await fetch(`${BASE}${path}`, opts);
  if (!res.ok) throw new Error(`API error ${res.status}`);
  const text = await res.text();
  return text ? JSON.parse(text) : ({} as T);
}

export const getState = () => api<GameState>('/api/state');
export const setDifficulty = (d: string) => api<GameState>('/api/difficulty', 'POST', { difficulty: d });
export const save = () => api<void>('/api/save', 'POST');
export const load = () => api<GameState>('/api/load', 'POST');
export const getClasses = () => api<ClassInfo[]>('/api/classes');
export const getEquipment = () => api<Equipment[]>('/api/equipment');
export const createTeam = (heroes: { name: string; className: string }[]) =>
  api<GameState>('/api/team/create', 'POST', { heroes });
export const equipHero = (heroIndex: number, slot: string, itemName: string) =>
  api<GameState>('/api/team/equip', 'POST', { heroIndex, slot, itemName });
export const startCombat = () => api<CombatState>('/api/combat/start', 'POST');
export const startBossCombat = (bossType: string) =>
  api<{ dialogue: string; combat: CombatState }>('/api/combat/boss', 'POST', { bossType });
export const submitAction = (type: string, competenceIndex = -1, targetIndex = -1, objectIndex = -1) =>
  api<CombatState>('/api/combat/action', 'POST', { type, competenceIndex, targetIndex, objectIndex });
export const getCombatState = () => api<CombatState>('/api/combat/state');
export const abandonCombat = () => api<CombatState>('/api/combat/abandon', 'POST');
export const startArena = () =>
  api<{ intro: string; wave: number; combat: CombatState }>('/api/arena/start', 'POST');
export const arenaAction = (type: string, competenceIndex = -1, targetIndex = -1, objectIndex = -1) =>
  api<CombatState>('/api/arena/action', 'POST', { type, competenceIndex, targetIndex, objectIndex });
export const arenaRest = (choice: number) =>
  api<{ wave: number; combat: CombatState }>('/api/arena/rest', 'POST', { choice });
export const startDungeon = (depth: number) => api<any>('/api/dungeon/start', 'POST', { depth });
export const dungeonAction = (type: string, competenceIndex = -1, targetIndex = -1, objectIndex = -1) =>
  api<CombatState>('/api/dungeon/action', 'POST', { type, competenceIndex, targetIndex, objectIndex });
export const dungeonProceed = () => api<any>('/api/dungeon/proceed', 'POST');
export const dungeonEvent = (choice: string) => api<any>('/api/dungeon/event', 'POST', { choice });
export const getShopItems = () => api<ShopData>('/api/shop/items');
export const buyItem = (name: string, category: string, quantity = 1, heroIndex = -1) =>
  api<GameState>('/api/shop/buy', 'POST', { name, category, quantity, heroIndex });
export const sellEquipment = (heroIndex: number, slot: string) =>
  api<GameState>('/api/shop/sell', 'POST', { heroIndex, slot });
export const getBestiary = () => api<BestiaryEntry[]>('/api/bestiary');
export const getAchievements = () => api<Achievement[]>('/api/achievements');
export const getHistory = () => api<HistoryData>('/api/history');
export const getQuests = () => api<QuestData>('/api/quests');
