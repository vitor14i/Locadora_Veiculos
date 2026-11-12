using System.Windows;
using System.Windows.Controls;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.AreaCliente;

public partial class DetalhesReservaView : UserControl
{
    private readonly ReservaExemplo _reserva;

    public DetalhesReservaView(int reservaId)
    {
        InitializeComponent();

        // TODO: buscar a reserva pelo controller.
        _reserva = DadosDeExemplo.Reservas.First(reserva => reserva.Id == reservaId);

        TextoNumero.Text = $"Reserva nº {_reserva.Id}";
        PainelStatus.DataContext = _reserva;
        Resumo.DataContext = MontarResumo(_reserva);
        ConfigurarAcoes(_reserva.Status);
    }

    private static ResumoReservaItem MontarResumo(ReservaExemplo reserva)
    {
        var rotuloTotal = reserva.Status is "Ativa" or "Concluída" ? "Valor total" : "Valor estimado";
        var resumo = ResumoReservaItem.Criar(reserva.Veiculo, reserva.Retirada, reserva.Devolucao, rotuloTotal);

        if (reserva.Contrato is not null)
        {
            resumo.Linhas.Insert(0, new DetalheLinha("Contrato", $"nº {reserva.Contrato}"));
        }

        return resumo;
    }

    /// <summary>Mostra apenas as ações permitidas para o status da reserva.</summary>
    private void ConfigurarAcoes(string status)
    {
        BotaoCancelar.Visibility = status == "Agendada" ? Visibility.Visible : Visibility.Collapsed;
        BotaoNovaReserva.Visibility = status is "Cancelada" or "Expirada" ? Visibility.Visible : Visibility.Collapsed;

        if (status == "Agendada")
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

        // TODO: trocar pelo controller, que também deve conferir se a reserva ainda pode ser cancelada.
        DadosDeExemplo.Cancelar(_reserva.Id);
        Navegador.IrPara(new MinhasReservasView("Reserva cancelada."));
    }

    private void Voltar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new MinhasReservasView());

    private void NovaReserva_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new BuscaVeiculosView());
}