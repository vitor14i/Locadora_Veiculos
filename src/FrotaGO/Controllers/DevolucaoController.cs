using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Models.Enums;
using FrotaGO.Repositories.Interfaces;

namespace FrotaGO.Controllers;

/// <summary>Prévia das multas e registro da devolução: vistoria, multas, pagamento e encerramento do contrato.</summary>
public class DevolucaoController
{
    /// <summary>Quilômetros incluídos em cada diária. O que passar disso é km excedente.</summary>
    private const decimal LimiteKmPorDiaria = 200m;

    /// <summary>Valor cobrado por km excedente.</summary>
    private const decimal ValorPorKmExcedente = 1.00m;

    // Início do motivo de cada multa. É também a chave que a tela usa para cancelar um tipo.
    private const string TipoAtraso = "Atraso";
    private const string TipoKmExcedente = "Km excedente";
    private const string TipoAvaria = "Avaria";
    private const string SeparadorMotivo = " · ";

    private const string DescricaoManutencao = "Revisão após devolução";

    private readonly IContratoRepository _contratos;
    private readonly IDevolucaoRepository _devolucoes;
    private readonly IManutencaoRepository _manutencoes;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;

    public DevolucaoController(IContratoRepository contratos, IDevolucaoRepository devolucoes,
                               IManutencaoRepository manutencoes, IUsuarioRepository usuarios,
                               IUnidadeDeTrabalho unidadeDeTrabalho)
    {
        _contratos = contratos;
        _devolucoes = devolucoes;
        _manutencoes = manutencoes;
        _usuarios = usuarios;
        _unidadeDeTrabalho = unidadeDeTrabalho;
    }

    /// <summary>Contrato com a reserva, o cliente, os itens com os veículos e os pagamentos. Rastreado.</summary>
    public Contrato? ObterContrato(int contratoId) => _contratos.ObterPorId(contratoId);

    /// <summary>Tipo da multa pelo início do motivo, como "Atraso" ou "Km excedente". É a chave do cancelamento.</summary>
    public static string TipoDe(Multa multa) => multa.Motivo.Split(SeparadorMotivo)[0];

    /// <summary>
    /// Calcula as multas sem acessar o banco. Usa a diária, as diárias e o km de saída gravados
    /// nos itens do contrato. Um valor nulo não gera a multa correspondente.
    /// O mesmo km de chegada vale para todos os itens, e cada tipo gera no máximo uma multa.
    /// </summary>
    public List<Multa> CalcularMultas(Contrato contrato, DateTime? dataDevolucao, decimal? kmChegada,
                                      decimal? valorAvaria)
    {
        var multas = new List<Multa>();

        if (dataDevolucao is { } data)
        {
            var diasAtraso = new Devolucao { DataDevolucao = data }.CalcularDiasAtraso(contrato.Reserva!.DataFim);
            if (diasAtraso > 0)
            {
                multas.Add(new Multa
                {
                    Motivo = Motivo(TipoAtraso, FormatoHelper.Quantidade(diasAtraso, "dia", "dias")),
                    // Uma diária por dia de atraso, somando a diária de cada veículo do contrato.
                    Valor = diasAtraso * contrato.Itens.Sum(item => item.ValorDiaria)
                });
            }
        }

        if (kmChegada is { } km)
        {
            var kmExcedente = contrato.Itens.Sum(item => CalcularKmExcedente(item, km));
            if (kmExcedente > 0)
            {
                multas.Add(new Multa
                {
                    Motivo = Motivo(TipoKmExcedente, $"{kmExcedente:N0} km"),
                    Valor = kmExcedente * ValorPorKmExcedente
                });
            }
        }

        if (valorAvaria is > 0)
        {
            multas.Add(new Multa { Motivo = TipoAvaria, Valor = valorAvaria.Value });
        }

        return multas;
    }

    /// <summary>
    /// Registra a devolução: cria a devolução, uma vistoria por item, as multas e o pagamento,
    /// aplica o desconto, encerra o contrato e libera o veículo ou o envia para manutenção.
    /// Grava tudo com um único Salvar().
    /// </summary>
    public Resultado Registrar(DadosDevolucao dados, int usuarioId)
    {
        var agora = DateTime.Now;

        // Rastreado, com a reserva, os itens com os veículos e os pagamentos.
        var contrato = _contratos.ObterPorId(dados.ContratoId);
        if (contrato is null)
        {
            return Resultado.Falha("Locação não encontrada.");
        }

        // As multas são calculadas de novo aqui, sem confiar na prévia da tela.
        // São objetos novos, ainda fora do contexto, então cancelar agora não altera nada rastreado.
        var multas = CalcularMultas(contrato, dados.DataDevolucao, dados.KmChegada,
                                    dados.TemAvaria ? dados.ValorAvaria : null);
        foreach (var multa in multas.Where(multa => dados.TiposCancelados.Contains(TipoDe(multa))))
        {
            multa.Cancelar();
        }

        var naoCanceladas = multas.Where(multa => multa.Status != StatusMulta.Cancelada).ToList();
        var subtotal = naoCanceladas.Sum(multa => multa.Valor);
        var total = subtotal - dados.Desconto;

        // Todas as validações vêm antes de qualquer alteração nos objetos rastreados.
        var erro = Validar(contrato, dados, usuarioId, subtotal, total, agora);
        if (erro is not null)
        {
            return Resultado.Falha(erro);
        }

        var observacoes = dados.Observacoes.Trim();
        var devolucao = new Devolucao
        {
            ContratoId = contrato.Id,
            DataDevolucao = dados.DataDevolucao,
            Observacoes = observacoes
        };

        // Uma vistoria por veículo do contrato.
        foreach (var item in contrato.Itens)
        {
            devolucao.Vistorias.Add(new Vistoria
            {
                ItemContratoId = item.Id,
                KmFinal = dados.KmChegada,
                TemAvarias = dados.TemAvaria,
                Observacoes = observacoes
            });
        }

        devolucao.Multas.AddRange(multas);
        contrato.AplicarDesconto(dados.Desconto);

        // As não canceladas ficam pagas: pelo pagamento ou, quando o desconto zera o total, quitadas por ele.
        foreach (var multa in naoCanceladas)
        {
            multa.Pagar();
        }

        if (total > 0)
        {
            // O valor pago é sempre o calculado aqui, e não o valor que veio da tela.
            var pagamento = dados.Pagamento!;
            pagamento.Valor = total;
            pagamento.DataPagamento = agora;
            contrato.Pagamentos.Add(pagamento);
        }

        contrato.Encerrar();

        foreach (var veiculo in contrato.Itens.Select(item => item.Veiculo!))
        {
            veiculo.Quilometragem = dados.KmChegada;

            if (dados.EnviarParaManutencao)
            {
                _manutencoes.Adicionar(new Manutencao
                {
                    VeiculoId = veiculo.Id,
                    GerenteId = usuarioId,
                    Descricao = DescricaoManutencao,
                    DataInicio = agora
                });
                veiculo.Status = StatusVeiculo.EmManutencao;
            }
            else
            {
                veiculo.Status = StatusVeiculo.Disponivel;
            }
        }

        // Devolução, vistorias, multas, pagamento, contrato, veículos e manutenção vão juntos, em uma transação.
        _devolucoes.Adicionar(devolucao);
        _unidadeDeTrabalho.Salvar();
        return Resultado.Ok();
    }

    /// <summary>Retorna a primeira regra que impede a devolução, ou null se todas forem atendidas.</summary>
    private string? Validar(Contrato contrato, DadosDevolucao dados, int usuarioId,
                            decimal subtotal, decimal total, DateTime agora)
    {
        if (contrato.Status != StatusContrato.Ativo)
        {
            return "Só uma locação ativa pode ser devolvida.";
        }

        if (_devolucoes.ExisteParaContrato(contrato.Id))
        {
            return "Esta locação já foi devolvida.";
        }

        // A locação pode ter começado antes do horário da reserva, então vale o início do contrato.
        if (dados.DataDevolucao < contrato.DataGeracao)
        {
            return "A devolução não pode ser antes do início da locação.";
        }

        if (dados.DataDevolucao > agora)
        {
            return "A data da devolução não pode ser futura.";
        }

        var kmSaida = contrato.Itens.Max(item => item.KmInicial);
        if (dados.KmChegada < kmSaida)
        {
            return $"A quilometragem de chegada não pode ser menor que a de saída ({kmSaida:N0} km).";
        }

        if (dados.TemAvaria && dados.ValorAvaria <= 0)
        {
            return "Informe o valor da avaria.";
        }

        // Conferido pelo banco, e não pela tela.
        var usuario = _usuarios.ObterPorId(usuarioId);
        if (usuario is not Funcionario { Ativo: true })
        {
            return "Somente um funcionário ativo pode registrar a devolução.";
        }

        var usaAcaoDeGerente = dados.TiposCancelados.Count > 0 || dados.Desconto != 0 || dados.EnviarParaManutencao;
        if (usaAcaoDeGerente && usuario is not Gerente)
        {
            return "Somente o gerente pode cancelar multas, dar desconto ou enviar o veículo para manutenção.";
        }

        if (dados.Desconto < 0)
        {
            return "O desconto não pode ser negativo.";
        }

        if (dados.Desconto > subtotal)
        {
            return "O desconto não pode ser maior que o total das multas.";
        }

        if (total > 0)
        {
            if (dados.Pagamento is null)
            {
                return "Informe o pagamento das multas.";
            }

            // A validação do Cartao e do Pix não depende do valor, então pode vir antes de defini-lo.
            if (!dados.Pagamento.Validar())
            {
                return dados.Pagamento is Cartao
                    ? "Informe a bandeira, os 4 últimos dígitos e as parcelas do cartão."
                    : "Informe o código da transação Pix.";
            }
        }

        return null;
    }

    private static string Motivo(string tipo, string detalhe) => $"{tipo}{SeparadorMotivo}{detalhe}";

    /// <summary>Km acima do limite das diárias do item. Usa o km de saída gravado no item.</summary>
    private static decimal CalcularKmExcedente(ItemContrato item, decimal kmChegada)
    {
        var kmRodados = new Vistoria { KmFinal = kmChegada }.CalcularKmRodados(item.KmInicial);
        return Math.Max(0, kmRodados - LimiteKmPorDiaria * item.QuantidadeDias);
    }
}
