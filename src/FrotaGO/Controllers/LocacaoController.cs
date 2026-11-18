using FrotaGO.Models;
using FrotaGO.Models.Enums;
using FrotaGO.Repositories.Interfaces;

namespace FrotaGO.Controllers;

/// <summary>Início da locação (contrato, item e pagamento), lista das locações ativas e busca de um contrato.</summary>
public class LocacaoController
{
    private readonly IReservaRepository _reservas;
    private readonly IContratoRepository _contratos;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;

    public LocacaoController(IReservaRepository reservas, IContratoRepository contratos,
                             IUnidadeDeTrabalho unidadeDeTrabalho)
    {
        _reservas = reservas;
        _contratos = contratos;
        _unidadeDeTrabalho = unidadeDeTrabalho;
    }

    /// <summary>
    /// Converte a reserva em locação: cria o contrato com o item e o pagamento, muda a reserva
    /// para Convertida e o veículo para Alugado, e grava tudo com um único Salvar().
    /// </summary>
    public Resultado<Contrato> IniciarLocacao(int reservaId, decimal kmSaida, bool documentosConferidos,
                                              Pagamento pagamento)
    {
        var agora = DateTime.Now;

        // A reserva vem rastreada, com o veículo, o cliente e o contrato.
        var reserva = _reservas.ObterPorId(reservaId);
        if (reserva is null)
        {
            return Resultado<Contrato>.Falha("Reserva não encontrada.");
        }

        // Todas as validações vêm antes de qualquer alteração nos objetos rastreados.
        var erro = Validar(reserva, kmSaida, documentosConferidos, pagamento, agora);
        if (erro is not null)
        {
            return Resultado<Contrato>.Falha(erro);
        }

        var veiculo = reserva.Veiculo!;

        // O AdicionarItem lê o KmInicial da quilometragem do veículo.
        veiculo.Quilometragem = kmSaida;

        // O item grava a diária já com a regra de tarifa do tipo de veículo.
        var contrato = new Contrato { ReservaId = reserva.Id, DataGeracao = agora };
        contrato.AdicionarItem(veiculo, reserva.CalcularDias());

        // O valor pago é sempre o total do contrato, e não o valor que veio da tela.
        pagamento.Valor = contrato.ValorTotal;
        pagamento.DataPagamento = agora;
        contrato.Pagamentos.Add(pagamento);

        reserva.Confirmar();
        veiculo.Status = StatusVeiculo.Alugado;

        // Contrato, item, pagamento, reserva e veículo são gravados juntos, em uma transação.
        _contratos.Adicionar(contrato);
        _unidadeDeTrabalho.Salvar();
        return Resultado<Contrato>.Ok(contrato);
    }

    public List<Contrato> ListarAtivos() => _contratos.ListarAtivos();

    public Contrato? ObterContrato(int contratoId) => _contratos.ObterPorId(contratoId);

    /// <summary>Retorna a primeira regra que impede a locação, ou null se todas forem atendidas.</summary>
    private string? Validar(Reserva reserva, decimal kmSaida, bool documentosConferidos,
                            Pagamento pagamento, DateTime agora)
    {
        var veiculo = reserva.Veiculo!;

        if (reserva.Status != StatusReserva.Agendada)
        {
            return "Só uma reserva agendada pode virar locação.";
        }

        if (reserva.Contrato is not null)
        {
            return "Esta reserva já tem um contrato.";
        }

        // Mesmo limite do ListarAgendadasVencidas: com o prazo vencido, a reserva expira.
        if (agora >= reserva.DataInicio.AddHours(Reserva.PrazoComparecimentoHoras))
        {
            return "O prazo de comparecimento desta reserva já terminou.";
        }

        if (!reserva.Cliente!.Ativo)
        {
            return "Este cliente está desativado.";
        }

        if (veiculo.Status != StatusVeiculo.Disponivel)
        {
            return veiculo.Status == StatusVeiculo.EmManutencao
                ? "Este veículo está em manutenção."
                : "Este veículo ainda está alugado.";
        }

        if (reserva.DataInicio.Date > agora.Date)
        {
            return "A locação só pode começar no dia da retirada.";
        }

        // Começando antes do horário, o veículo não pode estar reservado para outra pessoa até lá.
        // A própria reserva fica de fora, porque o período termina exatamente no início dela.
        if (reserva.DataInicio > agora &&
            _reservas.ExisteConflito(veiculo.Id, agora, reserva.DataInicio, agora))
        {
            return "Outra reserva usa este veículo antes do horário desta retirada.";
        }

        if (!documentosConferidos)
        {
            return "Confira o documento com foto e a CNH do condutor.";
        }

        if (kmSaida < veiculo.Quilometragem)
        {
            return $"A quilometragem de saída não pode ser menor que a atual do veículo ({veiculo.Quilometragem:N0} km).";
        }

        // A validação do Cartao e do Pix não depende do valor, então pode vir antes de calculá-lo.
        if (!pagamento.Validar())
        {
            return pagamento is Cartao
                ? "Informe a bandeira, os 4 últimos dígitos e as parcelas do cartão."
                : "Informe o código da transação Pix.";
        }

        return null;
    }
}
