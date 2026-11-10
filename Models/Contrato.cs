namespace Locadora_Veiculos.Models;

/// <summary>
/// Representa o contrato de locação entre um cliente e a locadora.
/// </summary>
public class Contrato
{
    /// <summary>Identificador único do contrato.</summary>
    public int Id { get; set; }

    /// <summary>Identificador do cliente responsável pelo contrato.</summary>
    public int ClienteId { get; private set; }

    /// <summary>Cliente associado ao contrato.</summary>
    public Cliente Cliente { get; private set; } = null!;

    /// <summary>Veículos incluídos neste contrato.</summary>
    public ICollection<Veiculo> Veiculos { get; } = new List<Veiculo>();

    /// <summary>Associações com valores de locação individuais por veículo.</summary>
    public ICollection<ContratoVeiculo> ContratoVeiculos { get; } = new List<ContratoVeiculo>();

    /// <summary>Pagamentos realizados para este contrato.</summary>
    public ICollection<Pagamento> Pagamentos { get; } = new List<Pagamento>();

    /// <summary>Data e hora de início da locação.</summary>
    public DateTime DataInicio { get; set; }

    /// <summary>Quantidade de dias contratados.</summary>
    public int DiasLocacao { get; set; }

    /// <summary>Valor total calculado para a locação.</summary>
    public decimal ValorTotal { get; set; }

    /// <summary>Data e hora em que o veículo foi devolvido; nula enquanto a locação estiver aberta.</summary>
    public DateTime? DataDevolucao { get; set; }

    /// <summary>Quantidade de dias de atraso apurada na devolução.</summary>
    public int DiasAtraso { get; set; }

    /// <summary>Valor da multa calculada para o atraso.</summary>
    public decimal ValorMulta { get; set; }

    /// <summary>Indica se o contrato está ativo, permitindo inativação lógica.</summary>
    public bool Ativo { get; set; } = true;

    public void AssociarCliente(Cliente cliente)
    {
        ArgumentNullException.ThrowIfNull(cliente);

        if (Cliente is not null && !ReferenceEquals(Cliente, cliente))
        {
            throw new InvalidOperationException("O contrato já está associado a outro cliente.");
        }

        Cliente = cliente;
        ClienteId = cliente.Id;
        cliente.AdicionarContrato(this);
    }

    public void AdicionarVeiculo(Veiculo veiculo, decimal valorLocacao)
    {
        ArgumentNullException.ThrowIfNull(veiculo);

        if (valorLocacao < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(valorLocacao));
        }

        if (Veiculos.Contains(veiculo))
        {
            return;
        }

        Veiculos.Add(veiculo);
        veiculo.AdicionarContrato(this);
        ContratoVeiculos.Add(new ContratoVeiculo
        {
            Contrato = this,
            Veiculo = veiculo,
            ValorLocacao = valorLocacao
        });
    }

    public void AdicionarPagamento(Pagamento pagamento)
    {
        ArgumentNullException.ThrowIfNull(pagamento);

        if (Pagamentos.Contains(pagamento))
        {
            return;
        }

        pagamento.AssociarContrato(this);
    }
}
