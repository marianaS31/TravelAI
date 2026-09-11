import { useState } from 'react';
import { api } from './api/api';
import type { ItinerarioDTO } from './types/viagem';
import { FormCriarViagem } from './components/FormCriarViagem';
import type { DadosViagem, DadosGeracao } from './components/FormCriarViagem';
import { ChatAssistente } from './components/ChatAssistente';
import { MapaLugares } from './components/MapaLugares';
import {
  Compass,
  CloudSun,
  MapPin,
  MessageSquareText,
  Clock,
  ArrowLeft,
  Building2,
  UtensilsCrossed,
  Star,
  ExternalLink,
  Plane,
} from 'lucide-react';

export default function App() {
  const [carregando, setCarregando] = useState(false);
  const [itinerario, setItinerario] = useState<ItinerarioDTO | null>(null);
  const [viagemAtual, setViagemAtual] = useState<DadosViagem | null>(null);
  const [origemPartida, setOrigemPartida] = useState<string | undefined>(undefined);
  const [erro, setErro] = useState<string | null>(null);
  const [chatAberto, setChatAberto] = useState(false);

  const handleCriarViagem = async (viagem: DadosViagem, geracao: DadosGeracao) => {
    setCarregando(true);
    setErro(null);

    try {
      const respostaViagem = await api.post('/viagens', viagem);
      const viagemId: string = respostaViagem.data.id;
      setViagemAtual(viagem);
      setOrigemPartida(geracao.origemPartida);

      const respostaItinerario = await api.post('/itinerarios/gerar', {
        viagemId,
        instrucoesAdicionais: geracao.instrucoesAdicionais,
        origemPartida: geracao.origemPartida,
      });

      setItinerario(respostaItinerario.data);
    } catch (err: any) {
      const mensagemErro =
        err.response?.data?.erro ||
        err.response?.data?.title ||
        'Erro ao gerar o itinerário. Verifica se o backend e o LM Studio estão ativos.';
      setErro(mensagemErro);
    } finally {
      setCarregando(false);
    }
  };

  const ordenarPorAvaliacao = <T extends { avaliacao?: number }>(lista: T[]): T[] =>
    [...lista].sort((a, b) => (b.avaliacao ?? -1) - (a.avaliacao ?? -1));

  const alojamentos = ordenarPorAvaliacao(itinerario?.alojamentosReais ?? []);
  const restaurantes = ordenarPorAvaliacao(itinerario?.restaurantesReais ?? []);
  const voos = itinerario?.voosReais ?? [];

  return (
    <div className="min-h-screen bg-[#F3F4F0] text-[#17324B]" style={{ fontFamily: "'Work Sans', sans-serif" }}>
      <header className="bg-[#17324B] sticky top-0 z-40">
        <div className="max-w-6xl mx-auto px-6 h-16 flex items-center justify-between">
          <div className="flex items-center gap-2.5">
            <Compass size={22} className="text-[#C8973C]" strokeWidth={1.75} />
            <span
              className="text-xl text-white tracking-tight"
              style={{ fontFamily: "'Fraunces', serif", fontStyle: 'italic', fontWeight: 500 }}
            >
              TravelAI
            </span>
          </div>

          {itinerario && (
            <div className="flex items-center gap-3">
              <button
                onClick={() => setItinerario(null)}
                className="inline-flex items-center gap-2 px-4 py-2 text-white/70 hover:text-white transition text-sm"
              >
                <ArrowLeft size={15} />
                <span>Novo roteiro</span>
              </button>
              <button
                onClick={() => setChatAberto(!chatAberto)}
                className="inline-flex items-center gap-2 px-4 py-2 bg-[#C8973C] text-[#17324B] rounded-full font-semibold hover:bg-[#d9aa50] transition text-sm"
              >
                <MessageSquareText size={16} />
                <span>Alterar viagem</span>
              </button>
            </div>
          )}
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-6 py-10">
        {erro && (
          <div className="p-4 bg-[#fbe9e3] text-[#8a3a20] border-l-4 border-[#c1440e] rounded-r-lg mb-6 text-sm">
            {erro}
          </div>
        )}

        {!itinerario ? (
          <FormCriarViagem aoSubmeter={handleCriarViagem} carregando={carregando} />
        ) : (
          <div className="space-y-12">
            <div className="bg-white rounded-2xl border border-[#E4E1D8] overflow-hidden flex flex-col sm:flex-row">
              <div className="flex-1 p-6">
                <p className="text-xs text-[#4B5A68] mb-1">Destino</p>
                <h2
                  className="text-3xl text-[#17324B] leading-tight"
                  style={{ fontFamily: "'Fraunces', serif", fontWeight: 600 }}
                >
                  {viagemAtual?.destino}
                </h2>
                <p className="text-sm text-[#4B5A68] mt-2">
                  {viagemAtual?.dataInicio} — {viagemAtual?.dataFim} &nbsp;·&nbsp; {viagemAtual?.numViajantes}{' '}
                  viajante{viagemAtual && viagemAtual.numViajantes > 1 ? 's' : ''}
                </p>
              </div>

              <div className="hidden sm:flex flex-col items-center justify-center px-1">
                <div className="w-px h-full border-l-2 border-dashed border-[#E4E1D8]" />
              </div>

              <div className="p-6 flex sm:flex-col items-center justify-center gap-2 bg-[#F3F4F0]/60 sm:w-40">
                <span className="text-3xl text-[#C8973C]" style={{ fontFamily: "'Fraunces', serif", fontWeight: 600 }}>
                  {String(itinerario.versao).padStart(2, '0')}
                </span>
                <span className="text-xs text-[#4B5A68]">versão do roteiro</span>
              </div>
            </div>

            <section>
              <h3
                className="text-lg text-[#17324B] mb-4"
                style={{ fontFamily: "'Fraunces', serif", fontWeight: 600 }}
              >
                Roteiro dia a dia
              </h3>
              <div className="flex gap-4 overflow-x-auto pb-4 -mx-6 px-6 snap-x">
                {itinerario.dias.map((dia) => (
                  <div
                    key={dia.id}
                    className="flex-none w-80 snap-start bg-white rounded-2xl border border-[#E4E1D8] overflow-hidden flex flex-col"
                  >
                    <div className="px-5 py-3 border-b border-dashed border-[#E4E1D8]">
                      <div className="flex items-baseline gap-2">
                        <span
                          className="text-2xl text-[#17324B]"
                          style={{ fontFamily: "'Fraunces', serif", fontWeight: 600 }}
                        >
                          {String(dia.numeroDia).padStart(2, '0')}
                        </span>
                        <span className="text-xs text-[#4B5A68]">
                          {new Date(dia.data).toLocaleDateString('pt-PT', {
                            weekday: 'long',
                            day: 'numeric',
                            month: 'short',
                          })}
                        </span>
                      </div>
                      {dia.previsaoTempo && (
                        <div className="flex items-center gap-1.5 text-xs text-[#0E6B63] mt-1.5">
                          <CloudSun size={13} />
                          <span>
                            {dia.previsaoTempo.condicao} · {dia.previsaoTempo.tempMin}°–{dia.previsaoTempo.tempMax}°C
                          </span>
                        </div>
                      )}
                    </div>

                    <div className="p-5 flex-1 relative">
                      <div className="absolute left-[26px] top-6 bottom-6 w-px bg-[#E4E1D8]" />
                      <div className="space-y-5">
                        {dia.atividades.map((atv) => (
                          <div key={atv.id} className="flex gap-3 relative">
                            <div className="w-2.5 h-2.5 rounded-full bg-[#C8973C] mt-1.5 shrink-0 z-10" />
                            <div className="min-w-0">
                              <h4 className="font-semibold text-[#17324B] text-sm leading-snug">{atv.nome}</h4>
                              {atv.horaInicio && (
                                <div className="flex items-center gap-1 text-xs text-[#4B5A68] mt-1">
                                  <Clock size={11} />
                                  <span>
                                    {atv.horaInicio} {atv.horaFim ? `– ${atv.horaFim}` : ''}
                                  </span>
                                </div>
                              )}
                              {atv.local && (
                                <div className="flex items-center gap-1 text-xs text-[#4B5A68] mt-0.5">
                                  <MapPin size={11} />
                                  <span className="truncate">{atv.local}</span>
                                </div>
                              )}
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </section>

            {voos.length > 0 && (
              <section className="bg-white rounded-2xl border border-[#E4E1D8] overflow-hidden">
                <div className="px-5 py-4 flex items-center gap-2 text-[#17324B]">
                  <Plane size={17} className="text-[#C8973C]" />
                  <h3 style={{ fontFamily: "'Fraunces', serif", fontWeight: 600 }} className="text-base">
                    Voos
                  </h3>
                </div>
                <div className="divide-y divide-[#F0EEE7]">
                  {voos.map((voo, idx) => (
                    <div
                      key={voo.id ?? idx}
                      className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 px-5 py-4 border-l-4 border-[#C8973C]"
                    >
                      <div className="space-y-1">
                        {voo.companhia && (
                          <div className="text-xs font-semibold text-[#C8973C]">{voo.companhia}</div>
                        )}
                        {voo.segmentos.map((s, si) => (
                          <div key={si} className="flex items-center gap-2 text-sm text-[#17324B]">
                            <span className="font-semibold">{s.origem}</span>
                            <span className="text-[#4B5A68]">→</span>
                            <span className="font-semibold">{s.destino}</span>
                            {s.duracao && (
                              <span className="text-xs text-[#4B5A68]">
                                ({s.duracao.replace('PT', '').toLowerCase()})
                              </span>
                            )}
                            <span className="text-xs text-[#4B5A68]">
                              {s.paragens === 0 ? 'direto' : `${s.paragens} escala(s)`}
                            </span>
                          </div>
                        ))}
                      </div>
                      {voo.preco && (
                        <span
                          className="text-lg text-[#17324B] shrink-0"
                          style={{ fontFamily: "'Fraunces', serif", fontWeight: 600 }}
                        >
                          {voo.preco}
                        </span>
                      )}
                    </div>
                  ))}
                </div>
                {voos[0]?.url && (
                  <div className="px-5 py-3 border-t border-[#F0EEE7]">
                    <a
                      href={voos[0].url}
                      target="_blank"
                      rel="noreferrer"
                      className="inline-flex items-center gap-1 text-xs font-semibold text-[#C8973C] hover:text-[#a97b2c]"
                    >
                      <span>Ver mais opções no Google Flights</span>
                      <ExternalLink size={11} />
                    </a>
                  </div>
                )}
              </section>
            )}

            <MapaLugares alojamentos={alojamentos} restaurantes={restaurantes} />

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <section className="bg-white rounded-2xl border border-[#E4E1D8] overflow-hidden">
                <div className="px-5 py-4 flex items-center gap-2 text-[#17324B]">
                  <Building2 size={17} className="text-[#6B4C7A]" />
                  <h3 style={{ fontFamily: "'Fraunces', serif", fontWeight: 600 }} className="text-base">
                    Alojamento
                  </h3>
                </div>
                <div className="divide-y divide-[#F0EEE7]">
                  {alojamentos.length === 0 ? (
                    <p className="text-sm text-[#4B5A68] text-center py-8 px-5">
                      Nenhum alojamento encontrado para este destino.
                    </p>
                  ) : (
                    alojamentos.map((a, idx) => (
                      <div key={idx} className="px-5 py-4 border-l-4 border-[#6B4C7A]">
                        <div className="flex items-start justify-between gap-2">
                          <h4 className="font-semibold text-[#17324B] text-sm">{a.nome}</h4>
                          {a.avaliacao !== undefined && (
                            <div className="flex items-center gap-1 text-xs font-semibold text-[#17324B] shrink-0">
                              <Star size={11} className="fill-[#C8973C] text-[#C8973C]" />
                              <span>{a.avaliacao}</span>
                            </div>
                          )}
                        </div>
                        {a.morada && (
                          <div className="flex items-center gap-1 text-xs text-[#4B5A68] mt-1.5">
                            <MapPin size={11} />
                            <span>{a.morada}</span>
                          </div>
                        )}
                        {(a.numAvaliacoes || a.nivelPreco) && (
                          <p className="text-xs text-[#8a95a0] mt-1">
                            {a.numAvaliacoes ? `${a.numAvaliacoes} avaliações` : ''}
                            {a.numAvaliacoes && a.nivelPreco ? ' · ' : ''}
                            {a.nivelPreco ?? ''}
                          </p>
                        )}
                        {a.url && (
                          <a
                            href={a.url}
                            target="_blank"
                            rel="noreferrer"
                            className="inline-flex items-center gap-1 mt-2 text-xs font-semibold text-[#6B4C7A] hover:text-[#523a5e]"
                          >
                            <span>Ver no Booking.com</span>
                            <ExternalLink size={11} />
                          </a>
                        )}
                      </div>
                    ))
                  )}
                </div>
              </section>

              <section className="bg-white rounded-2xl border border-[#E4E1D8] overflow-hidden">
                <div className="px-5 py-4 flex items-center gap-2 text-[#17324B]">
                  <UtensilsCrossed size={17} className="text-[#0E6B63]" />
                  <h3 style={{ fontFamily: "'Fraunces', serif", fontWeight: 600 }} className="text-base">
                    Restaurantes
                  </h3>
                </div>
                <div className="divide-y divide-[#F0EEE7]">
                  {restaurantes.length === 0 ? (
                    <p className="text-sm text-[#4B5A68] text-center py-8 px-5">
                      Nenhum restaurante encontrado para este destino.
                    </p>
                  ) : (
                    restaurantes.map((r, idx) => (
                      <div key={idx} className="px-5 py-4 border-l-4 border-[#0E6B63]">
                        <div className="flex items-start justify-between gap-2">
                          <h4 className="font-semibold text-[#17324B] text-sm">{r.nome}</h4>
                          {r.avaliacao !== undefined && (
                            <div className="flex items-center gap-1 text-xs font-semibold text-[#17324B] shrink-0">
                              <Star size={11} className="fill-[#C8973C] text-[#C8973C]" />
                              <span>{r.avaliacao}</span>
                            </div>
                          )}
                        </div>
                        {r.morada && (
                          <div className="flex items-center gap-1 text-xs text-[#4B5A68] mt-1.5">
                            <MapPin size={11} />
                            <span>{r.morada}</span>
                          </div>
                        )}
                        {(r.numAvaliacoes || r.nivelPreco) && (
                          <p className="text-xs text-[#8a95a0] mt-1">
                            {r.numAvaliacoes ? `${r.numAvaliacoes} avaliações` : ''}
                            {r.numAvaliacoes && r.nivelPreco ? ' · ' : ''}
                            {r.nivelPreco ?? ''}
                          </p>
                        )}
                        {r.url && (
                          <a
                            href={r.url}
                            target="_blank"
                            rel="noreferrer"
                            className="inline-flex items-center gap-1 mt-2 text-xs font-semibold text-[#0E6B63] hover:text-[#0a4d47]"
                          >
                            <span>Ver no Google Maps</span>
                            <ExternalLink size={11} />
                          </a>
                        )}
                      </div>
                    ))
                  )}
                </div>
              </section>
            </div>
          </div>
        )}
      </main>

      {itinerario && (
        <ChatAssistente
          viagemId={itinerario.viagemId}
          origemPartida={origemPartida}
          aberto={chatAberto}
          onFechar={() => setChatAberto(false)}
          onItinerarioAtualizado={(novo) => setItinerario(novo)}
        />
      )}
    </div>
  );
}
