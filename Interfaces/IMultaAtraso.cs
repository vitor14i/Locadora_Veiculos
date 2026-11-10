namespace Locadora_Veiculos.Interfaces;

/// <summary>
/// Define o cálculo de multa devido à devolução tardia de um veículo.
/// </summary>
public interface IMultaAtraso
{
    /// <summary>Calcula a multa para uma diária e quantidade de dias de atraso.</summary>
    decimal Calcular(decimal valorDiaria, int diasAtraso);
}
