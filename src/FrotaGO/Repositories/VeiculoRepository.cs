using FrotaGO.Data;
using FrotaGO.Models;
using FrotaGO.Models.Enums;
using FrotaGO.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrotaGO.Repositories;

public class VeiculoRepository : IVeiculoRepository
{
    private readonly FrotaGODbContext _contexto;

    public VeiculoRepository(FrotaGODbContext contexto) => _contexto = contexto;

    /// <summary>Toda a frota, na ordem de cadastro. Apenas para leitura.</summary>
    public List<Veiculo> ListarTodos() =>
        _contexto.Veiculos
            .AsNoTracking()
            .OrderBy(veiculo => veiculo.Id)
            .ToList();

    /// <summary>Veículos com o status informado, na ordem de cadastro. Apenas para leitura.</summary>
    public List<Veiculo> ListarPorStatus(StatusVeiculo status) =>
        _contexto.Veiculos
            .AsNoTracking()
            .Where(veiculo => veiculo.Status == status)
            .OrderBy(veiculo => veiculo.Id)
            .ToList();

    /// <summary>Rastreado, porque é usado na edição e ao iniciar uma manutenção.</summary>
    public Veiculo? ObterPorId(int id) =>
        _contexto.Veiculos.FirstOrDefault(veiculo => veiculo.Id == id);

    public bool PlacaExiste(string placa, int? ignorarVeiculoId = null) =>
        _contexto.Veiculos.Any(veiculo =>
            veiculo.Placa == placa &&
            (ignorarVeiculoId == null || veiculo.Id != ignorarVeiculoId));

    public void Adicionar(Veiculo veiculo) => _contexto.Veiculos.Add(veiculo);
}
