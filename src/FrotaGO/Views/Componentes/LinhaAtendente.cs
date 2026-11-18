namespace FrotaGO.Views.Componentes;

public record LinhaAtendente(int Id, string Nome, string Email, string Matricula, string Status)
{
    public string Acao => Status == "Ativo" ? "Desativar" : "Reativar";
}
