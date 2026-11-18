using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Models;

namespace FrotaGO.Views.Gestao;

public partial class ManutencaoFormView : UserControl
{
    private readonly ManutencaoController _manutencaoController;

    public ManutencaoFormView()
    {
        InitializeComponent();
        _manutencaoController = FabricaDeControllers.CriarManutencaoController();

        CampoVeiculo.ItemsSource = _manutencaoController.ListarVeiculosDisponiveis();
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

        // O gerente responsável é o usuário logado. Sem usuário, o id 0 não existe e o controller recusa.
        var resultado = _manutencaoController.Iniciar(veiculo.Id, CampoDescricao.Text, Navegador.Usuario?.Id ?? 0);
        if (!resultado.Sucesso)
        {
            MostrarErro(resultado.Erro);
            return;
        }

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