using Locadora_Veiculos.Enums;

namespace Locadora_Veiculos.Models;

/// <summary>
/// Representa os dados comuns a todos os veículos disponíveis no sistema.
/// </summary>
public abstract class Veiculo
{
    /// <summary>Identificador único do veículo.</summary>
    public int Id { get; set; }

    /// <summary>Placa de identificação do veículo.</summary>
    public required string Placa { get; set; }

    /// <summary>Marca do veículo.</summary>
    public required string Marca { get; set; }

    /// <summary>Modelo do veículo.</summary>
    public required string Modelo { get; set; }

    /// <summary>Categoria utilizada para classificação e relatórios gerenciais.</summary>
    public required string Categoria { get; set; }

    /// <summary>Ano de fabricação do veículo.</summary>
    public int Ano { get; set; }

    /// <summary>Valor da diária de locação.</summary>
    public decimal ValorDiaria { get; set; }

    /// <summary>Situação atual do veículo.</summary>
    public StatusVeiculo Status { get; set; } = StatusVeiculo.Disponivel;

    /// <summary>Indica se o registro está ativo, permitindo inativação lógica.</summary>
    public bool Ativo { get; set; } = true;

    /// <summary>Token de concorrência otimista gerado pelo SQL Server.</summary>
    public byte[] RowVersion { get; set; } = [];
}
