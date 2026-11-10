namespace Locadora_Veiculos.Interfaces;

/// <summary>
/// Define o contrato para cálculo do valor de uma locação.
/// </summary>
public interface ITarifa
{
    /// <summary>
    /// Calcula o valor da locação a partir da diária e da quantidade de dias.
    /// </summary>
    /// <param name="valorDiaria">Valor diário cadastrado para o veículo.</param>
    /// <param name="diasLocacao">Quantidade de dias da locação.</param>
    /// <returns>Valor total da locação.</returns>
    decimal Calcular(decimal valorDiaria, int diasLocacao);
}
