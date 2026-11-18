namespace FrotaGO.Controllers;

/// <summary>Campos do formulário de cadastro de cliente, como foram digitados na tela.</summary>
public record DadosCliente(
    bool EhPessoaFisica,
    string Nome,
    string Documento,
    string CnhOuRazaoSocial,
    string Email,
    string Telefone,
    string Endereco,
    DateTime? DataNascimento,
    string Senha);
