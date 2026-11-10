using Locadora_Veiculos.Data;
using Locadora_Veiculos.Enums;
using Locadora_Veiculos.Interfaces;
using Locadora_Veiculos.Models;
using Microsoft.EntityFrameworkCore;

namespace Locadora_Veiculos.Controllers;

/// <summary>
/// Orquestra a devolução do veículo e o cálculo de multa por atraso.
/// </summary>
public sealed class DevolucaoController
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IMultaAtraso _calculadoraMulta;

    public DevolucaoController(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IMultaAtraso calculadoraMulta)
    {
        _dbContextFactory = dbContextFactory
            ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _calculadoraMulta = calculadoraMulta
            ?? throw new ArgumentNullException(nameof(calculadoraMulta));
    }

    /// <summary>
    /// Registra a devolução, calcula a multa percentual e libera o veículo.
    /// Qualquer fração de dia após o prazo é cobrada como um dia completo.
    /// </summary>
    /// <param name="contratoId">Identificador do contrato em aberto.</param>
    /// <param name="dataDevolucao">Data/hora da devolução; valores sem fuso são interpretados como UTC.</param>
    /// <returns>Contrato atualizado com atraso, multa e data de devolução.</returns>
    public async Task<Contrato> RegistrarDevolucaoAsync(int contratoId, DateTime dataDevolucao)
    {
        if (dataDevolucao == default)
        {
            throw new ArgumentException("Informe uma data de devolução válida.", nameof(dataDevolucao));
        }

        var dataDevolucaoUtc = dataDevolucao.Kind switch
        {
            DateTimeKind.Utc => dataDevolucao,
            DateTimeKind.Local => dataDevolucao.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dataDevolucao, DateTimeKind.Utc)
        };

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var contrato = await context.Contratos
            .IgnoreQueryFilters()
            .Include(item => item.Veiculos)
            .SingleOrDefaultAsync(item => item.Id == contratoId)
            ?? throw new InvalidOperationException("Contrato não encontrado.");

        if (!contrato.Ativo)
        {
            throw new InvalidOperationException("O contrato está inativo.");
        }

        if (contrato.DataDevolucao.HasValue)
        {
            throw new InvalidOperationException("Este contrato já foi devolvido.");
        }

        if (dataDevolucaoUtc < contrato.DataInicio)
        {
            throw new ArgumentException(
                "A data de devolução não pode ser anterior ao início da locação.",
                nameof(dataDevolucao));
        }

        var dataPrevista = contrato.DataInicio.AddDays(contrato.DiasLocacao);
        var diasAtraso = dataDevolucaoUtc <= dataPrevista
            ? 0
            : (int)Math.Ceiling((dataDevolucaoUtc - dataPrevista).TotalDays);

        if (contrato.Veiculos.Count == 0)
        {
            throw new InvalidOperationException("O contrato não possui veículos associados.");
        }

        contrato.DataDevolucao = dataDevolucaoUtc;
        contrato.DiasAtraso = diasAtraso;
        contrato.ValorMulta = contrato.Veiculos.Sum(veiculo =>
            _calculadoraMulta.Calcular(veiculo.ValorDiaria, diasAtraso));

        foreach (var veiculo in contrato.Veiculos)
        {
            veiculo.Status = StatusVeiculo.Disponivel;
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new InvalidOperationException(
                "O contrato ou veículo foi atualizado por outra operação. Atualize os dados e tente novamente.",
                exception);
        }

        return contrato;
    }
}
