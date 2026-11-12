using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrotaGO.Views;

/// <summary>Dados do usuário logado usados pela interface.</summary>
public record UsuarioSessao(string Nome, string Email, string Perfil)
{
    public bool EhGerente => Perfil == "Gerente";
    public bool EhCliente => Perfil == "Cliente";
}
