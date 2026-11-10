namespace FrotaGO.Models;

public class Cartao : Pagamento
{
    public string Bandeira { get; set; } = string.Empty;
    public string UltimosDigitos { get; set; } = string.Empty;
    public int Parcelas { get; set; } = 1;

    public override bool Validar() =>
        !string.IsNullOrWhiteSpace(Bandeira) && UltimosDigitos.Length == 4 && Parcelas >= 1;
}
