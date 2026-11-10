using Locadora_Veiculos.Enums;

namespace Locadora_Veiculos.Models;

/// <summary>
/// Representa os dados comuns a todos os usuários do sistema.
/// </summary>
public abstract class Usuario
{
    /// <summary>Identificador único do usuário.</summary>
    public int Id { get; set; }

    /// <summary>E-mail utilizado pelo usuário para identificação.</summary>
    public required string Email { get; set; }

    /// <summary>Hash da senha do usuário; nunca armazene a senha em texto puro.</summary>
    public required string SenhaHash { get; set; }

    /// <summary>Indica se a conta está ativa.</summary>
    public bool Ativo { get; set; } = true;

    /// <summary>Perfil de acesso do usuário.</summary>
    public TipoPerfil Perfil { get; set; }
}
