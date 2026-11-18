using FrotaGO.Repositories.Resultados;

namespace FrotaGO.Controllers;

/// <summary>Relatório de um período: o resumo do SQL, o valor médio calculado e o ranking de veículos.</summary>
public record RelatorioPeriodo(ResumoFaturamento Resumo, decimal ValorMedio, List<VeiculoMaisLocado> Ranking);
