import { useState, useEffect, useCallback, useRef } from 'react';
import type { GameState, CombatState, LogEntry } from './types';
import * as api from './api';
import MainMenu from './pages/MainMenu';
import TeamCreation from './pages/TeamCreation';
import EquipTeam from './pages/EquipTeam';
import Combat from './pages/Combat';
import Shop from './pages/Shop';
import Bestiary from './pages/Bestiary';
import Achievements from './pages/Achievements';
import History from './pages/History';
import Quests from './pages/Quests';
import TeamView from './pages/TeamView';
import DifficultySelect from './pages/DifficultySelect';
import BossSelect from './pages/BossSelect';
import Arena from './pages/Arena';
import Dungeon from './pages/Dungeon';

export type Page = 'menu' | 'difficulty' | 'team-create' | 'team-equip' | 'team-view'
  | 'combat' | 'boss-select' | 'arena' | 'dungeon'
  | 'shop' | 'bestiary' | 'achievements' | 'history' | 'quests';

export default function App() {
  const [page, setPage] = useState<Page>('menu');
  const [state, setState] = useState<GameState | null>(null);
  const [combat, setCombat] = useState<CombatState | null>(null);
  const [allLogs, setAllLogs] = useState<LogEntry[]>([]);
  const [loading, setLoading] = useState(false);
  const [arenaWave, setArenaWave] = useState(0);
  const [dungeonData, setDungeonData] = useState<any>(null);
  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const refresh = useCallback(async () => {
    const s = await api.getState();
    setState(s);
    return s;
  }, []);

  useEffect(() => { refresh(); }, [refresh]);

  // Auto-poll when combat is active but not waiting for action (safety net)
  useEffect(() => {
    if (pollRef.current) { clearInterval(pollRef.current); pollRef.current = null; }
    if (combat && combat.active && !combat.waitingForAction && !combat.finished) {
      pollRef.current = setInterval(async () => {
        try {
          const cs = await api.getCombatState();
          handleCombatUpdate(cs);
        } catch { /* ignore */ }
      }, 2000);
    }
    return () => { if (pollRef.current) { clearInterval(pollRef.current); pollRef.current = null; } };
  }, [combat?.active, combat?.waitingForAction, combat?.finished]);

  const go = (p: Page) => { setPage(p); setAllLogs([]); };

  const withLoading = async <T,>(fn: () => Promise<T>): Promise<T> => {
    setLoading(true);
    try { return await fn(); } finally { setLoading(false); }
  };

  const handleCombatUpdate = (cs: CombatState) => {
    if (cs.logs?.length) setAllLogs(prev => [...prev, ...cs.logs]);
    setCombat(prev => {
      // Don't let an incomplete response overwrite a finished combat state
      if (prev?.finished && !cs.finished) return prev;
      return cs;
    });
    if (cs.finished) {
      refresh();
    }
  };

  if (!state) return <div className="flex items-center justify-center h-screen"><div className="rpg-title text-2xl animate-pulse">Chargement...</div></div>;

  return (
    <div className="min-h-screen p-4 max-w-6xl mx-auto">
      {loading && <div className="fixed top-4 right-4 bg-rpg-gold/20 text-rpg-gold px-3 py-1 rounded-lg text-sm z-50 animate-pulse">...</div>}

      {page === 'menu' && (
        <MainMenu state={state} onNavigate={go} onSave={async () => { await api.save(); await refresh(); }}
          onLoad={async () => { const s = await api.load(); setState(s); }}
          onNewCombat={async () => { setAllLogs([]); setCombat(null); const cs = await withLoading(() => api.startCombat()); handleCombatUpdate(cs); go('combat'); }}
        />
      )}
      {page === 'difficulty' && (
        <DifficultySelect current={state.difficulty} onSelect={async (d) => { const s = await api.setDifficulty(d); setState(s); go('menu'); }} onBack={() => go('menu')} />
      )}
      {page === 'team-create' && (
        <TeamCreation onDone={async (heroes) => { const s = await api.createTeam(heroes); setState(s); go('team-equip'); }} onBack={() => go('menu')} />
      )}
      {page === 'team-equip' && (
        <EquipTeam state={state} onDone={async () => { await refresh(); go('menu'); }}
          onEquip={async (hi, slot, item) => { const s = await api.equipHero(hi, slot, item); setState(s); }}
        />
      )}
      {page === 'team-view' && <TeamView team={state.team} onBack={() => go('menu')} />}
      {page === 'combat' && combat && (
        <Combat combat={combat} logs={allLogs} onAction={async (t, ci, ti, oi) => {
          const cs = await withLoading(() => api.submitAction(t, ci, ti, oi));
          handleCombatUpdate(cs);
        }} onBack={() => { setCombat(null); go('menu'); }}
        onAbandon={async () => { await withLoading(() => api.abandonCombat()); setCombat(null); await refresh(); go('menu'); }} />
      )}
      {page === 'boss-select' && (
        <BossSelect bossTypes={state.allBossTypes} defeated={state.bossesDefeated}
          onSelect={async (bt) => {
            setAllLogs([]); setCombat(null);
            const res = await withLoading(() => api.startBossCombat(bt));
            handleCombatUpdate(res.combat);
            go('combat');
          }} onBack={() => go('menu')} />
      )}
      {page === 'arena' && (
        <Arena combat={combat} logs={allLogs} wave={arenaWave}
          onStart={async () => {
            setAllLogs([]); setCombat(null);
            const res = await withLoading(() => api.startArena());
            setArenaWave(res.wave);
            handleCombatUpdate(res.combat);
          }}
          onAction={async (t, ci, ti, oi) => {
            const cs = await withLoading(() => api.arenaAction(t, ci, ti, oi));
            handleCombatUpdate(cs);
          }}
          onNextWave={async () => {
            setAllLogs([]); setCombat(null);
            const res = await withLoading(() => api.arenaRest(1));
            setArenaWave(res.wave);
            handleCombatUpdate(res.combat);
          }}
          onAbandon={async () => { await withLoading(() => api.abandonCombat()); setCombat(null); await refresh(); go('menu'); }}
          onBack={() => { setCombat(null); go('menu'); }}
        />
      )}
      {page === 'dungeon' && (
        <Dungeon data={dungeonData} combat={combat} logs={allLogs}
          onStart={async (depth) => {
            setAllLogs([]); setCombat(null); setDungeonData(null);
            const res = await withLoading(() => api.startDungeon(depth));
            const room = (res as any).dungeon;
            setDungeonData(room);
            if (room?.combat) handleCombatUpdate(room.combat);
          }}
          onAction={async (t, ci, ti, oi) => {
            const cs = await withLoading(() => api.dungeonAction(t, ci, ti, oi));
            handleCombatUpdate(cs);
          }}
          onProceed={async () => {
            setAllLogs([]); setCombat(null);
            const res = await withLoading(() => api.dungeonProceed());
            setDungeonData(res);
            if (res.combat) handleCombatUpdate(res.combat);
          }}
          onEvent={async (choice) => {
            setAllLogs([]); setCombat(null);
            const res = await withLoading(() => api.dungeonEvent(choice));
            setDungeonData(res);
            if (res.combat) handleCombatUpdate(res.combat);
            await refresh();
          }}
          onAbandon={async () => { await withLoading(() => api.abandonCombat()); setCombat(null); setDungeonData(null); await refresh(); go('menu'); }}
          onBack={() => { setCombat(null); setDungeonData(null); go('menu'); }}
        />
      )}
      {page === 'shop' && <Shop state={state} onUpdate={(s) => setState(s)} onBack={() => go('menu')} />}
      {page === 'bestiary' && <Bestiary onBack={() => go('menu')} />}
      {page === 'achievements' && <Achievements onBack={() => go('menu')} />}
      {page === 'history' && <History onBack={() => go('menu')} />}
      {page === 'quests' && <Quests onBack={() => go('menu')} />}
    </div>
  );
}
