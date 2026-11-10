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
