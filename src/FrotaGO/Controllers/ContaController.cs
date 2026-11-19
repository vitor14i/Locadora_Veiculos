using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Repositories.Interfaces;

namespace FrotaGO.Controllers;

public class ContaController
{
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

    public Usuario? ObterUsuario(int usuarioId) => _usuarios.ObterPorId(usuarioId);

    /// <summary>Altera nome e e-mail de qualquer usuário. O telefone só vale para o cliente.</summary>
    public Resultado AtualizarDados(int usuarioId, string nome, string email, string? telefone)
    {
        var usuario = _usuarios.ObterPorId(usuarioId);
        if (usuario is null)
        {
            return Resultado.Falha("Usuário não encontrado.");
        }

        var emailNormalizado = NormalizarEmail(email);
        var erro = ValidarDados(usuario, nome, emailNormalizado, telefone);
        if (erro is not null)
        {
            return Resultado.Falha(erro);
        }

        // Só depois de todas as regras: se alguma falhar, o objeto rastreado não muda.
        usuario.AlterarDados(nome.Trim(), emailNormalizado);
        if (usuario is Cliente cliente)
        {
            // ValidarDados já garantiu que o telefone do cliente foi informado.
            cliente.Telefone = telefone!.Trim();
        }

        _unidadeDeTrabalho.Salvar();
        return Resultado.Ok();
    }

    public Resultado AlterarSenha(int usuarioId, string senhaAtual, string novaSenha)
    {
        if (string.IsNullOrEmpty(senhaAtual) || string.IsNullOrEmpty(novaSenha))
        {
            return Resultado.Falha("Preencha a senha atual e a nova senha.");
        }

        var usuario = _usuarios.ObterPorId(usuarioId);
        if (usuario is null)
        {
            return Resultado.Falha("Usuário não encontrado.");
        }

        if (!usuario.ValidarSenha(senhaAtual))
        {
            return Resultado.Falha("A senha atual está incorreta.");
        }

        if (novaSenha.Length < Usuario.TamanhoMinimoSenha)
        {
            return Resultado.Falha($"A nova senha deve ter pelo menos {Usuario.TamanhoMinimoSenha} caracteres.");
        }

        if (novaSenha == senhaAtual)
        {
            return Resultado.Falha("A nova senha deve ser diferente da atual.");
        }

        // Só depois de todas as regras. O model guarda o hash, nunca o texto da senha.
        usuario.DefinirSenha(novaSenha);
        _unidadeDeTrabalho.Salvar();
        return Resultado.Ok();
    }

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

        if (dados.Senha.Length < Usuario.TamanhoMinimoSenha)
        {
            return $"A senha deve ter pelo menos {Usuario.TamanhoMinimoSenha} caracteres.";
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

    /// <summary>O e-mail é único entre todos os usuários, sem contar o próprio usuário.</summary>
    private string? ValidarDados(Usuario usuario, string nome, string email, string? telefone)
    {
        if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email))
        {
            return "Informe o nome e o e-mail.";
        }

        if (usuario is Cliente && string.IsNullOrWhiteSpace(telefone))
        {
            return "Informe o telefone.";
        }

        if (!email.Contains('@'))
        {
            return "Informe um e-mail válido.";
        }

        return _usuarios.EmailExiste(email, ignorarUsuarioId: usuario.Id)
            ? "Este e-mail já está cadastrado."
            : null;
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
        cliente.DefinirSenha(dados.Senha);
        return cliente;
    }

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
