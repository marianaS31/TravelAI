"""
mcp-weather (Python)
Servidor MCP que expõe a tool `get_weather`, devolvendo temperatura atual
e previsão para um destino, usando a API gratuita Open-Meteo (sem API key).
"""

from typing import Optional
import httpx
from mcp.server.fastmcp import FastMCP

# --- Servidor MCP ---
mcp = FastMCP("mcp-weather")

# Mapeamento de códigos de tempo (WMO) para descrição em português
WEATHER_CODES = {
    0: "céu limpo",
    1: "praticamente limpo",
    2: "parcialmente nublado",
    3: "nublado",
    45: "nevoeiro",
    48: "nevoeiro com geada",
    51: "chuva fraca (chuvisco)",
    53: "chuva moderada (chuvisco)",
    55: "chuva forte (chuvisco)",
    61: "chuva fraca",
    63: "chuva moderada",
    65: "chuva forte",
    71: "neve fraca",
    73: "neve moderada",
    75: "neve forte",
    80: "aguaceiros fracos",
    81: "aguaceiros moderados",
    82: "aguaceiros fortes",
    95: "trovoada",
    96: "trovoada com granizo fraco",
    99: "trovoada com granizo forte",
}


def descrever_tempo(codigo: int) -> str:
    return WEATHER_CODES.get(codigo, "condição desconhecida")


async def geocode_cidade(cidade: str) -> dict:
    """Converte o nome de uma cidade em coordenadas (lat/lon) via Open-Meteo Geocoding."""
    url = "https://geocoding-api.open-meteo.com/v1/search"
    params = {"name": cidade, "count": 1, "language": "pt", "format": "json"}

    async with httpx.AsyncClient(timeout=10.0) as client:
        resp = await client.get(url, params=params)
        resp.raise_for_status()
        data = resp.json()

    resultados = data.get("results")
    if not resultados:
        raise ValueError(f"Não foi possível encontrar a cidade: {cidade}")

    r = resultados[0]
    return {
        "latitude": r["latitude"],
        "longitude": r["longitude"],
        "name": r["name"],
        "country": r.get("country", ""),
    }


async def get_previsao_tempo(latitude: float, longitude: float) -> dict:
    """Obtém temperatura atual e previsão diária via Open-Meteo Forecast API."""
    url = "https://api.open-meteo.com/v1/forecast"
    params = {
        "latitude": latitude,
        "longitude": longitude,
        "current": "temperature_2m,weather_code,relative_humidity_2m,wind_speed_10m",
        "daily": "temperature_2m_max,temperature_2m_min,weather_code",
        "timezone": "auto",
        "forecast_days": 7,
    }

    async with httpx.AsyncClient(timeout=10.0) as client:
        resp = await client.get(url, params=params)
        resp.raise_for_status()
        return resp.json()


@mcp.tool()
async def get_weather(cidade: str, dias: Optional[int] = 3) -> str:
    """Devolve a temperatura atual e a previsão dos próximos dias para uma cidade/destino.
    Útil para planeamento de viagens.

    Args:
        cidade: Nome da cidade ou destino (ex: "Lisboa", "Porto", "Paris")
        dias: Número de dias de previsão a incluir (1-7, default 3)
    """
    dias = max(1, min(dias or 3, 7))

    try:
        local = await geocode_cidade(cidade)
        previsao = await get_previsao_tempo(local["latitude"], local["longitude"])
    except httpx.HTTPError as e:
        return f"Erro ao contactar o serviço meteorológico: {e}"
    except ValueError as e:
        return str(e)

    atual = previsao["current"]
    diario = previsao["daily"]

    linhas = [
        f"📍 {local['name']}, {local['country']}",
        "",
        f"🌡️ Agora: {atual['temperature_2m']}°C, {descrever_tempo(atual['weather_code'])}",
        f"💧 Humidade: {atual['relative_humidity_2m']}%",
        f"💨 Vento: {atual['wind_speed_10m']} km/h",
        "",
        f"📅 Previsão para os próximos {dias} dia(s):",
    ]

    for i in range(min(dias, len(diario["time"]))):
        linhas.append(
            f"- {diario['time'][i]}: min {diario['temperature_2m_min'][i]}°C / "
            f"max {diario['temperature_2m_max'][i]}°C, "
            f"{descrever_tempo(diario['weather_code'][i])}"
        )

    return "\n".join(linhas)


if __name__ == "__main__":
    # Arranca o servidor via stdio (para o LM Studio)
    mcp.run(transport="stdio")
