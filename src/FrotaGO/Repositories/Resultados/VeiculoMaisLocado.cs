namespace FrotaGO.Repositories.Resultados;

/// <summary>
/// Linha do ranking de veículos mais locados. As propriedades têm os mesmos nomes das colunas
/// do SQL, porque o EF preenche cada uma pelo nome.
/// </summary>
public class VeiculoMaisLocado
{
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }

    /// <summary>Quantidade de contratos do período com este veículo.</summary>
    public int Locacoes { get; set; }

    /// <summary>Receita só das diárias (ValorDiaria × QuantidadeDias), sem as multas.</summary>
    public decimal Faturamento { get; set; }
}
