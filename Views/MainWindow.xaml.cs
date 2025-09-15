using System.Windows;
using WpfApp.Views;
using WpfApp.Views.Produtos;
using System.Windows.Controls;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainContent.Content = new PedidoView();
        }

        private void PessoasScreenClick(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new PessoaView();
        }

        private void ProdutosScreenClick(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new ProdutoView();
        }

        private void PedidosScreenClick(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new PedidoView();
        }

        public void TrocarConteudo(UserControl novaView)
        {
            MainContent.Content = novaView;
        }

    }
}
