import { useState, useEffect } from 'react';
import type { Achievement } from '../types';
import * as api from '../api';

export default function Achievements({ onBack }: { onBack: () => void }) {
  const [list, setList] = useState<Achievement[]>([]);
  useEffect(() => { api.getAchievements().then(setList); }, []);

  const unlocked = list.filter(a => a.debloque);
  const locked = list.filter(a => !a.debloque);

  return (
    <div className="flex flex-col gap-6 py-8">
      <h1 className="rpg-title text-2xl">🏆 Succès</h1>
      <p className="text-center text-gray-400">{unlocked.length}/{list.length} débloqués</p>

      {unlocked.length > 0 && (
        <div>
          <h2 className="text-rpg-gold font-semibold mb-2">Débloqués</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {unlocked.map((a, i) => (
              <div key={i} className="rpg-panel border-rpg-gold/50">
                <div className="flex items-center gap-2">
                  <span className="text-2xl">{a.icone}</span>
                  <div>
                    <p className="text-rpg-gold font-semibold text-sm">{a.nom}</p>
                    <p className="text-gray-400 text-xs">{a.description}</p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {locked.length > 0 && (
        <div>
          <h2 className="text-gray-500 font-semibold mb-2">Verrouillés</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {locked.map((a, i) => (
              <div key={i} className="rpg-panel opacity-50">
                <div className="flex items-center gap-2">
                  <span className="text-2xl">🔒</span>
                  <div>
                    <p className="text-gray-400 font-semibold text-sm">{a.nom}</p>
                    <p className="text-gray-500 text-xs">{a.description}</p>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      <button className="rpg-btn mx-auto" onClick={onBack}>← Retour</button>
    </div>
  );
}
