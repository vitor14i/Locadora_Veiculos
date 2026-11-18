using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Models;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.AreaCliente;

public partial class MinhasReservasView : UserControl
{
    private const string FiltroTodas = "Todas";

    // As chaves são os textos dos filtros no .xaml; os valores, as constantes de status.
    private static readonly Dictionary<string, string[]> StatusPorFiltro = new()
    {
        ["Agendadas"] = [MapeadorDeTela.StatusAgendada],
        ["Ativas"] = [MapeadorDeTela.StatusAtiva],
        ["Concluídas"] = [MapeadorDeTela.StatusConcluida],
        ["Canceladas"] = [MapeadorDeTela.StatusCancelada, MapeadorDeTela.StatusExpirada]
    };

    private readonly ReservaController _reservaController;

    /// <summary>Carregadas uma vez. Os filtros só mudam o que é exibido.</summary>
    private readonly List<Reserva> _reservas;

    private string _filtro = FiltroTodas;

    public MinhasReservasView(string? mensagemSucesso = null)
    {
        InitializeComponent();
        _reservaController = FabricaDeControllers.CriarReservaController();
        _reservas = _reservaController.ListarDoCliente(Navegador.Usuario!.Id);

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
        var cards = _reservas
            .Where(reserva => _filtro == FiltroTodas ||
                              StatusPorFiltro[_filtro].Contains(MapeadorDeTela.TextoStatus(reserva)))
            .OrderBy(reserva => MapeadorDeTela.TextoStatus(reserva) is MapeadorDeTela.StatusAtiva
                                                                     or MapeadorDeTela.StatusAgendada ? 0 : 1)
            .ThenByDescending(reserva => reserva.DataInicio)
            .Select(MapeadorDeTela.CriarCard)
            .ToList();

        ListaCards.ItemsSource = cards;
        TextoVazio.Text = _reservas.Count > 0
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
