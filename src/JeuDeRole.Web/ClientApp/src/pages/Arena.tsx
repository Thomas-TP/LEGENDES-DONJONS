import { useState } from 'react';
import type { CombatState, LogEntry } from '../types';
import Combat from './Combat';

interface Props {
  combat: CombatState | null;
  logs: LogEntry[];
  wave: number;
  onStart: () => void;
  onAction: (type: string, ci?: number, ti?: number, oi?: number) => void;
  onNextWave: () => void;
  onBack: () => void;
  onAbandon?: () => void;
}

export default function Arena(props: Props) {
  const { combat, logs, wave, onStart, onAction, onNextWave, onBack } = props;
  const [started, setStarted] = useState(false);

  if (!started || !combat) {
    return (
      <div className="flex flex-col items-center gap-6 py-12">
        <h1 className="rpg-title text-2xl">🏟️ Arène</h1>
        <p className="text-gray-400 text-center max-w-md">
          Affrontez des vagues de monstres de plus en plus puissants !
          Survivez le plus longtemps possible.
        </p>
        <button className="rpg-btn-primary text-lg px-8" onClick={() => { setStarted(true); onStart(); }}>
          ⚔️ Entrer dans l'arène
        </button>
        <button className="rpg-btn" onClick={onBack}>← Retour</button>
      </div>
    );
  }

  if (combat.finished) {
    return (
      <div className="flex flex-col items-center gap-6 py-8">
        <h2 className="rpg-title text-xl">🏟️ Arène — Vague {wave}</h2>
        {combat.victory ? (
          <>
            <p className="text-rpg-green text-lg">✅ Vague {wave} terminée !</p>
            <div className="flex gap-3">
              <button className="rpg-btn-primary px-6" onClick={onNextWave}>⚔️ Vague suivante</button>
              <button className="rpg-btn" onClick={onBack}>Quitter l'arène</button>
            </div>
          </>
        ) : (
          <>
            <p className="text-rpg-red text-lg">💀 Vous avez survécu {wave} vague(s) !</p>
            <button className="rpg-btn" onClick={onBack}>Retour au menu</button>
          </>
        )}
      </div>
    );
  }

  return (
    <div className="h-full w-full">
      <Combat title={`Arène - Vague ${wave}`} combat={combat} logs={logs} onAction={onAction} onBack={onBack} onAbandon={props.onAbandon} />
    </div>
  );
}
