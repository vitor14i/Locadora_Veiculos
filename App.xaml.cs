using Locadora_Veiculos.Controllers;
using Locadora_Veiculos.Data;
using Locadora_Veiculos.Interfaces;
using Locadora_Veiculos.Models;
using Locadora_Veiculos.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
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
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();

            var connectionString = configuration.GetConnectionString("LocadoraDb")
                ?? throw new InvalidOperationException("Connection string 'LocadoraDb' não encontrada.");
            services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString));
            services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
            services.AddOptions<TarifasConfig>();

            services.AddKeyedTransient<ITarifa, Carro>(typeof(Carro));
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
