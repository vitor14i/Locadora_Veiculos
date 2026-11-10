namespace Locadora_Veiculos.Enums;

/// <summary>
/// Representa a situação atual de um veículo na locadora.
/// </summary>
public enum StatusVeiculo
{
    /// <summary>Veículo disponível para locação.</summary>
    Disponivel,

    /// <summary>Veículo atualmente locado.</summary>
    Locado,

    /// <summary>Veículo indisponível por estar em manutenção.</summary>
    EmManutencao
}
