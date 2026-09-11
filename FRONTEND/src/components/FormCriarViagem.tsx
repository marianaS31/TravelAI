import React, { useState } from 'react';
import { MapPin, Calendar, Coins, Compass, FileText, Sparkles, Loader2, Users, Plane } from 'lucide-react';

export interface DadosViagem {
  titulo: string;
  destino: string;
  dataInicio: string;
  dataFim: string;
  numViajantes: number;
  orcamento?: number;
}

export interface DadosGeracao {
  origemPartida?: string;
  instrucoesAdicionais?: string;
}

interface FormCriarViagemProps {
  aoSubmeter: (viagem: DadosViagem, geracao: DadosGeracao) => void;
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

const CATEGORIAS_ORCAMENTO = [
  { label: 'Económico', valor: 400 },
  { label: 'Moderado', valor: 900 },
  { label: 'Luxo', valor: 2000 },
];

const campoBase =
  'w-full bg-transparent border-0 border-b border-[#D9D5C9] focus:border-[#17324B] focus:outline-none focus:ring-0 text-[#17324B] placeholder-[#9AA3AB] py-2 pl-7 text-sm transition-colors';

export const FormCriarViagem: React.FC<FormCriarViagemProps> = ({ aoSubmeter, carregando }) => {
  const [destino, setDestino] = useState('');
  const [origemPartida, setOrigemPartida] = useState('');
  const [dataInicio, setDataInicio] = useState('');
  const [dataFim, setDataFim] = useState('');
  const [numViajantes, setNumViajantes] = useState<number | ''>(1);
  const [orcamento, setOrcamento] = useState<number | ''>('');
  const [estiloViagem, setEstiloViagem] = useState(ESTILOS_VIAGEM[0]);
  const [notasAdicionais, setNotasAdicionais] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!destino.trim() || !dataInicio || !dataFim || !numViajantes) return;

    const viagem: DadosViagem = {
      titulo: `Viagem a ${destino.trim()}`,
      destino: destino.trim(),
      dataInicio,
      dataFim,
      numViajantes: Number(numViajantes),
      orcamento: orcamento === '' ? undefined : Number(orcamento),
    };

    const instrucoesPartes = [
      `Estilo de viagem: ${estiloViagem}.`,
      notasAdicionais.trim() ? `Preferências: ${notasAdicionais.trim()}.` : '',
    ].filter(Boolean);

    const geracao: DadosGeracao = {
      origemPartida: origemPartida.trim().toUpperCase() || undefined,
      instrucoesAdicionais: instrucoesPartes.join(' '),
    };

    aoSubmeter(viagem, geracao);
  };

  return (
    <section className="max-w-2xl mx-auto">
      <div className="mb-10 text-center">
        <p className="text-xs tracking-wide text-[#0E6B63] mb-2">para onde vamos?</p>
        <h2
          className="text-4xl text-[#17324B]"
          style={{ fontFamily: "'Fraunces', serif", fontStyle: 'italic', fontWeight: 500 }}
        >
          Planeia a tua próxima aventura
        </h2>
      </div>

      <form onSubmit={handleSubmit} className="bg-white rounded-2xl border border-[#E4E1D8] p-8 sm:p-10 space-y-8">
        {/* Destino — hero do formulário */}
        <div className="relative">
          <MapPin className="absolute left-0 top-2.5 text-[#9AA3AB]" size={16} />
          <input
            type="text"
            required
            value={destino}
            onChange={(e) => setDestino(e.target.value)}
            placeholder="Roma, Paris, Tóquio..."
            className={`${campoBase} text-2xl py-3`}
            style={{ fontFamily: "'Fraunces', serif", fontWeight: 500 }}
          />
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-x-8 gap-y-6">
          <div>
            <label className="block text-xs text-[#4B5A68] mb-1">Origem (IATA)</label>
            <div className="relative">
              <Plane className="absolute left-0 top-2.5 text-[#9AA3AB]" size={15} />
              <input
                type="text"
                maxLength={3}
                value={origemPartida}
                onChange={(e) => setOrigemPartida(e.target.value.toUpperCase())}
                placeholder="OPO"
                className={`${campoBase} uppercase`}
              />
            </div>
            <p className="text-xs text-[#9AA3AB] mt-1">opcional — sem isto, não há voos</p>
          </div>

          <div>
            <label className="block text-xs text-[#4B5A68] mb-1">Nº de viajantes</label>
            <div className="relative">
              <Users className="absolute left-0 top-2.5 text-[#9AA3AB]" size={15} />
              <input
                type="number"
                required
                min={1}
                value={numViajantes}
                onChange={(e) => setNumViajantes(e.target.value ? Number(e.target.value) : '')}
                className={campoBase}
              />
            </div>
          </div>

          <div>
            <label className="block text-xs text-[#4B5A68] mb-1">Data de início</label>
            <div className="relative">
              <Calendar className="absolute left-0 top-2.5 text-[#9AA3AB]" size={15} />
              <input
                type="date"
                required
                value={dataInicio}
                onChange={(e) => setDataInicio(e.target.value)}
                className={campoBase}
              />
            </div>
          </div>

          <div>
            <label className="block text-xs text-[#4B5A68] mb-1">Data de fim</label>
            <div className="relative">
              <Calendar className="absolute left-0 top-2.5 text-[#9AA3AB]" size={15} />
              <input
                type="date"
                required
                min={dataInicio}
                value={dataFim}
                onChange={(e) => setDataFim(e.target.value)}
                className={campoBase}
              />
            </div>
          </div>
        </div>

        {/* Orçamento */}
        <div>
          <label className="block text-xs text-[#4B5A68] mb-2">Orçamento estimado</label>
          <div className="flex gap-2 mb-3">
            {CATEGORIAS_ORCAMENTO.map((cat) => (
              <button
                key={cat.label}
                type="button"
                onClick={() => setOrcamento(cat.valor)}
                className={`px-3 py-1 rounded-full text-xs font-medium border transition ${
                  orcamento === cat.valor
                    ? 'bg-[#17324B] text-white border-[#17324B]'
                    : 'bg-transparent text-[#4B5A68] border-[#D9D5C9] hover:border-[#17324B]'
                }`}
              >
                {cat.label}
              </button>
            ))}
          </div>
          <div className="relative">
            <Coins className="absolute left-0 top-2.5 text-[#9AA3AB]" size={15} />
            <input
              type="number"
              min="0"
              step="50"
              value={orcamento}
              onChange={(e) => setOrcamento(e.target.value ? Number(e.target.value) : '')}
              placeholder="800"
              className={campoBase}
            />
            <span className="absolute right-0 top-2 text-sm text-[#9AA3AB]">€</span>
          </div>
        </div>

        {/* Estilo */}
        <div>
          <label className="block text-xs text-[#4B5A68] mb-1">Estilo de viagem</label>
          <div className="relative">
            <Compass className="absolute left-0 top-2.5 text-[#9AA3AB]" size={15} />
            <select
              value={estiloViagem}
              onChange={(e) => setEstiloViagem(e.target.value)}
              className={`${campoBase} bg-white appearance-none cursor-pointer`}
            >
              {ESTILOS_VIAGEM.map((estilo) => (
                <option key={estilo} value={estilo}>
                  {estilo}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Notas */}
        <div>
          <label className="block text-xs text-[#4B5A68] mb-1">Notas e preferências pessoais</label>
          <div className="relative">
            <FileText className="absolute left-0 top-2.5 text-[#9AA3AB]" size={15} />
            <textarea
              rows={2}
              value={notasAdicionais}
              onChange={(e) => setNotasAdicionais(e.target.value)}
              placeholder="Gostamos de caminhar bastante, não comemos marisco..."
              className={`${campoBase} resize-none`}
            />
          </div>
        </div>

        <div className="flex justify-end pt-2">
          <button
            type="submit"
            disabled={carregando}
            className="inline-flex items-center gap-2 px-6 py-3 bg-[#C8973C] hover:bg-[#d9aa50] text-[#17324B] font-semibold rounded-full transition disabled:opacity-50"
          >
            {carregando ? (
              <>
                <Loader2 size={17} className="animate-spin" />
                <span>A orquestrar plano com IA... (pode demorar vários minutos)</span>
              </>
            ) : (
              <>
                <Sparkles size={17} />
                <span>Gerar roteiro</span>
              </>
            )}
          </button>
        </div>
      </form>
    </section>
  );
};
