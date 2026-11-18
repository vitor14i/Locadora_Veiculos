namespace FrotaGO.Models;

public class Moto : Veiculo
{
    public int Cilindradas { get; set; }

    public override decimal Calcular(int dias) => ValorDiaria * dias;
}
