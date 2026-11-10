using Locadora_Veiculos.Controllers;
using System.Windows;

namespace Locadora_Veiculos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AuthController AuthController { get; }
        public LocacaoController LocacaoController { get; }
        public RelatorioController RelatorioController { get; }
        public DevolucaoController DevolucaoController { get; }

        /// <summary>
        /// A View recebe suas dependências pelo construtor, sem criar Controllers ou regras concretas.
        /// </summary>
        public MainWindow(
            AuthController authController,
            LocacaoController locacaoController,
            RelatorioController relatorioController,
            DevolucaoController devolucaoController)
        {
            AuthController = authController;
            LocacaoController = locacaoController;
            RelatorioController = relatorioController;
            DevolucaoController = devolucaoController;

            InitializeComponent();
        }
    }
}