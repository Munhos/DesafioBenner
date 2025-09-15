using System.Windows.Controls;
using WpfApp.Models;
using WpfApp.ViewModels;

namespace WpfApp.Views
{
    public partial class PessoaView : UserControl
    {
        public PessoaView()
        {
            InitializeComponent();
            DataContext = new PessoaViewModel();
        }

    }
}
