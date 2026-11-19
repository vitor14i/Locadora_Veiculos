namespace FrotaGO.Helpers;
public static class NomeHelper
{
    public static string PrimeiroNome(string nome) =>
        Partes(nome).FirstOrDefault() ?? string.Empty;

    public static string Iniciais(string nome)
    {
        var partes = Partes(nome);
        return partes.Length switch
        {
            0 => "?",
            1 => partes[0][..1].ToUpper(),
            _ => $"{partes[0][0]}{partes[^1][0]}".ToUpper()
        };
    }

    private static string[] Partes(string nome) =>
        nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
}
