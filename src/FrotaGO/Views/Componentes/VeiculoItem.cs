using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrotaGO.Helpers;
using FrotaGO.Models;

namespace FrotaGO.Views.Componentes;

/// <summary>Dados exibidos em um card da busca de veículos.</summary>
public record VeiculoItem
{
    public int VeiculoId { get; init; }
    public string Tipo { get; init; } = string.Empty;
    public string Categoria { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;

    /// <summary>Diária já com as regras de tarifa do tipo de veículo.</summary>
    public decimal Diaria { get; init; }

    public int Diarias { get; init; } = 1;

    public decimal Total => Diaria * Diarias;
    public string DiariaTexto => $"{FormatoHelper.Moeda(Diaria)}/dia";
    public string TotalTexto => $"Total: {FormatoHelper.Moeda(Total)}";

    /// <summary>Retorna uma cópia com as diárias do período, usando a mesma regra da Reserva.</summary>
    public VeiculoItem ParaPeriodo(DateTime retirada, DateTime devolucao) =>
        this with { Diarias = new Reserva { DataInicio = retirada, DataFim = devolucao }.CalcularDias() };
}