using FrotaGO.Models.Enums;

namespace FrotaGO.Models;

public class Multa
{
    public int Id { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public StatusMulta Status { get; set; } = StatusMulta.Pendente;

    public int DevolucaoId { get; set; }
    public Devolucao? Devolucao { get; set; }

    public void Cancelar() => Status = StatusMulta.Cancelada;

    /// <summary>Marca como paga a multa pendente. Uma multa cancelada continua cancelada.</summary>
    public void Pagar()
    {
        if (Status == StatusMulta.Pendente)
        {
            Status = StatusMulta.Paga;
        }
    }
}
