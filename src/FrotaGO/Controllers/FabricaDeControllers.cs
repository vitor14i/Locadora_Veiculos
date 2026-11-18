using FrotaGO.Data;
using FrotaGO.Repositories;

namespace FrotaGO.Controllers;

/// <summary>Monta cada controller com o contexto, os repositórios e a unidade de trabalho.</summary>
public static class FabricaDeControllers
{
    public static ContaController CriarContaController()
    {
        var contexto = new FrotaGODbContext();
        return new ContaController(new UsuarioRepository(contexto), new UnidadeDeTrabalho(contexto));
    }

    public static VeiculoController CriarVeiculoController()
    {
        var contexto = new FrotaGODbContext();
        return new VeiculoController(new VeiculoRepository(contexto), new UnidadeDeTrabalho(contexto));
    }

    /// <summary>Os quatro objetos usam o mesmo contexto, então um único Salvar() grava tudo.</summary>
    public static ManutencaoController CriarManutencaoController()
    {
        var contexto = new FrotaGODbContext();
        return new ManutencaoController(
            new VeiculoRepository(contexto),
            new ManutencaoRepository(contexto),
            new UsuarioRepository(contexto),
            new UnidadeDeTrabalho(contexto));
    }

    /// <summary>Os quatro objetos usam o mesmo contexto, então um único Salvar() grava tudo.</summary>
    public static ReservaController CriarReservaController()
    {
        var contexto = new FrotaGODbContext();
        return new ReservaController(
            new ReservaRepository(contexto),
            new VeiculoRepository(contexto),
            new UsuarioRepository(contexto),
            new UnidadeDeTrabalho(contexto));
    }

    /// <summary>Os três objetos usam o mesmo contexto, então um único Salvar() grava tudo.</summary>
    public static LocacaoController CriarLocacaoController()
    {
        var contexto = new FrotaGODbContext();
        return new LocacaoController(
            new ReservaRepository(contexto),
            new ContratoRepository(contexto),
            new UnidadeDeTrabalho(contexto));
    }
}
