using System.Windows;
using System.Windows.Controls;
using FrotaGO.Views.Acesso;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.AreaFuncionario;

public partial class ClientesView : UserControl
{
    public ClientesView(string? mensagemSucesso = null)
    {
        InitializeComponent();

        if (mensagemSucesso is not null)
        {
            TextoMensagem.Text = mensagemSucesso;
            TextoMensagem.Visibility = Visibility.Visible;
        }

        // TODO: trocar pelos clientes vindos do controller.
        TabelaClientes.ItemsSource = DadosDeExemplo.Clientes.OrderBy(cliente => cliente.Nome).ToList();
    }

    private void NovoCliente_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new CriarContaView(modoFuncionario: true));
}