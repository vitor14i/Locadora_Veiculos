using FrotaGO.Data;
using FrotaGO.Models;
using FrotaGO.Repositories.Interfaces;

namespace FrotaGO.Repositories;

public class DevolucaoRepository : IDevolucaoRepository
{
    private readonly FrotaGODbContext _contexto;

    public DevolucaoRepository(FrotaGODbContext contexto) => _contexto = contexto;

    /// <summary>Consulta o banco, e não o contrato carregado, para impedir uma segunda devolução.</summary>
    public bool ExisteParaContrato(int contratoId) =>
        _contexto.Devolucoes.Any(devolucao => devolucao.ContratoId == contratoId);

    /// <summary>As vistorias e as multas da devolução são gravadas junto, pela composição.</summary>
    public void Adicionar(Devolucao devolucao) => _contexto.Devolucoes.Add(devolucao);
}
