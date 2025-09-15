using System.Windows;

namespace WpfApp.Views.Pessoas
{
    public partial class NovaPessoaView : Window
    {
        public NovaPessoaView()
        {
            InitializeComponent();
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
