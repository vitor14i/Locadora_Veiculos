using FrotaGO.Data;
using FrotaGO.Models;
using FrotaGO.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrotaGO.Repositories;

public class ManutencaoRepository : IManutencaoRepository
{
    private readonly FrotaGODbContext _contexto;

    public ManutencaoRepository(FrotaGODbContext contexto) => _contexto = contexto;

    /// <summary>
    /// Todas as manutenções com o veículo: primeiro as em andamento (sem data de fim),
    /// depois as mais recentes. Apenas para leitura.
    /// </summary>
    public List<Manutencao> ListarTodas() =>
        _contexto.Manutencoes
            .AsNoTracking()
            .Include(manutencao => manutencao.Veiculo)
            .OrderByDescending(manutencao => manutencao.DataFim == null)
            .ThenByDescending(manutencao => manutencao.DataInicio)
            .ToList();

    /// <summary>Rastreado e com o veículo, porque ao finalizar os dois são alterados.</summary>
    public Manutencao? ObterPorId(int id) =>
        _contexto.Manutencoes
            .Include(manutencao => manutencao.Veiculo)
            .FirstOrDefault(manutencao => manutencao.Id == id);

    public void Adicionar(Manutencao manutencao) => _contexto.Manutencoes.Add(manutencao);
}
