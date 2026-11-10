namespace Locadora_Veiculos.Models;

/// <summary>
/// Total recebido por pagamentos de contratos finalizados em um mês e ano.
/// </summary>
public class FaturamentoMensal
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public decimal ValorTotal { get; set; }
}
