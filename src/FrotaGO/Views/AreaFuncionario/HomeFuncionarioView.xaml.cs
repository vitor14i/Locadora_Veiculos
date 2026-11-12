using FrotaGO.Helpers;
using FrotaGO.Views.AreaCliente;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;
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

namespace FrotaGO.Views.AreaFuncionario;

public partial class HomeFuncionarioView : UserControl
{
    public HomeFuncionarioView(string nomeFuncionario, bool ehGerente)
    {
        InitializeComponent();
        TextoSaudacao.Text = $"Olá, {NomeHelper.PrimeiroNome(nomeFuncionario)}";
        LinhaGestao.Visibility = ehGerente ? Visibility.Visible : Visibility.Collapsed;

        // TODO: substituir pelas visitas do dia vindas do controller.
        ExibirVisitas(DadosDeExemplo.VisitasDeHoje());
    }

    private void ExibirVisitas(IReadOnlyList<VisitaItem> visitas)
    {
        ListaVisitas.ItemsSource = visitas;
        TextoSemVisitas.Visibility = visitas.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        TextoResumo.Text = visitas.Count switch
        {
            0 => "Nenhuma visita agendada para hoje.",
            1 => "Você tem 1 visita agendada para hoje.",
            _ => $"Você tem {visitas.Count} visitas agendadas para hoje."
        };
    }
    private void NovaReserva_Click(object sender, RoutedEventArgs e) =>
    Navegador.IrPara(new BuscaVeiculosView(modoFuncionario: true));

    private void RegistrarDevolucao_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new LocacoesAtivasView());

    private void ListaVisitas_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement { DataContext: VisitaItem visita })
        {
            Navegador.IrPara(new IniciarLocacaoView(visita.ReservaId));
        }
    }
}
