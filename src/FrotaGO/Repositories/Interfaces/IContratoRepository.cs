using FrotaGO.Models;

namespace FrotaGO.Repositories.Interfaces;

public interface IContratoRepository
{
    Contrato? ObterPorId(int id);

    List<Contrato> ListarAtivos();

    void Adicionar(Contrato contrato);
}
