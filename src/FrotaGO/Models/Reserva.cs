using FrotaGO.Models.Enums;

namespace FrotaGO.Models;

public class Reserva
{
    /// <summary>Horas, depois do horário da retirada, que o cliente tem para comparecer antes de a reserva expirar.</summary>
    public const int PrazoComparecimentoHoras = 2;

    public int Id { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public StatusReserva Status { get; set; } = StatusReserva.Agendada;

    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public int VeiculoId { get; set; }
    public Veiculo? Veiculo { get; set; }
    public Contrato? Contrato { get; set; } // 1x1

    public int CalcularDias() => Math.Max(1, (DataFim.Date - DataInicio.Date).Days);

    public void Confirmar() => Status = StatusReserva.Convertida;

    public void Cancelar() => Status = StatusReserva.Cancelada;

    /// <summary>Marca como expirada a reserva cujo cliente não compareceu. Só muda uma reserva agendada.</summary>
    public void Expirar()
    {
        if (Status == StatusReserva.Agendada)
        {
            Status = StatusReserva.Expirada;
        }
    }
}
