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

    public List<Veiculo> ListarTodos() =>
        _contexto.Veiculos
            .AsNoTracking()
            .OrderBy(veiculo => veiculo.Id)
            .ToList();

    public List<Veiculo> ListarPorStatus(StatusVeiculo status) =>
        _contexto.Veiculos
            .AsNoTracking()
            .Where(veiculo => veiculo.Status == status)
            .OrderBy(veiculo => veiculo.Id)
            .ToList();

    /// <summary>
    /// Veículos fora de manutenção e sem reserva que ocupe o período, na ordem de cadastro.
    /// <paramref name="tipo"/> é "Carro", "Moto" ou null para todos. Apenas para leitura.
    /// </summary>
    public List<Veiculo> ListarDisponiveis(DateTime inicio, DateTime fim, string? tipo, DateTime agora)
    {
        // A mesma regra do ReservaRepository.ExisteConflito. Vira uma subconsulta (NOT IN),
        // e o EF gera uma única consulta ao banco.
        var ocupados = _contexto.Reservas
            .Where(FiltrosDeReserva.OcupaVeiculo(inicio, fim, agora))
            .Select(reserva => reserva.VeiculoId);

        var consulta = _contexto.Veiculos
            .AsNoTracking()
            .Where(veiculo => veiculo.Status != StatusVeiculo.EmManutencao &&
                              !ocupados.Contains(veiculo.Id));

        consulta = tipo switch
        {
            "Carro" => consulta.Where(veiculo => veiculo is Carro),
            "Moto" => consulta.Where(veiculo => veiculo is Moto),
            _ => consulta
        };

        return consulta.OrderBy(veiculo => veiculo.Id).ToList();
    }

    /// <summary>Rastreado, porque é usado na edição e ao iniciar uma manutenção.</summary>
    public Veiculo? ObterPorId(int id) =>
        _contexto.Veiculos.FirstOrDefault(veiculo => veiculo.Id == id);

    public bool PlacaExiste(string placa, int? ignorarVeiculoId = null) =>
        _contexto.Veiculos.Any(veiculo =>
            veiculo.Placa == placa &&
            (ignorarVeiculoId == null || veiculo.Id != ignorarVeiculoId));

    public void Adicionar(Veiculo veiculo) => _contexto.Veiculos.Add(veiculo);
}
