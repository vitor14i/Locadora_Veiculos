using Microsoft.Extensions.Options;

namespace Locadora_Veiculos.Services;

/// <summary>
/// Calcula tarifas demonstrativas para locações de motos.
/// </summary>
public sealed class CalculadoraTarifaMoto : CalculadoraTarifaBase
{
    public CalculadoraTarifaMoto(IOptions<TarifasConfig> tarifasConfig)
        : base(tarifasConfig)
    {
    }

    /// <summary>Calcula a diária de moto acrescida da taxa diária de capacete.</summary>
    protected override decimal CalcularTotal(decimal valorDiaria, int diasLocacao)
    {
        // [SOLID - OCP] A regra da moto pode variar sem modificar a calculadora de carro.
        return (valorDiaria + TarifasConfig.TaxaCapaceteDiaria) * diasLocacao;
    }
}
