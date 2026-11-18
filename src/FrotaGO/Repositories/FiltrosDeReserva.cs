using System.Linq.Expressions;
using FrotaGO.Models;
using FrotaGO.Models.Enums;

namespace FrotaGO.Repositories;

/// <summary>Regras de consulta de reservas usadas por mais de um repositório.</summary>
public static class FiltrosDeReserva
{
    /// <summary>
    /// Reservas que ocupam o veículo em algum momento do período pedido:
    /// uma agendada, ainda dentro do prazo de comparecimento, com o período sobreposto; ou
    /// uma convertida com contrato ativo, do início da locação (DataGeracao) em diante, mesmo que a
    /// locação tenha começado antes do horário da retirada ou que a devolução atrase.
    /// </summary>
    public static Expression<Func<Reserva, bool>> OcupaVeiculo(DateTime inicio, DateTime fim, DateTime agora)
    {
        // Calculado fora da expressão, para o EF receber um valor pronto.
        var limiteComparecimento = agora.AddHours(-Reserva.PrazoComparecimentoHoras);

        return reserva =>
            (reserva.Status == StatusReserva.Agendada &&
             reserva.DataInicio > limiteComparecimento &&
             inicio < reserva.DataFim &&
             fim > reserva.DataInicio) ||
            (reserva.Status == StatusReserva.Convertida &&
             reserva.Contrato!.Status == StatusContrato.Ativo &&
             fim > reserva.Contrato!.DataGeracao);
    }
}
