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

namespace FrotaGO.Views.AreaFuncionario;

public partial class HomeFuncionarioView : UserControl
{
    public HomeFuncionarioView(string nomeFuncionario, bool ehGerente)
    {
        InitializeComponent();
        TextoSaudacao.Text = $"Olá, {NomeHelper.PrimeiroNome(nomeFuncionario)}";
        LinhaGestao.Visibility = ehGerente ? Visibility.Visible : Visibility.Collapsed;

        // TODO: substituir pelas visitas do dia vindas do controller.
        ExibirVisitas(CriarVisitasDeExemplo());
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

    private static List<VisitaItem> CriarVisitasDeExemplo() =>
    [
        new() { ReservaId = 3, Horario = "09:00", Cliente = "Maria Silva", Veiculo = "BYD Dolphin · 2025", Status = "Agendada" },
        new() { ReservaId = 4, Horario = "10:30", Cliente = "João Souza", Veiculo = "Honda NS 160 · 2024", Status = "Agendada" }
    ];
}
