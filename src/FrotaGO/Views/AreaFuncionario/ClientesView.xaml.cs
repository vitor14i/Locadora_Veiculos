using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Views.Acesso;
using FrotaGO.Views.Componentes;

namespace FrotaGO.Views.AreaFuncionario;

public partial class ClientesView : UserControl
{
    private readonly ContaController _contaController;

    public ClientesView(string? mensagemSucesso = null)
    {
        InitializeComponent();
        _contaController = FabricaDeControllers.CriarContaController();

        if (mensagemSucesso is not null)
        {
            TextoMensagem.Text = mensagemSucesso;
            TextoMensagem.Visibility = Visibility.Visible;
        }

        CarregarClientes();
    }

    private void CarregarClientes()
    {
        TabelaClientes.ItemsSource = _contaController.ListarClientesAtivos()
            .Select(CriarLinha)
            .OrderBy(linha => linha.Nome)
            .ToList();
    }

    /// <summary>Pessoa jurídica aparece pela razão social. O documento é exibido com máscara.</summary>
    private static LinhaCliente CriarLinha(Cliente cliente) => new(
        cliente is PessoaJuridica empresa ? empresa.RazaoSocial : cliente.Nome,
        FormatoHelper.Documento(cliente.ObterDocumento()),
        cliente.Email,
        cliente.Telefone);

    private void NovoCliente_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new CriarContaView(modoFuncionario: true));
}
