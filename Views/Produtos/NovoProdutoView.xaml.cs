using System.Windows;

namespace WpfApp.Views.Produtos
{
    public partial class NovoProdutoView : Window
    {
        public NovoProdutoView()
        {
            InitializeComponent();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
