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
}
