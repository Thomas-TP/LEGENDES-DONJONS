interface Props {
  bossTypes: string[];
  defeated: string[];
  onSelect: (bt: string) => void;
  onBack: () => void;
}

const BOSS_INFO: Record<string, { name: string; icon: string; desc: string }> = {
  Liche: { name: 'Liche Ancienne', icon: '💀', desc: 'Nécromancienne surpuissante, 2 phases' },
  DragonAncien: { name: 'Dragon Ancien', icon: '🐉', desc: 'Dragon légendaire, 3 phases' },
  GolemCristal: { name: 'Golem de Cristal', icon: '💎', desc: 'Forteresse de pierre, 2 phases' },
  Hydre: { name: 'Hydre Venimeuse', icon: '🐍', desc: 'Multi-têtes venimeuse, 3 phases' },
  SeigneurDemon: { name: 'Seigneur Démon', icon: '😈', desc: 'Roi des démons, 3 phases' },
};

export default function BossSelect({ bossTypes, defeated, onSelect, onBack }: Props) {
  const isDefeated = (bt: string) => {
    const info = BOSS_INFO[bt];
    return info ? defeated.includes(info.name) : false;
  };

  return (
    <div className="flex flex-col items-center gap-6 py-8">
      <h1 className="rpg-title text-2xl">👹 Combat de Boss</h1>
      <p className="text-gray-400 text-sm">{defeated.length}/{bossTypes.length} vaincus</p>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 max-w-3xl w-full">
        {bossTypes.map(bt => {
          const info = BOSS_INFO[bt] || { name: bt, icon: '👹', desc: '' };
          const done = isDefeated(bt);
          return (
            <button key={bt}
              className={`rpg-panel text-center cursor-pointer hover:ring-2 hover:ring-rpg-red transition-all
                ${done ? 'opacity-60 border-rpg-green/50' : 'border-rpg-red/50'}`}
              onClick={() => onSelect(bt)}>
              <span className="text-4xl">{info.icon}</span>
              <p className="text-rpg-red font-semibold mt-2">{info.name}</p>
              <p className="text-gray-400 text-xs mt-1">{info.desc}</p>
              {done && <p className="text-rpg-green text-xs mt-1">✅ Vaincu</p>}
            </button>
          );
        })}
      </div>

      <button className="rpg-btn" onClick={onBack}>← Retour</button>
    </div>
  );
}
