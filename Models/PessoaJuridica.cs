namespace Locadora_Veiculos.Models;

/// <summary>
/// Representa um cliente pessoa jurídica.
/// </summary>
public class PessoaJuridica : Cliente
{
    /// <summary>Razão social do cliente.</summary>
    public required string RazaoSocial { get; set; }

    /// <summary>Cadastro Nacional da Pessoa Jurídica do cliente.</summary>
    public required string CNPJ { get; set; }
}
