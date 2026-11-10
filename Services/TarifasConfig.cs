namespace Locadora_Veiculos.Services;

/// <summary>
/// Centraliza valores configuráveis de tarifas e multas da locadora.
/// </summary>
public class TarifasConfig
{
    public decimal TaxaSeguroDiario { get; set; } = 20m;
    public decimal TaxaCapaceteDiaria { get; set; } = 10m;
    public decimal PercentualDescontoLongaLocacao { get; set; } = 0.10m;
    public int DiasParaDescontoLongaLocacao { get; set; } = 7;
    public decimal PercentualMultaAtrasoPorDia { get; set; } = 0.20m;
}
