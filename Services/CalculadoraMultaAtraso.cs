using Locadora_Veiculos.Interfaces;
using Microsoft.Extensions.Options;

namespace Locadora_Veiculos.Services;

/// <summary>
/// Calcula a multa como percentual configurável da diária por dia de atraso.
/// </summary>
public sealed class CalculadoraMultaAtraso : IMultaAtraso
{
    private readonly TarifasConfig _tarifasConfig;

    public CalculadoraMultaAtraso(IOptions<TarifasConfig> tarifasConfig)
    {
        ArgumentNullException.ThrowIfNull(tarifasConfig);
        _tarifasConfig = tarifasConfig.Value;
    }

    public decimal Calcular(decimal valorDiaria, int diasAtraso)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(valorDiaria);
        ArgumentOutOfRangeException.ThrowIfNegative(diasAtraso);

        // [SOLID - OCP] A política de multa é isolada e pode ser trocada sem alterar Controllers ou Contrato.
        return valorDiaria * _tarifasConfig.PercentualMultaAtrasoPorDia * diasAtraso;
    }
}
