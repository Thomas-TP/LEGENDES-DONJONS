import { useState, useEffect } from 'react';
import type { GameState, ShopData } from '../types';
import * as api from '../api';

interface Props {
  state: GameState;
  onUpdate: (s: GameState) => void;
  onBack: () => void;
}

export default function Shop({ state, onUpdate, onBack }: Props) {
  const [shop, setShop] = useState<ShopData | null>(null);
  const [tab, setTab] = useState<'equipment' | 'items'>('equipment');
  const [msg, setMsg] = useState('');

  useEffect(() => { api.getShopItems().then(setShop); }, []);

  const buy = async (name: string, cat: string, heroIdx: number = -1) => {
    const s = await api.buyItem(name, cat, 1, heroIdx);
    onUpdate(s);
    setShop(await api.getShopItems());
    setMsg(`Acheté: ${name}`);
    setTimeout(() => setMsg(''), 2000);
  };

  const sell = async (heroIdx: number, slot: string) => {
    const s = await api.sellEquipment(heroIdx, slot);
    onUpdate(s);
    setShop(await api.getShopItems());
    setMsg('Équipement vendu !');
    setTimeout(() => setMsg(''), 2000);
  };

  if (!shop) return <div className="text-center py-12 text-gray-400">Chargement...</div>;

  return (
    <div className="flex flex-col gap-6 py-8">
      <h1 className="rpg-title text-2xl">🏪 Boutique</h1>
      <p className="text-center text-rpg-gold text-lg">💰 Or: {shop.gold}</p>

      {msg && <div className="text-center text-rpg-green text-sm animate-pulse">{msg}</div>}

      <div className="flex gap-2 justify-center">
        <button className={`rpg-btn ${tab === 'equipment' ? 'rpg-btn-primary' : ''}`} onClick={() => setTab('equipment')}>⚔️ Équipements</button>
        <button className={`rpg-btn ${tab === 'items' ? 'rpg-btn-primary' : ''}`} onClick={() => setTab('items')}>🧪 Objets</button>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
        {(tab === 'equipment' ? shop.equipment : shop.items).map((item, i) => (
          <div key={i} className="rpg-panel flex flex-col gap-2">
            <div className="flex justify-between items-start">
              <div>
                <p className="text-rpg-gold font-semibold text-sm">{item.nom}</p>
                <p className="text-gray-400 text-xs">{item.description}</p>
                <p className="text-xs text-gray-500">{item.categorie}</p>
              </div>
              <span className="text-rpg-gold font-bold">{item.prix}💰</span>
            </div>
            {tab === 'equipment' ? (
              <div className="flex gap-1 flex-wrap">
                {state.team.map((h, hi) => (
                  <button key={hi} className="rpg-btn-primary text-[10px] py-1 px-2"
                    disabled={shop.gold < item.prix}
                    onClick={() => buy(item.nom, item.categorie, hi)}>
                    {h.nom}
                  </button>
                ))}
              </div>
            ) : (
              <button className="rpg-btn-primary text-xs" disabled={shop.gold < item.prix}
                onClick={() => buy(item.nom, 'Objet')}>Acheter</button>
            )}
          </div>
        ))}
      </div>

      {state.team.length > 0 && (
        <div className="rpg-panel">
          <h3 className="text-rpg-gold font-semibold mb-2">Vendre un équipement</h3>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
            {state.team.map((h, hi) => (
              <div key={hi} className="text-xs">
                <p className="font-semibold text-gray-300">{h.nom}</p>
                {h.arme && <button className="rpg-btn-danger text-[10px] mr-1" onClick={() => sell(hi, 'arme')}>Vendre {h.arme.nom}</button>}
                {h.armure && <button className="rpg-btn-danger text-[10px] mr-1" onClick={() => sell(hi, 'armure')}>Vendre {h.armure.nom}</button>}
                {h.accessoire && <button className="rpg-btn-danger text-[10px]" onClick={() => sell(hi, 'accessoire')}>Vendre {h.accessoire.nom}</button>}
              </div>
            ))}
          </div>
        </div>
      )}

      <button className="rpg-btn mx-auto" onClick={onBack}>← Retour</button>
    </div>
  );
}
