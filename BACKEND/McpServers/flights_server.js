import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { z } from "zod";
import dotenv from "dotenv";

dotenv.config();

const DUFFEL_API_KEY = process.env.DUFFEL_API_KEY;
const DUFFEL_BASE_URL = "https://api.duffel.com";

const server = new McpServer({
  name: "flights-server",
  version: "1.0.0"
});

server.registerTool(
  "pesquisar_voos",
  {
    title: "Pesquisar Voos",
    description: "Pesquisa voos entre dois aeroportos numa data específica, usando a API da Duffel (modo de teste, dados simulados).",
    inputSchema: {
      origem: z.string().length(3).describe("Código IATA do aeroporto de origem, ex: OPO"),
      destino: z.string().length(3).describe("Código IATA do aeroporto de destino, ex: CDG"),
      dataPartida: z.string().describe("Data de partida no formato YYYY-MM-DD"),
      dataRegresso: z.string().optional().describe("Data de regresso (opcional, para voos de ida e volta)"),
      numPassageiros: z.number().int().min(1).default(1)
    }
  },
  async ({ origem, destino, dataPartida, dataRegresso, numPassageiros }) => {
    try {
      const slices = [{ origin: origem, destination: destino, departure_date: dataPartida }];
      if (dataRegresso) {
        slices.push({ origin: destino, destination: origem, departure_date: dataRegresso });
      }

      const passengers = Array.from({ length: numPassageiros }, () => ({ type: "adult" }));

      // Criar o "offer request" — passo obrigatório da API da Duffel antes de listar ofertas
      const respostaCriacao = await fetch(`${DUFFEL_BASE_URL}/air/offer_requests`, {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${DUFFEL_API_KEY}`,
          "Duffel-Version": "v2",
          "Content-Type": "application/json"
        },
        body: JSON.stringify({ data: { slices, passengers, cabin_class: "economy" } })
      });

      if (!respostaCriacao.ok) {
        const erro = await respostaCriacao.text();
        return {
          content: [{ type: "text", text: `Erro ao pesquisar voos: ${erro}` }],
          isError: true
        };
      }

      const dadosCriacao = await respostaCriacao.json();
      const offerRequestId = dadosCriacao.data.id;

      // Listar as ofertas geradas para esse offer request
      const respostaOfertas = await fetch(
        `${DUFFEL_BASE_URL}/air/offers?offer_request_id=${offerRequestId}&limit=5`,
        {
          headers: {
            "Authorization": `Bearer ${DUFFEL_API_KEY}`,
            "Duffel-Version": "v2"
          }
        }
      );

      const dadosOfertas = await respostaOfertas.json();

      const resumo = dadosOfertas.data.map(oferta => ({
        id: oferta.id,
        companhia: oferta.owner?.name,
        preco: `${oferta.total_amount} ${oferta.total_currency}`,
        segmentos: oferta.slices.map(s => ({
          origem: s.origin.iata_code,
          destino: s.destination.iata_code,
          duracao: s.duration,
          paragens: s.segments.length - 1
        }))
      }));

      return {
        content: [{ type: "text", text: JSON.stringify(resumo, null, 2) }]
      };
    } catch (erro) {
      return {
        content: [{ type: "text", text: `Erro inesperado: ${erro.message}` }],
        isError: true
      };
    }
  }
);

async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
}

main().catch(erro => {
  console.error("Falha ao iniciar flights-server:", erro);
  process.exit(1);
});