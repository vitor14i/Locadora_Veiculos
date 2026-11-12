using System.Windows;
using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.AreaFuncionario;

public partial class RegistrarDevolucaoView : UserControl
{
    // TODO: confirmar estas regras com o grupo e registrá-las no documento de regras de negócio.
    private const decimal LimiteKmPorDiaria = 200m;
    private const decimal ValorPorKmExcedente = 1.00m;

    private readonly ReservaExemplo _reserva;
    private List<Multa> _multas = [];

    public RegistrarDevolucaoView(int reservaId)
    {
        InitializeComponent();

        // TODO: buscar a locação pelo controller.
        _reserva = DadosDeExemplo.Buscar(reservaId);

        var resumo = ResumoReservaItem.Criar(_reserva.Veiculo, _reserva.Retirada, _reserva.Devolucao,
                                             "Valor da locação");
        resumo.Linhas.InsertRange(0, [new("Cliente", _reserva.Cliente.Nome),
                                      new("Contrato", $"nº {_reserva.Contrato}"),
                                      new("Km de saída", $"{_reserva.KmSaida:N0}")]);
        Resumo.DataContext = resumo;

        CampoDataDevolucao.Text = FormatoHelper.DataHoraCampo(DateTime.Now);
        Recalcular();
    }

    private void Campo_TextChanged(object sender, TextChangedEventArgs e) => Recalcular();

    private void Avaria_Alterada(object sender, RoutedEventArgs e) => Recalcular();

    /// <summary>Recalcula as multas e mostra o pagamento apenas quando há valor a cobrar.</summary>
    private void Recalcular()
    {
        // Os eventos também disparam durante o InitializeComponent.
        if (!IsInitialized) return;

        PainelAvaria.Visibility = CampoAvaria.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

        _multas = CalcularMultas();
        var total = _multas.Sum(multa => multa.Valor);

        ListaMultas.ItemsSource = _multas
            .Select(multa => new DetalheLinha(multa.Motivo, FormatoHelper.Moeda(multa.Valor)))
            .ToList();
        TextoSemMultas.Visibility = _multas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TextoTotalMultas.Text = FormatoHelper.Moeda(total);
        PainelPagamento.Visibility = total > 0 ? Visibility.Visible : Visibility.Collapsed;
        SecaoPagamento.Valor = total;
    }

    /// <summary>Usa as regras dos models Devolucao e Vistoria para calcular as multas.</summary>
    private List<Multa> CalcularMultas()
    {
        var multas = new List<Multa>();

        if (FormatoHelper.TentarLerDataHora(CampoDataDevolucao.Text, out var dataDevolucao))
        {
            var diasAtraso = new Devolucao { DataDevolucao = dataDevolucao }.CalcularDiasAtraso(_reserva.Devolucao);
            if (diasAtraso > 0)
            {
                multas.Add(new Multa
                {
                    Motivo = $"Atraso · {FormatoHelper.Quantidade(diasAtraso, "dia", "dias")}",
                    Valor = diasAtraso * _reserva.Veiculo.Diaria
                });
            }
        }

        if (FormatoHelper.TentarLerQuilometragem(CampoKmChegada.Text, out var kmChegada) &&
            _reserva.KmSaida is decimal kmSaida)
        {
            var kmRodados = new Vistoria { KmFinal = kmChegada }.CalcularKmRodados(kmSaida);
            var kmExcedente = kmRodados - LimiteKmPorDiaria * _reserva.Veiculo.Diarias;
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

    private void ConcluirDevolucao_Click(object sender, RoutedEventArgs e)
    {
        var erro = Validar(out var dataDevolucao);
        if (erro is not null)
        {
            TextoMensagem.Text = erro;
            TextoMensagem.Visibility = Visibility.Visible;
            return;
        }

        // TODO: trocar pelo controller: cria a Devolucao, a Vistoria, as Multas e o Pagamento,
        // encerra o contrato e libera o veículo ou o envia para manutenção.
        DadosDeExemplo.ConcluirDevolucao(_reserva.Id, dataDevolucao);

        var total = _multas.Sum(multa => multa.Valor);
        var mensagem = total > 0
            ? $"Devolução registrada. Multas: {FormatoHelper.Moeda(total)}, pagas por {SecaoPagamento.Descricao}."
            : "Devolução registrada sem multas.";
        if (CampoManutencao.IsChecked == true)
        {
            mensagem += "\nO veículo foi enviado para manutenção.";
        }

        MessageBox.Show(mensagem, "Devolução registrada", MessageBoxButton.OK, MessageBoxImage.Information);
        Navegador.IrParaHome();
    }

    private string? Validar(out DateTime dataDevolucao)
    {
        if (!FormatoHelper.TentarLerDataHora(CampoDataDevolucao.Text, out dataDevolucao))
        {
            return "Informe a data da devolução no formato dd/mm/aaaa hh:mm.";
        }

        if (dataDevolucao < _reserva.Retirada)
        {
            return "A devolução não pode ser antes da retirada.";
        }

        if (!FormatoHelper.TentarLerQuilometragem(CampoKmChegada.Text, out var kmChegada) ||
            kmChegada < (_reserva.KmSaida ?? 0))
        {
            return "Informe uma quilometragem de chegada maior ou igual à de saída.";
        }

        if (CampoAvaria.IsChecked == true &&
            (!FormatoHelper.TentarLerValor(CampoValorAvaria.Text, out var valorAvaria) || valorAvaria <= 0))
        {
            return "Informe o valor da avaria.";
        }

        return _multas.Count > 0 ? SecaoPagamento.Validar() : null;
    }

    private void Voltar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new LocacoesAtivasView());
}