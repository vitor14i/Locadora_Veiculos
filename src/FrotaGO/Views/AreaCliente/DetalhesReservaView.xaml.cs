using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Models;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.AreaCliente;

public partial class DetalhesReservaView : UserControl
{
    private readonly ReservaController _reservaController;
    private readonly int _reservaId;

    public DetalhesReservaView(int reservaId)
    {
        InitializeComponent();
        _reservaController = FabricaDeControllers.CriarReservaController();
        _reservaId = reservaId;

        // Só encontra as reservas do cliente logado.
        var reserva = _reservaController.ObterDoCliente(reservaId, Navegador.Usuario!.Id);
        if (reserva is null)
        {
            TextoNumero.Text = "Reserva não encontrada.";
            Resumo.Visibility = Visibility.Collapsed;
            ConfigurarAcoes(string.Empty);
            return;
        }

        // O selo de status liga em {Binding Status}: usa o item de tela, com o status já em texto.
        var card = MapeadorDeTela.CriarCard(reserva);

        TextoNumero.Text = $"Reserva nº {reserva.Id}";
        PainelStatus.DataContext = card;
        Resumo.DataContext = MontarResumo(reserva);
        ConfigurarAcoes(card.Status);
    }

    private static ResumoReservaItem MontarResumo(Reserva reserva)
    {
        if (reserva.Contrato is null)
        {
            var estimado = MapeadorDeTela.CriarItem(reserva.Veiculo!).ParaPeriodo(reserva.DataInicio, reserva.DataFim);
            return ResumoReservaItem.Criar(estimado, reserva.DataInicio, reserva.DataFim, "Valor estimado");
        }

        // Com contrato, vale o valor gravado na retirada, e não o preço atual do veículo.
        var itemContrato = reserva.Contrato.Itens.First(item => item.VeiculoId == reserva.VeiculoId);
        var resumo = ResumoReservaItem.Criar(MapeadorDeTela.CriarItem(itemContrato),
                                             reserva.DataInicio, reserva.DataFim, "Valor total");
        resumo.Linhas.Insert(0, new DetalheLinha("Contrato", $"nº {reserva.Contrato.Id}"));
        return resumo;
    }

    /// <summary>Mostra apenas as ações permitidas para o status da reserva.</summary>
    private void ConfigurarAcoes(string status)
    {
        var agendada = status == MapeadorDeTela.StatusAgendada;

        BotaoCancelar.Visibility = agendada ? Visibility.Visible : Visibility.Collapsed;
        BotaoNovaReserva.Visibility = status is MapeadorDeTela.StatusCancelada or MapeadorDeTela.StatusExpirada
            ? Visibility.Visible
            : Visibility.Collapsed;

        if (agendada)
        {
            TextoAviso.Text = "O pagamento é feito na retirada. Leve um documento com foto e sua CNH.";
            TextoAviso.Visibility = Visibility.Visible;
        }
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        var resposta = MessageBox.Show(
            "Tem certeza que deseja cancelar esta reserva?", "Cancelar reserva",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (resposta != MessageBoxResult.Yes) return;

        // O controller confere se a reserva é do cliente e se ainda pode ser cancelada.
        var resultado = _reservaController.Cancelar(_reservaId, Navegador.Usuario!.Id);
        if (!resultado.Sucesso)
        {
            MessageBox.Show(resultado.Erro, "Cancelar reserva",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Navegador.IrPara(new MinhasReservasView("Reserva cancelada."));
    }

    private void Voltar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new MinhasReservasView());

    private void NovaReserva_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new BuscaVeiculosView());
}
