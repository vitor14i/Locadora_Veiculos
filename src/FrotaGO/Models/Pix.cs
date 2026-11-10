namespace FrotaGO.Models;

public class Pix : Pagamento
{
    public string CodigoTransacao { get; set; } = string.Empty;

    public override bool Validar() => !string.IsNullOrWhiteSpace(CodigoTransacao);
}
