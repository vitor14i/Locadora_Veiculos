namespace FrotaGO.Repositories.Resultados;

/// <summary>
/// Linha única da consulta de resumo do relatório. As propriedades têm os mesmos nomes das
/// colunas do SQL, porque o EF preenche cada uma pelo nome.
/// </summary>
public class ResumoFaturamento
{
    /// <summary>Soma dos pagamentos do período: diárias e multas.</summary>
    public decimal Faturamento { get; set; }

    /// <summary>Quantidade de contratos gerados no período.</summary>
    public int Locacoes { get; set; }
}
