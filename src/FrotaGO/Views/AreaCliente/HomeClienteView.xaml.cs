using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Helpers;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.AreaCliente;

public partial class HomeClienteView : UserControl
{
    private readonly ReservaController _reservaController;

    public HomeClienteView(string nomeCliente)
    {
        InitializeComponent();
        _reservaController = FabricaDeControllers.CriarReservaController();
        TextoSaudacao.Text = $"Olá, {NomeHelper.PrimeiroNome(nomeCliente)}";

        var cards = _reservaController.ListarDoCliente(Navegador.Usuario!.Id)
            .Where(reserva => MapeadorDeTela.TextoStatus(reserva) is MapeadorDeTela.StatusAtiva
                                                                  or MapeadorDeTela.StatusAgendada)
            .OrderBy(reserva => reserva.DataInicio)
            .Take(2)
            .Select(MapeadorDeTela.CriarCard)
            .ToList();

        ExibirCards(cards);
    }

    private void ExibirCards(IReadOnlyList<CardReservaItem> cards)
    {
        ListaCards.ItemsSource = cards;
        TextoResumo.Text = MontarResumo(cards);
        TextoVazio.Visibility = cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string MontarResumo(IReadOnlyList<CardReservaItem> cards)
    {
        var ativas = cards.Count(card => card.Status == MapeadorDeTela.StatusAtiva);
        var agendadas = cards.Count(card => card.Status == MapeadorDeTela.StatusAgendada);

        var partes = new List<string>();
        if (ativas > 0) partes.Add(ativas == 1 ? "1 locação ativa" : $"{ativas} locações ativas");
        if (agendadas > 0) partes.Add(agendadas == 1 ? "1 reserva agendada" : $"{agendadas} reservas agendadas");

        return partes.Count == 0
            ? "Escolha um veículo para fazer sua primeira reserva."
            : $"Você tem {string.Join(" e ", partes)}.";
    }

    private void ReservarVeiculo_Click(object sender, RoutedEventArgs e) =>
    Navegador.IrPara(new BuscaVeiculosView());

    private void MinhasReservas_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new MinhasReservasView());

    private void ListaCards_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: CardReservaItem card })
        {
            Navegador.IrPara(new DetalhesReservaView(card.ReservaId));
        }
    }
}
