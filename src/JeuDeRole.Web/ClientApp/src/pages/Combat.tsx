import { useRef, useEffect, useState } from 'react';
import type { CombatState, LogEntry, Hero, Fighter, Competence, Item } from '../types';
import { ClassIcon } from '../components/GameIcons';

interface Props {
  combat: CombatState;
  logs: LogEntry[];
  title?: string;
  onAction: (type: string, compIdx?: number, targetIdx?: number, objIdx?: number) => void;
  onBack: () => void;
  onAbandon?: () => void;
}

export default function Combat({ combat, logs, title, onAction, onBack, onAbandon }: Props) {
  const logRef = useRef<HTMLDivElement>(null);
  const [actionMode, setActionMode] = useState<'main' | 'attack' | 'target' | 'item' | 'itemTarget'>('main');
  const [selectedComp, setSelectedComp] = useState(-1);
  const [selectedItem, setSelectedItem] = useState(-1);

  // Auto-scroll logs
  useEffect(() => {
    if (logRef.current) logRef.current.scrollTop = logRef.current.scrollHeight;
  }, [logs]);

  // Reset menu on turn change
  useEffect(() => { setActionMode('main'); }, [combat.activeHero?.nom]);

  const hero = combat.activeHero;
  const enemies = combat.enemies || [];
  const allies = combat.allies || [];
  const items = combat.inventory || [];
  const isHeroTurn = combat.waitingForAction && !combat.finished && !!hero;

  // --- VICTORY / DEFEAT SCREEN ---
  if (combat.finished) {
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-md animate-in fade-in duration-500">
        <div className="max-w-xl w-full mx-4 relative overflow-hidden rounded-2xl border border-white/10 bg-gray-900/90 shadow-2xl p-8 text-center">
            {/* Background Glow */}
            <div className={`absolute -top-20 -left-20 w-64 h-64 rounded-full blur-[100px] opacity-20 ${combat.victory ? 'bg-yellow-400' : 'bg-red-600'}`}></div>
            <div className={`absolute -bottom-20 -right-20 w-64 h-64 rounded-full blur-[100px] opacity-20 ${combat.victory ? 'bg-yellow-400' : 'bg-red-600'}`}></div>

            <h1 className={`relative text-6xl font-black italic tracking-tighter mb-2 drop-shadow-lg ${combat.victory ? 'text-transparent bg-clip-text bg-gradient-to-b from-yellow-300 to-yellow-600' : 'text-red-500'}`}>
              {combat.victory ? 'VICTOIRE' : 'DÉFAITE'}
            </h1>
            <div className="h-1 w-24 mx-auto bg-gradient-to-r from-transparent via-gray-500 to-transparent mb-8"></div>

            {combat.result && (
              <div className="grid grid-cols-2 gap-4 mb-8 relative z-10">
                 <div className="bg-gray-800/50 p-4 rounded-xl border border-gray-700">
                    <div className="text-gray-400 text-xs uppercase tracking-widest mb-1">Tours</div>
                    <div className="text-2xl font-bold text-white">{combat.result.turns}</div>
                 </div>
                 <div className="bg-gray-800/50 p-4 rounded-xl border border-gray-700">
                    <div className="text-gray-400 text-xs uppercase tracking-widest mb-1">XP Totale</div>
                    <div className="text-2xl font-bold text-purple-400">+{combat.result.xp}</div>
                 </div>
                 <div className="bg-gray-800/50 p-4 rounded-xl border border-gray-700 col-span-2">
                    <div className="text-gray-400 text-xs uppercase tracking-widest mb-1">MVP (Dégâts)</div>
                    <div className="text-lg font-bold text-red-300">
                        {combat.result.damagePerHero && Object.entries(combat.result.damagePerHero).sort(([,a],[,b]) => b-a)[0] 
                            ? `${Object.entries(combat.result.damagePerHero).sort(([,a],[,b]) => b-a)[0][0]} (${Object.entries(combat.result.damagePerHero).sort(([,a],[,b]) => b-a)[0][1]} dmg)`
                            : "Personne"}
                    </div>
                 </div>
              </div>
            )}

            <button 
              onClick={onBack}
              className={`relative z-10 px-10 py-4 text-xl font-bold uppercase tracking-widest transition-all transform hover:scale-105 active:scale-95 rounded-lg shadow-lg ${combat.victory ? 'bg-gradient-to-r from-yellow-600 to-amber-700 text-white hover:shadow-yellow-500/20' : 'bg-gray-700 text-gray-300 hover:bg-gray-600'}`}
            >
              C ontinuer
            </button>
        </div>
      </div>
    );
  }

  // --- MAIN COMBAT VIEW ---
  return (
    <div className="flex flex-col h-[calc(100vh-80px)] overflow-hidden bg-gradient-to-b from-gray-900 via-[#111] to-black text-gray-100 font-sans select-none">
      
      {/* TOP BAR: Turn Info & Log Toggle */}
      <div className="h-14 flex items-center justify-between px-6 bg-black/30 backdrop-blur-sm border-b border-white/5 z-20 shrink-0">
         <div className="flex items-center gap-6">
             {title && (
                <div className="text-lg font-black italic tracking-tighter text-transparent bg-clip-text bg-gradient-to-r from-rpg-gold to-yellow-600 uppercase">
                   {title}
                </div>
             )}
             <div className="flex flex-col">
                <span className="text-[10px] text-gray-500 uppercase tracking-widest font-bold">Tour Actuel</span>
                <span className="text-xl font-mono text-white leading-none">{Math.floor((logs.filter(l => l.type === 'tour').length / 2) + 1)}</span>
             </div>
             {combat.active && !isHeroTurn && !combat.finished && (
                <div className="flex items-center gap-2 px-3 py-1 rounded-full bg-yellow-900/20 border border-yellow-700/30 text-yellow-500 text-xs animate-pulse">
                   <span>⏳ Tour Ennemi...</span>
                </div>
             )}
         </div>
         
         {onAbandon && (
             <button onClick={onAbandon} className="text-xs text-red-900 hover:text-red-500 transition-colors uppercase tracking-wider font-bold px-3 py-1 rounded hover:bg-red-900/10">
                Abandonner
             </button>
         )}
      </div>

      {/* BATTLEFIELD LAYER */}
      <div className="flex-1 relative overflow-hidden flex flex-col lg:flex-row">
         
         {/* Background Decoration */}
         <div className="absolute inset-0 z-0 pointer-events-none opacity-20">
             <div className="absolute top-1/2 left-1/4 w-96 h-96 bg-blue-500/20 blur-[120px] rounded-full"></div>
             <div className="absolute top-1/3 right-1/4 w-96 h-96 bg-red-500/20 blur-[120px] rounded-full"></div>
         </div>

         {/* LEFT: ALLIES (Heroes) */}
         <div className="flex-1 z-10 p-2 md:p-6 flex flex-row lg:flex-col justify-center items-center lg:items-end lg:pr-12 gap-2 md:gap-4 lg:gap-6 overflow-x-auto lg:overflow-x-visible lg:overflow-y-auto custom-scrollbar w-full lg:w-auto h-[140px] lg:h-full shrink-0 lg:shrink">
            {allies.map((a, i) => {
                const isCurrentActor = hero?.nom === a.nom;
                const isTarget = (actionMode === 'itemTarget') || (actionMode === 'target' && selectedComp >= 0 && hero?.competences[selectedComp]?.cible === 'UnAllie');
                
                return (
                  <HeroCard 
                     key={i} 
                     hero={a} 
                     isActive={isCurrentActor} 
                     isTargetable={isTarget}
                     onClick={isTarget ? () => {
                        if(actionMode === 'itemTarget') onAction('item', -1, i, selectedItem);
                        else onAction('attack', selectedComp, i);
                        setActionMode('main');
                     } : undefined}
                  />
                );
            })}
         </div>

         {/* CENTER VS / LOGS (Hidden on mobile mostly or small) */}
         <div className="hidden lg:flex w-48 xl:w-64 z-10 flex-col justify-end pb-6 gap-4 pointer-events-none">
             {/* Dynamic Log Feed */}
             <div className="h-full max-h-[400px] w-full flex flex-col justify-end pointer-events-auto">
                 <CombatLog logs={logs} logRef={logRef} />
             </div>
         </div>

         {/* RIGHT: ENEMIES */}
         <div className="flex-1 z-10 p-2 md:p-6 flex flex-row lg:flex-col justify-center items-center lg:items-start lg:pl-12 gap-2 md:gap-4 lg:gap-6 overflow-x-auto lg:overflow-x-visible lg:overflow-y-auto custom-scrollbar w-full lg:w-auto h-[160px] lg:h-full content-center flex-wrap lg:flex-nowrap">
            <div className="flex flex-row lg:flex-col lg:flex-wrap justify-center gap-2 md:gap-4 lg:gap-6">
               {enemies.map((e, i) => {
                  const isTarget = actionMode === 'target' && selectedComp >= 0 && hero?.competences[selectedComp]?.cible !== 'UnAllie';
                  return (
                     <EnemyCard 
                        key={i} 
                        enemy={e} 
                        isTargetable={isTarget}
                        onClick={isTarget ? () => {
                           onAction('attack', selectedComp, i);
                           setActionMode('main');
                        } : undefined}
                     />
                  );
               })}
            </div>
         </div>
      </div>

      {/* BOTTOM CONTROL PANEL (Only visible if Hero Turn) */}
      <div className="h-auto min-h-[140px] z-30 bg-gray-950/90 backdrop-blur-xl border-t border-white/10 flex flex-col lg:flex-row items-stretch shadow-[0_-10px_40px_rgba(0,0,0,0.5)] transition-transform duration-300">
         
         {/* Mobile Log (Visible only on small screens) */}
         <div className="lg:hidden h-24 overflow-y-auto bg-black/40 p-2 text-xs border-b border-white/5">
            <CombatLog logs={logs} logRef={logRef} />
         </div>

         {isHeroTurn ? (
            <>
               {/* ACTIVE HERO INFO */}
               <div className="w-full lg:w-64 p-4 lg:border-r border-white/10 flex flex-row lg:flex-col items-center lg:items-start justify-between gap-2 bg-gradient-to-r lg:bg-gradient-to-b from-blue-900/10 to-transparent">
                  <div>
                     <div className="text-[10px] text-blue-400 uppercase tracking-widest font-bold mb-1">Votre Tour</div>
                     <div className="text-2xl font-black text-white">{hero.nom}</div>
                  </div>
                  <div className="flex flex-col w-32 gap-1 text-xs font-mono">
                     <ProgressBar current={hero.pv} max={hero.pvMax} color="bg-green-500" label="PV" />
                     <ProgressBar current={hero.pm} max={hero.pmMax} color="bg-blue-500" label="PM" />
                  </div>
               </div>

               {/* ACTION GRID */}
               <div className="flex-1 p-4 relative">
                  {/* MAIN MENU */}
                  {actionMode === 'main' && (
                     <div className="grid grid-cols-2 md:grid-cols-4 gap-3 h-full max-w-4xl mx-auto items-center">
                        <MenuButton icon="⚔️" label="Attaque" hotkey="1" onClick={() => setActionMode('attack')} color="red" />
                        <MenuButton icon="🛡️" label="Défense" hotkey="2" onClick={() => { onAction('defend'); }} color="blue" description="-50% Dégâts" />
                        <MenuButton icon="🎒" label="Objets" hotkey="3" onClick={() => setActionMode('item')} color="green" disabled={items.length === 0} count={items.length} />
                        <MenuButton icon="🏃" label="Fuir" hotkey="4" onClick={() => onAbandon && onAbandon()} color="gray" secondary />
                     </div>
                  )}

                  {/* SKILLS MENU */}
                  {actionMode === 'attack' && (
                     <div className="flex flex-col h-full animate-in slide-in-from-bottom-4 fade-in duration-300">
                        <div className="flex justify-between items-center mb-2 px-2">
                           <h3 className="text-sm font-bold text-red-400 uppercase tracking-widest">Choisir une compétence</h3>
                           <button onClick={() => setActionMode('main')} className="text-xs text-gray-500 hover:text-white transition-colors">ECHAP</button>
                        </div>
                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3 overflow-y-auto max-h-[140px] pr-2 custom-scrollbar">
                           {hero.competences.map((c, i) => (
                              <SkillButton key={i} comp={c} hero={hero} onClick={() => {
                                 setSelectedComp(i);
                                 if (c.cible === 'Soi' || c.cible === 'TousLesEnnemis') {
                                    onAction('attack', i, 0); 
                                    setActionMode('main'); 
                                 } else {
                                    setActionMode('target');
                                 }
                              }} />
                           ))}
                        </div>
                     </div>
                  )}

                  {/* ITEMS MENU */}
                  {actionMode === 'item' && (
                     <div className="flex flex-col h-full animate-in slide-in-from-bottom-4 fade-in duration-300">
                         <div className="flex justify-between items-center mb-2 px-2">
                           <h3 className="text-sm font-bold text-green-400 uppercase tracking-widest">Inventaire</h3>
                           <button onClick={() => setActionMode('main')} className="text-xs text-gray-500 hover:text-white transition-colors">ECHAP</button>
                        </div>
                        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3 overflow-y-auto max-h-[140px] pr-2 custom-scrollbar">
                           {items.map((it, i) => (
                              <button key={i} 
                                 className="flex flex-col p-3 bg-gray-800/50 hover:bg-gray-700 border border-gray-700/50 hover:border-green-500/50 rounded-lg transition-all group"
                                 onClick={() => { setSelectedItem(i); setActionMode('itemTarget'); }}>
                                 <div className="flex justify-between w-full mb-1">
                                     <span className="font-semibold text-gray-200 group-hover:text-green-300 text-sm truncate">{it.nom}</span>
                                     <span className="text-[10px] bg-gray-900 px-1.5 rounded-full text-gray-400">x{it.quantite}</span>
                                 </div>
                                 <div className="text-[10px] text-gray-500 truncate">{it.description}</div>
                              </button>
                           ))}
                        </div>
                     </div>
                  )}

                  {/* TARGET SELECTION PROMPT */}
                  {(actionMode === 'target' || actionMode === 'itemTarget') && (
                     <div className="flex flex-col items-center justify-center h-full gap-3 animate-pulse">
                        <div className="text-3xl text-yellow-500">🎯</div>
                        <div className="text-lg text-white font-medium tracking-wide">Sélectionnez une cible sur le terrain</div>
                        <button onClick={() => setActionMode(actionMode === 'target' ? 'attack' : 'item')} className="px-4 py-1 rounded-full bg-gray-800 text-xs text-gray-400 hover:bg-gray-700 hover:text-white transition-colors">Annuler</button>
                     </div>
                  )}
               </div>
            </>
         ) : (
            <div className="flex w-full items-center justify-center h-full gap-3 opacity-50">
               <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
               <span className="text-sm font-medium tracking-widest text-gray-400 uppercase">En attente...</span>
            </div>
         )}
      </div>
    </div>
  );
}

// --- MICRO COMPONENTS ---

function ProgressBar({ current, max, color, label }: { current: number, max: number, color: string, label?: string }) {
    const pct = Math.max(0, Math.min(100, (current / max) * 100));
    return (
        <div className="w-full">
            {label && <div className="flex justify-between text-[10px] uppercase text-gray-500 mb-0.5"><span>{label}</span><span>{current}/{max}</span></div>}
            <div className="h-1.5 w-full bg-gray-800 rounded-full overflow-hidden">
                <div className={`h-full transition-all duration-500 ease-out ${color}`} style={{ width: `${pct}%` }}></div>
            </div>
        </div>
    );
}

function MenuButton({ icon, label, hotkey, onClick, color, description, disabled, count, secondary }: any) {
    const baseColor = color === 'red' ? 'hover:bg-red-900/20 hover:border-red-500/50' 
                   : color === 'blue' ? 'hover:bg-blue-900/20 hover:border-blue-500/50'
                   : color === 'green' ? 'hover:bg-green-900/20 hover:border-green-500/50'
                   : 'hover:bg-gray-800 hover:border-gray-500';
    
    return (
        <button 
            onClick={onClick}
            disabled={disabled}
            className={`relative flex flex-col items-center justify-center p-3 h-full rounded-xl border border-white/5 bg-white/5 transition-all duration-200 group disabled:opacity-30 disabled:cursor-not-allowed active:scale-95 ${baseColor} ${secondary ? 'opacity-70' : ''}`}
        >
            <div className="text-2xl mb-1 group-hover:scale-110 transition-transform">{icon}</div>
            <div className="font-bold text-sm tracking-wide text-gray-200">{label}</div>
            {description && <div className="text-[10px] text-gray-500 mt-1">{description}</div>}
            {count !== undefined && <div className="absolute top-2 right-2 text-[10px] bg-gray-900 px-1.5 rounded-full text-gray-400">{count}</div>}
            <div className="absolute top-2 left-2 text-[9px] text-gray-600 font-mono hidden md:block">{hotkey}</div>
        </button>
    )
}

function SkillButton({ comp, hero, onClick }: { comp: Competence, hero: Hero, onClick: () => void }) {
    const cost = comp.cout;
    const canAfford = hero.pm >= cost;
    return (
        <button 
           onClick={onClick}
           disabled={!canAfford}
           className={`text-left p-3 rounded-lg border bg-gray-800/40 hover:bg-gray-700/60 transition-all flex flex-col justify-center gap-1 group disabled:opacity-40 disabled:cursor-not-allowed ${canAfford ? 'border-gray-700 hover:border-gray-500' : 'border-red-900/30'}`}
        >
            <div className="flex justify-between w-full items-center">
               <span className={`font-semibold text-sm ${canAfford ? 'text-gray-200 group-hover:text-white' : 'text-gray-500'}`}>{comp.nom}</span>
               {cost > 0 && <span className={`text-[10px] font-mono px-1.5 rounded ${canAfford ? 'bg-blue-900/30 text-blue-300' : 'text-red-500'}`}>{cost} PM</span>}
            </div>
            <div className="flex gap-2 text-[10px] text-gray-500">
               <span className={`${elementColor(comp.element)} opacity-80`}>{comp.element}</span>
               <span>•</span>
               <span>{comp.cible}</span>
            </div>
        </button>
    )
}

function HeroCard({ hero, isActive, isTargetable, onClick }: { hero: any, isActive: boolean, isTargetable?: boolean, onClick?: () => void }) {
    const isDead = !hero.estVivant;
    return (
        <div 
           onClick={!isDead ? onClick : undefined}
           className={`relative w-40 md:w-56 lg:w-64 transition-all duration-300 p-2 md:p-4 rounded-xl border backdrop-blur-md flex flex-col gap-2 md:gap-3 group shrink-0
             ${isActive ? 'bg-blue-900/40 border-blue-400 shadow-[0_0_20px_rgba(59,130,246,0.3)] scale-105 z-20' : 'bg-gray-900/60 border-gray-700 hover:bg-gray-800/80'}
             ${isTargetable ? 'cursor-pointer ring-2 ring-yellow-400 animate-pulse hover:scale-105 shadow-yellow-900/20' : ''}
             ${isDead ? 'opacity-50 grayscale' : ''}
           `}
        >
            <div className="flex items-center gap-2 md:gap-4">
               <div className="relative shrink-0">
                  <ClassIcon type={hero.classe || 'Guerrier'} size={40} className={`drop-shadow-lg md:scale-110 ${isActive ? 'brightness-125' : 'brightness-90'}`} />
                  {/* Status Icons */}
                  <div className="absolute -bottom-2 -right-2 flex gap-0.5 flex-wrap max-w-[40px] justify-end">
                     {hero.effets?.map((e: any, i: number) => (
                        <span key={i} title={e.statut} className="text-[8px] bg-gray-950 border border-gray-700 rounded-full w-3 h-3 md:w-4 md:h-4 flex items-center justify-center scale-90 md:scale-100">{statusIcon(e.statut)}</span>
                     ))}
                  </div>
               </div>
               <div className="flex-1 min-w-0">
                  <div className={`font-bold text-sm md:text-lg truncate ${isActive ? 'text-blue-300' : 'text-gray-300'}`}>{hero.nom}</div>
                  <div className="text-[8px] md:text-[10px] text-gray-500 uppercase tracking-widest font-mono">Niv {hero.niveau}</div>
               </div>
            </div>

            <div className="space-y-1 md:space-y-2 w-full">
               <ProgressBar current={hero.pv} max={hero.pvMax} color={hero.pv < hero.pvMax * 0.3 ? "bg-red-500" : "bg-green-500"} label="PV" />
               <ProgressBar current={hero.pm} max={hero.pmMax} color="bg-blue-500" label="PM" />
            </div>

            {/* Active turn indicator */}
            {isActive && <div className="absolute -left-1 md:-left-2 top-1/2 -translate-y-1/2 w-1 h-8 md:h-12 bg-blue-500 rounded-full shadow-[0_0_10px_rgba(59,130,246,0.8)]"></div>}
        </div>
    )
}

function EnemyCard({ enemy, isTargetable, onClick }: { enemy: any, isTargetable?: boolean, onClick?: () => void }) {
    const isDead = !enemy.estVivant;
    const hpPct = (enemy.pv / enemy.pvMax) * 100;
    
    // Icon Logic
    let iconType = "Monstre";
    const name = enemy.nom || "";
    if (enemy.isBoss) iconType = "DragonAncien";
    else if (name.includes("Liche")) iconType = "Liche";
    else if (name.includes("Squelette")) iconType = "Squelette";
    else if (name.includes("Gobelin")) iconType = "Gobelin";
    else if (name.includes("Orc")) iconType = "Orc";
    else if (name.includes("Loup")) iconType = "Loup";
    else if (name.includes("Dragon")) iconType = "Dragon";
    else if (name.includes("Spectre")) iconType = "Spectre";
    
    return (
        <div 
           onClick={!isDead ? onClick : undefined}
           className={`relative w-28 md:w-36 lg:w-40 transition-all duration-300 p-2 md:p-3 rounded-xl border backdrop-blur-sm flex flex-col items-center gap-1 md:gap-2 group shrink-0
             ${isTargetable ? 'cursor-pointer ring-2 ring-red-500 scale-105 bg-red-900/10 border-red-500/50 shadow-[0_0_20px_rgba(239,68,68,0.3)] z-20' : 'bg-gray-900/40 border-gray-800 hover:bg-gray-800/60'}
             ${isDead ? 'opacity-0 scale-90 pointer-events-none w-0 p-0 m-0 overflow-hidden' : ''}
           `}
        >
            {enemy.isBoss && <div className="absolute -top-2 md:-top-3 px-1.5 md:px-2 py-0.5 bg-red-900 border border-red-500 text-[6px] md:text-[8px] uppercase font-bold text-red-200 tracking-widest rounded shadow-lg z-10">BOSS</div>}
            
            <div className={`relative transition-transform duration-300 ${isTargetable ? 'scale-110' : ''}`}>
               <ClassIcon type={iconType} size={48} className="drop-shadow-2xl md:scale-125" />
            </div>

            <div className="text-center w-full min-w-0">
               <div className="font-bold text-xs md:text-sm text-red-200 truncate px-1 w-full" title={name}>{name}</div>
               <div className="w-full h-1 md:h-1.5 bg-gray-800 rounded-full mt-1 overflow-hidden">
                   <div className={`h-full transition-all duration-300 ${hpPct < 30 ? 'bg-red-600' : 'bg-red-500'}`} style={{ width: `${hpPct}%` }}></div>
               </div>
            </div>

            <div className="flex gap-0.5 justify-center min-h-[12px] md:min-h-[16px] flex-wrap">
               {enemy.effets?.map((e: any, i: number) => (
                   <span key={i} className="text-[8px] md:text-[10px] animate-bounce" style={{ animationDelay: `${i*0.1}s` }}>
                      {statusIcon(e.statut)}
                   </span>
               ))}
            </div>
        </div>
    )
}

function CombatLog({ logs, logRef }: { logs: LogEntry[], logRef: any }) {
    if (logs.length === 0) return <div className="text-center text-gray-600 text-xs italic mt-4">Le combat commence...</div>;
    return (
       <div ref={logRef} className="flex flex-col gap-1 overflow-y-auto h-full pr-1 custom-scrollbar mask-image-b">
          {logs.map((l, i) => (
             <div key={i} className={`text-xs p-1.5 rounded animate-in fade-in slide-in-from-left-2 duration-200 ${logStyle(l.type)}`}>
                 <span className="mr-2 opacity-50 text-[10px]">{new Date().toLocaleTimeString([], {hour: '2-digit', minute:'2-digit', second:'2-digit'})}</span>
                 {l.message}
             </div>
          ))}
       </div>
    )
}

function logStyle(type: string) {
   switch(type) {
      case 'degats': return 'text-red-300 bg-red-900/10 border-l-2 border-red-500';
      case 'soin': return 'text-green-300 bg-green-900/10 border-l-2 border-green-500';
      case 'critique': return 'text-yellow-300 font-bold bg-yellow-900/10 border-l-2 border-yellow-500';
      case 'mort': return 'text-gray-400 line-through decoration-red-500';
      case 'tour': return 'text-blue-300 text-center font-bold border-y border-blue-900/30 py-1 bg-blue-900/5 my-1';
      default: return 'text-gray-400';
   }
}

function statusIcon(s: string) {
    switch (s) {
      case 'Poison': return '🧪'; case 'Brulure': return '🔥'; case 'Gel': return '❄️';
      case 'Paralysie': return '⚡'; case 'Sommeil': return '💤';
      case 'BuffAttaque': return '⚔️'; case 'DebuffDefense': return '🛡️';
      default: return '✨';
    }
}
  
function elementColor(e: string) {
    switch (e) {
      case 'Feu': return 'text-red-400'; case 'Glace': return 'text-cyan-400';
      case 'Foudre': return 'text-yellow-400'; case 'Tenebres': return 'text-purple-400';
      case 'Lumiere': return 'text-yellow-200'; default: return 'text-gray-400';
    }
}

