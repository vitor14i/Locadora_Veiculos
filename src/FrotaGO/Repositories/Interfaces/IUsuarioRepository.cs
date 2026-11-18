using FrotaGO.Models;

namespace FrotaGO.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Usuario? ObterPorEmail(string email);

    bool EmailExiste(string email, int? ignorarUsuarioId = null);

    bool CpfExiste(string cpf);

    bool CnpjExiste(string cnpj);

    List<Cliente> ListarClientesAtivos();

    void Adicionar(Usuario usuario);
}
