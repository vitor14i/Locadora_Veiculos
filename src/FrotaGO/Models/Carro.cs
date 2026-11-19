using FrotaGO.Models.Enums;

namespace FrotaGO.Models;

public class Carro : Veiculo
{
    private const decimal AcrescimoCambioAutomatico = 0.10m;

    public int NumeroPortas { get; set; }
    public TipoCambio Cambio { get; set; }
    public string Categoria { get; set; } = string.Empty;

    public override decimal Calcular(int dias) => CalcularDiariaEfetiva() * dias;

    private decimal CalcularDiariaEfetiva() =>
        Cambio == TipoCambio.Automatico
            ? Math.Round(ValorDiaria * (1 + AcrescimoCambioAutomatico), 2, MidpointRounding.AwayFromZero)
            : ValorDiaria;
}
