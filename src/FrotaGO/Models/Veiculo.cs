using FrotaGO.Models.Enums;

namespace FrotaGO.Models;

public abstract class Veiculo : ITarifa
{
    public int Id { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
    public decimal Quilometragem { get; set; }
    public decimal ValorDiaria { get; set; }
    public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

    public List<Manutencao> Manutencoes { get; set; } = new();
    public List<Reserva> Reservas { get; set; } = new();
    public List<ItemContrato> ItensContrato { get; set; } = new();
    public List<Contrato> Contratos { get; set; } = new(); // NxN via ItemContrato

    public abstract decimal Calcular(int dias);
}
