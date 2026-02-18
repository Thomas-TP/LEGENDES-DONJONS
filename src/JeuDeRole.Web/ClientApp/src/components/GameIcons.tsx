import React from 'react';

// ================= COULEURS ET TYPES =================
const CLASS_COLORS: Record<string, string> = {
  Guerrier: '#ef4444', // Red
  Mage: '#3b82f6', // Blue
  Voleur: '#10b981', // Emerald
  Clerc: '#eab308', // Yellow
  Paladin: '#f59e0b', // Amber
  Necromancien: '#a855f7', // Purple
  Assassin: '#71717a', // Zinc
  Druide: '#84cc16', // Lime
  Monstre: '#dc2626', // Red
  Boss: '#7f1d1d', // Dark Red
};

// ================= COMPOSANTS SVG =================

export const ClassIcon = ({ type, size = 64, className = "" }: { type: string; size?: number, className?: string }) => {
  const color = CLASS_COLORS[type] || '#9ca3af';
  
  const renderIcon = () => {
    switch (type) {
      case 'Guerrier':
        return (
          <g transform="translate(12,12) scale(0.6)">
             <path d="M40 10 L50 20 L40 30 L10 60 L0 50 L30 20 Z" fill={color} stroke="white" strokeWidth="2"/>
             <path d="M50 20 L80 50 L70 60 L40 30 Z" fill={color} stroke="white" strokeWidth="2"/>
             <rect x="5" y="5" width="20" height="90" transform="rotate(-45 15 50)" fill="#555" />
          </g>
        );
      case 'Mage':
        return (
          <g transform="translate(16,16) scale(0.6)">
            <circle cx="50" cy="20" r="15" fill={color} filter="url(#glow)" />
            <path d="M45 35 L55 35 L60 90 L40 90 Z" fill="#8B4513" />
            <path d="M30 20 Q50 0 70 20 Q50 40 30 20" fill="none" stroke="white" strokeWidth="2" opacity="0.5"/>
          </g>
        );
      case 'Voleur':
        return (
          <g transform="translate(16,16) scale(0.6)">
            <path d="M20 20 L50 80 L80 20 Q50 0 20 20" fill="#333" />
            <path d="M30 40 L50 80 L70 40" fill={color} />
            <circle cx="35" cy="35" r="5" fill="white" />
            <circle cx="65" cy="35" r="5" fill="white" />
          </g>
        );
      case 'Clerc':
        return (
          <g transform="translate(16,16) scale(0.6)">
            <path d="M40 10 L60 10 L60 30 L80 30 L80 50 L60 50 L60 90 L40 90 L40 50 L20 50 L20 30 L40 30 Z" fill={color} stroke="white" strokeWidth="3" />
            <circle cx="50" cy="50" r="30" fill="none" stroke="gold" strokeWidth="2" opacity="0.6"/>
          </g>
        );
      case 'Paladin':
        return (
          <g transform="translate(12,12) scale(0.6)">
            <path d="M20 20 L80 20 L90 50 L50 90 L10 50 Z" fill={color} stroke="silver" strokeWidth="3" />
            <path d="M40 30 L60 30 L60 45 L75 45 L75 55 L60 55 L60 80 L40 80 L40 55 L25 55 L25 45 L40 45 Z" fill="white" />
          </g>
        );
      case 'Necromancien':
        return (
          <g transform="translate(16,16) scale(0.6)">
             <path d="M20 60 Q50 10 80 60 Q50 90 20 60" fill="#2d2d2d" />
             <circle cx="35" cy="50" r="8" fill="red" />
             <circle cx="65" cy="50" r="8" fill="red" />
             <path d="M40 70 Q50 60 60 70" stroke="white" strokeWidth="2" fill="none"/>
             <path d="M50 10 L50 30" stroke={color} strokeWidth="3" />
          </g>
        );
      case 'Assassin':
        return (
          <g transform="translate(16,16) scale(0.6)">
             <path d="M20 80 L80 20 L90 30 L30 90 Z" fill="silver" />
             <path d="M25 85 L15 95" stroke="red" strokeWidth="3" /> 
             <path d="M30 40 L60 40" stroke={color} strokeWidth="2" />
          </g>
        );
      case 'Druide':
        return (
            <g transform="translate(16,16) scale(0.6)">
              <path d="M50 10 Q80 40 50 90 Q20 40 50 10" fill={color} />
              <path d="M50 10 L50 90" stroke="#14532d" strokeWidth="2" />
              <path d="M50 30 L70 10 M50 50 L30 30" stroke="#14532d" strokeWidth="2" />
            </g>
        );
      // Monstres
      case 'Orc':
      case 'Gobelin':
        return (
          <g transform="translate(16,16) scale(0.6)">
            <circle cx="50" cy="50" r="40" fill="#4d7c0f" />
            <path d="M30 40 L50 50 L70 40" fill="none" stroke="black" strokeWidth="3" />
            <circle cx="35" cy="35" r="5" fill="red" />
            <circle cx="65" cy="35" r="5" fill="red" />
            <path d="M30 70 Q50 80 70 70" fill="none" stroke="white" strokeWidth="3" />
            <path d="M35 70 L40 60 M65 70 L60 60" stroke="white" strokeWidth="3" />
          </g>
        );
      case 'Liche':
      case 'Squelette':
        return (
            <g transform="translate(16,16) scale(0.6)">
                <path d="M20 20 Q50 0 80 20 L80 70 Q50 100 20 70 Z" fill="#e5e7eb" />
                <circle cx="35" cy="40" r="10" fill="black" />
                <circle cx="65" cy="40" r="10" fill="black" />
                <circle cx="35" cy="40" r="3" fill="#a855f7" />
                 <circle cx="65" cy="40" r="3" fill="#a855f7" />
                <path d="M40 70 L40 80 M50 70 L50 80 M60 70 L60 80" stroke="black" strokeWidth="2" />
            </g>
        );
      case 'DragonAncien':
      case 'Dragon':
      case 'Serpent de Feu':
          return (
              <g transform="translate(10,10) scale(0.7)">
                  <path d="M20 50 Q40 20 60 50 Q80 20 90 60 L70 90 L30 90 L10 60 Z" fill={type.includes('Serpent') ? '#f97316' : '#b91c1c'} />
                  {!type.includes('Serpent') && <path d="M20 50 L10 20 L40 40" fill="#ef4444" />}
                  {!type.includes('Serpent') && <path d="M80 50 L90 20 L60 40" fill="#ef4444" />}
                  <circle cx="40" cy="60" r="5" fill="yellow" />
                  <circle cx="60" cy="60" r="5" fill="yellow" />
              </g>
          );
      case 'Spectre':
          return (
             <g transform="translate(16,16) scale(0.6)">
                <path d="M20 80 Q50 0 80 80 L70 95 L50 80 L30 95 Z" fill="#4c1d95" opacity="0.8" />
                <circle cx="35" cy="40" r="4" fill="#a78bfa" />
                <circle cx="65" cy="40" r="4" fill="#a78bfa" />
                <path d="M40 70 Q50 60 60 70" stroke="#a78bfa" strokeWidth="2" fill="none" />
             </g>
          );
      case 'Loup':
          return (
             <g transform="translate(10,10) scale(0.7)">
                <path d="M30 30 L10 10 L40 50 L50 90 L60 50 L90 10 L70 30 Z" fill="#525252" />
                <path d="M40 50 L30 70 L50 90 L70 70 L60 50" fill="#262626" />
                <circle cx="40" cy="40" r="3" fill="red" />
                <circle cx="60" cy="40" r="3" fill="red" />
             </g>
          );
      case 'Minotaure':
          return (
             <g transform="translate(10,10) scale(0.7)">
                <circle cx="50" cy="50" r="35" fill="#78350f" />
                <path d="M15 30 Q30 50 15 70" stroke="#fcd34d" strokeWidth="5" fill="none" />
                <path d="M85 30 Q70 50 85 70" stroke="#fcd34d" strokeWidth="5" fill="none" />
                <circle cx="40" cy="50" r="5" fill="red" />
                <circle cx="60" cy="50" r="5" fill="red" />
                <path d="M45 70 L55 70" stroke="black" strokeWidth="3" />
             </g>
          );
      case 'Golem de Pierre':
          return (
             <g transform="translate(10,10) scale(0.7)">
                <rect x="25" y="25" width="50" height="60" rx="5" fill="#57534e" />
                <rect x="15" y="35" width="10" height="40" rx="2" fill="#44403c" />
                <rect x="75" y="35" width="10" height="40" rx="2" fill="#44403c" />
                <rect x="35" y="40" width="10" height="5" fill="#22d3ee" />
                <rect x="55" y="40" width="10" height="5" fill="#22d3ee" />
             </g>
          );
      case 'Harpie':
          return (
             <g transform="translate(10,10) scale(0.7)">
                <path d="M30 40 Q50 20 70 40 L90 20 L80 60 L50 90 L20 60 L10 20 Z" fill="#0ea5e9" />
                <circle cx="40" cy="40" r="4" fill="yellow" />
                <circle cx="60" cy="40" r="4" fill="yellow" />
             </g>
          );
      default: // Monstre generique
        return (
          <g transform="translate(16,16) scale(0.6)">
             <path d="M20 20 H80 V80 H20 Z" fill={color} rx="10" />
             <path d="M30 40 L50 50 L70 40" stroke="black" strokeWidth="3" fill="none" />
             <circle cx="35" cy="30" r="5" fill="yellow" />
             <circle cx="65" cy="30" r="5" fill="yellow" />
             <path d="M30 65 Q50 80 70 65" stroke="black" strokeWidth="3" fill="none"/>
          </g>
        );
    }
  };

  return (
    <div className={`relative flex items-center justify-center rounded-xl bg-gray-900 border-2 shadow-lg overflow-hidden ${className}`} 
         style={{ width: size, height: size, borderColor: color, boxShadow: `0 0 10px ${color}40` }}>
      <svg width="100%" height="100%" viewBox="0 0 100 100">
        <defs>
          <filter id="glow" x="-20%" y="-20%" width="140%" height="140%">
            <feGaussianBlur stdDeviation="2" result="blur" />
            <feComposite in="SourceGraphic" in2="blur" operator="over" />
          </filter>
        </defs>
        <rect width="100%" height="100%" fill="url(#bgGradient)" opacity="0.3" />
        <radialGradient id="bgGradient">
            <stop offset="0%" stopColor={color} stopOpacity="0.5" />
            <stop offset="100%" stopColor="transparent" stopOpacity="0" />
        </radialGradient>
        {renderIcon()}
      </svg>
    </div>
  );
};
