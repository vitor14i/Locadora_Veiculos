using FrotaGO.Data;
using FrotaGO.Models;
using FrotaGO.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FrotaGO.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly FrotaGODbContext _contexto;

    public UsuarioRepository(FrotaGODbContext contexto) => _contexto = contexto;

    public Usuario? ObterPorEmail(string email) =>
        _contexto.Usuarios.FirstOrDefault(usuario => usuario.Email == email);

    public bool EmailExiste(string email, int? ignorarUsuarioId = null) =>
        _contexto.Usuarios.Any(usuario =>
            usuario.Email == email &&
            (ignorarUsuarioId == null || usuario.Id != ignorarUsuarioId));

    public bool CpfExiste(string cpf) =>
        _contexto.PessoasFisicas.Any(pessoa => pessoa.Cpf == cpf);

    public bool CnpjExiste(string cnpj) =>
        _contexto.PessoasJuridicas.Any(empresa => empresa.Cnpj == cnpj);

    public List<Cliente> ListarClientesAtivos() =>
        _contexto.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.Ativo)
            .ToList();

    public void Adicionar(Usuario usuario) => _contexto.Usuarios.Add(usuario);
}
