namespace FrotaGO.Repositories.Interfaces;

/// <summary>Grava de uma vez tudo o que os repositórios alteraram em uma operação.</summary>
public interface IUnidadeDeTrabalho
{
    void Salvar();
}
