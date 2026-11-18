using FrotaGO.Models;
using FrotaGO.Models.Enums;

namespace FrotaGO.Repositories.Interfaces;

public interface IVeiculoRepository
{
    List<Veiculo> ListarTodos();

    List<Veiculo> ListarPorStatus(StatusVeiculo status);

    List<Veiculo> ListarDisponiveis(DateTime inicio, DateTime fim, string? tipo, DateTime agora);

    Veiculo? ObterPorId(int id);

    bool PlacaExiste(string placa, int? ignorarVeiculoId = null);

    void Adicionar(Veiculo veiculo);
}
