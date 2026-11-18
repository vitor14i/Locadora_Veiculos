using FrotaGO.Data;
using FrotaGO.Models;
using FrotaGO.Models.Enums;
using FrotaGO.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrotaGO.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly FrotaGODbContext _contexto;

    public ReservaRepository(FrotaGODbContext contexto) => _contexto = contexto;

    /// <summary>Rastreado, porque é usado para cancelar e para iniciar a locação.</summary>
    public Reserva? ObterPorId(int id) =>
        _contexto.Reservas
            .Include(reserva => reserva.Veiculo)
            .Include(reserva => reserva.Cliente)
            // Os itens com o veículo dão aos detalhes da reserva o valor gravado no contrato.
            .Include(reserva => reserva.Contrato)
                .ThenInclude(contrato => contrato!.Itens)
                    .ThenInclude(item => item.Veiculo)
            .FirstOrDefault(reserva => reserva.Id == id);

    /// <summary>
    /// Reservas do cliente, das mais recentes para as mais antigas, com o contrato, a devolução e
    /// os pagamentos para os cards de locação. Apenas para leitura.
    /// </summary>
    public List<Reserva> ListarPorCliente(int clienteId) =>
        _contexto.Reservas
            .AsNoTracking()
            .Include(reserva => reserva.Veiculo)
            .Include(reserva => reserva.Contrato)
                .ThenInclude(contrato => contrato!.Devolucao)
            // O "Valor final" do card Concluída soma os pagamentos.
            .Include(reserva => reserva.Contrato)
                .ThenInclude(contrato => contrato!.Pagamentos)
            .Where(reserva => reserva.ClienteId == clienteId)
            .OrderByDescending(reserva => reserva.DataInicio)
            .ToList();

    public List<Reserva> ListarVisitasDoDia(DateTime data)
    {
        var inicioDoDia = data.Date;
        var inicioDoDiaSeguinte = inicioDoDia.AddDays(1);

        return _contexto.Reservas
            .AsNoTracking()
            .Include(reserva => reserva.Cliente)
            .Include(reserva => reserva.Veiculo)
            .Where(reserva => reserva.Status == StatusReserva.Agendada &&
                              reserva.DataInicio >= inicioDoDia &&
                              reserva.DataInicio < inicioDoDiaSeguinte)
            .OrderBy(reserva => reserva.DataInicio)
            .ToList();
    }

    /// <summary>
    /// Agendadas com retirada até o limite. É o complemento da regra do OcupaVeiculo.
    /// Rastreadas, porque o controller muda o status delas para Expirada.
    /// </summary>
    public List<Reserva> ListarAgendadasVencidas(DateTime limite) =>
        _contexto.Reservas
            .Where(reserva => reserva.Status == StatusReserva.Agendada && reserva.DataInicio <= limite)
            .ToList();

    public bool ExisteConflito(int veiculoId, DateTime inicio, DateTime fim, DateTime agora) =>
        _contexto.Reservas
            .Where(reserva => reserva.VeiculoId == veiculoId)
            .Any(FiltrosDeReserva.OcupaVeiculo(inicio, fim, agora));

    public void Adicionar(Reserva reserva) => _contexto.Reservas.Add(reserva);
}
