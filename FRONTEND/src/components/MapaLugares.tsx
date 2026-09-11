import React from 'react';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import type { LugarSugeridoDTO } from '../types/viagem';
import { Map as MapIcon } from 'lucide-react';

interface MapaLugaresProps {
  alojamentos: LugarSugeridoDTO[];
  restaurantes: LugarSugeridoDTO[];
}

// Ícones coloridos por categoria (SVG simples, sem dependência de ficheiros de imagem externos)
function criarIcone(cor: string) {
  return L.divIcon({
    className: '',
    html: `<div style="
      background:${cor};
      width:14px;height:14px;
      border-radius:50%;
      border:2px solid white;
      box-shadow:0 1px 3px rgba(0,0,0,0.4);
    "></div>`,
    iconSize: [14, 14],
    iconAnchor: [7, 7],
  });
}

const ICONE_ALOJAMENTO = criarIcone('#6B4C7A'); // purple-600
const ICONE_RESTAURANTE = criarIcone('#0E6B63'); // emerald-600

export const MapaLugares: React.FC<MapaLugaresProps> = ({ alojamentos, restaurantes }) => {
  const comCoordenadas = (lista: LugarSugeridoDTO[]) =>
    lista.filter((l) => l.latitude !== undefined && l.longitude !== undefined);

  const alojamentosComCoord = comCoordenadas(alojamentos);
  const restaurantesComCoord = comCoordenadas(restaurantes);
  const todos = [...alojamentosComCoord, ...restaurantesComCoord];

  if (todos.length === 0) {
    return (
      <section className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden">
        <div className="px-5 py-3.5 bg-slate-50 border-b border-slate-100 flex items-center gap-2 text-slate-800 font-semibold">
          <MapIcon size={18} className="text-[#17324B]" />
          <span>Mapa</span>
        </div>
        <div className="h-64 flex flex-col items-center justify-center text-slate-400 text-sm gap-2">
          <MapIcon size={32} className="opacity-40" />
          <span>Sem coordenadas disponíveis para mostrar no mapa.</span>
        </div>
      </section>
    );
  }

  const centro: [number, number] = [
    todos.reduce((soma, l) => soma + l.latitude!, 0) / todos.length,
    todos.reduce((soma, l) => soma + l.longitude!, 0) / todos.length,
  ];

  return (
    <section className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden">
      <div className="px-5 py-3.5 bg-slate-50 border-b border-slate-100 flex items-center justify-between">
        <div className="flex items-center gap-2 text-slate-800 font-semibold">
          <MapIcon size={18} className="text-[#17324B]" />
          <span>Mapa</span>
        </div>
        <div className="flex items-center gap-3 text-xs text-slate-500">
          <span className="flex items-center gap-1">
            <span className="w-2.5 h-2.5 rounded-full bg-[#6B4C7A] inline-block" /> Alojamento
          </span>
          <span className="flex items-center gap-1">
            <span className="w-2.5 h-2.5 rounded-full bg-[#0E6B63] inline-block" /> Restaurantes
          </span>
        </div>
      </div>

      <div className="h-96 w-full">
        <MapContainer center={centro} zoom={13} style={{ height: '100%', width: '100%' }}>
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />

          {alojamentosComCoord.map((a, idx) => (
            <Marker key={`a-${idx}`} position={[a.latitude!, a.longitude!]} icon={ICONE_ALOJAMENTO}>
              <Popup>
                <strong>{a.nome}</strong>
                {a.avaliacao !== undefined && <div>⭐ {a.avaliacao}</div>}
                {a.morada && <div className="text-xs">{a.morada}</div>}
                {a.url && (
                  <a href={a.url} target="_blank" rel="noreferrer" className="text-[#17324B] underline text-xs">
                    Ver no Booking.com
                  </a>
                )}
              </Popup>
            </Marker>
          ))}

          {restaurantesComCoord.map((r, idx) => (
            <Marker key={`r-${idx}`} position={[r.latitude!, r.longitude!]} icon={ICONE_RESTAURANTE}>
              <Popup>
                <strong>{r.nome}</strong>
                {r.avaliacao !== undefined && <div>⭐ {r.avaliacao}</div>}
                {r.morada && <div className="text-xs">{r.morada}</div>}
                {r.url && (
                  <a href={r.url} target="_blank" rel="noreferrer" className="text-[#17324B] underline text-xs">
                    Ver no Google Maps
                  </a>
                )}
              </Popup>
            </Marker>
          ))}
        </MapContainer>
      </div>
    </section>
  );
};
