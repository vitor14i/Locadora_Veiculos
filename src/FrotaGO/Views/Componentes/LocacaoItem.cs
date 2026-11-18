namespace FrotaGO.Views.Componentes;

public class LocacaoItem
{
    public int ContratoId { get; init; }
    public string Contrato { get; init; } = string.Empty;
    public string Cliente { get; init; } = string.Empty;
    public string Veiculo { get; init; } = string.Empty;
    public string DevolucaoPrevista { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}