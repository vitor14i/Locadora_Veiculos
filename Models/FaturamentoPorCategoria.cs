namespace Locadora_Veiculos.Models;

/// <summary>
/// Projeção de faturamento agregado por categoria de veículo, usada em relatórios.
/// </summary>
public class FaturamentoPorCategoria
{
    /// <summary>Categoria do veículo à qual o faturamento corresponde.</summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Soma dos valores dos contratos ativos da categoria.</summary>
    public decimal ValorTotal { get; set; }
}
