using System.Windows;
using System.Windows.Controls;
using FrotaGO.Views.Acesso;

namespace FrotaGO.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Navegador.Iniciar(this);
        Navegador.IrPara(new LoginView(), mostrarCabecalho: false);
    }

    public void ExibirTela(UserControl tela, bool mostrarCabecalho)
    {
        AreaConteudo.Content = tela;
        Cabecalho.Visibility = mostrarCabecalho ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Logo_Click(object sender, RoutedEventArgs e)
    {
        // TODO (PR 2): voltar para a home do perfil logado.
    }

    private void Perfil_Click(object sender, RoutedEventArgs e)
    {
        // TODO (PR 4): abrir a PerfilView.
    }

    private void Sair_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new LoginView(), mostrarCabecalho: false);
}