namespace Locadora_Veiculos.Models;

/// <summary>
/// Representa um cliente pessoa física.
/// </summary>
public class PessoaFisica : Cliente
{
    /// <summary>Nome completo do cliente.</summary>
    public required string Nome { get; set; }

    /// <summary>Cadastro de Pessoa Física do cliente.</summary>
    public required string CPF { get; set; }

    /// <summary>Carteira Nacional de Habilitação do cliente.</summary>
    public required string CNH { get; set; }
}
