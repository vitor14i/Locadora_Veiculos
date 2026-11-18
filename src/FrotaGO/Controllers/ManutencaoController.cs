using FrotaGO.Models;
using FrotaGO.Models.Enums;
using FrotaGO.Repositories.Interfaces;

namespace FrotaGO.Controllers;

public class ManutencaoController
{
    private const string MensagemSomenteGerente = "Somente um gerente ativo pode registrar manutenções.";

    private readonly IVeiculoRepository _veiculos;
    private readonly IManutencaoRepository _manutencoes;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;

    public ManutencaoController(IVeiculoRepository veiculos, IManutencaoRepository manutencoes,
                                IUsuarioRepository usuarios, IUnidadeDeTrabalho unidadeDeTrabalho)
    {
        _veiculos = veiculos;
        _manutencoes = manutencoes;
        _usuarios = usuarios;
        _unidadeDeTrabalho = unidadeDeTrabalho;
    }

    public List<Manutencao> Listar() => _manutencoes.ListarTodas();

    public List<Veiculo> ListarVeiculosDisponiveis() => _veiculos.ListarPorStatus(StatusVeiculo.Disponivel);

    public Resultado Iniciar(int veiculoId, string descricao, int gerenteId)
    {
        if (!EhGerenteAtivo(gerenteId))
        {
            return Resultado.Falha(MensagemSomenteGerente);
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            return Resultado.Falha("Descreva o serviço.");
        }

        var veiculo = _veiculos.ObterPorId(veiculoId);
        if (veiculo is null)
        {
            return Resultado.Falha("Veículo não encontrado.");
        }

        if (veiculo.Status != StatusVeiculo.Disponivel)
        {
            return Resultado.Falha("Só um veículo disponível pode entrar em manutenção.");
        }

        _manutencoes.Adicionar(new Manutencao
        {
            VeiculoId = veiculo.Id,
            GerenteId = gerenteId,
            Descricao = descricao.Trim(),
            DataInicio = DateTime.Now
        });
        veiculo.Status = StatusVeiculo.EmManutencao;

        // Um único Salvar(): o registro e o novo status são gravados juntos, na mesma transação.
        _unidadeDeTrabalho.Salvar();
        return Resultado.Ok();
    }

    public Resultado Finalizar(int manutencaoId, int gerenteId)
    {
        if (!EhGerenteAtivo(gerenteId))
        {
            return Resultado.Falha(MensagemSomenteGerente);
        }

        var manutencao = _manutencoes.ObterPorId(manutencaoId);
        if (manutencao is null)
        {
            return Resultado.Falha("Manutenção não encontrada.");
        }

        if (!manutencao.EstaEmAndamento())
        {
            return Resultado.Falha("Esta manutenção já foi finalizada.");
        }

        manutencao.Finalizar(DateTime.Now);
        // O ObterPorId do repositório inclui o veículo, que é obrigatório.
        manutencao.Veiculo!.Status = StatusVeiculo.Disponivel;

        _unidadeDeTrabalho.Salvar();
        return Resultado.Ok();
    }

    /// <summary>Confere pelo banco, e não pela tela, que o usuário é um gerente ativo.</summary>
    private bool EhGerenteAtivo(int usuarioId) =>
        _usuarios.ObterPorId(usuarioId) is Gerente { Ativo: true };
}
