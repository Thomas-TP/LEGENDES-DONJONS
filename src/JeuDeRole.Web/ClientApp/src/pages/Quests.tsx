import { useState, useEffect } from 'react';
import type { QuestData } from '../types';
import * as api from '../api';

export default function Quests({ onBack }: { onBack: () => void }) {
  const [data, setData] = useState<QuestData | null>(null);
  useEffect(() => { api.getQuests().then(setData); }, []);

  if (!data) return <div className="text-center py-12 text-gray-400">Chargement...</div>;

  return (
    <div className="flex flex-col gap-6 py-8">
      <h1 className="rpg-title text-2xl">📜 Quêtes</h1>

      {data.active.length > 0 && (
        <div>
          <h2 className="text-rpg-gold font-semibold mb-2">Quêtes actives ({data.active.length})</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {data.active.map((q, i) => (
              <div key={i} className="rpg-panel">
                <div className="flex items-start gap-2">
                  <span className="text-xl">{q.icone}</span>
                  <div className="flex-1">
                    <p className="text-rpg-gold font-semibold text-sm">{q.nom}</p>
                    <p className="text-gray-400 text-xs">{q.description}</p>
                    <p className="text-gray-500 text-[10px] mt-1">Objectif: {q.objectif}</p>
                    <div className="flex gap-2 mt-1 text-[10px]">
                      <span className="text-rpg-gold">+{q.recompenseOr} 💰</span>
                      <span className="text-rpg-purple">+{q.recompenseXp} XP</span>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {data.completed.length > 0 && (
        <div>
          <h2 className="text-rpg-green font-semibold mb-2">Quêtes terminées ({data.completed.length})</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {data.completed.map((q, i) => (
              <div key={i} className="rpg-panel border-rpg-green/30 opacity-80">
                <div className="flex items-center gap-2">
                  <span className="text-xl">✅</span>
                  <div>
                    <p className="text-rpg-green font-semibold text-sm">{q.nom}</p>
                    <p className="text-gray-500 text-xs">{q.description}</p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {data.all.length === 0 && <p className="text-center text-gray-500">Aucune quête disponible.</p>}

      <button className="rpg-btn mx-auto" onClick={onBack}>← Retour</button>
    </div>
  );
}
