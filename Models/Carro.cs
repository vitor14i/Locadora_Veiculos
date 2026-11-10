using Locadora_Veiculos.Interfaces;
using Locadora_Veiculos.Services;
using Microsoft.Extensions.Options;

namespace Locadora_Veiculos.Models;

/// <summary>
/// Representa um carro locável, mapeado na hierarquia TPH de veículos.
/// </summary>
public class Carro : Veiculo, ITarifa
{
    private readonly TarifasConfig _tarifasConfig;

    public Carro()
        : this(new TarifasConfig())
    {
    }

    public Carro(IOptions<TarifasConfig> tarifasConfig)
        : this(tarifasConfig?.Value ?? throw new ArgumentNullException(nameof(tarifasConfig)))
    {
    }

    private Carro(TarifasConfig tarifasConfig)
    {
        _tarifasConfig = tarifasConfig;
    }

    /// <summary>Calcula o preço do carro com seguro e desconto configurável por locação longa.</summary>
    public decimal Calcular(decimal valorDiaria, int diasLocacao)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(valorDiaria);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(diasLocacao);

        var valorTotal = (valorDiaria + _tarifasConfig.TaxaSeguroDiario) * diasLocacao;

        if (diasLocacao > _tarifasConfig.DiasParaDescontoLongaLocacao)
        {
            valorTotal *= 1 - _tarifasConfig.PercentualDescontoLongaLocacao;
        }

        // [SOLID - OCP] Carro implementa o contrato de tarifa e pode ser resolvido como Strategy keyed.
        return valorTotal;
    }
}
