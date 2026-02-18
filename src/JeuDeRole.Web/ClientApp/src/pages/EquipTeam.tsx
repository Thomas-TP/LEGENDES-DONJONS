import { useState, useEffect } from 'react';
import type { GameState, Equipment } from '../types';
import * as api from '../api';

interface Props {
  state: GameState;
  onDone: () => void;
  onEquip: (heroIndex: number, slot: string, itemName: string) => void;
}

export default function EquipTeam({ state, onDone, onEquip }: Props) {
  const [equipment, setEquipment] = useState<Equipment[]>([]);
  const [selectedHero, setSelectedHero] = useState(0);
  const [selectedSlot, setSelectedSlot] = useState<string>('');

  useEffect(() => { api.getEquipment().then(setEquipment); }, []);

  const hero = state.team[selectedHero];
  const filteredEquip = equipment.filter(e => {
    if (selectedSlot === 'Arme') return e.type === 'Arme';
    if (selectedSlot === 'Armure') return e.type === 'Armure';
    if (selectedSlot === 'Accessoire') return e.type === 'Accessoire';
    return true;
  });

  return (
    <div className="flex flex-col gap-6 py-8">
      <h1 className="rpg-title text-2xl">🛡️ Équipement</h1>

      <div className="flex gap-2 justify-center flex-wrap">
        {state.team.map((h, i) => (
          <button key={i} className={`rpg-btn text-sm ${selectedHero === i ? 'rpg-btn-primary' : ''}`}
            onClick={() => { setSelectedHero(i); setSelectedSlot(''); }}>
            {h.nom} ({h.classe})
          </button>
        ))}
      </div>

      {hero && (
        <div className="rpg-panel">
          <h3 className="text-rpg-gold font-semibold mb-2">{hero.nom} — {hero.classe}</h3>
          <div className="grid grid-cols-3 gap-2 mb-4">
            {(['Arme', 'Armure', 'Accessoire'] as const).map(slot => {
              const current = slot === 'Arme' ? hero.arme : slot === 'Armure' ? hero.armure : hero.accessoire;
              return (
                <button key={slot}
                  className={`rpg-btn text-xs flex flex-col items-center gap-1 ${selectedSlot === slot ? 'border-rpg-gold' : ''}`}
                  onClick={() => setSelectedSlot(slot)}>
                  <span className="text-rpg-gold">{slot}</span>
                  <span className="text-gray-300">{current?.nom || '— Vide —'}</span>
                </button>
              );
            })}
          </div>

          {selectedSlot && (
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
              {filteredEquip.map(eq => (
                <button key={eq.nom} className="rpg-btn text-left text-xs"
                  onClick={() => { onEquip(selectedHero, selectedSlot, eq.nom); setSelectedSlot(''); }}>
                  <div className="text-rpg-gold font-semibold">{eq.nom}</div>
                  <div className="text-gray-400">
                    {formatBonusStats(eq.bonus)}
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>
      )}

      <button className="rpg-btn-primary mx-auto px-8" onClick={onDone}>✅ Terminé</button>
    </div>
  );
}

function formatBonusStats(s: any): string {
  const parts: string[] = [];
  if (s.force) parts.push(`FOR ${s.force > 0 ? '+' : ''}${s.force}`);
  if (s.intelligence) parts.push(`INT ${s.intelligence > 0 ? '+' : ''}${s.intelligence}`);
  if (s.agilite) parts.push(`AGI ${s.agilite > 0 ? '+' : ''}${s.agilite}`);
  if (s.defense) parts.push(`DEF ${s.defense > 0 ? '+' : ''}${s.defense}`);
  if (s.resMag) parts.push(`RES ${s.resMag > 0 ? '+' : ''}${s.resMag}`);
  if (s.pvMax) parts.push(`PV ${s.pvMax > 0 ? '+' : ''}${s.pvMax}`);
  if (s.pmMax) parts.push(`PM ${s.pmMax > 0 ? '+' : ''}${s.pmMax}`);
  return parts.join(' | ') || 'Aucun bonus';
}
