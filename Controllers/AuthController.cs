using Locadora_Veiculos.Data;
using Locadora_Veiculos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Locadora_Veiculos.Controllers;

/// <summary>
/// Orquestra as validações básicas de acesso de usuários.
/// </summary>
public class AuthController
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    /// <summary>
    /// Recebe o contexto por injeção de dependência (SRP: acesso a dados fica separado da interface).
    /// </summary>
    /// <param name="dbContextFactory">Factory para criar contextos curtos por operação.</param>
    /// <param name="passwordHasher">Serviço para gerar e verificar hashes de senha.</param>
    public AuthController(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IPasswordHasher<Usuario> passwordHasher)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    /// <summary>
    /// Autentica o usuário verificando o hash da senha e o estado da conta.
    /// </summary>
    /// <param name="email">E-mail da conta.</param>
    /// <param name="senha">Senha informada.</param>
    /// <returns>Usuário autenticado.</returns>
    public async Task<Usuario> LoginAsync(
        string email,
        string senha)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Informe um e-mail válido.", nameof(email));
        }

        if (!SenhaValida(senha))
        {
            throw new ArgumentException(
                "A senha deve conter pelo menos 8 caracteres.", nameof(senha));
        }

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var usuario = await context.Usuarios
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(conta => conta.Email == email);

        if (usuario is null)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        var resultado = _passwordHasher.VerifyHashedPassword(
            usuario, usuario.SenhaHash, senha);

        if (resultado == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas.");
        }

        if (!usuario.Ativo)
        {
            throw new UnauthorizedAccessException("A conta está inativa.");
        }

        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.SenhaHash = _passwordHasher.HashPassword(usuario, senha);
            await context.SaveChangesAsync();
        }

        return usuario;
    }

    /// <summary>
    /// Centraliza a regra de comprimento mínimo exigida pelo backend.
    /// </summary>
    private static bool SenhaValida(string senha)
    {
        return senha is { Length: >= 8 };
    }
}
