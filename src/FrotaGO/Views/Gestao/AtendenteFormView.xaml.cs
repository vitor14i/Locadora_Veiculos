using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;

namespace FrotaGO.Views.Gestao;

public partial class AtendenteFormView : UserControl
{
    private readonly AtendenteController _atendenteController;

    public AtendenteFormView()
    {
        InitializeComponent();
        _atendenteController = FabricaDeControllers.CriarAtendenteController();
    }

    private void Cadastrar_Click(object sender, RoutedEventArgs e)
    {
        var erro = ValidarFormulario();
        if (erro is not null)
        {
            MostrarErro(erro);
            return;
        }

        var dados = new DadosAtendente(CampoNome.Text, CampoEmail.Text, CampoMatricula.Text, CampoSenha.Password);

        // O gerente é o usuário logado. Sem usuário, o id 0 não existe e o controller recusa.
        var resultado = _atendenteController.Cadastrar(dados, Navegador.Usuario?.Id ?? 0);
        if (!resultado.Sucesso)
        {
            MostrarErro(resultado.Erro);
            return;
        }

        Navegador.IrPara(new AtendentesView("Atendente cadastrado com sucesso."));
    }

    /// <summary>Só preenchimento e confirmação da senha. As regras de negócio ficam no controller.</summary>
    private string? ValidarFormulario()
    {
        TextBox[] obrigatorios = [CampoNome, CampoEmail, CampoMatricula];
        if (obrigatorios.Any(campo => string.IsNullOrWhiteSpace(campo.Text)) || CampoSenha.Password.Length == 0)
        {
            return "Preencha todos os campos.";
        }

        if (CampoSenha.Password != CampoConfirmarSenha.Password)
        {
            return "As senhas não coincidem.";
        }

        return null;
    }

    private void MostrarErro(string texto)
    {
        TextoMensagem.Text = texto;
        TextoMensagem.Visibility = Visibility.Visible;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new AtendentesView());
}
