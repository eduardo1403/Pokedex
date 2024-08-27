using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pokedex.Shared;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Pokedex.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public PokemonController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("todoPokemon")]
        public async Task<IActionResult> GetPokemons()
        {
            var source = await _httpClient.GetAsync("https://pokeapi.co/api/v2/pokemon?limit=10&offset=0");

            if (source.IsSuccessStatusCode)
            {
                var content = await source.Content.ReadAsStringAsync();
                var pokemonList = JsonSerializer.Deserialize<JsonDocument>(content);

                var pokemons = new List<Pokemon>();

                foreach (var pokemonSummary in pokemonList.RootElement.GetProperty("results").EnumerateArray())
                {
                    var pokemonUrl = pokemonSummary.GetProperty("url").GetString();
                    var pokemonDetailResponse = await _httpClient.GetAsync(pokemonUrl);

                    if (pokemonDetailResponse.IsSuccessStatusCode)
                    {
                        var pokemonDetailContent = await pokemonDetailResponse.Content.ReadAsStringAsync();
                        var pokemon = JsonSerializer.Deserialize<Pokemon>(pokemonDetailContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (pokemon != null)
                        {
                            pokemons.Add(pokemon);
                        }
                    }
                }

                return Ok(pokemons);
            }

            return StatusCode((int)source.StatusCode, "Error en la solicitud");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPokemonById(int id)
        {
            if (id <= 0)
            {
                return Ok(new { Message = "El ID proporcionado no es válido." });
            }

            var source = await _httpClient.GetAsync($"https://pokeapi.co/api/v2/pokemon/{id}");

            if (source.IsSuccessStatusCode)
            {
                var content = await source.Content.ReadAsStringAsync();
                var pokemon = JsonSerializer.Deserialize<Pokemon>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (pokemon != null)
                {
                    return Ok(pokemon);
                }

                return NotFound("Pokémon no encontrado.");
            }

            // Devuelve el código de estado y un mensaje de error personalizado
            return StatusCode((int)source.StatusCode, "Error al obtener el Pokémon desde la API.");
        }

        [HttpGet("searchPokemon/{name}")]
        public async Task<IActionResult> GetPokemonByName(string name)
        {
            // Validar el nombre del Pokémon
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("El nombre del Pokémon no puede estar vacío.");
            }

            try
            {
                var source = await _httpClient.GetAsync($"https://pokeapi.co/api/v2/pokemon/{name.ToLower()}");

                if (source.IsSuccessStatusCode)
                {
                    var content = await source.Content.ReadAsStringAsync();
                    var pokemon = JsonSerializer.Deserialize<Pokemon>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (pokemon != null)
                    {
                        return Ok(pokemon);
                    }

                    return NotFound("Pokémon no encontrado");
                }

                // Devuelve un mensaje de error basado en el código de estado
                return StatusCode((int)source.StatusCode, "Error al obtener el Pokémon");
            }
            catch (HttpRequestException httpEx)
            {
                // Manejo de errores de solicitud HTTP
                return StatusCode(500, $"Error en la solicitud: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                // Manejo de errores generales
                return StatusCode(500, $"Error inesperado: {ex.Message}");
            }
        }
    }
}
