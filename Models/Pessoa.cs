using System.ComponentModel;

namespace WpfApp.Models
{
    public class Pessoa : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string nome) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));

        private int _id;
        public int Id { get => _id; set { _id = value; OnPropertyChanged(nameof(Id)); } }

        private string _nome = "";
        public string Nome { get => _nome; set { _nome = value; OnPropertyChanged(nameof(Nome)); } }

        private string _cpf = "";
        public string CPF { get => _cpf; set { _cpf = value; OnPropertyChanged(nameof(CPF)); } }

        private string? _endereco;
        public string? Endereco { get => _endereco; set { _endereco = value; OnPropertyChanged(nameof(Endereco)); } }
    }
}
