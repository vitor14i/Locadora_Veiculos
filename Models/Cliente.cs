namespace Locadora_Veiculos.Models;

/// <summary>
/// Representa os dados comuns a clientes pessoa física e pessoa jurídica.
/// </summary>
public abstract class Cliente : Usuario
{
    /// <summary>Contratos associados a este cliente (um cliente pode possuir vários).</summary>
    public ICollection<Contrato> Contratos { get; } = new List<Contrato>();

    internal void AdicionarContrato(Contrato contrato)
    {
        ArgumentNullException.ThrowIfNull(contrato);

        if (!Contratos.Contains(contrato))
        {
            Contratos.Add(contrato);
        }
    }
}
