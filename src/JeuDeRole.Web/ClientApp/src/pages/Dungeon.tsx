import { useState } from 'react';
import type { CombatState, LogEntry, DungeonRoom } from '../types';
import Combat from './Combat';

interface Props {
  data: any;
  combat: CombatState | null;
  logs: LogEntry[];
  onStart: (depth: number) => void;
  onAction: (type: string, ci?: number, ti?: number, oi?: number) => void;
  onProceed: () => void;
  onEvent: (choice: string) => void;
  onBack: () => void;
  onAbandon?: () => void;
}

export default function Dungeon(props: Props) {
  const { data, combat, logs, onStart, onAction, onProceed, onEvent, onBack, onAbandon } = props;
  const [depth, setDepth] = useState(5);
  const [started, setStarted] = useState(false);

  if (!started) {
    return (
      <div className="flex flex-col items-center gap-6 py-12">
        <h1 className="rpg-title text-2xl">🏰 Donjon</h1>
        <p className="text-gray-400 text-center max-w-md">
          Explorez un donjon peuplé de monstres, événements et boss !
        </p>
        <div className="flex items-center gap-3">
          <span className="text-gray-400">Profondeur:</span>
          {[3, 5, 7, 10].map(d => (
            <button key={d} className={`rpg-btn text-sm ${depth === d ? 'rpg-btn-primary' : ''}`}
              onClick={() => setDepth(d)}>{d} étages</button>
          ))}
        </div>
        <button className="rpg-btn-primary text-lg px-8" onClick={() => { setStarted(true); onStart(depth); }}>
          🏰 Entrer dans le donjon
        </button>
        <button className="rpg-btn" onClick={onBack}>← Retour</button>
      </div>
    );
  }

  // If combat is active, show combat
  if (combat && combat.active && !combat.finished) {
    return (
      <div className="h-full w-full">
        <Combat title={`Donjon - Étage ${data?.floor || '?'}`} combat={combat} logs={logs} onAction={onAction} onBack={onBack} onAbandon={onAbandon} />
      </div>
    );
  }

  // If combat just finished
  if (combat && combat.finished) {
    return (
      <div className="flex flex-col items-center gap-6 py-8">
        <h2 className="rpg-title text-xl">🏰 Donjon</h2>
        {combat.victory ? (
          <>
            <p className="text-rpg-green text-lg">✅ Combat remporté !</p>
            <button className="rpg-btn-primary px-6" onClick={onProceed}>Salle suivante →</button>
          </>
        ) : (
          <>
            <p className="text-rpg-red text-lg">💀 Vous avez été vaincu dans le donjon !</p>
            <button className="rpg-btn" onClick={onBack}>Retour au menu</button>
          </>
        )}
      </div>
    );
  }

  // Show dungeon state
  if (data) {
    // Dungeon finished
    if (data.finished) {
      return (
        <div className="flex flex-col items-center gap-6 py-12">
          <h1 className="rpg-title text-3xl">🏰 Donjon Terminé !</h1>
          <p className="text-rpg-gold text-lg">Félicitations, vous avez conquis le donjon !</p>
          <button className="rpg-btn-primary px-8" onClick={onBack}>Retour au menu</button>
        </div>
      );
    }

    // Event room
    if (data.type === 'Evenement' && data.eventType) {
      return (
        <div className="flex flex-col items-center gap-6 py-8">
          <h2 className="rpg-title text-xl">🏰 Étage {data.floor}</h2>
          <DungeonMap floors={data.floors} currentFloor={data.floor} />
          {data.narration && <p className="text-gray-400 text-center italic max-w-md">{data.narration}</p>}
          <div className="rpg-panel max-w-md text-center">
            <p className="text-rpg-gold font-semibold">{data.eventName}</p>
            <p className="text-gray-400 text-sm mt-1">{data.eventDesc}</p>
          </div>
          <button className="rpg-btn-primary" onClick={() => onEvent('accept')}>Continuer</button>
        </div>
      );
    }

    // Rest room
    if (data.type === 'Repos') {
      return (
        <div className="flex flex-col items-center gap-6 py-8">
          <h2 className="rpg-title text-xl">🏰 Étage {data.floor} — Salle de repos</h2>
          <DungeonMap floors={data.floors} currentFloor={data.floor} />
          {data.narration && <p className="text-gray-400 text-center italic max-w-md">{data.narration}</p>}
          <p className="text-rpg-green">💚 Votre équipe se repose et récupère !</p>
          <button className="rpg-btn-primary" onClick={onProceed}>Salle suivante →</button>
        </div>
      );
    }

    // Event result (after choice)
    if (data.message) {
      return (
        <div className="flex flex-col items-center gap-6 py-8">
          <h2 className="rpg-title text-xl">🏰 Donjon</h2>
          <p className="text-rpg-gold">{data.message}</p>
          <button className="rpg-btn-primary" onClick={onProceed}>Salle suivante →</button>
        </div>
      );
    }

    // Combat room with pending combat
    if (data.combat?.active) {
      return (
        <div>
          <h2 className="rpg-title text-xl mb-2">🏰 Étage {data.floor}</h2>
          <DungeonMap floors={data.floors} currentFloor={data.floor} />
          {data.narration && <p className="text-gray-400 text-center italic mb-4">{data.narration}</p>}
          <Combat combat={data.combat} logs={logs} onAction={onAction} onBack={onBack} onAbandon={onAbandon} />
        </div>
      );
    }
  }

  return (
    <div className="flex flex-col items-center gap-6 py-12">
      <h2 className="rpg-title text-xl">🏰 Donjon</h2>
      <p className="text-gray-400">Exploration en cours...</p>
      <button className="rpg-btn-primary" onClick={onProceed}>Continuer →</button>
      <button className="rpg-btn" onClick={onBack}>← Quitter</button>
    </div>
  );
}

function DungeonMap({ floors, currentFloor }: { floors?: DungeonRoom[]; currentFloor?: number }) {
  if (!floors) return null;
  return (
    <div className="rpg-panel max-w-xs w-full text-xs">
      <p className="text-rpg-gold font-semibold mb-2 text-center">Carte du donjon</p>
      {floors.map((f, i) => {
        const icon = f.type === 'Combat' ? '⚔️' : f.type === 'Evenement' ? '❓' : f.type === 'Repos' ? '💚'
          : f.type === 'MiniBoss' ? '☠️' : f.type === 'BossFinal' ? '💀' : '❓';
        const isCurrent = f.etage === currentFloor;
        return (
          <div key={i} className={`flex items-center gap-2 py-0.5 ${isCurrent ? 'text-rpg-gold font-bold' : f.visitee ? 'text-gray-500' : 'text-gray-600'}`}>
            <span>{isCurrent ? '→' : ' '}</span>
            <span>{f.visitee ? '✓' : isCurrent ? '●' : '○'}</span>
            <span>{icon}</span>
            <span>{f.visitee || isCurrent ? f.nom : '???'}</span>
          </div>
        );
      })}
    </div>
  );
}
