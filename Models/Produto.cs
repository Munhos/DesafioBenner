using System.ComponentModel;

namespace WpfApp.Models
{
    public class Produto : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string nome) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));

        private int _id;
        public int Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }

        private string _nome = "";
        public string Nome { get => _nome; set { _nome = value; OnPropertyChanged(nameof(Nome)); } }

        private string _codigo = "";
        public string Codigo { get => _codigo; set { _codigo = value; OnPropertyChanged(nameof(Codigo)); } }

        private double _valor;
        public double Valor { get => _valor; set { _valor = value; OnPropertyChanged(nameof(Valor)); } }
    }
}
