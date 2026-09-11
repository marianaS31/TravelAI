// Lida com o Tipo da atividade venha como string ("VOO") ou número (0),
// consoante o backend ter ou não o JsonStringEnumConverter configurado.

const ORDEM_ENUM = [
  'VOO',
  'ALOJAMENTO',
  'PONTO_INTERESSE',
  'ALUGUER_CARRO',
  'REFEICAO',
  'DESLOCACAO',
  'OUTRO',
] as const;

export type TipoAtividade = (typeof ORDEM_ENUM)[number];

const ROTULOS: Record<TipoAtividade, { label: string; classes: string }> = {
  VOO: { label: 'Voo', classes: 'bg-sky-100 text-sky-800' },
  ALOJAMENTO: { label: 'Alojamento', classes: 'bg-purple-100 text-purple-800' },
  PONTO_INTERESSE: { label: 'Ponto de Interesse', classes: 'bg-blue-100 text-blue-800' },
  ALUGUER_CARRO: { label: 'Aluguer de Carro', classes: 'bg-orange-100 text-orange-800' },
  REFEICAO: { label: 'Refeição', classes: 'bg-emerald-100 text-emerald-800' },
  DESLOCACAO: { label: 'Deslocação', classes: 'bg-slate-200 text-slate-700' },
  OUTRO: { label: 'Outro', classes: 'bg-slate-100 text-slate-600' },
};

export function normalizarTipo(tipo: string | number): TipoAtividade {
  if (typeof tipo === 'number') {
    return ORDEM_ENUM[tipo] ?? 'OUTRO';
  }
  return (ORDEM_ENUM as readonly string[]).includes(tipo)
    ? (tipo as TipoAtividade)
    : 'OUTRO';
}

export function rotuloTipo(tipo: string | number) {
  return ROTULOS[normalizarTipo(tipo)];
}

export function tipoIgual(tipo: string | number, alvo: TipoAtividade): boolean {
  return normalizarTipo(tipo) === alvo;
}
