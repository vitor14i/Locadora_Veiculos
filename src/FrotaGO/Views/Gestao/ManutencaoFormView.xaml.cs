using System.Windows;
using System.Windows.Controls;
using FrotaGO.Models;
using FrotaGO.Models.Enums;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.Gestao;

public partial class ManutencaoFormView : UserControl
{
    public ManutencaoFormView()
    {
        InitializeComponent();

        // TODO: trocar pelos veículos disponíveis vindos do controller.
        CampoVeiculo.ItemsSource = DadosDeExemplo.Frota
            .Where(veiculo => veiculo.Status == StatusVeiculo.Disponivel)
            .ToList();
    }

    private void Iniciar_Click(object sender, RoutedEventArgs e)
    {
        if (CampoVeiculo.SelectedItem is not Veiculo veiculo)
        {
            MostrarErro("Escolha o veículo.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CampoDescricao.Text))
        {
            MostrarErro("Descreva o serviço.");
            return;
        }

        // TODO: trocar pelo controller, registrando também o gerente responsável.
        DadosDeExemplo.IniciarManutencao(veiculo, CampoDescricao.Text.Trim());
        Navegador.IrPara(new ManutencoesView("Manutenção iniciada. O veículo está indisponível para reservas."));
    }

    private void MostrarErro(string texto)
    {
        TextoMensagem.Text = texto;
        TextoMensagem.Visibility = Visibility.Visible;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new ManutencoesView());
}