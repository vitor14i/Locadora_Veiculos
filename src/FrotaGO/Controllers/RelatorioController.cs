using FrotaGO.Repositories.Interfaces;

namespace FrotaGO.Controllers;

public class RelatorioController
{
    private readonly IRelatorioRepository _relatorios;

    // Só leitura: não precisa da unidade de trabalho.
    public RelatorioController(IRelatorioRepository relatorios) => _relatorios = relatorios;

    /// <summary>
    /// Valida o período e monta o relatório. As datas valem como dias inteiros: o limite final é
    /// exclusivo, no início do dia seguinte ao fim, para o dia final entrar inteiro.
    /// </summary>
    public Resultado<RelatorioPeriodo> Gerar(DateTime inicio, DateTime fim)
    {
        var inicioDoPeriodo = inicio.Date;
        var fimDoPeriodo = fim.Date;

        if (fimDoPeriodo < inicioDoPeriodo)
        {
            return Resultado<RelatorioPeriodo>.Falha("A data final deve ser igual ou posterior à inicial.");
        }

        var fimExclusivo = fimDoPeriodo.AddDays(1);
        var resumo = _relatorios.ObterResumo(inicioDoPeriodo, fimExclusivo);
        var ranking = _relatorios.ObterVeiculosMaisLocados(inicioDoPeriodo, fimExclusivo);

        // Sem locações no período, o valor médio fica zero, em vez de dividir por zero.
        var valorMedio = resumo.Locacoes == 0
            ? 0
            : Math.Round(resumo.Faturamento / resumo.Locacoes, 2, MidpointRounding.AwayFromZero);

        return Resultado<RelatorioPeriodo>.Ok(new RelatorioPeriodo(resumo, valorMedio, ranking));
    }
}
