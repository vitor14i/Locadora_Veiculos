namespace FrotaGO.Models;

public class Devolucao
{
    public int Id { get; set; }
    public DateTime DataDevolucao { get; set; } = DateTime.Now;
    public string Observacoes { get; set; } = string.Empty;

    public int ContratoId { get; set; }
    public Contrato? Contrato { get; set; }
    public List<Vistoria> Vistorias { get; set; } = new();
    public List<Multa> Multas { get; set; } = new();

    public int CalcularDiasAtraso(DateTime dataFimPrevista) =>
        Math.Max(0, (DataDevolucao.Date - dataFimPrevista.Date).Days);
}
