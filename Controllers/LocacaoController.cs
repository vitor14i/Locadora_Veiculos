using Locadora_Veiculos.Data;
using Locadora_Veiculos.Enums;
using Locadora_Veiculos.Interfaces;
using Locadora_Veiculos.Models;
using Microsoft.EntityFrameworkCore;

namespace Locadora_Veiculos.Controllers;

/// <summary>
/// Orquestra o registro de contratos de locação.
/// </summary>
public class LocacaoController
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly ITarifaResolver _tarifaResolver;

    /// <summary>
    /// Recebe o contexto de dados por injeção de dependência.
    /// </summary>
    /// <param name="dbContextFactory">Factory para criar contextos curtos por operação.</param>
    /// <param name="tarifaResolver">Resolve a Strategy keyed para o tipo concreto de veículo.</param>
    public LocacaoController(
        IDbContextFactory<AppDbContext> dbContextFactory,
        ITarifaResolver tarifaResolver)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _tarifaResolver = tarifaResolver ?? throw new ArgumentNullException(nameof(tarifaResolver));
    }

    /// <summary>
    /// Registra um contrato para um veículo disponível e um cliente existente.
    /// </summary>
    /// <param name="veiculoId">Identificador do veículo.</param>
    /// <param name="clienteId">Identificador do cliente.</param>
    /// <param name="dias">Quantidade de dias da locação.</param>
    /// <returns>Contrato persistido.</returns>
    public async Task<Contrato> RegistrarContratoAsync(
        int veiculoId,
        int clienteId,
        int dias)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var veiculo = await context.Veiculos
            .SingleOrDefaultAsync(item => item.Id == veiculoId)
            ?? throw new InvalidOperationException("Veículo não encontrado.");

        if (veiculo.Status != StatusVeiculo.Disponivel)
        {
            throw new InvalidOperationException(
                "O veículo não está disponível para locação.");
        }

        var cliente = await context.Clientes
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(item => item.Id == clienteId)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        if (!cliente.Ativo)
        {
            throw new InvalidOperationException("O cliente está inativo e não pode realizar uma locação.");
        }

        // OCP/Strategy: o controller usa o contrato ITarifa sem conhecer a regra concreta de preço.
        var calculadoraTarifa = _tarifaResolver.ResolverPara(veiculo);
        var valorTotal = calculadoraTarifa.Calcular(veiculo.ValorDiaria, dias);
        var contrato = new Contrato
        {
            Veiculo = veiculo,
            Cliente = cliente,
            DataInicio = DateTime.UtcNow,
            DiasLocacao = dias,
            ValorTotal = valorTotal
        };

        veiculo.Status = StatusVeiculo.Locado;
        context.Contratos.Add(contrato);

        try
        {
            // SaveChanges usa transação relacional; RowVersion impede a locação concorrente do mesmo veículo.
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new InvalidOperationException(
                "O veículo foi atualizado por outra operação. Atualize os dados e tente novamente.",
                exception);
        }

        return contrato;
    }
}
