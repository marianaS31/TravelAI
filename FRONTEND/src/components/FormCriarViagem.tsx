import React, { useState } from 'react';
import type { CriarViagemDTO } from '../types/viagem';
import { MapPin, Calendar, Coins, Compass, FileText, Sparkles, Loader2 } from 'lucide-react';

interface FormCriarViagemProps {
  aoSubmeter: (dados: CriarViagemDTO) => void;
  carregando: boolean;
}

const ESTILOS_VIAGEM = [
  'Cultural e Histórico',
  'Económico / Mochileiro',
  'Gastronómico e Vinhos',
  'Natureza e Aventura',
  'Relaxamento e Praias',
  'Luxo e Conforto',
];

export const FormCriarViagem: React.FC<FormCriarViagemProps> = ({ aoSubmeter, carregando }) => {
  const [destino, setDestino] = useState('');
  const [dataInicio, setDataInicio] = useState('');
  const [dataFim, setDataFim] = useState('');
  const [orcamento, setOrcamento] = useState<number | ''>('');
  const [estiloViagem, setEstiloViagem] = useState(ESTILOS_VIAGEM[0]);
  const [notasAdicionais, setNotasAdicionais] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!destino.trim() || !dataInicio || !dataFim) return;

    aoSubmeter({
      destino: destino.trim(),
      dataInicio,
      dataFim,
      orcamento: orcamento === '' ? undefined : Number(orcamento),
      estiloViagem,
      promptLinguagemNatural: notasAdicionais.trim() || undefined,
    });
  };

  return (
    <section className="bg-white rounded-3xl p-6 sm:p-8 shadow-sm border border-slate-200">
      <div className="mb-6">
        <h2 className="text-2xl font-bold text-slate-900">Planeia a tua próxima aventura</h2>
        <p className="text-sm text-slate-500 mt-1">
          Indica os teus planos e preferências para o assistente TravelAI orquestrar o teu roteiro.
        </p>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
          {/* Destino */}
          <div className="md:col-span-2">
            <label className="block text-xs font-bold uppercase tracking-wider text-slate-600 mb-2">
              Destino
            </label>
            <div className="relative">
              <MapPin className="absolute left-3.5 top-3 text-slate-400" size={20} />
              <input
                type="text"
                required
                value={destino}
                onChange={(e) => setDestino(e.target.value)}
                placeholder="Ex.: Roma, Paris, Tóquio..."
                className="w-full pl-11 pr-4 py-2.5 rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-600 text-slate-800 placeholder-slate-400"
              />
            </div>
          </div>

          {/* Data Início */}
          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-slate-600 mb-2">
              Data de Início
            </label>
            <div className="relative">
              <Calendar className="absolute left-3.5 top-3 text-slate-400" size={20} />
              <input
                type="date"
                required
                value={dataInicio}
                onChange={(e) => setDataInicio(e.target.value)}
                className="w-full pl-11 pr-4 py-2.5 rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-600 text-slate-800"
              />
            </div>
          </div>

          {/* Data Fim */}
          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-slate-600 mb-2">
              Data de Fim
            </label>
            <div className="relative">
              <Calendar className="absolute left-3.5 top-3 text-slate-400" size={20} />
              <input
                type="date"
                required
                min={dataInicio}
                value={dataFim}
                onChange={(e) => setDataFim(e.target.value)}
                className="w-full pl-11 pr-4 py-2.5 rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-600 text-slate-800"
              />
            </div>
          </div>

          {/* Orçamento */}
          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-slate-600 mb-2">
              Orçamento Estimado (€)
            </label>
            <div className="relative">
              <Coins className="absolute left-3.5 top-3 text-slate-400" size={20} />
              <input
                type="number"
                min="0"
                step="50"
                value={orcamento}
                onChange={(e) => setOrcamento(e.target.value ? Number(e.target.value) : '')}
                placeholder="Ex.: 800"
                className="w-full pl-11 pr-4 py-2.5 rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-600 text-slate-800 placeholder-slate-400"
              />
            </div>
          </div>

          {/* Estilo de Viagem */}
          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-slate-600 mb-2">
              Estilo de Viagem
            </label>
            <div className="relative">
              <Compass className="absolute left-3.5 top-3 text-slate-400" size={20} />
              <select
                value={estiloViagem}
                onChange={(e) => setEstiloViagem(e.target.value)}
                className="w-full pl-11 pr-4 py-2.5 rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-600 text-slate-800 bg-white"
              >
                {ESTILOS_VIAGEM.map((estilo) => (
                  <option key={estilo} value={estilo}>
                    {estilo}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Notas Adicionais em Linguagem Natural */}
          <div className="md:col-span-2">
            <label className="block text-xs font-bold uppercase tracking-wider text-slate-600 mb-2">
              Notas Adicionais / Preferências Pessoais
            </label>
            <div className="relative">
              <FileText className="absolute left-3.5 top-3 text-slate-400" size={20} />
              <textarea
                rows={3}
                value={notasAdicionais}
                onChange={(e) => setNotasAdicionais(e.target.value)}
                placeholder="Ex.: Gostamos de caminhar bastante, não comemos marisco e queremos visitar o museu local ao final do dia."
                className="w-full pl-11 pr-4 py-2.5 rounded-xl border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-600 text-slate-800 placeholder-slate-400 resize-none"
              />
            </div>
          </div>
        </div>

        {/* Botão de Submissão */}
        <div className="flex justify-end">
          <button
            type="submit"
            disabled={carregando}
            className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-xl shadow-sm transition disabled:opacity-50"
          >
            {carregando ? (
              <>
                <Loader2 size={18} className="animate-spin" />
                <span>A orquestrar plano com IA e MCP...</span>
              </>
            ) : (
              <>
                <Sparkles size={18} />
                <span>Gerar Roteiro Personalizado</span>
              </>
            )}
          </button>
        </div>
      </form>
    </section>
  );
};