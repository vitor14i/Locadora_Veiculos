using FrotaGO.Helpers;

namespace FrotaGO.Models;

public abstract class Usuario
{
    public const int TamanhoMinimoSenha = 8;

    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;

    public bool ValidarSenha(string senha) => PasswordHasher.Verify(senha, SenhaHash);

    public void DefinirSenha(string senha) => SenhaHash = PasswordHasher.Hash(senha);

    public void AlternarSituacao() => Ativo = !Ativo;

    public void AlterarDados(string nome, string email)
    {
        Nome = nome;
        Email = email;
    }
}
