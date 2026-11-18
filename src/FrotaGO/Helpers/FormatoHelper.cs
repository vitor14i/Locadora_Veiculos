using System.Globalization;

namespace FrotaGO.Helpers;

public static class FormatoHelper
{
    public const string FormatoDataHora = "dd/MM/yyyy HH:mm";

    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Moeda(decimal valor) => valor.ToString("C", PtBr);

    public static string DiaEHora(DateTime data) => data.ToString("dd/MM 'às' HH:mm", PtBr);

    public static string DataHoraCampo(DateTime data) =>
        data.ToString(FormatoDataHora, CultureInfo.InvariantCulture);

    public static bool TentarLerDataHora(string texto, out DateTime data) =>
        DateTime.TryParseExact(texto.Trim(), FormatoDataHora, CultureInfo.InvariantCulture,
                               DateTimeStyles.None, out data);

    public static string Quantidade(int quantidade, string singular, string plural) =>
        $"{quantidade} {(quantidade == 1 ? singular : plural)}";

    public static bool TentarLerQuilometragem(string texto, out decimal km) =>
        decimal.TryParse(texto.Replace(".", "").Trim(), NumberStyles.None,
                         CultureInfo.InvariantCulture, out km);

    public static bool TentarLerValor(string texto, out decimal valor) =>
        decimal.TryParse(texto.Replace("R$", "").Trim(), NumberStyles.Number, PtBr, out valor);

    public const string FormatoData = "dd/MM/yyyy";

    public static string ValorCampo(decimal valor) => valor.ToString("0.00", PtBr);

    public static string DataCampo(DateTime data) =>
        data.ToString(FormatoData, CultureInfo.InvariantCulture);

    public static bool TentarLerData(string texto, out DateTime data) =>
        DateTime.TryParseExact(texto.Trim(), FormatoData, CultureInfo.InvariantCulture,
                               DateTimeStyles.None, out data);

    /// <summary>
    /// Coloca a máscara em um CPF ou CNPJ salvo só com números, apenas para exibição.
    /// Outros tamanhos aparecem como estão.
    /// </summary>
    public static string Documento(string documento) => documento.Length switch
    {
        DocumentoHelper.DigitosCpf => $"{documento[..3]}.{documento[3..6]}.{documento[6..9]}-{documento[9..]}",
        DocumentoHelper.DigitosCnpj => $"{documento[..2]}.{documento[2..5]}.{documento[5..8]}/{documento[8..12]}-{documento[12..]}",
        _ => documento
    };
}

