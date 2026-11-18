using FrotaGO.Models;
using FrotaGO.Repositories.Interfaces;

namespace FrotaGO.Controllers;

public class VeiculoController
{
    private const int AnoMinimo = 1990;
    private const int TamanhoPlaca = 7;
    private const int PortasMinimo = 2;
    private const int PortasMaximo = 5;

    private readonly IVeiculoRepository _veiculos;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;

    public VeiculoController(IVeiculoRepository veiculos, IUnidadeDeTrabalho unidadeDeTrabalho)
    {
        _veiculos = veiculos;
        _unidadeDeTrabalho = unidadeDeTrabalho;
    }

    public List<Veiculo> ListarTodos() => _veiculos.ListarTodos();

    public Veiculo? Obter(int id) => _veiculos.ObterPorId(id);

    /// <summary>
    /// Sem <paramref name="veiculoId"/>, cadastra um carro ou uma moto.
    /// Com ele, altera o veículo existente, sem mudar o tipo nem o status.
    /// </summary>
    public Resultado Salvar(DadosVeiculo dados, int? veiculoId)
    {
        Veiculo? existente = null;

        if (veiculoId is int id)
        {
            existente = _veiculos.ObterPorId(id);
            if (existente is null)
            {
                return Resultado.Falha("Veículo não encontrado.");
            }

            if ((existente is Carro) != dados.EhCarro)
            {
                return Resultado.Falha("O tipo do veículo não pode ser alterado.");
            }
        }

        var placa = NormalizarPlaca(dados.Placa);

        var erro = ValidarVeiculo(dados, placa, veiculoId);
        if (erro is not null)
        {
            return Resultado.Falha(erro);
        }

        var veiculo = existente ?? CriarVeiculo(dados.EhCarro);
        Preencher(veiculo, dados, placa);

        if (existente is null)
        {
            _veiculos.Adicionar(veiculo);
        }

        // Na edição, o veículo veio rastreado do repositório: basta salvar.
        _unidadeDeTrabalho.Salvar();
        return Resultado.Ok();
    }

    private string? ValidarVeiculo(DadosVeiculo dados, string placa, int? veiculoId)
    {
        if (string.IsNullOrWhiteSpace(dados.Marca) || string.IsNullOrWhiteSpace(dados.Modelo))
        {
            return "Preencha a placa, a marca e o modelo.";
        }

        if (placa.Length != TamanhoPlaca || !placa.All(char.IsAsciiLetterOrDigit))
        {
            return $"A placa deve ter {TamanhoPlaca} letras ou números.";
        }

        var anoMaximo = DateTime.Today.Year + 1;
        if (dados.Ano < AnoMinimo || dados.Ano > anoMaximo)
        {
            return $"Informe um ano entre {AnoMinimo} e {anoMaximo}.";
        }

        if (dados.Quilometragem < 0)
        {
            return "A quilometragem não pode ser negativa.";
        }

        if (dados.ValorDiaria <= 0)
        {
            return "O valor da diária deve ser maior que zero.";
        }

        var erroDoTipo = dados.EhCarro ? ValidarCarro(dados) : ValidarMoto(dados);
        if (erroDoTipo is not null)
        {
            return erroDoTipo;
        }

        // Por último, porque é a única validação que consulta o banco.
        // Na edição, o próprio veículo é ignorado.
        return _veiculos.PlacaExiste(placa, veiculoId) ? "Já existe um veículo com essa placa." : null;
    }

    private static string? ValidarCarro(DadosVeiculo dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Categoria))
        {
            return "Informe a categoria.";
        }

        if (dados.NumeroPortas is not (>= PortasMinimo and <= PortasMaximo))
        {
            return $"O carro deve ter de {PortasMinimo} a {PortasMaximo} portas.";
        }

        return dados.Cambio is null ? "Informe o câmbio." : null;
    }

    private static string? ValidarMoto(DadosVeiculo dados) =>
        dados.Cilindradas is > 0 ? null : "As cilindradas devem ser maiores que zero.";

    private static Veiculo CriarVeiculo(bool ehCarro) => ehCarro ? new Carro() : new Moto();

    private static void Preencher(Veiculo veiculo, DadosVeiculo dados, string placa)
    {
        veiculo.Placa = placa;
        veiculo.Marca = dados.Marca.Trim();
        veiculo.Modelo = dados.Modelo.Trim();
        veiculo.Ano = dados.Ano;
        veiculo.Quilometragem = dados.Quilometragem;
        veiculo.ValorDiaria = dados.ValorDiaria;

        // Os campos do tipo já foram validados em ValidarCarro ou ValidarMoto.
        switch (veiculo)
        {
            case Carro carro:
                carro.Categoria = dados.Categoria.Trim();
                carro.NumeroPortas = dados.NumeroPortas!.Value;
                carro.Cambio = dados.Cambio!.Value;
                break;
            case Moto moto:
                moto.Cilindradas = dados.Cilindradas!.Value;
                break;
        }
    }

    /// <summary>Placa salva sem máscara: sem hífen, sem espaços e em maiúsculas, como o CPF.</summary>
    private static string NormalizarPlaca(string placa) =>
        placa.Replace("-", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
}
