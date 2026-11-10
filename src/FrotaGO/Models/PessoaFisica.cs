using FrotaGO.Helpers;

namespace FrotaGO.Models;

public class PessoaFisica : Cliente
{
    private string _cpf = string.Empty;

    /// <summary>CPF salvo sem máscara (somente 11 dígitos). A máscara é removida ao atribuir.</summary>
    public string Cpf
    {
        get => _cpf;
        set => _cpf = DocumentoHelper.SomenteNumeros(value);
    }

    public DateTime DataNascimento { get; set; }
    public string NumeroCnh { get; set; } = string.Empty;

    public override string ObterDocumento() => Cpf;
}
