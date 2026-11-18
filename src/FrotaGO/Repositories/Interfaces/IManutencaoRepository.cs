using FrotaGO.Models;

namespace FrotaGO.Repositories.Interfaces;

public interface IManutencaoRepository
{
    List<Manutencao> ListarTodas();

    Manutencao? ObterPorId(int id);

    void Adicionar(Manutencao manutencao);
}
