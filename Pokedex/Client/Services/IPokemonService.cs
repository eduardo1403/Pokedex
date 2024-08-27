using Pokedex.Shared;

namespace Pokedex.Client.Services
{
    public interface IPokemonService
    {
        public Task<List<Pokemon>> GetPokemons();
        public Task<Pokemon> GetPokemonById(int id);
        public Task<Pokemon> GetPokemonByName(string name);
    }
}
