using FrotaGO.Models;

namespace FrotaGO.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Usuario? ObterPorEmail(string email);

    Usuario? ObterPorId(int id);

    bool EmailExiste(string email, int? ignorarUsuarioId = null);

    bool CpfExiste(string cpf);

    bool CnpjExiste(string cnpj);

    List<Cliente> ListarClientesAtivos();

    /// <summary>Todos os atendentes, ativos e desativados, em ordem de nome. Só leitura.</summary>
    List<Atendente> ListarAtendentes();

    /// <summary>Procura em todos os funcionários, gerentes incluídos. Recebe a matrícula já normalizada.</summary>
    bool MatriculaExiste(string matricula);

    void Adicionar(Usuario usuario);
}
