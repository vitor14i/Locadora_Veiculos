using System.Windows;
using FrotaGO.Data;
using Microsoft.EntityFrameworkCore;

namespace FrotaGO;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        if (!TentarInicializarBanco())
        {
            Shutdown(1);
            return;
        }

        base.OnStartup(e);
    }

    /// <summary>Aplica migrations pendentes e popula o banco se estiver vazio.</summary>
    private static bool TentarInicializarBanco()
    {
        try
        {
            using var context = new FrotaGODbContext();
            context.Database.Migrate();
            DbSeeder.Seed(context);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Não foi possível abrir o banco de dados.\n\n" +
                "Verifique se o SQL Server LocalDB está instalado e se as migrations foram geradas " +
                "(Add-Migration / Update-Database).\n\n" +
                $"Detalhe: {ex.GetBaseException().Message}",
                "FrotaGO - Erro de banco de dados",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }
    }
}
