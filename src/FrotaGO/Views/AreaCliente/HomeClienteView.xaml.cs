using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FrotaGO.Helpers;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.AreaCliente;

public partial class HomeClienteView : UserControl
{
    public HomeClienteView(string nomeCliente)
    {
        InitializeComponent();
        TextoSaudacao.Text = $"Olá, {NomeHelper.PrimeiroNome(nomeCliente)}";

        // TODO: substituir pelos dados vindos do controller.
        var cards = DadosDeExemplo.Reservas
        .Where(reserva => reserva.Status is "Ativa" or "Agendada")
        .OrderBy(reserva => reserva.Retirada)
        .Take(2)
        .Select(DadosDeExemplo.CriarCard)
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
        var ativas = cards.Count(card => card.Status == "Ativa");
        var agendadas = cards.Count(card => card.Status == "Agendada");

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
