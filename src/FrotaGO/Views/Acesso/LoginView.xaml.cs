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

namespace FrotaGO.Views.Acesso;

public partial class LoginView : UserControl
{
    public LoginView(string? mensagemSucesso = null)
    {
        InitializeComponent();

        if (mensagemSucesso is not null)
        {
            MostrarMensagem(mensagemSucesso, sucesso: true);
        }
    }

    private void Entrar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CampoEmail.Text) || CampoSenha.Password.Length == 0)
        {
            MostrarMensagem("Preencha o e-mail e a senha.");
            return;
        }

        // TODO: chamar ContaController.Login e abrir a home do perfil.
        MostrarMensagem("O login ainda não está conectado ao sistema.");
    }

    private void CriarConta_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new CriarContaView(), mostrarCabecalho: false);

    private void MostrarMensagem(string texto, bool sucesso = false)
    {
        TextoMensagem.Text = texto;
        TextoMensagem.Foreground = (Brush)FindResource(sucesso ? "CorSucesso" : "CorErro");
        TextoMensagem.Visibility = Visibility.Visible;
    }
}
