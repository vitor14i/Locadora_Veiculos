using FrotaGO.Helpers;

namespace FrotaGO.Models;

public class PessoaJuridica : Cliente
{
    private string _cnpj = string.Empty;

    /// <summary>CNPJ salvo sem máscara (somente 14 dígitos). A máscara é removida ao atribuir.</summary>
    public string Cnpj
    {
        get => _cnpj;
        set => _cnpj = DocumentoHelper.SomenteNumeros(value);
    }

    public string RazaoSocial { get; set; } = string.Empty;

    public override string ObterDocumento() => Cnpj;
}
