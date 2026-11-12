using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using FrotaGO.Views.AreaFuncionario;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.Acesso;

public partial class CriarContaView : UserControl
{
    private const int TamanhoMinimoSenha = 8;
    private const string FormatoData = "dd/MM/yyyy";

    /// <summary>Verdadeiro quando um funcionário cadastra um cliente atendido na loja.</summary>
    private readonly bool _modoFuncionario;

    public CriarContaView(bool modoFuncionario = false)
    {
        InitializeComponent();
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

        // Pessoa jurídica aparece na lista pela razão social.
        var nome = EhPessoaFisica ? CampoNome.Text.Trim() : CampoComplemento.Text.Trim();
        var documento = $"{RotuloDocumento.Text} {CampoDocumento.Text.Trim()}";

        // TODO: trocar pelo controller, que valida e salva o cliente com a senha em hash.
        DadosDeExemplo.AdicionarCliente(nome, documento, CampoEmail.Text.Trim().ToLowerInvariant(),
                                        CampoTelefone.Text.Trim());

        if (_modoFuncionario)
        {
            Navegador.IrPara(new ClientesView("Cliente cadastrado com sucesso."));
            return;
        }

        Navegador.IrPara(new LoginView("Conta criada com sucesso. Entre para continuar."),
                         mostrarCabecalho: false);
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

        var email = CampoEmail.Text.Trim().ToLowerInvariant();
        if (!email.Contains('@'))
        {
            return "Informe um e-mail válido.";
        }

        if (DadosDeExemplo.Clientes.Any(cliente => cliente.Email == email))
        {
            return "Este e-mail já está cadastrado.";
        }

        if (EhPessoaFisica && !DateTime.TryParseExact(
                CampoNascimento.Text, FormatoData, CultureInfo.InvariantCulture,
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