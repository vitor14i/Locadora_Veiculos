namespace Locadora_Veiculos.Models;

/// <summary>
/// Associação entre contrato e veículo, registrando o valor locado do veículo no contrato.
/// </summary>
public class ContratoVeiculo
{
    public int ContratoId { get; set; }
    public Contrato Contrato { get; set; } = null!;

    public int VeiculoId { get; set; }
    public Veiculo Veiculo { get; set; } = null!;

    /// <summary>Valor calculado para este veículo dentro do contrato.</summary>
    public decimal ValorLocacao { get; set; }
}
