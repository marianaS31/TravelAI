// Tipos alinhados com TravelAI.DTOs (backend .NET)
// Nota: Ids são GUIDs — representados como string em JSON/TS, nunca number.

export interface AtividadeDTO {
  id: string;
  ordem: number;
  nome: string;
  // string se o backend tiver JsonStringEnumConverter, número caso contrário
  // — usa normalizarTipo()/rotuloTipo() de '../utils/tipoAtividade' para lidar com ambos
  tipo: string | number;
  horaInicio: string;
  horaFim: string;
  local: string;
  detalhes?: string;
}

export interface PrevisaoTempoDTO {
  tempMax: number;
  tempMin: number;
  condicao: string;
  probabilidadePrecipitacao?: number;
}

export interface DiaItinerarioDTO {
  id: string;
  numeroDia: number;
  data: string;
  atividades: AtividadeDTO[];
  previsaoTempo?: PrevisaoTempoDTO | null;
}

export interface LugarSugeridoDTO {
  nome: string;
  morada?: string;
  avaliacao?: number;
  numAvaliacoes?: number;
  nivelPreco?: string;
  url?: string;
  latitude?: number;
  longitude?: number;
}

export interface SegmentoVooDTO {
  origem: string;
  destino: string;
  duracao?: string;
  paragens: number;
}

export interface VooSugeridoDTO {
  id?: string;
  companhia?: string;
  preco?: string;
  segmentos: SegmentoVooDTO[];
  url?: string;
}

export interface ItinerarioDTO {
  id: string;
  viagemId: string;
  versao: number;
  criadoEm: string;
  dias: DiaItinerarioDTO[];
  // Só vêm preenchidos na resposta imediata de POST /itinerarios/gerar —
  // não são persistidos, por isso um GET posterior devolve listas vazias.
  alojamentosReais?: LugarSugeridoDTO[];
  restaurantesReais?: LugarSugeridoDTO[];
  voosReais?: VooSugeridoDTO[];
}

// Corresponde ao que o ViagemController espera em POST /api/viagens
// ⚠️ Confirmar campos exatos assim que tiveres o ViagemController — isto é
// a melhor estimativa com base no modelo Viagem.cs (Titulo, Destino,
// DataInicio, DataFim, NumViajantes, Orcamento).
export interface CriarViagemDTO {
  titulo: string;
  destino: string;
  dataInicio: string;
  dataFim: string;
  numViajantes: number;
  orcamento?: number;
}

// Campos que NÃO pertencem à Viagem em si, mas que alimentam o prompt
// enviado a /api/itinerarios/gerar (GerarItinerarioRequestDTO)
export interface GerarItinerarioRequestDTO {
  viagemId: string;
  instrucoesAdicionais?: string;
  origemPartida?: string;
}

export interface ViagemResponseDTO {
  id: string;
  titulo: string;
  destino: string;
  dataInicio: string;
  dataFim: string;
  numViajantes: number;
  orcamento?: number;
  estado: string;
  criadoEm: string;
  atualizadoEm: string;
}
