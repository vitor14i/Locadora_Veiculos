using FrotaGO.Models.Enums;

namespace FrotaGO.Models;

public class Carro : Veiculo
{
    /// <summary>Acréscimo de 10% sobre a diária para carros com câmbio automático.</summary>
    private const decimal AcrescimoCambioAutomatico = 0.10m;

    public int NumeroPortas { get; set; }
    public TipoCambio Cambio { get; set; }
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Regra do carro: câmbio automático paga 10% a mais na diária.</summary>
    public override decimal Calcular(int dias) => CalcularDiariaEfetiva() * dias;

    // Arredonda a diária (e não o total) para que Calcular(1) seja sempre igual
    // ao ValorDiaria gravado no ItemContrato, evitando diferença de centavos.
    private decimal CalcularDiariaEfetiva() =>
        Cambio == TipoCambio.Automatico
            ? Math.Round(ValorDiaria * (1 + AcrescimoCambioAutomatico), 2, MidpointRounding.AwayFromZero)
            : ValorDiaria;
}
