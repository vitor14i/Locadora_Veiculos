namespace FrotaGO.Models;

public class Manutencao
{
    public int Id { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public string Descricao { get; set; } = string.Empty;

    public int VeiculoId { get; set; }
    public Veiculo? Veiculo { get; set; }
    public int GerenteId { get; set; }
    public Gerente? Gerente { get; set; }

    public void Finalizar(DateTime dataFim) => DataFim = dataFim;

    public bool EstaEmAndamento() => DataFim == null;
}
