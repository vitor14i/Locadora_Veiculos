namespace FrotaGO.Controllers;

public class Resultado
{
    protected Resultado(bool sucesso, string erro)
    {
        Sucesso = sucesso;
        Erro = erro;
    }

    public bool Sucesso { get; }

    /// <summary>Mensagem para a tela quando a operação falha. Fica vazia no sucesso.</summary>
    public string Erro { get; }

    public static Resultado Ok() => new(true, string.Empty);

    public static Resultado Falha(string mensagem) => new(false, mensagem);
}

public class Resultado<T> : Resultado
{
    private readonly T? _valor;

    private Resultado(bool sucesso, T? valor, string erro) : base(sucesso, erro) => _valor = valor;

    /// <summary>Valor da operação. Só pode ser lido quando <see cref="Resultado.Sucesso"/> é verdadeiro.</summary>
    public T Valor => Sucesso
        ? _valor!
        : throw new InvalidOperationException("Uma operação com falha não tem valor.");

    public static Resultado<T> Ok(T valor) => new(true, valor, string.Empty);

    public static new Resultado<T> Falha(string mensagem) => new(false, default, mensagem);
}
