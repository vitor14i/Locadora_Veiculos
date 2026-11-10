namespace Locadora_Veiculos.Models;

/// <summary>
/// Representa o contrato de locação entre um cliente e a locadora.
/// </summary>
public class Contrato
{
    /// <summary>Identificador único do contrato.</summary>
    public int Id { get; set; }

    /// <summary>Identificador do veículo locado.</summary>
    public int VeiculoId { get; set; }

    /// <summary>Veículo associado ao contrato.</summary>
    public Veiculo Veiculo { get; set; } = null!;

    /// <summary>Identificador do cliente responsável pela locação.</summary>
    public int ClienteId { get; set; }

    /// <summary>Cliente associado ao contrato.</summary>
    public Cliente Cliente { get; set; } = null!;

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
}
