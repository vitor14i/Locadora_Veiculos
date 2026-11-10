using Locadora_Veiculos.Enums;

namespace Locadora_Veiculos.Models;

/// <summary>
/// Representa um usuário com perfil de gerente.
/// </summary>
public class Gerente : Usuario
{
    /// <summary>
    /// Define o perfil de gerente para novas instâncias.
    /// </summary>
    public Gerente()
    {
        Perfil = TipoPerfil.Gerente;
    }
}
