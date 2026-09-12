import React, { useEffect, useState } from 'react';
import { api } from '../api/api';
import type { ViagemResponseDTO } from '../types/viagem';
import { Calendar, Users, Loader2, ArrowLeft, MapPin } from 'lucide-react';

interface HistoricoViagensProps {
  onAbrirViagem: (viagem: ViagemResponseDTO) => void;
  onVoltar: () => void;
}

export const HistoricoViagens: React.FC<HistoricoViagensProps> = ({ onAbrirViagem, onVoltar }) => {
  const [viagens, setViagens] = useState<ViagemResponseDTO[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    const carregar = async () => {
      try {
        const resposta = await api.get<ViagemResponseDTO[]>('/viagens');
        setViagens(resposta.data);
      } catch {
        setErro('Não foi possível carregar o histórico de viagens.');
      } finally {
        setCarregando(false);
      }
    };
    carregar();
  }, []);

  return (
    <div className="max-w-3xl mx-auto">
      <button
        onClick={onVoltar}
        className="inline-flex items-center gap-2 text-sm text-[#4B5A68] hover:text-[#17324B] mb-6"
      >
        <ArrowLeft size={15} />
        <span>Voltar</span>
      </button>

      <h2
        className="text-3xl text-[#17324B] mb-8"
        style={{ fontFamily: "'Fraunces', serif", fontStyle: 'italic', fontWeight: 500 }}
      >
        As tuas viagens
      </h2>

      {carregando ? (
        <div className="flex items-center gap-2 text-sm text-[#4B5A68]">
          <Loader2 size={16} className="animate-spin" />
          <span>A carregar...</span>
        </div>
      ) : erro ? (
        <p className="text-sm text-[#c1440e]">{erro}</p>
      ) : viagens.length === 0 ? (
        <p className="text-sm text-[#4B5A68]">Ainda não geraste nenhuma viagem com esta conta.</p>
      ) : (
        <div className="space-y-3">
          {viagens.map((v) => (
            <button
              key={v.id}
              onClick={() => onAbrirViagem(v)}
              className="w-full text-left bg-white rounded-2xl border border-[#E4E1D8] px-6 py-4 hover:border-[#C8973C] transition flex items-center justify-between gap-4"
            >
              <div>
                <div className="flex items-center gap-1.5 text-[#17324B] font-semibold">
                  <MapPin size={14} className="text-[#6B4C7A]" />
                  <span>{v.destino}</span>
                </div>
                <div className="flex items-center gap-4 text-xs text-[#4B5A68] mt-1.5">
                  <span className="flex items-center gap-1">
                    <Calendar size={12} />
                    {new Date(v.dataInicio).toLocaleDateString('pt-PT')} – {new Date(v.dataFim).toLocaleDateString('pt-PT')}
                  </span>
                  <span className="flex items-center gap-1">
                    <Users size={12} />
                    {v.numViajantes}
                  </span>
                </div>
              </div>
              <span className="text-xs text-[#9AA3AB]">
                {new Date(v.criadoEm).toLocaleDateString('pt-PT')}
              </span>
            </button>
          ))}
        </div>
      )}
    </div>
  );
};
