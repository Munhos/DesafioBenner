using System.Windows;
using System.Windows.Controls;
using WpfApp.Models;
using WpfApp.ViewModels;

namespace WpfApp.Views.Pedidos
{
    public partial class NovoPedidoView : Window
    {
        public NovoPedidoView()
        {
            InitializeComponent();
        }

        private void ProdutosListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is PedidosViewModel vm)
            {
                vm.ProdutosSelecionados.Clear();
                foreach (Produto p in ProdutosListBox.SelectedItems)
                    vm.ProdutosSelecionados.Add(p);
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
