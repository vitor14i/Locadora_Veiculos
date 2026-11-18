using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Models.Enums;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.AreaFuncionario;

public partial class RegistrarDevolucaoView : UserControl
{
    // TODO: confirmar estas regras com o grupo e registrá-las no documento de regras de negócio.
    private const decimal LimiteKmPorDiaria = 200m;
    private const decimal ValorPorKmExcedente = 1.00m;

    // ---------- Alterado na POO-18 ----------
    private readonly LocacaoController _locacaoController;

    /// <summary>Contrato com a reserva, o cliente e os itens. Fica nulo se não for encontrado.</summary>
    private readonly Contrato? _contrato;
    // ---------- Fim do trecho alterado ----------

    /// <summary>Tipos de multa cancelados pelo gerente, como "Atraso" ou "Avaria".</summary>
    private readonly HashSet<string> _multasCanceladas = [];

    private List<Multa> _multas = [];
    private decimal _desconto;
    private decimal _total;

    // ---------- Alterado na POO-18 ----------

    public RegistrarDevolucaoView(int contratoId)
    {
        InitializeComponent();
        _locacaoController = FabricaDeControllers.CriarLocacaoController();

        _contrato = _locacaoController.ObterContrato(contratoId);
        if (_contrato is null)
        {
            TextoMensagem.Text = "Locação não encontrada.";
            TextoMensagem.Visibility = Visibility.Visible;
            return;
        }

        var veiculo = MapeadorDeTela.CriarItem(Item);
        var cliente = MapeadorDeTela.CriarClienteItem(Reserva.Cliente!);

        var resumo = ResumoReservaItem.Criar(veiculo, Reserva.DataInicio, Reserva.DataFim, "Valor da locação");
        resumo.Linhas.InsertRange(0, [new("Cliente", cliente.Nome),
                                      new("Contrato", $"nº {_contrato.Id}"),
                                      new("Km de saída", $"{Item.KmInicial:N0}")]);
        Resumo.DataContext = resumo;

        CampoDataDevolucao.Text = FormatoHelper.DataHoraCampo(DateTime.Now);
        Recalcular();
    }

    // Só são usados depois de o contrato ser encontrado.
    private Reserva Reserva => _contrato!.Reserva!;

    /// <summary>A reserva tem um único veículo, então o contrato tem um único item.</summary>
    private ItemContrato Item => _contrato!.Itens.First();

    // ---------- Fim do trecho alterado ----------

    private static bool UsuarioEhGerente => Navegador.Usuario?.EhGerente == true;

    private void Campo_TextChanged(object sender, TextChangedEventArgs e) => Recalcular();

    private void Avaria_Alterada(object sender, RoutedEventArgs e) => Recalcular();

    /// <summary>Recalcula as multas e mostra o pagamento apenas quando há valor a cobrar.</summary>
    private void Recalcular()
    {
        // Os eventos também disparam durante o InitializeComponent.
        // Alterado na POO-18: sem contrato, não há o que calcular.
        if (!IsInitialized || _contrato is null) return;

        PainelAvaria.Visibility = CampoAvaria.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        PainelDesconto.Visibility = UsuarioEhGerente ? Visibility.Visible : Visibility.Collapsed;

        _multas = CalcularMultas();
        foreach (var multa in _multas.Where(multa => _multasCanceladas.Contains(Chave(multa))))
        {
            multa.Cancelar();
        }

        var subtotal = _multas.Where(multa => multa.Status != StatusMulta.Cancelada).Sum(multa => multa.Valor);
        _desconto = UsuarioEhGerente && FormatoHelper.TentarLerValor(CampoDesconto.Text, out var desconto)
            ? desconto
            : 0;
        _total = Math.Max(0, subtotal - _desconto);

        ListaMultas.ItemsSource = _multas
            .Select(multa => new LinhaMulta(
                Chave(multa),
                multa.Motivo,
                multa.Status == StatusMulta.Cancelada ? "Cancelada" : FormatoHelper.Moeda(multa.Valor),
                multa.Status == StatusMulta.Cancelada,
                UsuarioEhGerente))
            .ToList();
        TextoSemMultas.Visibility = _multas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TextoTotalMultas.Text = FormatoHelper.Moeda(_total);
        PainelPagamento.Visibility = _total > 0 ? Visibility.Visible : Visibility.Collapsed;
        SecaoPagamento.Valor = _total;
    }

    /// <summary>
    /// Usa as regras dos models Devolucao e Vistoria para calcular as multas.
    /// Alterado na POO-18: a diária, as diárias e o km de saída vêm do item do contrato.
    /// </summary>
    private List<Multa> CalcularMultas()
    {
        var multas = new List<Multa>();

        if (FormatoHelper.TentarLerDataHora(CampoDataDevolucao.Text, out var dataDevolucao))
        {
            var diasAtraso = new Devolucao { DataDevolucao = dataDevolucao }.CalcularDiasAtraso(Reserva.DataFim);
            if (diasAtraso > 0)
            {
                multas.Add(new Multa
                {
                    Motivo = $"Atraso · {FormatoHelper.Quantidade(diasAtraso, "dia", "dias")}",
                    Valor = diasAtraso * Item.ValorDiaria
                });
            }
        }

        if (FormatoHelper.TentarLerQuilometragem(CampoKmChegada.Text, out var kmChegada))
        {
            var kmRodados = new Vistoria { KmFinal = kmChegada }.CalcularKmRodados(Item.KmInicial);
            var kmExcedente = kmRodados - LimiteKmPorDiaria * Item.QuantidadeDias;
            if (kmExcedente > 0)
            {
                multas.Add(new Multa
                {
                    Motivo = $"Km excedente · {kmExcedente:N0} km",
                    Valor = kmExcedente * ValorPorKmExcedente
                });
            }
        }

        if (CampoAvaria.IsChecked == true &&
            FormatoHelper.TentarLerValor(CampoValorAvaria.Text, out var valorAvaria) && valorAvaria > 0)
        {
            multas.Add(new Multa { Motivo = "Avaria", Valor = valorAvaria });
        }

        return multas;
    }

    /// <summary>Identifica o tipo da multa pelo início do motivo, como "Atraso" ou "Km excedente".</summary>
    private static string Chave(Multa multa) => multa.Motivo.Split(" · ")[0];

    /// <summary>Cancela ou restaura uma multa. Só aparece para o gerente.</summary>
    private void ListaMultas_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not FrameworkElement { DataContext: LinhaMulta linha }) return;

        if (!_multasCanceladas.Remove(linha.Chave))
        {
            _multasCanceladas.Add(linha.Chave);
        }

        Recalcular();
    }

    // ---------- Alterado na POO-18 ----------

    /// <summary>O botão fica desabilitado no .xaml até a POO-19.</summary>
    private void ConcluirDevolucao_Click(object sender, RoutedEventArgs e)
    {
        if (_contrato is null) return;

        var erro = Validar(out _);
        if (erro is not null)
        {
            TextoMensagem.Text = erro;
            TextoMensagem.Visibility = Visibility.Visible;
            return;
        }

        // TODO (POO-19): trocar pelo controller: cria a Devolucao, a Vistoria, as Multas e o Pagamento,
        // aplica o desconto no Contrato, encerra o contrato e libera o veículo ou o envia para manutenção.
        MessageBox.Show("O registro da devolução ainda não está disponível.", "Registrar devolução",
                        MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private string? Validar(out DateTime dataDevolucao)
    {
        if (!FormatoHelper.TentarLerDataHora(CampoDataDevolucao.Text, out dataDevolucao))
        {
            return "Informe a data da devolução no formato dd/mm/aaaa hh:mm.";
        }

        // A locação pode ter começado antes do horário da reserva, então vale o início do contrato.
        if (dataDevolucao < _contrato!.DataGeracao)
        {
            return "A devolução não pode ser antes do início da locação.";
        }

        if (!FormatoHelper.TentarLerQuilometragem(CampoKmChegada.Text, out var kmChegada) ||
            kmChegada < Item.KmInicial)
        {
            return "Informe uma quilometragem de chegada maior ou igual à de saída.";
        }

        // ---------- Fim do trecho alterado ----------

        if (CampoAvaria.IsChecked == true &&
            (!FormatoHelper.TentarLerValor(CampoValorAvaria.Text, out var valorAvaria) || valorAvaria <= 0))
        {
            return "Informe o valor da avaria.";
        }

        var subtotal = _multas.Where(multa => multa.Status != StatusMulta.Cancelada).Sum(multa => multa.Valor);
        if (_desconto > subtotal)
        {
            return "O desconto não pode ser maior que o total das multas.";
        }

        return _total > 0 ? SecaoPagamento.Validar() : null;
    }

    private void Voltar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new LocacoesAtivasView());
}