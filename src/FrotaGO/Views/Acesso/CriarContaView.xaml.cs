using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Helpers;
using FrotaGO.Views.AreaFuncionario;

namespace FrotaGO.Views.Acesso;

public partial class CriarContaView : UserControl
{
    /// <summary>Verdadeiro quando um funcionário cadastra um cliente atendido na loja.</summary>
    private readonly bool _modoFuncionario;

    private readonly ContaController _contaController;

    public CriarContaView(bool modoFuncionario = false)
    {
        InitializeComponent();
        _contaController = FabricaDeControllers.CriarContaController();
        _modoFuncionario = modoFuncionario;

        if (modoFuncionario)
        {
            ConfigurarModoFuncionario();
        }

        AtualizarCamposPorTipo();
    }

    private bool EhPessoaFisica => OpcaoPessoaFisica.IsChecked == true;

    private void ConfigurarModoFuncionario()
    {
        TituloTela.Text = "Novo cliente";
        SubtituloTela.Text = "Cadastre o cliente atendido na loja.";
        RotuloSenha.Text = "Senha temporária";
        BotaoCriar.Content = "Cadastrar cliente";
        PainelEntrar.Visibility = Visibility.Collapsed;
    }

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

        var resultado = _contaController.CadastrarCliente(LerFormulario());
        if (!resultado.Sucesso)
        {
            MostrarErro(resultado.Erro);
            return;
        }

        if (_modoFuncionario)
        {
            Navegador.IrPara(new ClientesView("Cliente cadastrado com sucesso."));
            return;
        }

        Navegador.IrPara(new LoginView("Conta criada com sucesso. Entre para continuar."),
                         mostrarCabecalho: false);
    }

    /// <summary>Junta os campos da tela para o controller. A data já chega convertida.</summary>
    private DadosCliente LerFormulario() => new(
        EhPessoaFisica,
        CampoNome.Text,
        CampoDocumento.Text,
        CampoComplemento.Text,
        CampoEmail.Text,
        CampoTelefone.Text,
        CampoEndereco.Text,
        LerDataNascimento(),
        CampoSenha.Password);

    /// <summary>Converte a data de nascimento. Só é chamado depois de ValidarFormulario conferir o formato.</summary>
    private DateTime? LerDataNascimento() =>
        EhPessoaFisica && FormatoHelper.TentarLerData(CampoNascimento.Text, out var data) ? data : null;

    /// <summary>
    /// Validações de preenchimento da tela: campos vazios, formato da data e confirmação da senha.
    /// As regras de negócio ficam no ContaController.
    /// </summary>
    private string? ValidarFormulario()
    {
        TextBox[] obrigatorios = [CampoNome, CampoDocumento, CampoComplemento,
                                  CampoEmail, CampoTelefone, CampoEndereco];

        if (obrigatorios.Any(campo => string.IsNullOrWhiteSpace(campo.Text)) ||
            CampoSenha.Password.Length == 0)
        {
            return "Preencha todos os campos.";
        }

        if (EhPessoaFisica && !FormatoHelper.TentarLerData(CampoNascimento.Text, out _))
        {
            return "Informe a data de nascimento no formato dd/mm/aaaa.";
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
