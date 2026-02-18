import { useState, useEffect } from 'react';
import type { ClassInfo } from '../types';
import { ClassIcon } from '../components/GameIcons';
import * as api from '../api';

interface Props {
  onDone: (heroes: { name: string; className: string }[]) => void;
  onBack: () => void;
}

export default function TeamCreation({ onDone, onBack }: Props) {
  const [classes, setClasses] = useState<ClassInfo[]>([]);
  const [heroes, setHeroes] = useState<{ name: string; className: string }[]>([
    { name: '', className: '' }, { name: '', className: '' }, { name: '', className: '' }, { name: '', className: '' }
  ]);
  const [count, setCount] = useState(3);

  useEffect(() => {
    // If no data, use mock for preview or fetch
    api.getClasses().then(data => {
       if (data && data.length > 0) setClasses(data);
       else {
          // Fallback if API fails or is empty, just to show visually
          setClasses([
             {name: 'Guerrier', description: 'Combattant robuste'}, {name: 'Mage', description: 'Maître des arcanes'},
             {name: 'Voleur', description: 'Rapide et mortel'}, {name: 'Clerc', description: 'Soigneur divin'},
             {name: 'Paladin', description: 'Défenseur sacré'}, {name: 'Necromancien', description: 'Maître des morts'},
             {name: 'Assassin', description: 'Tueur de l\'ombre'}, {name: 'Druide', description: 'Gardien de la nature'}
          ]);
       }
    });
  }, []);

  const updateHero = (index: number, field: keyof typeof heroes[0], value: string) => {
    const newHeroes = [...heroes];
    newHeroes[index] = { ...newHeroes[index], [field]: value };
    setHeroes(newHeroes);
  };
  
  const currentHeroes = heroes.slice(0, count);
  const canSubmit = currentHeroes.every(h => h.name.trim().length > 0 && h.className);

  return (
    <div className="flex flex-col h-full max-w-6xl mx-auto p-4 gap-6 animate-[fadeIn_0.5s_ease-out]">
      <div className="text-center space-y-2">
        <h1 className="text-4xl font-rpg text-rpg-gold drop-shadow-[0_2px_4px_rgba(0,0,0,0.8)]">Recrutement</h1>
        <p className="text-gray-400 max-w-lg mx-auto">Assemblez votre groupe d'aventuriers légendaires pour affronter les ténèbres.</p>
      </div>

      {/* Hero Count Selection */}
      <div className="flex justify-center items-center gap-4 bg-gray-900/50 p-3 rounded-full border border-gray-800 w-max mx-auto">
        <span className="text-sm text-gray-500 uppercase tracking-wider font-semibold mr-2">Taille du groupe:</span>
        {[1, 2, 3, 4].map(n => (
          <button 
            key={n} 
            onClick={() => setCount(n)}
            className={`w-10 h-10 rounded-full flex items-center justify-center transition-all font-bold ${
               count === n 
               ? 'bg-rpg-gold text-black shadow-[0_0_10px_rgba(234,179,8,0.5)] scale-110' 
               : 'bg-gray-800 text-gray-400 hover:bg-gray-700'
            }`}
          >
            {n}
          </button>
        ))}
      </div>

      {/* Heroes Configuration */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 flex-1 overflow-y-auto pr-2 custom-scrollbar">
        {Array.from({ length: count }).map((_, i) => (
          <div key={i} className="rpg-panel relative group border-l-4 border-l-rpg-gold">
            <div className="absolute top-2 right-2 text-6xl opacity-5 pointer-events-none font-rpg text-gray-500">{i + 1}</div>
            
            <div className="flex flex-col gap-4 relative z-10">
               <div className="flex items-center gap-4">
                  <div className="w-16 h-16 rounded bg-gray-900 border border-gray-700 flex items-center justify-center shrink-0 shadow-inner">
                     {heroes[i].className ? (
                        <ClassIcon type={heroes[i].className} size={50} />
                     ) : (
                        <span className="text-3xl opacity-20">?</span>
                     )}
                  </div>
                  <div className="flex-1">
                     <label className="text-[10px] uppercase text-gray-500 tracking-wider font-bold">Nom du Héros</label>
                     <input 
                        type="text" 
                        value={heroes[i].name}
                        onChange={(e) => updateHero(i, 'name', e.target.value)}
                        placeholder={`Héros ${i+1}`}
                        className="w-full bg-transparent border-b border-gray-700 text-xl font-rpg text-gray-200 focus:border-rpg-gold outline-none px-1 py-1 transition-colors bg-gray-900/30 rounded-t"
                     />
                  </div>
               </div>

               <div>
                  <label className="text-[10px] uppercase text-gray-500 tracking-wider font-bold mb-2 block">Classe</label>
                  <div className="grid grid-cols-4 sm:grid-cols-4 gap-2">
                     {classes.map(c => (
                        <button 
                           key={c.name}
                           onClick={() => updateHero(i, 'className', c.name)}
                           className={`flex flex-col items-center justify-center p-2 rounded border transition-all duration-200 ${
                              heroes[i].className === c.name
                              ? 'bg-rpg-gold/20 border-rpg-gold shadow-[0_0_10px_rgba(234,179,8,0.2)]'
                              : 'bg-gray-800/50 border-gray-700 hover:bg-gray-700 hover:border-gray-500 opacity-70 hover:opacity-100'
                           }`}
                           title={c.description}
                        >
                           <ClassIcon type={c.name} size={32} className="mb-1 pointer-events-none" />
                           <span className={`text-[9px] font-semibold truncate w-full text-center ${heroes[i].className === c.name ? 'text-rpg-gold' : 'text-gray-400'}`}>
                              {c.name}
                           </span>
                        </button>
                     ))}
                  </div>
                  {heroes[i].className && (
                     <p className="text-xs text-center mt-2 text-gray-400 italic">
                        "{classes.find(c => c.name === heroes[i].className)?.description}"
                     </p>
                  )}
               </div>
            </div>
          </div>
        ))}
      </div>

      <div className="flex justify-between items-center pt-4 border-t border-gray-800 mt-auto">
         <button onClick={onBack} className="text-gray-500 hover:text-white transition-colors flex items-center gap-2 px-4 py-2">
            <span>←</span> Retour
         </button>
         <button 
            disabled={!canSubmit}
            onClick={() => onDone(heroes.slice(0, count))}
            className="rpg-btn-primary px-12 py-3 text-lg font-bold shadow-lg disabled:opacity-50 disabled:cursor-not-allowed hover:scale-105 transform transition-all"
         >
            COMMENCER L'AVENTURE ⚔️
         </button>
      </div>
    </div>
  );
}

