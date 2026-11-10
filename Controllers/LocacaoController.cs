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
        return await RegistrarContratoAsync([veiculoId], clienteId, dias);
    }

    /// <summary>
    /// Registra um contrato para vários veículos disponíveis e um cliente existente.
    /// </summary>
    public async Task<Contrato> RegistrarContratoAsync(
        IReadOnlyCollection<int> veiculoIds,
        int clienteId,
        int dias)
    {
        ArgumentNullException.ThrowIfNull(veiculoIds);
        if (veiculoIds.Count == 0)
        {
            throw new ArgumentException("Informe ao menos um veículo.", nameof(veiculoIds));
        }

        if (veiculoIds.Distinct().Count() != veiculoIds.Count)
        {
            throw new ArgumentException("A lista contém veículos duplicados.", nameof(veiculoIds));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dias);

        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var veiculos = await context.Veiculos
            .Where(item => veiculoIds.Contains(item.Id))
            .ToListAsync();

        if (veiculos.Count != veiculoIds.Count)
        {
            throw new InvalidOperationException("Um ou mais veículos não foram encontrados.");
        }

        if (veiculos.Any(veiculo => veiculo.Status != StatusVeiculo.Disponivel))
        {
            throw new InvalidOperationException("Um ou mais veículos não estão disponíveis para locação.");
        }

        var cliente = await context.Clientes
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(item => item.Id == clienteId)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        if (!cliente.Ativo)
        {
            throw new InvalidOperationException("O cliente está inativo e não pode realizar uma locação.");
        }

        var valoresPorVeiculo = veiculos.Select(veiculo =>
        {
            // OCP/Strategy: o controller usa ITarifa sem conhecer a regra concreta de preço.
            var tarifa = _tarifaResolver.ResolverPara(veiculo);
            return (Veiculo: veiculo, Valor: tarifa.Calcular(veiculo.ValorDiaria, dias));
        }).ToList();

        var contrato = new Contrato
        {
            DataInicio = DateTime.UtcNow,
            DiasLocacao = dias,
            ValorTotal = valoresPorVeiculo.Sum(item => item.Valor)
        };
        contrato.AssociarCliente(cliente);

        foreach (var (veiculo, valor) in valoresPorVeiculo)
        {
            contrato.AdicionarVeiculo(veiculo, valor);
            veiculo.Status = StatusVeiculo.Locado;
        }

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
