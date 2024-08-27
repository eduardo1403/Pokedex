
using Pokedex.Shared;
using System.Net.Http.Json;
using System.Text.Json;

namespace Pokedex.Client.Services
{
    public class PokemonService : IPokemonService
    {
        private readonly HttpClient _httpClient;

        public PokemonService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Pokemon> GetPokemonById(int id)
        {
            var response = await _httpClient.GetStringAsync($"api/pokemon/{id}");
            var pokemon = JsonSerializer.Deserialize<Pokemon>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return pokemon;
        }

        public async Task<Pokemon> GetPokemonByName(string name)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/pokemon/searchPokemon/{name}");

                response.EnsureSuccessStatusCode(); // Lanza una excepción si el código de estado no es exitoso

                var responseString = await response.Content.ReadAsStringAsync();
                var pokemon = JsonSerializer.Deserialize<Pokemon>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (pokemon == null)
                {
                    throw new Exception("Pokémon no encontrado.");
                }

                return pokemon;
            }
            catch (HttpRequestException httpEx)
            {
                // Lanza una excepción que Blazor puede capturar
                throw new Exception($"Error en la solicitud: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                // Lanza una excepción que Blazor puede capturar
                throw new Exception($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<List<Pokemon>> GetPokemons()
        {
            var response = await _httpClient.GetStringAsync("api/pokemon/todoPokemon");
            var pokemons = JsonSerializer.Deserialize<List<Pokemon>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return pokemons;
        }
    }
}
