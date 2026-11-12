using System.Windows;
using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Models.Enums;
using FrotaGO.Views.Componentes;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.Gestao;

public partial class VeiculosView : UserControl
{
    public VeiculosView(string? mensagemSucesso = null)
    {
        InitializeComponent();

        if (mensagemSucesso is not null)
        {
            TextoMensagem.Text = mensagemSucesso;
            TextoMensagem.Visibility = Visibility.Visible;
        }

        // TODO: trocar pelos veículos vindos do controller.
        TabelaVeiculos.ItemsSource = DadosDeExemplo.Frota.Select(CriarLinha).ToList();
    }

    private static LinhaVeiculo CriarLinha(Veiculo veiculo) => new(
        veiculo.Id,
        veiculo.Placa,
        $"{veiculo.Marca} {veiculo.Modelo} · {veiculo.Ano}",
        veiculo is Moto ? "Moto" : "Carro",
        FormatoHelper.Moeda(veiculo.ValorDiaria),
        TextoStatus(veiculo.Status));

    private static string TextoStatus(StatusVeiculo status) => status switch
    {
        StatusVeiculo.Disponivel => "Disponível",
        StatusVeiculo.Alugado => "Alugado",
        _ => "Em manutenção"
    };

    private void NovoVeiculo_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new VeiculoFormView());

    private void Editar_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: LinhaVeiculo linha })
        {
            Navegador.IrPara(new VeiculoFormView(linha.Id));
        }
    }
}