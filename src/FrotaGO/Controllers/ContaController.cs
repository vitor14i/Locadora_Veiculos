using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Repositories.Interfaces;

namespace FrotaGO.Controllers;

/// <summary>Login, cadastro e consulta de clientes.</summary>
public class ContaController
{
    private const int TamanhoMinimoSenha = 8;
    private const string MensagemLoginInvalido = "E-mail ou senha incorretos.";

    private readonly IUsuarioRepository _usuarios;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;

    public ContaController(IUsuarioRepository usuarios, IUnidadeDeTrabalho unidadeDeTrabalho)
    {
        _usuarios = usuarios;
        _unidadeDeTrabalho = unidadeDeTrabalho;
    }

    public Resultado<Usuario> Entrar(string email, string senha)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(senha))
        {
            return Resultado<Usuario>.Falha("Preencha o e-mail e a senha.");
        }

        var usuario = _usuarios.ObterPorEmail(NormalizarEmail(email));

        // E-mail inexistente e senha errada têm a mesma mensagem, para não revelar quais e-mails existem.
        if (usuario is null || !usuario.ValidarSenha(senha))
        {
            return Resultado<Usuario>.Falha(MensagemLoginInvalido);
        }

        // Só depois de confirmar a senha, para quem não sabe a senha não descobrir que a conta existe.
        if (!usuario.Ativo)
        {
            return Resultado<Usuario>.Falha("Esta conta está desativada.");
        }

        return Resultado<Usuario>.Ok(usuario);
    }

    public Resultado CadastrarCliente(DadosCliente dados)
    {
        var email = NormalizarEmail(dados.Email);
        var documento = DocumentoHelper.SomenteNumeros(dados.Documento);

        var erro = ValidarCliente(dados, email, documento);
        if (erro is not null)
        {
            return Resultado.Falha(erro);
        }

        _usuarios.Adicionar(CriarCliente(dados, email, documento));
        _unidadeDeTrabalho.Salvar();
        return Resultado.Ok();
    }

    public List<Cliente> ListarClientesAtivos() => _usuarios.ListarClientesAtivos();

    private string? ValidarCliente(DadosCliente dados, string email, string documento)
    {
        string[] obrigatorios = [dados.Nome, dados.Documento, dados.CnhOuRazaoSocial,
                                 dados.Email, dados.Telefone, dados.Endereco];

        if (obrigatorios.Any(string.IsNullOrWhiteSpace) || string.IsNullOrEmpty(dados.Senha))
        {
            return "Preencha todos os campos.";
        }

        if (!email.Contains('@'))
        {
            return "Informe um e-mail válido.";
        }

        if (dados.Senha.Length < TamanhoMinimoSenha)
        {
            return $"A senha deve ter pelo menos {TamanhoMinimoSenha} caracteres.";
        }

        if (_usuarios.EmailExiste(email))
        {
            return "Este e-mail já está cadastrado.";
        }

        return ValidarDocumento(dados, documento);
    }

    private string? ValidarDocumento(DadosCliente dados, string documento)
    {
        if (dados.EhPessoaFisica)
        {
            if (documento.Length != DocumentoHelper.DigitosCpf)
            {
                return $"O CPF deve ter {DocumentoHelper.DigitosCpf} dígitos.";
            }

            if (dados.DataNascimento is null)
            {
                return "Informe a data de nascimento.";
            }

            return _usuarios.CpfExiste(documento) ? "Este CPF já está cadastrado." : null;
        }

        if (documento.Length != DocumentoHelper.DigitosCnpj)
        {
            return $"O CNPJ deve ter {DocumentoHelper.DigitosCnpj} dígitos.";
        }

        return _usuarios.CnpjExiste(documento) ? "Este CNPJ já está cadastrado." : null;
    }

    private static Cliente CriarCliente(DadosCliente dados, string email, string documento)
    {
        Cliente cliente = dados.EhPessoaFisica
            ? new PessoaFisica
            {
                Cpf = documento,
                NumeroCnh = dados.CnhOuRazaoSocial.Trim(),
                DataNascimento = dados.DataNascimento!.Value
            }
            : new PessoaJuridica
            {
                Cnpj = documento,
                RazaoSocial = dados.CnhOuRazaoSocial.Trim()
            };

        cliente.Nome = dados.Nome.Trim();
        cliente.Email = email;
        cliente.Telefone = dados.Telefone.Trim();
        cliente.Endereco = dados.Endereco.Trim();
        cliente.SenhaHash = PasswordHasher.Hash(dados.Senha);
        return cliente;
    }

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
