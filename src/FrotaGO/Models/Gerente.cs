namespace FrotaGO.Models;

public class Gerente : Funcionario
{
    public List<Manutencao> Manutencoes { get; set; } = new();
}
