using System.Windows;
using System.Windows.Controls;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.AreaCliente;

public partial class MinhasReservasView : UserControl
{
    private const string FiltroTodas = "Todas";

    private static readonly Dictionary<string, string[]> StatusPorFiltro = new()
    {
        ["Agendadas"] = ["Agendada"],
        ["Ativas"] = ["Ativa"],
        ["Concluídas"] = ["Concluída"],
        ["Canceladas"] = ["Cancelada", "Expirada"]
    };

    private string _filtro = FiltroTodas;

    public MinhasReservasView(string? mensagemSucesso = null)
    {
        InitializeComponent();

        if (mensagemSucesso is not null)
        {
            TextoMensagem.Text = mensagemSucesso;
            TextoMensagem.Visibility = Visibility.Visible;
        }

        ExibirReservas();
    }

    private void Filtro_Checked(object sender, RoutedEventArgs e)
    {
        // O evento também dispara durante o InitializeComponent.
        if (!IsInitialized) return;

        _filtro = (string)((RadioButton)sender).Content;
        ExibirReservas();
    }

    private void ExibirReservas()
    {
        // TODO: trocar pelas reservas do cliente logado vindas do controller.
        var cards = DadosDeExemplo.ReservasDoClienteAtual()
            .Where(reserva => _filtro == FiltroTodas || StatusPorFiltro[_filtro].Contains(reserva.Status))
            .OrderBy(reserva => reserva.Status is "Ativa" or "Agendada" ? 0 : 1)
            .ThenByDescending(reserva => reserva.Retirada)
            .Select(DadosDeExemplo.CriarCard)
            .ToList();

        ListaCards.ItemsSource = cards;
        TextoVazio.Text = DadosDeExemplo.ReservasDoClienteAtual().Any()
            ? "Nenhuma reserva com esse status."
            : "Você ainda não tem reservas.";
        TextoVazio.Visibility = cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ListaCards_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: CardReservaItem card })
        {
            Navegador.IrPara(new DetalhesReservaView(card.ReservaId));
        }
    }

    private void ReservarVeiculo_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new BuscaVeiculosView());
}