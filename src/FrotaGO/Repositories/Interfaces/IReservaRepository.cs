using FrotaGO.Models;

namespace FrotaGO.Repositories.Interfaces;

public interface IReservaRepository
{
    Reserva? ObterPorId(int id);

    List<Reserva> ListarPorCliente(int clienteId);

    List<Reserva> ListarVisitasDoDia(DateTime data);

    List<Reserva> ListarAgendadasVencidas(DateTime limite);

    bool ExisteConflito(int veiculoId, DateTime inicio, DateTime fim, DateTime agora);

    void Adicionar(Reserva reserva);
}
