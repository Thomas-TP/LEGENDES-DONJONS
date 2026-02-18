interface Props {
  current: string;
  onSelect: (d: string) => void;
  onBack: () => void;
}

const DIFFICULTIES = [
  { name: 'Facile', desc: 'Monstres affaiblis (x0.75)', color: 'text-rpg-green', icon: '🌿' },
  { name: 'Normal', desc: 'Expérience standard', color: 'text-rpg-gold', icon: '⚔️' },
  { name: 'Difficile', desc: 'Monstres renforcés (x1.3)', color: 'text-orange-400', icon: '🔥' },
  { name: 'Cauchemar', desc: 'Monstres très puissants (x1.6)', color: 'text-rpg-red', icon: '💀' },
];

export default function DifficultySelect({ current, onSelect, onBack }: Props) {
  return (
    <div className="flex flex-col items-center gap-6 py-12">
      <h1 className="rpg-title text-2xl">⚙️ Difficulté</h1>
      <p className="text-gray-400">Actuelle: <span className="text-rpg-gold">{current}</span></p>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 max-w-lg w-full">
        {DIFFICULTIES.map(d => (
          <button key={d.name}
            className={`rpg-panel text-center cursor-pointer hover:ring-2 hover:ring-rpg-gold transition-all
              ${current === d.name ? 'ring-2 ring-rpg-gold' : ''}`}
            onClick={() => onSelect(d.name)}>
            <span className="text-3xl">{d.icon}</span>
            <p className={`font-semibold mt-2 ${d.color}`}>{d.name}</p>
            <p className="text-gray-400 text-xs mt-1">{d.desc}</p>
          </button>
        ))}
      </div>

      <button className="rpg-btn" onClick={onBack}>← Retour</button>
    </div>
  );
}
