using Locadora_Veiculos.Data;
using Locadora_Veiculos.Models;
using Microsoft.EntityFrameworkCore;

namespace Locadora_Veiculos.Controllers;

/// <summary>
/// Orquestra consultas gerenciais da aplicação.
/// </summary>
public class RelatorioController
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    /// <summary>
    /// Recebe o contexto de dados por injeção de dependência.
    /// </summary>
    /// <param name="dbContextFactory">Factory para criar contextos curtos por operação.</param>
    public RelatorioController(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
    }

    /// <summary>
    /// Retorna o faturamento total dos contratos ativos agrupado por categoria de veículo.
    /// </summary>
    /// <returns>Faturamento agregado por categoria.</returns>
    public async Task<IReadOnlyList<FaturamentoPorCategoria>> ObterFaturamentoPorCategoriaAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        const string sql = """
            SELECT v.Categoria AS Categoria,
                   SUM(cv.ValorLocacao) AS ValorTotal
            FROM [ContratoVeiculos] AS cv
            INNER JOIN [Contratos] AS c ON c.Id = cv.ContratoId
            INNER JOIN [Veiculos] AS v ON v.Id = cv.VeiculoId
            WHERE c.Ativo = 1
            GROUP BY v.Categoria
            ORDER BY v.Categoria
            """;

        // Requisito acadêmico: SQL puro com JOIN explícito e agrupamento gerencial.
        return await context.Database
            .SqlQueryRaw<FaturamentoPorCategoria>(sql)
            .ToListAsync();
    }

    /// <summary>
    /// Soma pagamentos de contratos finalizados (devolvidos) no mês informado, usando SQL nativo parametrizado.
    /// </summary>
    public async Task<FaturamentoMensal> ObterFaturamentoMensalAsync(int ano, int mes)
    {
        if (ano < 1 || ano > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(ano));
        }

        if (mes is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(mes));
        }

        var inicioMes = new DateTime(ano, mes, 1);
        var inicioMesSeguinte = inicioMes.AddMonths(1);

        await using var context = await _dbContextFactory.CreateDbContextAsync();

        const string sql = """
            SELECT @ano AS Ano,
                   @mes AS Mes,
                   COALESCE(SUM(p.Valor), 0) AS ValorTotal
            FROM [Pagamentos] AS p
            INNER JOIN [Contratos] AS c ON c.Id = p.ContratoId
            WHERE p.Ativo = 1
              AND c.Ativo = 1
              AND c.DataDevolucao >= @inicioMes
              AND c.DataDevolucao < @inicioMesSeguinte
            """;

        var resultados = await context.Database.SqlQueryRaw<FaturamentoMensal>(
                sql,
                new Microsoft.Data.SqlClient.SqlParameter("@ano", ano),
                new Microsoft.Data.SqlClient.SqlParameter("@mes", mes),
                new Microsoft.Data.SqlClient.SqlParameter("@inicioMes", inicioMes),
                new Microsoft.Data.SqlClient.SqlParameter("@inicioMesSeguinte", inicioMesSeguinte))
            .ToListAsync();

        return resultados.Single();
    }

    /// <summary>
    /// Lista contratos ativos por SQL puro para atender consultas gerenciais específicas.
    /// </summary>
    public async Task<IReadOnlyList<Contrato>> ObterContratosAtivosComSqlPuroAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        const string sql = "SELECT * FROM [Contratos] WHERE [Ativo] = 1";

        // [SQL Puro] FromSqlRaw executa a instrução fixa no SQL Server; não há entrada concatenada do usuário.
        return await context.Contratos
            .FromSqlRaw(sql)
            .IgnoreQueryFilters()
            .AsNoTracking()
            .ToListAsync();
    }
}
