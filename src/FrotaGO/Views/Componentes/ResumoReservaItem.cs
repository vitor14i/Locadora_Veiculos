using FrotaGO.Helpers;


namespace FrotaGO.Views.Componentes;

public class ResumoReservaItem
{
    public string Tipo { get; init; } = string.Empty;
    public string Categoria { get; init; } = string.Empty;
    public string Veiculo { get; init; } = string.Empty;
    public List<DetalheLinha> Linhas { get; init; } = [];
    public string RotuloTotal { get; init; } = string.Empty;
    public string ValorTotal { get; init; } = string.Empty;

    public static ResumoReservaItem Criar(VeiculoItem veiculo, DateTime retirada,
                                          DateTime devolucao, string rotuloTotal) => new()
                                          {
                                              Tipo = veiculo.Tipo,
                                              Categoria = veiculo.Categoria,
                                              Veiculo = veiculo.Nome,
                                              Linhas =
        [
            new("Retirada", FormatoHelper.DiaEHora(retirada)),
            new("Devolução", FormatoHelper.DiaEHora(devolucao)),
            new("Diárias", veiculo.Diarias.ToString()),
            new("Valor da diária", FormatoHelper.Moeda(veiculo.Diaria))
        ],
                                              RotuloTotal = rotuloTotal,
                                              ValorTotal = FormatoHelper.Moeda(veiculo.Total)
                                          };
}
