using Microsoft.Extensions.Options;

namespace Locadora_Veiculos.Services;

/// <summary>
/// Calcula tarifas demonstrativas para locações de carros.
/// </summary>
public sealed class CalculadoraTarifaCarro : CalculadoraTarifaBase
{
    public CalculadoraTarifaCarro(IOptions<TarifasConfig> tarifasConfig)
        : base(tarifasConfig)
    {
    }

    /// <summary>Calcula a diária de carro, o seguro e eventual desconto por prazo longo.</summary>
    protected override decimal CalcularTotal(decimal valorDiaria, int diasLocacao)
    {
        var valorTotal = (valorDiaria + TarifasConfig.TaxaSeguroDiario) * diasLocacao;

        if (diasLocacao > TarifasConfig.DiasParaDescontoLongaLocacao)
        {
            valorTotal *= 1 - TarifasConfig.PercentualDescontoLongaLocacao;
        }

        // [SOLID - OCP] A regra de carro é substituível sem alterar o Controller ou as outras Strategies.
        return valorTotal;
    }
}
