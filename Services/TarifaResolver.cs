using Locadora_Veiculos.Interfaces;
using Locadora_Veiculos.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Locadora_Veiculos.Services;

/// <summary>
/// Seleciona calculadoras keyed sem acoplar Controllers às implementações concretas.
/// </summary>
public sealed class TarifaResolver : ITarifaResolver
{
    private readonly IServiceProvider _serviceProvider;

    public TarifaResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider
            ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public ITarifa ResolverPara(Veiculo veiculo)
    {
        ArgumentNullException.ThrowIfNull(veiculo);

        // [SOLID - OCP] O tipo concreto é a chave; novas strategies entram via registro keyed, sem alterar o Controller.
        return _serviceProvider.GetRequiredKeyedService<ITarifa>(veiculo.GetType());
    }
}
