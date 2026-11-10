using Locadora_Veiculos.Enums;

namespace Locadora_Veiculos.Models;

/// <summary>
/// Representa um usuário com perfil de atendente.
/// </summary>
public class Atendente : Usuario
{
    /// <summary>Define o perfil de atendente para novas instâncias.</summary>
    public Atendente()
    {
        Perfil = TipoPerfil.Atendente;
    }
}
