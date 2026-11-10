namespace Locadora_Veiculos.Models;

/// <summary>
/// Pagamento realizado com cartão, sem armazenar o número completo do cartão.
/// </summary>
public class Cartao : Pagamento
{
    /// <summary>Bandeira do cartão utilizado.</summary>
    public required string Bandeira { get; set; }

    /// <summary>Somente os quatro últimos dígitos do cartão.</summary>
    public required string UltimosQuatroDigitos { get; set; }

    /// <summary>Quantidade de parcelas.</summary>
    public int Parcelas { get; set; } = 1;
}
