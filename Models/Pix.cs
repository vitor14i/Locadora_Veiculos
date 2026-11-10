namespace Locadora_Veiculos.Models;

/// <summary>
/// Pagamento realizado por Pix.
/// </summary>
public class Pix : Pagamento
{
    /// <summary>Identificador da transação Pix informado pelo meio de pagamento.</summary>
    public required string IdentificadorTransacao { get; set; }
}
