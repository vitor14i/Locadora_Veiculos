using FrotaGO.Models;

namespace FrotaGO.Controllers;

/// <summary>Campos do formulário de devolução.</summary>
/// <param name="ValorAvaria">Só é usado quando <paramref name="TemAvaria"/> é verdadeiro.</param>
/// <param name="TiposCancelados">Tipos de multa cancelados pelo gerente, como "Atraso" ou "Avaria".</param>
/// <param name="Pagamento">Obrigatório só quando há valor a pagar. O valor é definido pelo controller.</param>
public record DadosDevolucao(
    int ContratoId,
    DateTime DataDevolucao,
    decimal KmChegada,
    string Observacoes,
    bool TemAvaria,
    decimal ValorAvaria,
    IReadOnlyCollection<string> TiposCancelados,
    decimal Desconto,
    bool EnviarParaManutencao,
    Pagamento? Pagamento);
