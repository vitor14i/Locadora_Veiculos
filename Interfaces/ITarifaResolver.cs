using Locadora_Veiculos.Models;

namespace Locadora_Veiculos.Interfaces;

/// <summary>
/// Resolve a Strategy de tarifa adequada ao tipo concreto do veículo.
/// </summary>
public interface ITarifaResolver
{
    /// <summary>Obtém a calculadora registrada para o tipo do veículo.</summary>
    ITarifa ResolverPara(Veiculo veiculo);
}
