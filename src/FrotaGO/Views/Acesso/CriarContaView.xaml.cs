using System;
using System.Collections.Generic;
using System.Globalization;
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

public partial class CriarContaView : UserControl
{
    private const int TamanhoMinimoSenha = 8;
    private const string FormatoData = "dd/MM/yyyy";

    public CriarContaView()
    {
        InitializeComponent();
        AtualizarCamposPorTipo();
    }

    private bool EhPessoaFisica => OpcaoPessoaFisica.IsChecked == true;

    private void TipoPessoa_Checked(object sender, RoutedEventArgs e)
    {
        // O evento também dispara durante o InitializeComponent, antes de os campos existirem.
        if (!IsInitialized) return;
        AtualizarCamposPorTipo();
    }

    private void AtualizarCamposPorTipo()
    {
        RotuloNome.Text = EhPessoaFisica ? "Nome completo" : "Nome do responsável";
        RotuloDocumento.Text = EhPessoaFisica ? "CPF" : "CNPJ";
        RotuloComplemento.Text = EhPessoaFisica ? "CNH" : "Razão social";

        PainelNascimento.Visibility = EhPessoaFisica ? Visibility.Visible : Visibility.Collapsed;
        Grid.SetColumnSpan(PainelEndereco, EhPessoaFisica ? 1 : 3);

        TextoMensagem.Visibility = Visibility.Collapsed;
    }

    private void CriarConta_Click(object sender, RoutedEventArgs e)
    {
        var erro = ValidarFormulario();
        if (erro is not null)
        {
            MostrarErro(erro);
            return;
        }

        // TODO: chamar ContaController.CriarConta e, se der certo,
        // voltar ao login com "Conta criada com sucesso. Entre para continuar."
        MostrarErro("O cadastro ainda não está conectado ao sistema.");
    }

    /// <summary>Validações de preenchimento da tela. As regras de negócio também são validadas no controller.</summary>
    private string? ValidarFormulario()
    {
        TextBox[] obrigatorios = [CampoNome, CampoDocumento, CampoComplemento,
                                  CampoEmail, CampoTelefone, CampoEndereco];

        if (obrigatorios.Any(campo => string.IsNullOrWhiteSpace(campo.Text)) ||
            CampoSenha.Password.Length == 0)
        {
            return "Preencha todos os campos.";
        }

        if (EhPessoaFisica && !DateTime.TryParseExact(
                CampoNascimento.Text, FormatoData, CultureInfo.CurrentCulture,
                DateTimeStyles.None, out _))
        {
            return "Informe a data de nascimento no formato dd/mm/aaaa.";
        }

        if (CampoSenha.Password.Length < TamanhoMinimoSenha)
        {
            return $"A senha deve ter pelo menos {TamanhoMinimoSenha} caracteres.";
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

    private void Entrar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new LoginView(), mostrarCabecalho: false);
}