using FrotaGO.Models;

namespace FrotaGO.Views;

public record UsuarioSessao(int Id, string Nome, string Email, string Perfil)
{
    private const string PerfilGerente = "Gerente";
    private const string PerfilAtendente = "Atendente";
    private const string PerfilCliente = "Cliente";

    public bool EhGerente => Perfil == PerfilGerente;
    public bool EhCliente => Perfil == PerfilCliente;

    /// <summary>Cria a sessão a partir do usuário que entrou, definindo o perfil pelo tipo.</summary>
    public static UsuarioSessao De(Usuario usuario) =>
        new(usuario.Id, usuario.Nome, usuario.Email, PerfilDe(usuario));

    private static string PerfilDe(Usuario usuario) => usuario switch
    {
        Gerente => PerfilGerente,
        Atendente => PerfilAtendente,
        Cliente => PerfilCliente,
        _ => throw new ArgumentException($"Tipo de usuário desconhecido: {usuario.GetType().Name}.")
    };
}
