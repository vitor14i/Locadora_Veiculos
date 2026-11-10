using Locadora_Veiculos.Controllers;
using Locadora_Veiculos.Data;
using Locadora_Veiculos.Interfaces;
using Locadora_Veiculos.Models;
using Locadora_Veiculos.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Locadora_Veiculos
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; } = null!;

        public App()
        {
            var services = new ServiceCollection();

            services.AddDbContextFactory<AppDbContext>();
            services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
            services.AddOptions<TarifasConfig>();

            services.AddKeyedTransient<ITarifa, CalculadoraTarifaCarro>(typeof(Carro));
            services.AddKeyedTransient<ITarifa, CalculadoraTarifaMoto>(typeof(Moto));
            services.AddTransient<ITarifaResolver, TarifaResolver>();
            services.AddTransient<IMultaAtraso, CalculadoraMultaAtraso>();

            services.AddTransient<AuthController>();
            services.AddTransient<LocacaoController>();
            services.AddTransient<RelatorioController>();
            services.AddTransient<DevolucaoController>();
            services.AddTransient<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var janelaPrincipal = ServiceProvider.GetRequiredService<MainWindow>();
            MainWindow = janelaPrincipal;
            janelaPrincipal.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }
    }

}
