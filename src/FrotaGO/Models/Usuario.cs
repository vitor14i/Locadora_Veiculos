using FrotaGO.Helpers;

namespace FrotaGO.Models;

public abstract class Usuario
{
    /// <summary>Tamanho mínimo de qualquer senha: no cadastro, na senha temporária e na troca de senha.</summary>
    public const int TamanhoMinimoSenha = 8;

    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;

    public bool ValidarSenha(string senha) => PasswordHasher.Verify(senha, SenhaHash);

    /// <summary>Guarda só o hash da senha, nunca o texto. Quem gera e quem confere o hash ficam no model.</summary>
    public void DefinirSenha(string senha) => SenhaHash = PasswordHasher.Hash(senha);

    /// <summary>Desativa a conta ativa ou reativa a desativada. Conta desativada não faz login.</summary>
    public void AlternarSituacao() => Ativo = !Ativo;

    public void AlterarDados(string nome, string email)
    {
        Nome = nome;
        Email = email;
    }
}
