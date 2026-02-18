import { useState, useEffect } from 'react';
import type { BestiaryEntry } from '../types';
import { ClassIcon } from '../components/GameIcons';
import * as api from '../api';

export default function Bestiary({ onBack }: { onBack: () => void }) {
  const [entries, setEntries] = useState<BestiaryEntry[]>([]);
  useEffect(() => { api.getBestiary().then(setEntries); }, []);

  const getIconType = (name: string) => {
    if (name.includes("Liche")) return "Liche";
    if (name.includes("Squelette")) return "Squelette";
    if (name.includes("Gobelin")) return "Gobelin";
    if (name.includes("Orc")) return "Orc";
    if (name.includes("Dragon")) return "Dragon";
    return "Monstre";
  };

  return (
    <div className="flex flex-col gap-8 py-8 items-center max-w-6xl mx-auto">
      <h1 className="text-4xl font-rpg text-rpg-gold text-center drop-shadow-[0_0_10px_rgba(234,179,8,0.5)]">📖 Bestiaire</h1>
      
      {entries.length === 0 ? (
        <p className="text-center text-gray-500 italic mt-10">Aucun monstre rencontré pour l'instant. Explorez le monde !</p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 w-full px-4">
          {entries.map((e, i) => {
             const iconType = getIconType(e.nom);
             return (
               <div key={i} className="rpg-panel relative group overflow-hidden hover:border-rpg-red/50 transition-all duration-300 hover:-translate-y-1">
                 <div className="absolute -right-4 -top-4 opacity-5 group-hover:opacity-10 transition-opacity rotate-12">
                    <ClassIcon type={iconType} size={150} />
                 </div>
                 
                 <div className="flex flex-col items-center mb-3 relative z-10">
                    <ClassIcon type={iconType} size={60} className="mb-2 shadow-lg group-hover:shadow-[0_0_15px_rgba(220,38,38,0.3)] transition-shadow" />
                    <h3 className="text-xl font-rpg text-gray-200 group-hover:text-rpg-red transition-colors text-center">{e.nom}</h3>
                    <div className="text-[10px] text-gray-500 uppercase tracking-widest mt-1">Niveau de Menace: Normal</div>
                 </div>

                 <div className="space-y-2 relative z-10 bg-black/20 p-2 rounded">
                    <div className="flex justify-between text-xs border-b border-gray-700/50 pb-1">
                       <span className="text-gray-400">PV Max</span>
                       <span className="text-green-400 font-mono">{e.pvMax}</span>
                    </div>
                    <div className="flex justify-between text-xs border-b border-gray-700/50 pb-1">
                       <span className="text-gray-400">Force</span>
                       <span className="text-red-400 font-mono">{e.force}</span>
                    </div>
                    <div className="flex justify-between text-xs border-b border-gray-700/50 pb-1">
                       <span className="text-gray-400">Défense</span>
                       <span className="text-blue-400 font-mono">{e.defense}</span>
                    </div>
                    <div className="flex justify-between text-xs border-b border-gray-700/50 pb-1">
                       <span className="text-gray-400">Expérience</span>
                       <span className="text-purple-400 font-mono">+{e.xp}</span>
                    </div>
                    <div className="flex justify-between text-xs pt-1">
                       <span className="text-gray-400">Éliminés</span>
                       <span className="text-rpg-gold font-bold">{e.nombreKills}</span>
                    </div>
                 </div>

                 {e.faiblesses.length > 0 && (
                   <div className="mt-3 relative z-10">
                     <p className="text-[10px] text-gray-500 mb-1 uppercase tracking-wider">Affinités Élémentaires</p>
                     <div className="flex gap-1.5 flex-wrap">
                       {e.faiblesses.filter(f => f.mult > 1.0).map((f, j) => (
                         <span key={`weak-${j}`} className="bg-red-900/30 border border-red-900/50 text-red-300 px-1.5 py-0.5 rounded text-[10px] flex items-center gap-1">
                           <span>🔥</span> {f.element} <span className="font-mono text-red-100">x{f.mult}</span>
                         </span>
                       ))}
                       {e.faiblesses.filter(f => f.mult < 1.0).map((f, j) => (
                         <span key={`res-${j}`} className="bg-blue-900/30 border border-blue-900/50 text-blue-300 px-1.5 py-0.5 rounded text-[10px] flex items-center gap-1">
                           <span>🛡️</span> {f.element} <span className="font-mono text-blue-100">x{f.mult}</span>
                         </span>
                       ))}
                     </div>
                   </div>
                 )}
               </div>
             );
          })}
        </div>
      )}
      <button className="rpg-btn-primary px-8 mt-4" onClick={onBack}>← Retour au Menu</button>
    </div>
  );
}

