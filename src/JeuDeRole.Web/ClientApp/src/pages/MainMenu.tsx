import type { GameState } from '../types';
import type { Page } from '../App';
import { ClassIcon } from '../components/GameIcons';

interface Props {
  state: GameState;
  onNavigate: (p: Page) => void;
  onSave: () => void;
  onLoad: () => void;
  onNewCombat: () => void;
}

export default function MainMenu({ state, onNavigate, onSave, onLoad, onNewCombat }: Props) {
  return (
    <div className="flex flex-col items-center gap-8 py-8 h-full max-w-5xl mx-auto px-4 animate-[fadeIn_0.5s_ease-out]">
      
      {/* Hero Header */}
      <div className="text-center relative py-8">
        <div className="absolute inset-0 bg-rpg-gold/5 blur-3xl rounded-full pointer-events-none"></div>
        <h1 className="relative z-10 text-6xl font-rpg text-transparent bg-clip-text bg-gradient-to-b from-rpg-gold to-yellow-700 drop-shadow-sm mb-2">
          LÉGENDES & DONJONS
        </h1>
        <p className="text-gray-400 font-serif italic max-w-lg mx-auto">
          "Forgez votre destinée, terrassez les monstres et devenez une légende."
        </p>
      </div>

      {/* Quick Stats Bar */}
      <div className="flex flex-wrap justify-center gap-4 sm:gap-8 bg-black/40 px-6 py-3 rounded-full border border-gray-800 backdrop-blur-sm">
         <StatsItem label="Difficulté" value={state.difficulty} color="text-gray-300" icon="⚙️" />
         <StatsItem label="Or" value={state.gold} color="text-yellow-400" icon="💰" />
         {state.hasTeam && <StatsItem label="Victoires" value={state.totalWins} color="text-green-400" icon="🏆" />}
         {state.hasTeam && <StatsItem label="Défaites" value={state.totalLosses} color="text-red-400" icon="💀" />}
      </div>

      {/* Main Actions Grid */}
      <div className="w-full grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        
        {/* Primary Actions */}
        <div className="lg:col-span-3 grid grid-cols-1 sm:grid-cols-2 gap-4">
           {state.hasTeam ? (
              <>
                <BigActionBtn 
                   icon="⚔️" title="COMBATTRE" desc="Affronter un monstre aléatoire" 
                   onClick={onNewCombat} type="primary"
                />
                <BigActionBtn 
                   icon="👹" title="BOSS RAID" desc="Défier les seigneurs du donjon" 
                   onClick={() => onNavigate('boss-select')} type="danger"
                />
              </>
           ) : (
              <BigActionBtn 
                 icon="✨" title="NOUVELLE AVENTURE" desc="Créer votre équipe de héros" 
                 onClick={() => onNavigate('team-create')} type="primary" className="col-span-1 sm:col-span-2"
              />
           )}
        </div>

        {state.hasTeam && (
           <>
              <SectionTitle title="Exploration" className="lg:col-span-3 mt-4" />
              <MenuCard icon="🏰" title="Donjon" desc="Explorer les profondeurs" onClick={() => onNavigate('dungeon')} color="blue" />
              <MenuCard icon="🏟️" title="Arène" desc="Survivre aux vagues" onClick={() => onNavigate('arena')} color="purple" />
              <MenuCard icon="📜" title="Quêtes" desc="Missions et récompenses" onClick={() => onNavigate('quests')} color="green" />

              <SectionTitle title="Gestion" className="lg:col-span-3 mt-4" />
              <MenuCard icon="👥" title="Équipe" desc="Voir les stats" onClick={() => onNavigate('team-view')} />
              <MenuCard icon="🛡️" title="Équipement" desc="Gérer l'inventaire" onClick={() => onNavigate('team-equip')} />
              <MenuCard icon="🏪" title="Boutique" desc="Acheter des objets" onClick={() => onNavigate('shop')} />
           </>
        )}

        <SectionTitle title="Système" className="lg:col-span-3 mt-4" />
        <MenuCard icon="📖" title="Bestiaire" desc="Encyclopédie des monstres" onClick={() => onNavigate('bestiary')} />
        <MenuCard icon="🏆" title="Succès" desc="Vos exploits" onClick={() => onNavigate('achievements')} />
        <MenuCard icon="📊" title="Historique" desc="Journal des combats" onClick={() => onNavigate('history')} />
        
        <div className="lg:col-span-3 flex justify-center gap-4 py-4 border-t border-gray-800 mt-4">
            <SmallBtn icon="💾" label="Sauvegarder" onClick={onSave} disabled={!state.hasTeam} />
            <SmallBtn icon="📂" label="Charger" onClick={onLoad} disabled={!state.hasSave} />
            <SmallBtn icon="⚙️" label="Options" onClick={() => onNavigate('difficulty')} />
        </div>

      </div>

      {state.allBossesDefeated && (
        <div className="fixed bottom-4 right-4 animate-bounce max-w-xs bg-rpg-gold/20 backdrop-blur border-2 border-rpg-gold text-center p-4 rounded-xl shadow-[0_0_20px_rgba(234,179,8,0.5)]">
          <p className="text-2xl">👑</p>
          <p className="font-bold text-rpg-gold">MAÎTRE DU JEU</p>
          <p className="text-xs text-yellow-100">Tous les boss vaincus !</p>
        </div>
      )}
    </div>
  );
}

// --- Components ---

function StatsItem({ label, value, color, icon }: any) {
   return (
      <div className="flex items-center gap-2 text-xs uppercase tracking-wider font-bold">
         <span>{icon}</span>
         <span className="text-gray-500">{label}</span>
         <span className={`${color} text-sm`}>{value}</span>
      </div>
   )
}

function SectionTitle({ title, className }: { title: string, className?: string }) {
   return (
      <div className={`flex items-center gap-2 ${className}`}>
         <div className="h-px bg-gray-800 flex-1"></div>
         <span className="text-gray-500 text-xs uppercase tracking-widest font-bold">{title}</span>
         <div className="h-px bg-gray-800 flex-1"></div>
      </div>
   )
}

function BigActionBtn({ icon, title, desc, onClick, type, className }: any) {
   const colors = type === 'primary' 
      ? 'bg-gradient-to-br from-rpg-gold/20 to-yellow-900/20 border-rpg-gold/50 hover:border-rpg-gold text-rpg-gold' 
      : 'bg-gradient-to-br from-red-900/20 to-red-950/20 border-red-800/50 hover:border-red-500 text-red-400';

   return (
      <button onClick={onClick} className={`group relative p-6 rounded-xl border flex flex-col items-center gap-2 transition-all hover:-translate-y-1 hover:shadow-lg ${colors} ${className}`}>
         <span className="text-4xl group-hover:scale-110 transition-transform">{icon}</span>
         <span className="text-xl font-bold font-rpg tracking-wide">{title}</span>
         <span className="text-xs opacity-70 font-sans">{desc}</span>
         <div className="absolute inset-0 bg-white/5 opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none rounded-xl"></div>
      </button>
   )
}

function MenuCard({ icon, title, desc, onClick, color }: any) {
   let borderClass = 'border-gray-800 hover:border-gray-600';
   if (color === 'purple') borderClass = 'border-purple-900/50 hover:border-purple-500/50 bg-purple-900/10';
   if (color === 'blue') borderClass = 'border-blue-900/50 hover:border-blue-500/50 bg-blue-900/10';
   if (color === 'green') borderClass = 'border-green-900/50 hover:border-green-500/50 bg-green-900/10';

   return (
      <button onClick={onClick} className={`flex items-center gap-4 p-4 rounded-lg border bg-gray-900/40 hover:bg-gray-800/60 transition-all text-left group ${borderClass}`}>
         <span className="text-2xl bg-gray-800 p-2 rounded-lg group-hover:scale-110 transition-transform">{icon}</span>
         <div>
            <div className="font-bold text-gray-200 group-hover:text-white">{title}</div>
            <div className="text-[10px] text-gray-500 group-hover:text-gray-400">{desc}</div>
         </div>
      </button>
   )
}

function SmallBtn({ icon, label, onClick, disabled }: any) {
   return (
      <button 
         onClick={onClick} 
         disabled={disabled}
         className="flex items-center gap-2 px-4 py-2 rounded border border-gray-700 bg-gray-900 text-xs hover:bg-gray-800 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
      >
         <span>{icon}</span> {label}
      </button>
   )
}

