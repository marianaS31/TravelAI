import React, { useEffect, useRef, useState } from 'react';
import type { DiaItinerarioDTO } from '../types/viagem';
import { Map, Loader2 } from 'lucide-react';

// Declaração global para o TypeScript reconhecer o objeto window.google
declare global {
  interface Window {
    google?: any;
    __googleMapsCallbackInit?: () => void;
  }
}

interface MapaItinerarioProps {
  dias: DiaItinerarioDTO[];
  apiKeyGoogleMaps?: string;
}

export const MapaItinerario: React.FC<MapaItinerarioProps> = ({
  dias,
  apiKeyGoogleMaps = 'A_TUA_CHAVE_GOOGLE_MAPS_AQUI',
}) => {
  const mapRef = useRef<HTMLDivElement>(null);
  const [apiPronta, setApiPronta] = useState<boolean>(false);
  const mapInstanceRef = useRef<any>(null);
  const markersRef = useRef<any[]>([]);
  const polylineRef = useRef<any>(null);

  // 1. Carrega o script do Google Maps sem bibliotecas externas
  useEffect(() => {
    if (window.google?.maps) {
      setApiPronta(true);
      return;
    }

    if (apiKeyGoogleMaps === 'A_TUA_CHAVE_GOOGLE_MAPS_AQUI') return;

    const scriptExistente = document.getElementById('google-maps-script');
    if (!scriptExistente) {
      window.__googleMapsCallbackInit = () => {
        setApiPronta(true);
      };

      const script = document.createElement('script');
      script.id = 'google-maps-script';
      script.src = `https://maps.googleapis.com/maps/api/js?key=${apiKeyGoogleMaps}&callback=__googleMapsCallbackInit&libraries=places,geometry`;
      script.async = true;
      script.defer = true;
      document.head.appendChild(script);
    } else {
      setApiPronta(true);
    }
  }, [apiKeyGoogleMaps]);

  // 2. Renderiza os marcadores e a rota quando o mapa estiver disponível
  useEffect(() => {
    if (!apiPronta || !mapRef.current || !window.google?.maps) return;

    const coordenadas: { lat: number; lng: number; titulo: string; dia: number }[] = [];

    dias.forEach((dia) => {
      dia.atividades.forEach((atv) => {
        if (atv.latitude && atv.longitude) {
          coordenadas.push({
            lat: Number(atv.latitude),
            lng: Number(atv.longitude),
            titulo: atv.titulo,
            dia: dia.numeroDia,
          });
        }
      });
    });

    if (coordenadas.length === 0) return;

    const centro = { lat: coordenadas[0].lat, lng: coordenadas[0].lng };

    if (!mapInstanceRef.current) {
      mapInstanceRef.current = new window.google.maps.Map(mapRef.current, {
        center: centro,
        zoom: 13,
      });
    }

    const map = mapInstanceRef.current;

    // Limpar marcadores anteriores
    markersRef.current.forEach((m) => m.setMap(null));
    markersRef.current = [];

    const bounds = new window.google.maps.LatLngBounds();
    const caminho: { lat: number; lng: number }[] = [];

    coordenadas.forEach((coord, i) => {
      const posicao = { lat: coord.lat, lng: coord.lng };
      bounds.extend(posicao);
      caminho.push(posicao);

      const marker = new window.google.maps.Marker({
        position: posicao,
        map: map,
        title: `Dia ${coord.dia}: ${coord.titulo}`,
        label: {
          text: `${i + 1}`,
          color: '#ffffff',
          fontWeight: 'bold',
        },
      });

      const infoWindow = new window.google.maps.InfoWindow({
        content: `<div style="padding: 4px; font-family: sans-serif;">
                    <strong>Dia ${coord.dia} - Paragem ${i + 1}</strong>
                    <p style="margin: 4px 0 0 0; font-size: 12px;">${coord.titulo}</p>
                  </div>`,
      });

      marker.addListener('click', () => {
        infoWindow.open(map, marker);
      });

      markersRef.current.push(marker);
    });

    if (polylineRef.current) {
      polylineRef.current.setMap(null);
    }

    polylineRef.current = new window.google.maps.Polyline({
      path: caminho,
      geodesic: true,
      strokeColor: '#2563eb',
      strokeOpacity: 0.8,
      strokeWeight: 3,
    });

    polylineRef.current.setMap(map);

    if (coordenadas.length > 1) {
      map.fitBounds(bounds);
    }
  }, [apiPronta, dias]);

  const totalCoordenadas = dias.reduce(
    (acc, dia) => acc + dia.atividades.filter((a) => a.latitude && a.longitude).length,
    0
  );

  return (
    <section className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden mt-8">
      <div className="px-6 py-4 bg-slate-50 border-b border-slate-100 flex items-center justify-between">
        <div className="flex items-center gap-2 text-slate-800 font-semibold">
          <Map size={18} className="text-blue-600" />
          <span>Mapa Interativo do Roteiro</span>
        </div>
        <span className="text-xs bg-blue-100 text-blue-800 font-medium px-2.5 py-1 rounded-full">
          {totalCoordenadas} localizações georreferenciadas
        </span>
      </div>

      {totalCoordenadas === 0 ? (
        <div className="h-64 flex flex-col items-center justify-center p-6 text-center text-slate-400 bg-slate-50/50">
          <Map size={36} className="mb-2 opacity-50" />
          <p className="text-sm font-medium text-slate-600">Nenhuma localização geográfica encontrada</p>
          <p className="text-xs text-slate-400 max-w-sm mt-1">
            As atividades ainda não possuem coordenadas válidas enviadas pela API TravelAI.
          </p>
        </div>
      ) : (
        <div className="relative w-full h-96">
          {!apiPronta && (
            <div className="absolute inset-0 flex items-center justify-center bg-slate-50 text-slate-500 text-sm gap-2">
              <Loader2 size={18} className="animate-spin" />
              <span>A carregar mapa...</span>
            </div>
          )}
          <div ref={mapRef} className="w-full h-full" />
        </div>
      )}
    </section>
  );
};