using FrotaGO.Data;
using FrotaGO.Models;
using FrotaGO.Models.Enums;
using FrotaGO.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrotaGO.Repositories;

public class ContratoRepository : IContratoRepository
{
    private readonly FrotaGODbContext _contexto;

    public ContratoRepository(FrotaGODbContext contexto) => _contexto = contexto;

    /// <summary>
    /// Contrato com o cliente, os veículos e os pagamentos.
    /// Rastreado, porque a POO-19 vai usá-lo para registrar a devolução.
    /// </summary>
    public Contrato? ObterPorId(int id) =>
        _contexto.Contratos
            .Include(contrato => contrato.Reserva)
                .ThenInclude(reserva => reserva!.Cliente)
            .Include(contrato => contrato.Itens)
                .ThenInclude(item => item.Veiculo)
            .Include(contrato => contrato.Pagamentos)
            .FirstOrDefault(contrato => contrato.Id == id);

    /// <summary>Contratos ativos, da devolução prevista mais próxima para a mais distante. Apenas para leitura.</summary>
    public List<Contrato> ListarAtivos() =>
        _contexto.Contratos
            .AsNoTracking()
            .Include(contrato => contrato.Reserva)
                .ThenInclude(reserva => reserva!.Cliente)
            .Include(contrato => contrato.Itens)
                .ThenInclude(item => item.Veiculo)
            .Where(contrato => contrato.Status == StatusContrato.Ativo)
            .OrderBy(contrato => contrato.Reserva!.DataFim)
            .ToList();

    public void Adicionar(Contrato contrato) => _contexto.Contratos.Add(contrato);
}
