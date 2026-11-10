namespace FrotaGO.Models;

public class Moto : Veiculo
{
    public int Cilindradas { get; set; }

    /// <summary>Regra da moto: diária simples, sem acréscimos.</summary>
    public override decimal Calcular(int dias) => ValorDiaria * dias;
}
