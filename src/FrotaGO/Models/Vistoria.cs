namespace FrotaGO.Models;

public class Vistoria
{
    public int Id { get; set; }
    public decimal KmFinal { get; set; }
    public bool TemAvarias { get; set; }
    public string Observacoes { get; set; } = string.Empty;

    public int DevolucaoId { get; set; }
    public Devolucao? Devolucao { get; set; }
    public int ItemContratoId { get; set; }
    public ItemContrato? ItemContrato { get; set; }

    public decimal CalcularKmRodados(decimal kmInicial) => KmFinal - kmInicial;
}
