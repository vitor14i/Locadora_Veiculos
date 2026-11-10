namespace FrotaGO.Models;

public abstract class Cliente : Usuario
{
    public string Telefone { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public List<Reserva> Reservas { get; set; } = new();

    public abstract string ObterDocumento();
}
