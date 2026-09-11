import React, { useState, useRef, useEffect } from 'react';
import { api } from '../api/api';
import type { ItinerarioDTO } from '../types/viagem';
import { Send, Bot, User, Loader2, Sparkles, X } from 'lucide-react';

interface Mensagem {
  remetente: 'user' | 'assistant';
  texto: string;
}

interface ChatAssistenteProps {
  viagemId: string;
  origemPartida?: string;
  onItinerarioAtualizado: (novoItinerario: ItinerarioDTO) => void;
  aberto: boolean;
  onFechar: () => void;
}

export const ChatAssistente: React.FC<ChatAssistenteProps> = ({
  viagemId,
  origemPartida,
  onItinerarioAtualizado,
  aberto,
  onFechar,
}) => {
  const [mensagens, setMensagens] = useState<Mensagem[]>([
    {
      remetente: 'assistant',
      texto: 'Olá! Sou o assistente TravelAI. Podes pedir-me alterações ao teu roteiro, como adicionar atividades ou ajustar horários.',
    },
  ]);
  const [input, setInput] = useState('');
  const [enviando, setEnviando] = useState(false);
  const fimChatRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    fimChatRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [mensagens]);

  if (!aberto) return null;

  const handleEnviar = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!input.trim() || enviando) return;

    const textoUtilizador = input.trim();
    setInput('');
    setMensagens((prev) => [...prev, { remetente: 'user', texto: textoUtilizador }]);
    setEnviando(true);

    try {
      // 🔧 corrigido: era '/itinerario/gerar' (404) — rota real é plural.
      // Inclui origemPartida guardada da criação original, senão a próxima
      // geração perde a pesquisa de voos.
      const resposta = await api.post('/itinerarios/gerar', {
        viagemId: viagemId,
        instrucoesAdicionais: textoUtilizador,
        origemPartida,
      });

      setMensagens((prev) => [
        ...prev,
        {
          remetente: 'assistant',
          texto: 'Itinerário atualizado com sucesso com base no teu pedido!',
        },
      ]);

      if (resposta.data) {
        onItinerarioAtualizado(resposta.data);
      }
    } catch {
      setMensagens((prev) => [
        ...prev,
        {
          remetente: 'assistant',
          texto: 'Ocorreu um erro ao tentar ajustar o plano. Verifica se o backend e o LM Studio estão ativos.',
        },
      ]);
    } finally {
      setEnviando(false);
    }
  };

  return (
    <aside className="fixed right-0 top-0 bottom-0 w-96 bg-white shadow-2xl border-l border-slate-200 flex flex-col z-50">
      {/* Cabeçalho */}
      <div className="p-4 border-b border-slate-100 flex items-center justify-between bg-slate-50">
        <div className="flex items-center gap-2">
          <div className="p-1.5 bg-[#17324B] rounded-lg text-white">
            <Sparkles size={18} />
          </div>
          <h2 className="font-semibold text-slate-800">Assistente de Roteiro</h2>
        </div>
        <button
          onClick={onFechar}
          className="text-slate-400 hover:text-slate-600 p-1 rounded-md transition"
        >
          <X size={20} />
        </button>
      </div>

      {/* Lista de Mensagens */}
      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {mensagens.map((msg, idx) => (
          <div
            key={idx}
            className={`flex gap-3 ${
              msg.remetente === 'user' ? 'justify-end' : 'justify-start'
            }`}
          >
            {msg.remetente === 'assistant' && (
              <div className="w-8 h-8 rounded-full bg-[#F3F4F0] text-[#0E6B63] flex items-center justify-center shrink-0">
                <Bot size={18} />
              </div>
            )}
            <div
              className={`p-3 rounded-2xl max-w-[75%] text-sm ${
                msg.remetente === 'user'
                  ? 'bg-[#17324B] text-white rounded-tr-none'
                  : 'bg-slate-100 text-slate-800 rounded-tl-none'
              }`}
            >
              {msg.texto}
            </div>
            {msg.remetente === 'user' && (
              <div className="w-8 h-8 rounded-full bg-slate-200 text-slate-600 flex items-center justify-center shrink-0">
                <User size={18} />
              </div>
            )}
          </div>
        ))}
        {enviando && (
          <div className="flex gap-3 items-center text-slate-400 text-sm">
            <Loader2 size={16} className="animate-spin" />
            <span>A ajustar itinerário com o modelo... (pode demorar vários minutos)</span>
          </div>
        )}
        <div ref={fimChatRef} />
      </div>

      {/* Campo de Envio */}
      <form onSubmit={handleEnviar} className="p-4 border-t border-slate-100 bg-slate-50">
        <div className="flex gap-2">
          <input
            type="text"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Ex: Troca o museu por um parque..."
            className="flex-1 px-3 py-2 text-sm border border-slate-300 rounded-xl focus:outline-none focus:ring-2 focus:ring-[#17324B] bg-white"
          />
          <button
            type="submit"
            disabled={enviando || !input.trim()}
            className="p-2.5 bg-[#17324B] text-white rounded-xl hover:bg-[#20476b] disabled:opacity-50 transition"
          >
            <Send size={16} />
          </button>
        </div>
      </form>
    </aside>
  );
};
