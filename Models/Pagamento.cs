namespace Locadora_Veiculos.Models;

/// <summary>
/// Dados comuns de um pagamento associado a um contrato.
/// </summary>
public abstract class Pagamento
{
    /// <summary>Identificador único do pagamento.</summary>
    public int Id { get; set; }

    /// <summary>Valor monetário pago.</summary>
    public decimal Valor { get; set; }

    /// <summary>Data e hora do pagamento.</summary>
    public DateTime DataPagamento { get; set; }

    /// <summary>Identificador do contrato pago.</summary>
    public int ContratoId { get; private set; }

    /// <summary>Contrato ao qual o pagamento pertence.</summary>
    public Contrato Contrato { get; private set; } = null!;

    /// <summary>Indica se o registro está ativo.</summary>
    public bool Ativo { get; set; } = true;

    internal void AssociarContrato(Contrato contrato)
    {
        ArgumentNullException.ThrowIfNull(contrato);

        if (Contrato is not null && !ReferenceEquals(Contrato, contrato))
        {
            throw new InvalidOperationException("O pagamento já está associado a outro contrato.");
        }

        Contrato = contrato;
        ContratoId = contrato.Id;

        if (!contrato.Pagamentos.Contains(this))
        {
            contrato.Pagamentos.Add(this);
        }
    }
}
