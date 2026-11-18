namespace FrotaGO.Controllers;

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
