using FrotaGO.Models;

namespace FrotaGO.Repositories.Interfaces;

public interface IDevolucaoRepository
{
    bool ExisteParaContrato(int contratoId);

    void Adicionar(Devolucao devolucao);
}
