using System.Windows;
using System.Windows.Controls;
using FrotaGO.Controllers;
using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Models.Enums;

namespace FrotaGO.Views.Gestao;

public partial class VeiculoFormView : UserControl
{
    private readonly VeiculoController _veiculoController;

    /// <summary>Preenchido na edição.</summary>
    private readonly int? _veiculoId;

    public VeiculoFormView(int? veiculoId = null)
    {
        InitializeComponent();
        _veiculoController = FabricaDeControllers.CriarVeiculoController();
        _veiculoId = veiculoId;

        // Se o veículo não existir, o Salvar do controller recusa com mensagem.
        if (veiculoId is int id && _veiculoController.Obter(id) is Veiculo veiculo)
        {
            PreencherCampos(veiculo);
        }

        AtualizarCamposPorTipo();
    }

    private bool EhCarro => OpcaoCarro.IsChecked == true;

    private TipoCambio CambioSelecionado =>
        OpcaoAutomatico.IsChecked == true ? TipoCambio.Automatico : TipoCambio.Manual;

    private void PreencherCampos(Veiculo veiculo)
    {
        TituloTela.Text = "Editar veículo";
        OpcaoMoto.IsChecked = veiculo is Moto;
        SeletorTipo.IsEnabled = false;

        CampoPlaca.Text = veiculo.Placa;
        CampoMarca.Text = veiculo.Marca;
        CampoModelo.Text = veiculo.Modelo;
        CampoAno.Text = veiculo.Ano.ToString();
        CampoKm.Text = veiculo.Quilometragem.ToString("0");
        CampoDiaria.Text = FormatoHelper.ValorCampo(veiculo.ValorDiaria);

        switch (veiculo)
        {
            case Carro carro:
                CampoCategoria.Text = carro.Categoria;
                CampoPortas.Text = carro.NumeroPortas.ToString();
                OpcaoAutomatico.IsChecked = carro.Cambio == TipoCambio.Automatico;
                break;
            case Moto moto:
                CampoCilindradas.Text = moto.Cilindradas.ToString();
                break;
        }
    }

    private void Tipo_Checked(object sender, RoutedEventArgs e)
    {
        // O evento também dispara durante o InitializeComponent.
        if (!IsInitialized) return;
        AtualizarCamposPorTipo();
    }

    private void AtualizarCamposPorTipo()
    {
        PainelCarro.Visibility = EhCarro ? Visibility.Visible : Visibility.Collapsed;
        PainelMoto.Visibility = EhCarro ? Visibility.Collapsed : Visibility.Visible;
    }

    private void Salvar_Click(object sender, RoutedEventArgs e)
    {
        var erro = LerFormulario(out var dados);
        if (erro is not null)
        {
            MostrarErro(erro);
            return;
        }

        // Sem erro, o LerFormulario sempre preenche os dados.
        var resultado = _veiculoController.Salvar(dados!, _veiculoId);
        if (!resultado.Sucesso)
        {
            MostrarErro(resultado.Erro);
            return;
        }

        Navegador.IrPara(new VeiculosView("Veículo salvo com sucesso."));
    }

    /// <summary>
    /// Confere o preenchimento e o formato, convertendo cada campo uma única vez.
    /// Devolve a mensagem do primeiro campo com problema, ou null com os dados prontos.
    /// As regras (faixas, categoria, placa repetida e tipo) ficam no VeiculoController.
    /// </summary>
    private string? LerFormulario(out DadosVeiculo? dados)
    {
        dados = null;

        TextBox[] obrigatorios = [CampoPlaca, CampoMarca, CampoModelo];
        if (obrigatorios.Any(campo => string.IsNullOrWhiteSpace(campo.Text)))
        {
            return "Preencha a placa, a marca e o modelo.";
        }

        if (!int.TryParse(CampoAno.Text, out var ano))
        {
            return "Informe um ano válido.";
        }

        if (!FormatoHelper.TentarLerQuilometragem(CampoKm.Text, out var km))
        {
            return "Informe a quilometragem.";
        }

        if (!FormatoHelper.TentarLerValor(CampoDiaria.Text, out var diaria))
        {
            return "Informe o valor da diária.";
        }

        int? portas = null;
        TipoCambio? cambio = null;
        int? cilindradas = null;

        if (EhCarro)
        {
            if (!int.TryParse(CampoPortas.Text, out var numeroPortas))
            {
                return "Informe o número de portas.";
            }

            portas = numeroPortas;
            cambio = CambioSelecionado;
        }
        else
        {
            if (!int.TryParse(CampoCilindradas.Text, out var valorCilindradas))
            {
                return "Informe as cilindradas.";
            }

            cilindradas = valorCilindradas;
        }

        dados = new DadosVeiculo(
            EhCarro: EhCarro,
            Placa: CampoPlaca.Text,
            Marca: CampoMarca.Text,
            Modelo: CampoModelo.Text,
            Ano: ano,
            Quilometragem: km,
            ValorDiaria: diaria,
            Categoria: EhCarro ? CampoCategoria.Text : string.Empty,
            NumeroPortas: portas,
            Cambio: cambio,
            Cilindradas: cilindradas);
        return null;
    }

    private void MostrarErro(string texto)
    {
        TextoMensagem.Text = texto;
        TextoMensagem.Visibility = Visibility.Visible;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new VeiculosView());
}