import type { Hero } from '../types';
import { ClassIcon } from '../components/GameIcons';

export default function TeamView({ team, onBack }: { team: Hero[]; onBack: () => void }) {
  return (
    <div className="flex flex-col gap-6 py-8 animate-[fadeIn_0.5s_ease-out] max-w-5xl mx-auto">
      <div className="text-center mb-4">
         <h1 className="text-4xl font-rpg text-rpg-gold drop-shadow-md">Mon Équipe</h1>
         <div className="h-1 w-24 bg-rpg-gold/50 mx-auto mt-2 rounded-full"></div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6 px-4">
        {team.map((h, i) => (
          <div key={i} className="rpg-panel relative overflow-hidden group hover:border-rpg-gold/50 transition-colors">
            {/* Background Decor */}
            <div className="absolute -right-10 -bottom-10 opacity-10 group-hover:opacity-20 transition-opacity">
               <ClassIcon type={h.classe} size={200} />
            </div>

            <div className="flex flex-col sm:flex-row gap-4 relative z-10">
               {/* Icon Column */}
               <div className="flex flex-col items-center gap-2">
                  <ClassIcon type={h.classe} size={80} className="shadow-lg border-2 border-gray-700 group-hover:border-rpg-gold/50 transition-colors" />
                  <span className={`text-xs px-2 py-0.5 rounded-full border ${h.estVivant ? 'bg-green-900/40 text-green-400 border-green-800' : 'bg-red-900/40 text-red-400 border-red-800'}`}>
                    {h.estVivant ? 'EN FORME' : 'K.O.'}
                  </span>
               </div>

               {/* Stats Column */}
               <div className="flex-1 space-y-3">
                  <div className="flex justify-between items-start border-b border-gray-800 pb-2">
                     <div>
                        <h3 className="text-xl font-bold text-gray-100">{h.nom}</h3>
                        <p className="text-rpg-gold text-sm font-mono">{h.classe} — Niv. {h.niveau}</p>
                     </div>
                     <div className="text-right">
                        <div className="text-xs text-gray-500 uppercase tracking-widest">XP</div>
                        <div className="text-purple-400 font-mono">{h.xp} / {h.xpNext}</div>
                     </div>
                  </div>

                  {/* Bars */}
                  <div className="space-y-1.5">
                    <Bar label="PV" current={h.pv} max={h.pvMax} color="bg-red-500" />
                    <Bar label="PM" current={h.pm} max={h.pmMax} color="bg-blue-500" />
                    <div className="w-full bg-gray-800 h-1 rounded-full overflow-hidden mt-1">
                       <div className="h-full bg-purple-500 opacity-70" style={{ width: `${(h.xp / h.xpNext) * 100}%` }}></div>
                    </div>
                  </div>
                  
                  {/* Primary Stats */}
                   <div className="grid grid-cols-4 gap-2 py-2">
                     <StatBadge label="FOR" value={h.stats.force} icon="⚔️" />
                     <StatBadge label="INT" value={h.stats.intelligence} icon="🧠" />
                     <StatBadge label="AGI" value={h.stats.agilite} icon="💨" />
                     <StatBadge label="DEF" value={h.stats.defense} icon="🛡️" />
                  </div>
               </div>
            </div>

            {/* Equipment & Skills Row */}
            <div className="mt-4 pt-3 border-t border-gray-800/50 grid grid-cols-1 sm:grid-cols-2 gap-4 relative z-10">
               <div>
                  <p className="text-[10px] uppercase text-gray-500 font-bold mb-2">Équipement</p>
                  <div className="space-y-1 text-xs">
                     <EquipSlot label="Arme" name={h.arme?.nom} icon="🗡️" />
                     <EquipSlot label="Armure" name={h.armure?.nom} icon="🛡️" />
                     <EquipSlot label="Accessoire" name={h.accessoire?.nom} icon="💍" />
                  </div>
               </div>
               <div>
                  <p className="text-[10px] uppercase text-gray-500 font-bold mb-2">Compétences</p>
                  <div className="flex flex-wrap gap-1.5">
                    {h.competences.map((c, j) => (
                      <span key={j} className="bg-gray-800 border border-gray-700 px-2 py-1 rounded text-[10px] text-gray-300 hover:text-white hover:border-gray-500 transition-colors cursor-help" title={`Coût: ${c.cout} PM`}>
                        {c.nom}
                      </span>
                    ))}
                  </div>
               </div>
            </div>
          </div>
        ))}
      </div>

      <button className="rpg-btn-primary mx-auto mt-4 px-8" onClick={onBack}>← Retour au Menu</button>
    </div>
  );
}

function Bar({ label, current, max, color }: { label: string; current: number; max: number; color: string }) {
  const pct = max > 0 ? Math.max(0, Math.min(100, (current / max) * 100)) : 0;
  return (
    <div className="flex items-center gap-2 text-xs">
      <span className="w-6 font-bold text-gray-400 text-right">{label}</span>
      <div className="flex-1 bg-gray-900 rounded-full h-2.5 overflow-hidden border border-gray-700/50">
        <div className={`h-full ${color} transition-all duration-500 shadow-[0_0_10px_currentColor]`} style={{ width: `${pct}%` }} />
      </div>
      <span className="w-16 text-right font-mono text-gray-300">{current}/{max}</span>
    </div>
  );
}

function StatBadge({ label, value, icon }: { label: string; value: number; icon: string }) {
  return (
    <div className="bg-gray-900/60 rounded p-1 text-center border border-gray-800">
      <div className="text-[10px] text-gray-500 mb-0.5">{icon} {label}</div>
      <div className="text-rpg-gold font-bold text-sm">{value}</div>
    </div>
  );
}

function EquipSlot({ label, name, icon }: { label: string; name?: string; icon: string }) {
  return (
    <div className="flex justify-between items-center bg-gray-900/30 px-2 py-1 rounded">
      <span className="text-gray-500 flex items-center gap-1"><span>{icon}</span> {label}</span>
      <span className={`truncate max-w-[120px] ${name ? 'text-rpg-gold' : 'text-gray-700 italic'}`}>{name || 'Vide'}</span>
    </div>
  );
}

