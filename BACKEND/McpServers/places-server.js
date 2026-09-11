import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";
import dotenv from "dotenv";

dotenv.config();

const GOOGLE_PLACES_API_KEY = process.env.GOOGLE_PLACES_API_KEY;
const PLACES_BASE_URL = "https://places.googleapis.com/v1/places:searchText";

const server = new McpServer({
  name: "places-server",
  version: "1.0.0"
});

// Resolve o centro geográfico do destino, para usar como locationBias nas
// pesquisas seguintes. Sem isto, a Text Search API usa o IP de origem do
// pedido como sinal de contexto e pode misturar resultados de sítios
// completamente errados (já visto: um restaurante em Braga a aparecer
// numa pesquisa "restaurantes em Tóquio").
async function resolverCentroDestino(destino) {
  const resposta = await fetch(PLACES_BASE_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-Goog-Api-Key": GOOGLE_PLACES_API_KEY,
      "X-Goog-FieldMask": "places.location"
    },
    body: JSON.stringify({ textQuery: destino })
  });

  if (!resposta.ok) return null;

  const dados = await resposta.json();
  const primeiro = dados.places?.[0];
  return primeiro?.location
    ? { latitude: primeiro.location.latitude, longitude: primeiro.location.longitude }
    : null;
}

async function pesquisarLugares(query, centro) {
  const corpo = { textQuery: query };

  // Restringe a pesquisa a um raio de 50km à volta do destino, quando
  // conseguimos resolver as suas coordenadas — evita resultados de outras
  // cidades/países a "contaminar" a lista.
  if (centro) {
    corpo.locationBias = {
      circle: {
        center: { latitude: centro.latitude, longitude: centro.longitude },
        radius: 50000.0
      }
    };
  }

  const resposta = await fetch(PLACES_BASE_URL, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-Goog-Api-Key": GOOGLE_PLACES_API_KEY,
      "X-Goog-FieldMask": "places.displayName,places.formattedAddress,places.rating,places.userRatingCount,places.priceLevel,places.location"
    },
    body: JSON.stringify(corpo)
  });

  if (!resposta.ok) {
    throw new Error(await resposta.text());
  }

  const dados = await resposta.json();
  return (dados.places || []).map(p => ({
    nome: p.displayName?.text,
    morada: p.formattedAddress,
    avaliacao: p.rating,
    numAvaliacoes: p.userRatingCount,
    nivelPreco: p.priceLevel,
    latitude: p.location?.latitude ?? null,
    longitude: p.location?.longitude ?? null
  }));
}

server.registerTool(
  "pesquisar_alojamento",
  {
    title: "Pesquisar Alojamento",
    description: "Pesquisa hotéis e alojamento numa cidade/região usando a API do Google Places.",
    inputSchema: {
      destino: z.string().describe("Cidade ou região, ex: Lisboa, Portugal"),
      criterios: z.string().optional().describe("Critérios adicionais, ex: 'perto do centro', 'económico'")
    }
  },
  async ({ destino, criterios }) => {
    try {
      const centro = await resolverCentroDestino(destino);
      const query = `hotéis em ${destino}${criterios ? " " + criterios : ""}`;
      const resultados = await pesquisarLugares(query, centro);
      return { content: [{ type: "text", text: JSON.stringify(resultados, null, 2) }] };
    } catch (erro) {
      return { content: [{ type: "text", text: `Erro: ${erro.message}` }], isError: true };
    }
  }
);

server.registerTool(
  "pesquisar_pontos_interesse",
  {
    title: "Pesquisar Pontos de Interesse",
    description: "Pesquisa atrações turísticas, museus, restaurantes e pontos de interesse numa cidade.",
    inputSchema: {
      destino: z.string().describe("Cidade ou região, ex: Paris, França"),
      tipo: z.string().optional().describe("Tipo de ponto de interesse, ex: 'museus', 'restaurantes', 'monumentos'")
    }
  },
  async ({ destino, tipo }) => {
    try {
      const centro = await resolverCentroDestino(destino);
      const query = `${tipo || "pontos turísticos"} em ${destino}`;
      const resultados = await pesquisarLugares(query, centro);
      return { content: [{ type: "text", text: JSON.stringify(resultados, null, 2) }] };
    } catch (erro) {
      return { content: [{ type: "text", text: `Erro: ${erro.message}` }], isError: true };
    }
  }
);

async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
}

main().catch(erro => {
  console.error("Falha ao iniciar places-server:", erro);
  process.exit(1);
});
