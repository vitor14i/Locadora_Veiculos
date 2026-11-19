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
    private readonly DevolucaoController _devolucaoController;

    /// <summary>Contrato com a reserva, o cliente e os itens. Fica nulo se não for encontrado.</summary>
    private readonly Contrato? _contrato;

    /// <summary>Tipos de multa cancelados pelo gerente, como "Atraso" ou "Avaria".</summary>
    private readonly HashSet<string> _multasCanceladas = [];

    /// <summary>Total a pagar na prévia. Define se o pagamento aparece.</summary>
    private decimal _total;

    public RegistrarDevolucaoView(int contratoId)
    {
        InitializeComponent();
        _devolucaoController = FabricaDeControllers.CriarDevolucaoController();

        _contrato = _devolucaoController.ObterContrato(contratoId);
        if (_contrato is null)
        {
            MostrarErro("Locação não encontrada.");
            return;
        }

        var veiculo = MapeadorDeTela.CriarItem(Item);
        var cliente = MapeadorDeTela.CriarClienteItem(Reserva.Cliente!);

        var resumo = ResumoReservaItem.Criar(veiculo, Reserva.DataInicio, Reserva.DataFim, "Valor da locação");
        resumo.Linhas.InsertRange(0, [new("Cliente", cliente.Nome),
                                      new("Contrato", $"nº {_contrato.Id}"),
                                      new("Km de saída", $"{Item.KmInicial:N0}")]);
        Resumo.DataContext = resumo;

        // Só o gerente registra manutenções.
        CampoManutencao.Visibility = UsuarioEhGerente ? Visibility.Visible : Visibility.Collapsed;

        CampoDataDevolucao.Text = FormatoHelper.DataHoraCampo(DateTime.Now);
        Recalcular();
    }

    // Só são usados depois de o contrato ser encontrado.
    private Reserva Reserva => _contrato!.Reserva!;

    /// <summary>A reserva tem um único veículo, então o contrato tem um único item.</summary>
    private ItemContrato Item => _contrato!.Itens.First();

    private static bool UsuarioEhGerente => Navegador.Usuario?.EhGerente == true;

    private void Campo_TextChanged(object sender, TextChangedEventArgs e) => Recalcular();

    private void Avaria_Alterada(object sender, RoutedEventArgs e) => Recalcular();

    /// <summary>Mostra a prévia das multas, calculada pelo controller, e o pagamento apenas quando há valor a cobrar.</summary>
    private void Recalcular()
    {
        // Os eventos também disparam durante o InitializeComponent.
        if (!IsInitialized || _contrato is null) return;

        PainelAvaria.Visibility = CampoAvaria.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        PainelDesconto.Visibility = UsuarioEhGerente ? Visibility.Visible : Visibility.Collapsed;

        // Campo vazio ou inválido vira null, e o controller não gera a multa correspondente.
        var multas = _devolucaoController.CalcularMultas(_contrato, LerDataDevolucao(), LerKmChegada(),
                                                         LerValorAvaria());
        foreach (var multa in multas.Where(multa => _multasCanceladas.Contains(DevolucaoController.TipoDe(multa))))
        {
            multa.Cancelar();
        }

        // A prévia segue a mesma conta do controller, que refaz tudo ao registrar.
        var subtotal = multas.Where(multa => multa.Status != StatusMulta.Cancelada).Sum(multa => multa.Valor);
        _total = Math.Max(0, subtotal - LerDesconto());

        ListaMultas.ItemsSource = multas
            .Select(multa => new LinhaMulta(
                DevolucaoController.TipoDe(multa),
                multa.Motivo,
                multa.Status == StatusMulta.Cancelada ? "Cancelada" : FormatoHelper.Moeda(multa.Valor),
                multa.Status == StatusMulta.Cancelada,
                UsuarioEhGerente))
            .ToList();
        TextoSemMultas.Visibility = multas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TextoTotalMultas.Text = FormatoHelper.Moeda(_total);
        PainelPagamento.Visibility = _total > 0 ? Visibility.Visible : Visibility.Collapsed;
        SecaoPagamento.Valor = _total;
    }

    // Leitura dos campos para a prévia. Campo vazio ou inválido vira null (ou zero, no desconto).
    private DateTime? LerDataDevolucao() =>
        FormatoHelper.TentarLerDataHora(CampoDataDevolucao.Text, out var data) ? data : null;

    private decimal? LerKmChegada() =>
        FormatoHelper.TentarLerQuilometragem(CampoKmChegada.Text, out var km) ? km : null;

    private decimal? LerValorAvaria() =>
        CampoAvaria.IsChecked == true && FormatoHelper.TentarLerValor(CampoValorAvaria.Text, out var valor)
            ? valor
            : null;

    /// <summary>Só o gerente vê o campo de desconto.</summary>
    private decimal LerDesconto() =>
        UsuarioEhGerente && FormatoHelper.TentarLerValor(CampoDesconto.Text, out var desconto) ? desconto : 0;

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

    private void ConcluirDevolucao_Click(object sender, RoutedEventArgs e)
    {
        if (_contrato is null) return;

        var erro = Validar(out var dataDevolucao, out var kmChegada);
        if (erro is not null)
        {
            MostrarErro(erro);
            return;
        }

        // As regras de negócio, inclusive as do gerente, ficam no controller.
        var dados = new DadosDevolucao(
            _contrato.Id,
            dataDevolucao,
            kmChegada,
            CampoObservacoes.Text,
            CampoAvaria.IsChecked == true,
            LerValorAvaria() ?? 0,
            _multasCanceladas.ToList(),
            LerDesconto(),
            CampoManutencao.IsChecked == true,
            _total > 0 ? SecaoPagamento.CriarPagamento() : null);

        var resultado = _devolucaoController.Registrar(dados, Navegador.Usuario?.Id ?? 0);
        if (!resultado.Sucesso)
        {
            MostrarErro(resultado.Erro);
            return;
        }

        var destino = dados.EnviarParaManutencao
            ? "O veículo foi enviado para manutenção."
            : "O veículo está disponível.";
        MessageBox.Show($"Devolução registrada. Contrato nº {_contrato.Id} encerrado. {destino}",
                        "Registrar devolução", MessageBoxButton.OK, MessageBoxImage.Information);
        Navegador.IrPara(new LocacoesAtivasView());
    }

    /// <summary>
    /// Só o preenchimento: o formato da data, da quilometragem, da avaria e do desconto,
    /// e os dados do pagamento. As regras de negócio ficam no controller.
    /// </summary>
    private string? Validar(out DateTime dataDevolucao, out decimal kmChegada)
    {
        kmChegada = 0;

        if (!FormatoHelper.TentarLerDataHora(CampoDataDevolucao.Text, out dataDevolucao))
        {
            return "Informe a data da devolução no formato dd/mm/aaaa hh:mm.";
        }

        if (!FormatoHelper.TentarLerQuilometragem(CampoKmChegada.Text, out kmChegada))
        {
            return "Informe a quilometragem de chegada.";
        }

        if (CampoAvaria.IsChecked == true && LerValorAvaria() is null)
        {
            return "Informe o valor da avaria.";
        }

        if (UsuarioEhGerente && !string.IsNullOrWhiteSpace(CampoDesconto.Text) &&
            !FormatoHelper.TentarLerValor(CampoDesconto.Text, out _))
        {
            return "Informe um valor de desconto válido.";
        }

        return _total > 0 ? SecaoPagamento.Validar() : null;
    }

    private void MostrarErro(string texto)
    {
        TextoMensagem.Text = texto;
        TextoMensagem.Visibility = Visibility.Visible;
    }

    private void Voltar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new LocacoesAtivasView());
}
