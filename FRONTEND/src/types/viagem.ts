export interface AtividadeDTO {
  id?: number;
  titulo: string;
  descricao?: string;
  periodo: string;
  ordem: number;
  horaInicio?: string;
  horaFim?: string;
  local?: string;
  custoEstimado?: number;
  latitude?: number;
  longitude?: number;
}

export interface PrevisaoTempoDTO {
  tempMax: number;
  tempMin: number;
  condicao: string;
  probabilidadePrecipitacao?: number;
}

export interface DiaItinerarioDTO {
  id?: number;
  numeroDia: number;
  data: string;
  previsaoTempo?: PrevisaoTempoDTO;
  atividades: AtividadeDTO[];
}

export interface ItinerarioDTO {
  id?: number;
  viagemId: number;
  versao: number;
  criadoEm: string;
  dias: DiaItinerarioDTO[];
}

export interface CriarViagemDTO {
  destino: string;
  origem?: string;
  dataInicio: string;
  dataFim: string;
  orcamento?: number;
  estiloViagem?: string;
  promptLinguagemNatural?: string;
}

export interface VooDTO {
  id?: number;
  companhiaAerea: string;
  origemIATA: string;
  destinoIATA: string;
  dataPartida: string;
  dataChegada: string;
  preco: number;
  escalas: number;
  linkReserva?: string;
}

export interface AlojamentoDTO {
  id?: number;
  nome: string;
  endereco?: string;
  classificacao?: number;
  precoEstimado?: number;
  urlReserva?: string;
  latitude?: number;
  longitude?: number;
}