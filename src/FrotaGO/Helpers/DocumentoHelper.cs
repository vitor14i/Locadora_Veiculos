namespace FrotaGO.Helpers;

/// <summary>
/// Responsabilidade única: padronizar documentos (CPF/CNPJ) para o formato salvo no banco,
/// que é somente dígitos. Use o mesmo método no cadastro e na busca.
/// </summary>
public static class DocumentoHelper
{
    public const int DigitosCpf = 11;
    public const int DigitosCnpj = 14;

    public static string SomenteNumeros(string? documento) =>
        string.IsNullOrEmpty(documento)
            ? string.Empty
            : string.Concat(documento.Where(c => c is >= '0' and <= '9'));
}
