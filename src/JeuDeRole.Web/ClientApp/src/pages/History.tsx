import { useState, useEffect } from 'react';
import type { HistoryData } from '../types';
import * as api from '../api';

export default function History({ onBack }: { onBack: () => void }) {
  const [data, setData] = useState<HistoryData | null>(null);
  useEffect(() => { api.getHistory().then(setData); }, []);

  if (!data) return <div className="text-center py-12 text-gray-400">Chargement...</div>;

  return (
    <div className="flex flex-col gap-6 py-8">
      <h1 className="rpg-title text-2xl">📊 Historique</h1>

      <div className="flex gap-4 justify-center text-sm">
        <div className="rpg-panel text-center px-6">
          <p className="text-gray-400">Total</p>
          <p className="text-rpg-gold text-xl font-bold">{data.total}</p>
        </div>
        <div className="rpg-panel text-center px-6">
          <p className="text-gray-400">Victoires</p>
          <p className="text-rpg-green text-xl font-bold">{data.wins}</p>
        </div>
        <div className="rpg-panel text-center px-6">
          <p className="text-gray-400">Défaites</p>
          <p className="text-rpg-red text-xl font-bold">{data.losses}</p>
        </div>
      </div>

      <div className="space-y-2">
        {data.combats.map((c, i) => (
          <div key={i} className={`rpg-panel flex justify-between items-center text-sm ${c.victoireHeros ? 'border-l-4 border-l-rpg-green' : 'border-l-4 border-l-rpg-red'}`}>
            <div>
              <span className={c.victoireHeros ? 'text-rpg-green' : 'text-rpg-red'}>
                {c.victoireHeros ? '✅ Victoire' : '❌ Défaite'}
              </span>
              <span className="text-gray-500 ml-2 text-xs">vs {c.monstresAffrontes.join(', ')}</span>
            </div>
            <div className="text-xs text-gray-400 flex gap-3">
              <span>{c.nombreTours} tours</span>
              <span className="text-rpg-red">{c.totalDegatsInfliges} dmg</span>
              <span className="text-rpg-purple">+{c.experienceGagnee} XP</span>
            </div>
          </div>
        ))}
        {data.combats.length === 0 && <p className="text-center text-gray-500">Aucun combat enregistré.</p>}
      </div>

      <button className="rpg-btn mx-auto" onClick={onBack}>← Retour</button>
    </div>
  );
}
