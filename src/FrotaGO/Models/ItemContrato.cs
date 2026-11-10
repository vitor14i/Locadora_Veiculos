namespace FrotaGO.Models;

public class ItemContrato
{
    public int Id { get; set; }
    public decimal KmInicial { get; set; }
    public decimal ValorDiaria { get; set; }
    public int QuantidadeDias { get; set; }

    public int ContratoId { get; set; }
    public Contrato? Contrato { get; set; }
    public int VeiculoId { get; set; }
    public Veiculo? Veiculo { get; set; }
    public Vistoria? Vistoria { get; set; } // 1x1

    public decimal CalcularSubtotal() => ValorDiaria * QuantidadeDias;
}
