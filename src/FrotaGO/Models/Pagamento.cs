namespace FrotaGO.Models;

public abstract class Pagamento
{
    public int Id { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataPagamento { get; set; } = DateTime.Now;

    public int ContratoId { get; set; }
    public Contrato? Contrato { get; set; }

    public abstract bool Validar();
}
