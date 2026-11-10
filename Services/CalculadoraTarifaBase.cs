using Locadora_Veiculos.Interfaces;
using Microsoft.Extensions.Options;

namespace Locadora_Veiculos.Services;

/// <summary>
/// Centraliza a validação comum e delega o cálculo específico para cada Strategy.
/// </summary>
public abstract class CalculadoraTarifaBase : ITarifa
{
    private readonly TarifasConfig _tarifasConfig;

    protected CalculadoraTarifaBase(IOptions<TarifasConfig> tarifasConfig)
    {
        ArgumentNullException.ThrowIfNull(tarifasConfig);
        _tarifasConfig = tarifasConfig.Value;
    }

    protected TarifasConfig TarifasConfig => _tarifasConfig;

    /// <inheritdoc />
    public decimal Calcular(decimal valorDiaria, int diasLocacao)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(valorDiaria);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(diasLocacao);

        // [Clean Code - Nomes Significativos] Parâmetros distintos deixam clara a base do cálculo.
        return CalcularTotal(valorDiaria, diasLocacao);
    }

    /// <summary>Aplica as regras específicas da categoria do veículo.</summary>
    protected abstract decimal CalcularTotal(decimal valorDiaria, int diasLocacao);
}