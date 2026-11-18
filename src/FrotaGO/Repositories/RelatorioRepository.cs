using System.Data;
using FrotaGO.Data;
using FrotaGO.Repositories.Interfaces;
using FrotaGO.Repositories.Resultados;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FrotaGO.Repositories;

/// <summary>
/// Consultas do relatório escritas em SQL puro, com JOIN, GROUP BY e parâmetros.
/// Os valores do período sempre vão como parâmetros e nunca são concatenados no SQL.
/// </summary>
public class RelatorioRepository : IRelatorioRepository
{
    // Faturamento: tudo o que foi pago no período, diárias e multas.
    // Locações: contratos gerados no período.
    private const string SqlResumo = """
        SELECT
            (SELECT COALESCE(SUM(p.Valor), 0)
               FROM Pagamentos p
              WHERE p.DataPagamento >= @inicio AND p.DataPagamento < @fim) AS Faturamento,
            (SELECT COUNT(*)
               FROM Contratos c
              WHERE c.DataGeracao >= @inicio AND c.DataGeracao < @fim) AS Locacoes
        """;

    // Uma linha por veículo, com a receita só das diárias. Empate na quantidade: vale a maior receita.
    private const string SqlVeiculosMaisLocados = """
        SELECT v.Marca, v.Modelo, v.Ano,
               COUNT(*) AS Locacoes,
               SUM(i.ValorDiaria * i.QuantidadeDias) AS Faturamento
          FROM ItensContrato i
         INNER JOIN Contratos c ON c.Id = i.ContratoId
         INNER JOIN Veiculos v ON v.Id = i.VeiculoId
         WHERE c.DataGeracao >= @inicio AND c.DataGeracao < @fim
         GROUP BY v.Id, v.Marca, v.Modelo, v.Ano
         ORDER BY Locacoes DESC, Faturamento DESC
        """;

    private readonly FrotaGODbContext _contexto;

    public RelatorioRepository(FrotaGODbContext contexto) => _contexto = contexto;

    /// <summary>
    /// A consulta devolve sempre uma linha. O ToList() vem direto no SqlQueryRaw, para o EF
    /// mandar o SQL como está, e o primeiro item é pego na lista.
    /// </summary>
    public ResumoFaturamento ObterResumo(DateTime inicio, DateTime fimExclusivo) =>
        _contexto.Database
            .SqlQueryRaw<ResumoFaturamento>(SqlResumo, Parametros(inicio, fimExclusivo))
            .ToList()[0];

    /// <summary>
    /// Do mais locado para o menos locado. Usa ToList() direto: com First() o EF envolveria o SQL
    /// em outra consulta, e o SQL Server não aceita ORDER BY dentro dela.
    /// </summary>
    public List<VeiculoMaisLocado> ObterVeiculosMaisLocados(DateTime inicio, DateTime fimExclusivo) =>
        _contexto.Database
            .SqlQueryRaw<VeiculoMaisLocado>(SqlVeiculosMaisLocados, Parametros(inicio, fimExclusivo))
            .ToList();

    /// <summary>
    /// Parâmetros novos a cada consulta, porque um SqlParameter não pode estar em dois comandos.
    /// O tipo é DateTime2, o mesmo das colunas DataPagamento e DataGeracao.
    /// </summary>
    private static object[] Parametros(DateTime inicio, DateTime fimExclusivo) =>
    [
        new SqlParameter("@inicio", SqlDbType.DateTime2) { Value = inicio },
        new SqlParameter("@fim", SqlDbType.DateTime2) { Value = fimExclusivo }
    ];
}
