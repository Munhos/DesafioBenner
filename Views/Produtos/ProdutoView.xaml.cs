using System.Windows.Controls;
using WpfApp.ViewModels;

namespace WpfApp.Views.Produtos
{
    public partial class ProdutoView : UserControl
    {
        public ProdutoView()
        {
            InitializeComponent();
            DataContext = new ProdutoViewModel(); 
        }
    }
}
