using FrotaGO.Data;
using FrotaGO.Repositories.Interfaces;

namespace FrotaGO.Repositories;

public class UnidadeDeTrabalho : IUnidadeDeTrabalho
{
    private readonly FrotaGODbContext _contexto;

    public UnidadeDeTrabalho(FrotaGODbContext contexto) => _contexto = contexto;

    public void Salvar() => _contexto.SaveChanges();
}
