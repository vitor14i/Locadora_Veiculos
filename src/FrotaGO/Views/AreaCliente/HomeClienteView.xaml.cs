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

namespace FrotaGO.Views.AreaCliente;

public partial class HomeClienteView : UserControl
{
    public HomeClienteView(string nomeCliente)
    {
        InitializeComponent();
        TextoSaudacao.Text = $"Olá, {NomeHelper.PrimeiroNome(nomeCliente)}";

        // TODO: substituir pelos dados vindos do controller.
        ExibirCards(CriarCardsDeExemplo());
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

    private static List<CardReservaItem> CriarCardsDeExemplo() =>
    [
        new()
        {
            ReservaId = 1, Titulo = "Locação", Status = "Ativa", Veiculo = "Chevrolet Onix · 2024",
            Detalhe1 = "Devolução: 15/11 às 14:00", Detalhe2 = "Pagamento: confirmado",
            Detalhe3 = "Contrato: nº 1024"
        },
        new()
        {
            ReservaId = 2, Titulo = "Reserva", Status = "Agendada", Veiculo = "Fiat Pulse · 2025",
            Detalhe1 = "Retirada: 16/11 às 15:00", Detalhe2 = "Devolução: 18/11 às 15:00",
            Detalhe3 = "Valor estimado: R$ 1.500,00"
        }
    ];
}
