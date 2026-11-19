using FrotaGO.Models;
using FrotaGO.Models.Enums;
using FrotaGO.Repositories.Interfaces;

namespace FrotaGO.Controllers;

public class ReservaController
{
    private readonly IReservaRepository _reservas;
    private readonly IVeiculoRepository _veiculos;
    private readonly IUsuarioRepository _usuarios;
    private readonly IUnidadeDeTrabalho _unidadeDeTrabalho;

    public ReservaController(IReservaRepository reservas, IVeiculoRepository veiculos,
                             IUsuarioRepository usuarios, IUnidadeDeTrabalho unidadeDeTrabalho)
    {
        _reservas = reservas;
        _veiculos = veiculos;
        _usuarios = usuarios;
        _unidadeDeTrabalho = unidadeDeTrabalho;
    }

    /// <summary>Veículos livres no período. <paramref name="tipo"/> é "Carro", "Moto" ou null para todos.</summary>
    public Resultado<List<Veiculo>> BuscarDisponiveis(DateTime inicio, DateTime fim, string? tipo)
    {
        var agora = DateTime.Now;

        var erro = ValidarPeriodo(inicio, fim, agora);
        if (erro is not null)
        {
            return Resultado<List<Veiculo>>.Falha(erro);
        }

        return Resultado<List<Veiculo>>.Ok(_veiculos.ListarDisponiveis(inicio, fim, tipo, agora));
    }

    public Resultado<Reserva> Criar(int clienteId, int veiculoId, DateTime inicio, DateTime fim)
    {
        var agora = DateTime.Now;

        var erro = ValidarPeriodo(inicio, fim, agora);
        if (erro is not null)
        {
            return Resultado<Reserva>.Falha(erro);
        }

        var veiculo = _veiculos.ObterPorId(veiculoId);
        if (veiculo is null)
        {
            return Resultado<Reserva>.Falha("Veículo não encontrado.");
        }

        if (veiculo.Status == StatusVeiculo.EmManutencao)
        {
            return Resultado<Reserva>.Falha("Este veículo está em manutenção.");
        }

        // Confere pelo banco, e não pela tela, que o cliente existe e está ativo.
        var usuario = _usuarios.ObterPorId(clienteId);
        if (usuario is not Cliente cliente)
        {
            return Resultado<Reserva>.Falha("Cliente não encontrado.");
        }

        if (!cliente.Ativo)
        {
            return Resultado<Reserva>.Falha("Este cliente está desativado.");
        }

        // Confere de novo, porque outra pessoa pode ter reservado o veículo depois da busca.
        if (_reservas.ExisteConflito(veiculoId, inicio, fim, agora))
        {
            return Resultado<Reserva>.Falha("Este veículo acabou de ser reservado nesse período. Faça uma nova busca.");
        }

        // Uma reserva é sempre de um único veículo. O status inicial é Agendada.
        var reserva = new Reserva
        {
            ClienteId = cliente.Id,
            VeiculoId = veiculo.Id,
            DataInicio = inicio,
            DataFim = fim
        };

        _reservas.Adicionar(reserva);
        _unidadeDeTrabalho.Salvar();
        return Resultado<Reserva>.Ok(reserva);
    }

    public List<Reserva> ListarDoCliente(int clienteId)
    {
        ExpirarNaoComparecidas();
        return _reservas.ListarPorCliente(clienteId);
    }

    /// <summary>A reserva, só se for do cliente. Para outro cliente, ela "não existe".</summary>
    public Reserva? ObterDoCliente(int reservaId, int clienteId)
    {
        ExpirarNaoComparecidas();
        var reserva = _reservas.ObterPorId(reservaId);
        return reserva?.ClienteId == clienteId ? reserva : null;
    }

    /// <summary>Qualquer reserva, para os funcionários.</summary>
    public Reserva? Obter(int reservaId)
    {
        ExpirarNaoComparecidas();
        return _reservas.ObterPorId(reservaId);
    }

    public Resultado Cancelar(int reservaId, int clienteId)
    {
        var reserva = _reservas.ObterPorId(reservaId);
        if (reserva is null || reserva.ClienteId != clienteId)
        {
            return Resultado.Falha("Reserva não encontrada.");
        }

        if (reserva.Status != StatusReserva.Agendada)
        {
            return Resultado.Falha("Só uma reserva agendada pode ser cancelada.");
        }

        if (reserva.DataInicio <= DateTime.Now)
        {
            return Resultado.Falha("A reserva só pode ser cancelada antes do horário de retirada.");
        }

        reserva.Cancelar();
        _unidadeDeTrabalho.Salvar();
        return Resultado.Ok();
    }

    public List<Reserva> ListarVisitasDeHoje()
    {
        ExpirarNaoComparecidas();
        return _reservas.ListarVisitasDoDia(DateTime.Today);
    }

    /// <summary>
    /// Expira as reservas agendadas cujo cliente não compareceu no prazo.
    /// Serve para mostrar o status certo: a disponibilidade já ignora essas reservas no OcupaVeiculo.
    /// </summary>
    public void ExpirarNaoComparecidas()
    {
        var limite = DateTime.Now.AddHours(-Reserva.PrazoComparecimentoHoras);
        var vencidas = _reservas.ListarAgendadasVencidas(limite);

        if (vencidas.Count == 0)
        {
            return;
        }

        foreach (var reserva in vencidas)
        {
            reserva.Expirar();
        }

        _unidadeDeTrabalho.Salvar();
    }

    private static string? ValidarPeriodo(DateTime inicio, DateTime fim, DateTime agora)
    {
        if (inicio <= agora)
        {
            return "A retirada deve ser em uma data futura.";
        }

        return fim <= inicio ? "A devolução deve ser depois da retirada." : null;
    }
}
