using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using WpfApp.Models;
using WpfApp.Services;

namespace WpfApp.ViewModels
{
    public class PessoaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void OnChanged(string nome) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));

        private readonly PessoaService _service = new PessoaService();

        public ObservableCollection<Pessoa> Pessoas { get; set; } = new();

        public string Nome { get; set; } = "";
        public string CPF { get; set; } = "";
        public string Endereco { get; set; } = "";

        private Pessoa? _pessoaSelecionada;
        public Pessoa? PessoaSelecionada
        {
            get => _pessoaSelecionada;
            set
            {
                _pessoaSelecionada = value;
                if (value != null)
                {
                    Nome = value.Nome;
                    CPF = value.CPF;
                    Endereco = value.Endereco ?? "";
                }
                else
                {
                    Nome = CPF = Endereco = "";
                }
                OnChanged(nameof(PessoaSelecionada));
                OnChanged(nameof(BotaoTexto));
                OnChanged(nameof(Nome));
                OnChanged(nameof(CPF));
                OnChanged(nameof(Endereco));
            }
        }

        public string BotaoTexto => PessoaSelecionada != null ? "Atualizar" : "Salvar";

        public object? ConteudoPessoa { get; set; }
        public bool IsNovaPessoaAberta => ConteudoPessoa != null;

        // Comandos
        public ICommand AdicionarOuAtualizarCommand { get; }
        public ICommand AbrirNovaPessoaCommand { get; }
        public ICommand FecharNovaPessoaCommand { get; }
        public ICommand EditarPessoaCommand { get; }
        public ICommand ExcluirPessoaCommand { get; }

        public PessoaViewModel()
        {
            Pessoas = new ObservableCollection<Pessoa>(_service.CarregarPessoas());

            // Salvar somente ao clicar em Salvar
            AdicionarOuAtualizarCommand = new RelayCommand(_ =>
            {
                if (Nome == "")
                {
                    MessageBox.Show("Preencha um nome");
                    return;
                }

                if (CPF == "")
                {
                    MessageBox.Show("Preencha um CPF");
                    return;
                }

                if (PessoaSelecionada != null)
                {
                    // Atualiza a pessoa selecionada
                    PessoaSelecionada.Nome = Nome;
                    PessoaSelecionada.CPF = CPF;
                    PessoaSelecionada.Endereco = Endereco;

                    _service.SalvarOuAtualizarPessoa(PessoaSelecionada);

                    // Atualiza a coleção para refletir mudanças no DataGrid
                    var index = Pessoas.IndexOf(PessoaSelecionada);
                    if (index >= 0) Pessoas[index] = PessoaSelecionada;
                }
                else
                {
                    // Cria nova pessoa
                    var nova = new Pessoa
                    {
                        Nome = Nome,
                        CPF = CPF,
                        Endereco = Endereco
                    };
                    Pessoas.Add(nova);
                    _service.SalvarOuAtualizarPessoa(nova);
                }

                LimparCampos();
            });

            // Abrir nova pessoa
            AbrirNovaPessoaCommand = new RelayCommand(_ =>
            {
                PessoaSelecionada = null;
                AbrirEdicao();
            });

            // Fechar janela de edição
            FecharNovaPessoaCommand = new RelayCommand(_ => ConteudoPessoa = null);

            // Editar pessoa selecionada
            EditarPessoaCommand = new RelayCommand(_ =>
            {
                if (PessoaSelecionada != null)
                    AbrirEdicao();
            });

            // Excluir pessoa selecionada
            ExcluirPessoaCommand = new RelayCommand(_ =>
            {
                if (PessoaSelecionada != null)
                {
                    _service.ExcluirPessoa(PessoaSelecionada.Id);
                    Pessoas.Remove(PessoaSelecionada);
                    LimparCampos();
                }
            });
        }

        private void AbrirEdicao()
        {
            ConteudoPessoa = new Views.Pessoas.NovaPessoaView { DataContext = this };
            ((Window)ConteudoPessoa).ShowDialog();
        }

        private void LimparCampos()
        {
            Nome = CPF = Endereco = "";
            PessoaSelecionada = null;
            ConteudoPessoa = null;
            OnChanged(nameof(Nome));
            OnChanged(nameof(CPF));
            OnChanged(nameof(Endereco));
        }
    }

}
