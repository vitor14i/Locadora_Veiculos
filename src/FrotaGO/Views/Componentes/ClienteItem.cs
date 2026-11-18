namespace FrotaGO.Views.Componentes;

/// <summary>
/// Cliente exibido na escolha da busca e no resumo da reserva feita na loja.
/// É um record para a comparação ser pelo valor: ao voltar da revisão, o mesmo cliente
/// continua selecionado na lista.
/// </summary>
public record ClienteItem(int Id, string Nome, string Documento)
{
    public string Descricao => $"{Nome} · {Documento}";
}
