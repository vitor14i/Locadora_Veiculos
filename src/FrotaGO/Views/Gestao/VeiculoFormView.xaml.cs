using System.Windows;
using System.Windows.Controls;
using FrotaGO.Helpers;
using FrotaGO.Models;
using FrotaGO.Models.Enums;
using FrotaGO.Views.Exemplos;

namespace FrotaGO.Views.Gestao;

public partial class VeiculoFormView : UserControl
{
    private const int AnoMinimo = 1990;

    /// <summary>Preenchido na edição.</summary>
    private readonly Veiculo? _veiculo;

    public VeiculoFormView(int? veiculoId = null)
    {
        InitializeComponent();

        if (veiculoId is int id)
        {
            // TODO: buscar o veículo pelo controller.
            _veiculo = DadosDeExemplo.VeiculoDaFrota(id);
            PreencherCampos(_veiculo);
        }

        AtualizarCamposPorTipo();
    }

    private bool EhCarro => OpcaoCarro.IsChecked == true;

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
        var erro = ValidarFormulario(out var ano, out var km, out var diaria);
        if (erro is not null)
        {
            TextoMensagem.Text = erro;
            TextoMensagem.Visibility = Visibility.Visible;
            return;
        }

        var veiculo = _veiculo ?? (EhCarro ? new Carro() : new Moto());
        veiculo.Placa = CampoPlaca.Text.Trim().ToUpperInvariant();
        veiculo.Marca = CampoMarca.Text.Trim();
        veiculo.Modelo = CampoModelo.Text.Trim();
        veiculo.Ano = ano;
        veiculo.Quilometragem = km;
        veiculo.ValorDiaria = diaria;

        switch (veiculo)
        {
            case Carro carro:
                carro.Categoria = CampoCategoria.Text.Trim();
                carro.NumeroPortas = int.Parse(CampoPortas.Text);
                carro.Cambio = OpcaoAutomatico.IsChecked == true ? TipoCambio.Automatico : TipoCambio.Manual;
                break;
            case Moto moto:
                moto.Cilindradas = int.Parse(CampoCilindradas.Text);
                break;
        }

        // TODO: trocar pelo controller.
        DadosDeExemplo.SalvarVeiculo(veiculo);
        Navegador.IrPara(new VeiculosView("Veículo salvo com sucesso."));
    }

    private string? ValidarFormulario(out int ano, out decimal km, out decimal diaria)
    {
        ano = 0;
        km = 0;
        diaria = 0;

        TextBox[] obrigatorios = [CampoPlaca, CampoMarca, CampoModelo];
        if (obrigatorios.Any(campo => string.IsNullOrWhiteSpace(campo.Text)))
        {
            return "Preencha a placa, a marca e o modelo.";
        }

        var placa = CampoPlaca.Text.Trim().ToUpperInvariant();
        if (DadosDeExemplo.Frota.Any(veiculo => veiculo.Placa == placa && veiculo != _veiculo))
        {
            return "Já existe um veículo com essa placa.";
        }

        if (!int.TryParse(CampoAno.Text, out ano) || ano < AnoMinimo || ano > DateTime.Today.Year + 1)
        {
            return "Informe um ano válido.";
        }

        if (!FormatoHelper.TentarLerQuilometragem(CampoKm.Text, out km))
        {
            return "Informe a quilometragem.";
        }

        if (!FormatoHelper.TentarLerValor(CampoDiaria.Text, out diaria) || diaria <= 0)
        {
            return "Informe o valor da diária.";
        }

        if (EhCarro)
        {
            if (string.IsNullOrWhiteSpace(CampoCategoria.Text))
            {
                return "Informe a categoria.";
            }

            if (!int.TryParse(CampoPortas.Text, out var portas) || portas is < 2 or > 5)
            {
                return "Informe o número de portas, de 2 a 5.";
            }
        }
        else if (!int.TryParse(CampoCilindradas.Text, out var cilindradas) || cilindradas <= 0)
        {
            return "Informe as cilindradas.";
        }

        return null;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e) =>
        Navegador.IrPara(new VeiculosView());
}