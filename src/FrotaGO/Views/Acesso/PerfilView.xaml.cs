using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FrotaGO.Controllers;
using FrotaGO.Helpers;
using FrotaGO.Models;

namespace FrotaGO.Views.Acesso;

public partial class PerfilView : UserControl
{
    private readonly ContaController _contaController;
    private readonly int _usuarioId;
    private readonly bool _ehCliente;

    public PerfilView()
    {
        InitializeComponent();
        _contaController = FabricaDeControllers.CriarContaController();

        var sessao = Navegador.Usuario ?? throw new InvalidOperationException("Nenhum usuário logado.");

        // Os dados vêm do banco, e não da sessão: a sessão não tem telefone, documento nem matrícula.
        var usuario = _contaController.ObterUsuario(sessao.Id)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        _usuarioId = usuario.Id;
        _ehCliente = usuario is Cliente;
        PreencherCampos(usuario);
    }

    /// <summary>Documento e matrícula aparecem só para leitura: o usuário não os altera.</summary>
    private void PreencherCampos(Usuario usuario)
    {
        CampoNome.Text = usuario.Nome;
        CampoEmail.Text = usuario.Email;

        if (usuario is Cliente cliente)
        {
            // Pessoa jurídica: o Nome é o responsável, e o documento é o CNPJ.
            CampoTelefone.Text = cliente.Telefone;
            RotuloInfo.Text = "Documento";
            TextoInfo.Text = FormatoHelper.Documento(cliente.ObterDocumento());
        }
        else if (usuario is Funcionario funcionario)
        {
            PainelTelefone.Visibility = Visibility.Collapsed;
            RotuloInfo.Text = "Matrícula";
            TextoInfo.Text = funcionario.Matricula;
        }
    }

    private void SalvarDados_Click(object sender, RoutedEventArgs e)
    {
        // Só o preenchimento. Formato e e-mail repetido ficam no controller.
        if (string.IsNullOrWhiteSpace(CampoNome.Text) || string.IsNullOrWhiteSpace(CampoEmail.Text) ||
            (_ehCliente && string.IsNullOrWhiteSpace(CampoTelefone.Text)))
        {
            Mostrar(MensagemDados, "Preencha todos os campos.");
            return;
        }

        // Só o cliente tem telefone. Para funcionários, o controller recebe null.
        var telefone = _ehCliente ? CampoTelefone.Text : null;
        var resultado = _contaController.AtualizarDados(_usuarioId, CampoNome.Text, CampoEmail.Text, telefone);
        if (!resultado.Sucesso)
        {
            Mostrar(MensagemDados, resultado.Erro);
            return;
        }

        // Relê o usuário gravado: a sessão e as iniciais do cabeçalho usam o nome e o e-mail já normalizados.
        var usuario = _contaController.ObterUsuario(_usuarioId);
        if (usuario is not null)
        {
            Navegador.AtualizarUsuario(UsuarioSessao.De(usuario));
            PreencherCampos(usuario);
        }

        Mostrar(MensagemDados, "Dados atualizados com sucesso.", sucesso: true);
    }

    private void AlterarSenha_Click(object sender, RoutedEventArgs e)
    {
        var erro = ValidarSenha();
        if (erro is not null)
        {
            Mostrar(MensagemSenha, erro);
            return;
        }

        // O controller confere a senha atual, o tamanho e se a nova é diferente da atual.
        var resultado = _contaController.AlterarSenha(_usuarioId, CampoSenhaAtual.Password, CampoNovaSenha.Password);
        if (!resultado.Sucesso)
        {
            Mostrar(MensagemSenha, resultado.Erro);
            return;
        }

        CampoSenhaAtual.Clear();
        CampoNovaSenha.Clear();
        CampoConfirmarSenha.Clear();
        Mostrar(MensagemSenha, "Senha alterada com sucesso.", sucesso: true);
    }

    /// <summary>Só preenchimento e confirmação. As regras de negócio ficam no controller.</summary>
    private string? ValidarSenha()
    {
        if (CampoSenhaAtual.Password.Length == 0 || CampoNovaSenha.Password.Length == 0)
        {
            return "Preencha a senha atual e a nova senha.";
        }

        if (CampoNovaSenha.Password != CampoConfirmarSenha.Password)
        {
            return "As senhas não coincidem.";
        }

        return null;
    }

    private void Mostrar(TextBlock alvo, string texto, bool sucesso = false)
    {
        alvo.Text = texto;
        alvo.Foreground = (Brush)FindResource(sucesso ? "CorSucesso" : "CorErro");
        alvo.Visibility = Visibility.Visible;
    }
}
