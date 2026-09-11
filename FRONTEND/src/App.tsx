import { useState } from 'react';
import { api } from './api/api';
import type { ItinerarioDTO, CriarViagemDTO } from './types/viagem';
import { FormCriarViagem } from './components/FormCriarViagem';
import { ChatAssistente } from './components/ChatAssistente';
import { VoosAlojamento } from './components/VoosAlojamento';
import type { VooDTO, AlojamentoDTO } from './types/viagem';
import {
  Compass,
  Calendar,
  CloudSun,
  MapPin,
  MessageSquareText,
  Clock,
  Coins,
  ArrowLeft
} from 'lucide-react';

export default function App() {
  const [carregando, setCarregando] = useState(false);
  const [itinerario, setItinerario] = useState<ItinerarioDTO | null>(null);
  const [viagemAtual, setViagemAtual] = useState<CriarViagemDTO | null>(null);
  const [erro, setErro] = useState<string | null>(null);
  const [chatAberto, setChatAberto] = useState(false);
  const [voos, setVoos] = useState<VooDTO[]>([]);
  const [alojamentos, setAlojamentos] = useState<AlojamentoDTO[]>([]);

  const handleCriarViagem = async (dados: CriarViagemDTO) => {
    setCarregando(true);
    setErro(null);

    try {
      // 1. Cria a Viagem no Backend
      const respostaViagem = await api.post('/viagem', dados);
      const viagemId = respostaViagem.data.id;
      setViagemAtual(dados);

      // 2. Constrói as instruções consolidadas para a IA e o MCP
      const instrucoes = `Destino: ${dados.destino}. Estilo: ${dados.estiloViagem}. Orçamento: ${
        dados.orcamento ? dados.orcamento + '€' : 'Livre'
      }. Notas: ${dados.promptLinguagemNatural || 'Nenhuma'}.`;

      // 3. Solicita a geração do itinerário
      const respostaItinerario = await api.post('/itinerarios/gerar', {
        viagemId: viagemId,
        instrucoesAdicionais: instrucoes,
      });

      setItinerario(respostaItinerario.data);
    } catch (err: any) {
      setErro(err.response?.data?.message || 'Erro ao gerar o itinerário. Verifica o backend e o LM Studio.');
    } finally {
      setCarregando(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 text-slate-800">
      {/* Barra de Topo */}
      <header className="bg-white border-b border-slate-200 sticky top-0 z-40">
        <div className="max-w-5xl mx-auto px-6 h-16 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <div className="p-2 bg-blue-600 rounded-lg text-white">
              <Compass size={24} />
            </div>
            <span className="text-xl font-bold tracking-tight text-slate-900">TravelAI</span>
          </div>

          {itinerario && (
            <div className="flex items-center gap-3">
              <button
                onClick={() => setItinerario(null)}
                className="inline-flex items-center gap-2 px-4 py-2 border border-slate-200 text-slate-600 rounded-xl font-medium hover:bg-slate-100 transition text-sm"
              >
                <ArrowLeft size={16} />
                <span>Novo Roteiro</span>
              </button>
              <button
                onClick={() => setChatAberto(!chatAberto)}
                className="inline-flex items-center gap-2 px-4 py-2 bg-blue-50 text-blue-600 rounded-xl font-medium hover:bg-blue-100 transition text-sm"
              >
                <MessageSquareText size={18} />
                <span>Ajustar com IA</span>
              </button>
            </div>
          )}
        </div>
      </header>

      <main className="max-w-5xl mx-auto px-6 py-8">
        {erro && (
          <div className="p-4 bg-red-50 text-red-700 border border-red-200 rounded-2xl mb-6">
            {erro}
          </div>
        )}

        {/* Exibe o formulário inicial caso ainda não haja itinerário gerado */}
        {!itinerario ? (
          <FormCriarViagem aoSubmeter={handleCriarViagem} carregando={carregando} />
        ) : (
          /* Visualização do Itinerário Criado */
          <div className="space-y-6">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 border-b border-slate-200 pb-4">
              <div>
                <h2 className="text-2xl font-bold text-slate-900">
                  Roteiro: {viagemAtual?.destino}
                </h2>
                <p className="text-sm text-slate-500">
                  {viagemAtual?.dataInicio} até {viagemAtual?.dataFim} • {viagemAtual?.estiloViagem}
                </p>
              </div>
              <span className="text-xs bg-blue-100 text-blue-800 font-semibold px-3 py-1 rounded-full self-start">
                Versão {itinerario.versao}
              </span>
            </div>

            <div className="space-y-6">
              {itinerario.dias.map((dia) => (
                <div
                  key={dia.numeroDia}
                  className="bg-white rounded-2xl shadow-sm border border-slate-200 overflow-hidden"
                >
                  <div className="px-6 py-4 bg-slate-50 border-b border-slate-100 flex flex-wrap items-center justify-between gap-4">
                    <div className="flex items-center gap-2 text-slate-800 font-semibold">
                      <Calendar size={18} className="text-blue-600" />
                      <span>Dia {dia.numeroDia}</span>
                      <span className="text-slate-400 font-normal">({dia.data})</span>
                    </div>

                    {dia.previsaoTempo && (
                      <div className="flex items-center gap-2 text-sm bg-blue-50 text-blue-700 px-3 py-1 rounded-full">
                        <CloudSun size={18} />
                        <span>
                          {dia.previsaoTempo.condicao} • {dia.previsaoTempo.tempMin}°C a {dia.previsaoTempo.tempMax}°C
                        </span>
                      </div>
                    )}
                  </div>

                  <div className="p-6 space-y-4">
                    {dia.atividades.map((atv, idx) => (
                      <div
                        key={idx}
                        className="p-4 rounded-xl border border-slate-100 bg-slate-50/50 hover:bg-slate-50 transition"
                      >
                        <div className="flex justify-between items-start gap-4">
                          <div>
                            <span className="inline-block px-2.5 py-0.5 rounded-full text-xs font-semibold uppercase tracking-wider bg-blue-100 text-blue-800 mb-1">
                              {atv.periodo}
                            </span>
                            <h3 className="font-semibold text-slate-900 text-base">{atv.titulo}</h3>
                          </div>
                          {atv.custoEstimado !== undefined && (
                            <span className="flex items-center gap-1 text-sm font-medium text-emerald-600 bg-emerald-50 px-2.5 py-1 rounded-md">
                              <Coins size={14} />
                              {atv.custoEstimado}€
                            </span>
                          )}
                        </div>

                        {atv.descricao && (
                          <p className="mt-2 text-sm text-slate-600 leading-relaxed">
                            {atv.descricao}
                          </p>
                        )}

                        <div className="mt-3 flex flex-wrap gap-4 text-xs text-slate-500">
                          {atv.local && (
                            <div className="flex items-center gap-1">
                              <MapPin size={14} className="text-slate-400" />
                              <span>{atv.local}</span>
                            </div>
                          )}
                          {atv.horaInicio && (
                            <div className="flex items-center gap-1">
                              <Clock size={14} className="text-slate-400" />
                              <span>
                                {atv.horaInicio} {atv.horaFim ? `- ${atv.horaFim}` : ''}
                              </span>
                            </div>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </main>

      {/* Assistente Lateral de Chat */}
      {itinerario && (
        <ChatAssistente
          viagemId={itinerario.viagemId}
          aberto={chatAberto}
          onFechar={() => setChatAberto(false)}
          onItinerarioAtualizado={(novo) => setItinerario(novo)}
        />
      )}
    </div>
  );
}