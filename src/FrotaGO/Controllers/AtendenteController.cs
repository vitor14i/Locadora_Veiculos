using FrotaGO.Models;
using FrotaGO.Repositories.Interfaces;

namespace FrotaGO.Controllers;

public class AtendenteController
{
    private const string MensagemSomenteGerente = "Somente um gerente ativo pode gerenciar atendentes.";

    private readonly IUsuarioRepository _usuarios;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;

    public AtendenteController(IUsuarioRepository usuarios, IUnidadeDeTrabalho unidadeDeTrabalho)
    {
        _usuarios = usuarios;
        _unidadeDeTrabalho = unidadeDeTrabalho;
    }

    public List<Atendente> Listar() => _usuarios.ListarAtendentes();

    public Resultado Cadastrar(DadosAtendente dados, int gerenteId)
    {
        if (!EhGerenteAtivo(gerenteId))
        {
            return Resultado.Falha(MensagemSomenteGerente);
        }

        var email = NormalizarEmail(dados.Email);
        var matricula = NormalizarMatricula(dados.Matricula);

        var erro = ValidarAtendente(dados, email, matricula);
        if (erro is not null)
        {
            return Resultado.Falha(erro);
        }

        var atendente = new Atendente
        {
            Nome = dados.Nome.Trim(),
            Email = email,
            Matricula = matricula
        };
        atendente.DefinirSenha(dados.Senha);

        _usuarios.Adicionar(atendente);
        _unidadeDeTrabalho.Salvar();
        return Resultado.Ok();
    }

    public Resultado AlterarSituacao(int atendenteId, int gerenteId)
    {
        if (!EhGerenteAtivo(gerenteId))
        {
            return Resultado.Falha(MensagemSomenteGerente);
        }

        // Só muda um Atendente: o gerente não desativa a si mesmo nem outro gerente por esta tela.
        if (_usuarios.ObterPorId(atendenteId) is not Atendente atendente)
        {
            return Resultado.Falha("Atendente não encontrado.");
        }

        // Conta desativada não entra: a regra já é aplicada no ContaController.Entrar.
        atendente.AlternarSituacao();
        _unidadeDeTrabalho.Salvar();
        return Resultado.Ok();
    }

    /// <summary>Todas as regras rodam antes de criar o atendente.</summary>
    private string? ValidarAtendente(DadosAtendente dados, string email, string matricula)
    {
        string[] obrigatorios = [dados.Nome, dados.Email, dados.Matricula];

        if (obrigatorios.Any(string.IsNullOrWhiteSpace) || string.IsNullOrEmpty(dados.Senha))
        {
            return "Preencha todos os campos.";
        }

        if (!email.Contains('@'))
        {
            return "Informe um e-mail válido.";
        }

        if (dados.Senha.Length < Usuario.TamanhoMinimoSenha)
        {
            return $"A senha deve ter pelo menos {Usuario.TamanhoMinimoSenha} caracteres.";
        }

        if (_usuarios.EmailExiste(email))
        {
            return "Este e-mail já está cadastrado.";
        }

        return _usuarios.MatriculaExiste(matricula) ? "Esta matrícula já está cadastrada." : null;
    }

    /// <summary>Confere pelo banco, e não pela tela, que o usuário é um gerente ativo.</summary>
    private bool EhGerenteAtivo(int usuarioId) =>
        _usuarios.ObterPorId(usuarioId) is Gerente { Ativo: true };

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    private static string NormalizarMatricula(string matricula) => matricula.Trim().ToUpperInvariant();
}
