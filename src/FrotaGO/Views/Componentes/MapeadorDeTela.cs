using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Models.Enums;

namespace FrotaGO.Views.Componentes;

/// <summary>Converte os models nos itens de exibição das telas de reserva.</summary>
public static class MapeadorDeTela
{
    // Textos de status usados pelas telas. Comparar sempre com estas constantes.
    public const string StatusAgendada = "Agendada";
    public const string StatusAtiva = "Ativa";
    public const string StatusConcluida = "Concluída";
    public const string StatusCancelada = "Cancelada";
    public const string StatusExpirada = "Expirada";

    /// <summary>Item da busca, usando a regra de tarifa do próprio model.</summary>
    public static VeiculoItem CriarItem(Veiculo veiculo) => new()
    {
        VeiculoId = veiculo.Id,
        Tipo = veiculo is Moto ? "Moto" : "Carro",
        Categoria = veiculo switch
        {
            Carro carro => carro.Categoria,
            Moto moto => $"{moto.Cilindradas} cc",
            _ => string.Empty
        },
        Nome = NomeVeiculo(veiculo),
        Diaria = veiculo.Calcular(1)
    };

    /// <summary>Mesma regra da ClientesView: pessoa jurídica aparece pela razão social.</summary>
    public static ClienteItem CriarClienteItem(Cliente cliente) => new(
        cliente.Id,
        cliente is PessoaJuridica empresa ? empresa.RazaoSocial : cliente.Nome,
        FormatoHelper.Documento(cliente.ObterDocumento()));

    /// <summary>Precisa do veículo e do contrato carregados.</summary>
    public static CardReservaItem CriarCard(Reserva reserva)
    {
        var status = TextoStatus(reserva);
        var veiculo = reserva.Veiculo!;

        return status switch
        {
            StatusAtiva => new()
            {
                ReservaId = reserva.Id,
                Titulo = "Locação",
                Status = status,
                Veiculo = NomeVeiculo(veiculo),
                Detalhe1 = $"Devolução: {FormatoHelper.DiaEHora(reserva.DataFim)}",
                Detalhe2 = "Pagamento: confirmado",
                Detalhe3 = $"Contrato: nº {reserva.Contrato!.Id}"
            },
            StatusConcluida => new()
            {
                ReservaId = reserva.Id,
                Titulo = "Locação",
                Status = status,
                Veiculo = NomeVeiculo(veiculo),
                // A devolução só vem no ListarPorCliente. Sem ela, a linha fica vazia.
                Detalhe1 = reserva.Contrato!.Devolucao is { } devolucao
                    ? $"Devolvido em: {FormatoHelper.DiaEHora(devolucao.DataDevolucao)}"
                    : string.Empty,
                Detalhe3 = $"Valor final: {FormatoHelper.Moeda(reserva.Contrato.ValorTotal)}"
            },
            _ => new()
            {
                ReservaId = reserva.Id,
                Titulo = "Reserva",
                Status = status,
                Veiculo = NomeVeiculo(veiculo),
                Detalhe1 = $"Retirada: {FormatoHelper.DiaEHora(reserva.DataInicio)}",
                Detalhe2 = $"Devolução: {FormatoHelper.DiaEHora(reserva.DataFim)}",
                Detalhe3 = $"Valor estimado: {FormatoHelper.Moeda(veiculo.Calcular(reserva.CalcularDias()))}"
            }
        };
    }

    /// <summary>Linha das visitas do dia. Precisa do cliente e do veículo carregados.</summary>
    public static VisitaItem CriarVisita(Reserva reserva) => new()
    {
        ReservaId = reserva.Id,
        Horario = reserva.DataInicio.ToString("HH:mm"),
        Cliente = CriarClienteItem(reserva.Cliente!).Nome,
        Veiculo = NomeVeiculo(reserva.Veiculo!),
        Status = TextoStatus(reserva)
    };

    /// <summary>
    /// Agendada, Cancelada e Expirada aparecem como estão (iguais às constantes). Uma convertida
    /// aparece pelo contrato: ativo é Ativa, encerrado é Concluída e cancelado é Cancelada.
    /// Uma convertida sem o contrato carregado é erro de consulta, e não uma reserva cancelada.
    /// </summary>
    public static string TextoStatus(Reserva reserva) => reserva.Status switch
    {
        StatusReserva.Convertida => reserva.Contrato?.Status switch
        {
            StatusContrato.Ativo => StatusAtiva,
            StatusContrato.Encerrado => StatusConcluida,
            StatusContrato.Cancelado => StatusCancelada,
            _ => throw new InvalidOperationException("Contrato não carregado.")
        },
        _ => reserva.Status.ToString()
    };

    private static string NomeVeiculo(Veiculo veiculo) => $"{veiculo.Marca} {veiculo.Modelo} · {veiculo.Ano}";
}
