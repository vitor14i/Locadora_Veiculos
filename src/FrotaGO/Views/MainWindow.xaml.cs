using System.Windows;
using FrotaGO.Data;

namespace FrotaGO.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Tela provisória, apenas para conferir o seed. Será substituída pelo login/telas.
        using var db = new FrotaGODbContext();
        TxtResumo.Text = $"Usuários: {db.Usuarios.Count()} | Veículos: {db.Veiculos.Count()}";
    }
}
