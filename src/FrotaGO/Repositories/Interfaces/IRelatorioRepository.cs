using FrotaGO.Repositories.Resultados;

namespace FrotaGO.Repositories.Interfaces;

public interface IRelatorioRepository
{
    ResumoFaturamento ObterResumo(DateTime inicio, DateTime fimExclusivo);

    List<VeiculoMaisLocado> ObterVeiculosMaisLocados(DateTime inicio, DateTime fimExclusivo);
}
