import React, { useState } from 'react';
import type { VooDTO, AlojamentoDTO } from '../types/viagem';
import { Plane, Building2, Star, ExternalLink, Clock, ArrowRight, ShieldCheck } from 'lucide-react';

interface VoosAlojamentoProps {
  voos: VooDTO[];
  alojamentos: AlojamentoDTO[];
}

export const VoosAlojamento: React.FC<VoosAlojamentoProps> = ({ voos, alojamentos }) => {
  const [separadorAtivo, setSeparadorAtivo] = useState<'voos' | 'hoteis'>('voos');

  return (
    <section className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden mb-8">
      {/* Barra de Seleção de Separador */}
      <div className="flex border-b border-slate-200 bg-slate-50/75">
        <button
          type="button"
          onClick={() => setSeparadorAtivo('voos')}
          className={`flex-1 py-3.5 px-4 font-semibold text-sm flex items-center justify-center gap-2 border-b-2 transition ${
            separadorAtivo === 'voos'
              ? 'border-blue-600 text-blue-600 bg-white'
              : 'border-transparent text-slate-500 hover:text-slate-700'
          }`}
        >
          <Plane size={18} />
          <span>Opções de Voos ({voos.length})</span>
        </button>

        <button
          type="button"
          onClick={() => setSeparadorAtivo('hoteis')}
          className={`flex-1 py-3.5 px-4 font-semibold text-sm flex items-center justify-center gap-2 border-b-2 transition ${
            separadorAtivo === 'hoteis'
              ? 'border-blue-600 text-blue-600 bg-white'
              : 'border-transparent text-slate-500 hover:text-slate-700'
          }`}
        >
          <Building2 size={18} />
          <span>Alojamentos Sugeridos ({alojamentos.length})</span>
        </button>
      </div>

      <div className="p-6">
        {/* Painel: Voos (Amadeus API) */}
        {separadorAtivo === 'voos' && (
          <div className="space-y-4">
            {voos.length === 0 ? (
              <p className="text-sm text-slate-400 text-center py-6">
                Nenhum voo encontrado ou atribuído para este destino.
              </p>
            ) : (
              voos.map((voo, idx) => (
                <div
                  key={idx}
                  className="flex flex-col md:flex-row md:items-center justify-between p-4 rounded-xl border border-slate-100 bg-slate-50 hover:border-slate-200 transition gap-4"
                >
                  <div className="space-y-1">
                    <div className="flex items-center gap-2 text-xs font-semibold text-blue-600 uppercase tracking-wider">
                      <ShieldCheck size={14} />
                      <span>{voo.companhiaAerea}</span>
                    </div>

                    <div className="flex items-center gap-3 text-lg font-bold text-slate-800">
                      <span>{voo.origemIATA}</span>
                      <ArrowRight size={18} className="text-slate-400" />
                      <span>{voo.destinoIATA}</span>
                    </div>

                    <div className="flex items-center gap-3 text-xs text-slate-500">
                      <span className="flex items-center gap-1">
                        <Clock size={14} />
                        {new Date(voo.dataPartida).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} - {new Date(voo.dataChegada).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                      </span>
                      <span>•</span>
                      <span>{voo.escalas === 0 ? 'Voo Direto' : `${voo.escalas} escala(s)`}</span>
                    </div>
                  </div>

                  <div className="flex md:flex-col items-center md:items-end justify-between gap-2 border-t md:border-t-0 pt-3 md:pt-0 border-slate-200">
                    <span className="text-xl font-extrabold text-slate-900">{voo.preco}€</span>
                    {voo.linkReserva && (
                      <a
                        href={voo.linkReserva}
                        target="_blank"
                        rel="noreferrer"
                        className="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-semibold bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
                      >
                        <span>Reservar</span>
                        <ExternalLink size={12} />
                      </a>
                    )}
                  </div>
                </div>
              ))
            )}
          </div>
        )}

        {/* Painel: Hotéis (Google Places API) */}
        {separadorAtivo === 'hoteis' && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {alojamentos.length === 0 ? (
              <p className="text-sm text-slate-400 text-center py-6 col-span-2">
                Nenhum alojamento recomendado disponível.
              </p>
            ) : (
              alojamentos.map((hotel, idx) => (
                <div
                  key={idx}
                  className="p-4 rounded-xl border border-slate-100 bg-slate-50 flex flex-col justify-between hover:border-slate-200 transition"
                >
                  <div>
                    <div className="flex items-start justify-between gap-2">
                      <h4 className="font-bold text-slate-900 text-base">{hotel.nome}</h4>
                      {hotel.classificacao && (
                        <div className="flex items-center gap-1 text-xs font-bold text-amber-700 bg-amber-50 border border-amber-200 px-2 py-0.5 rounded-md shrink-0">
                          <Star size={12} className="fill-amber-400 text-amber-500" />
                          <span>{hotel.classificacao}</span>
                        </div>
                      )}
                    </div>
                    {hotel.endereco && (
                      <p className="text-xs text-slate-500 mt-1 leading-relaxed">{hotel.endereco}</p>
                    )}
                  </div>

                  <div className="flex items-center justify-between pt-4 mt-3 border-t border-slate-200/60">
                    <div>
                      {hotel.precoEstimado ? (
                        <span className="text-sm font-semibold text-slate-800">
                          ~{hotel.precoEstimado}€ <span className="text-xs text-slate-400 font-normal">/ noite</span>
                        </span>
                      ) : (
                        <span className="text-xs text-slate-400">Preço sob consulta</span>
                      )}
                    </div>

                    {hotel.urlReserva && (
                      <a
                        href={hotel.urlReserva}
                        target="_blank"
                        rel="noreferrer"
                        className="inline-flex items-center gap-1 text-xs font-semibold text-blue-600 hover:text-blue-700 underline underline-offset-2"
                      >
                        <span>Ver no Booking</span>
                        <ExternalLink size={12} />
                      </a>
                    )}
                  </div>
                </div>
              ))
            )}
          </div>
        )}
      </div>
    </section>
  );
};