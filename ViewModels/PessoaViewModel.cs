using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfApp.Models;
using WpfApp.Services;
using WpfApp.Views;
using WpfApp.Views.Pessoas;

namespace WpfApp.ViewModels
{
    public class PessoaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnChanged(string nome) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));

        private readonly PessoaService _service = new PessoaService();

        public ObservableCollection<Pessoa> _pessoas = new();
        public ObservableCollection<Pessoa> Pessoas
        {
            get => _pessoas;
            set
            {
                _pessoas = value;
                OnChanged(nameof(Pessoas));
                FiltrarPessoas();
            }
        }
       
        private ObservableCollection<Pessoa> _pessoasFiltradas = new();
        public ObservableCollection<Pessoa> PessoasFiltradas
        {
            get => _pessoasFiltradas;
            set 
            { 
                _pessoasFiltradas = value; 
                OnChanged(nameof(PessoasFiltradas)); 
            }
        }

        private string _filtroNome = "";
        public string FiltroNome
        {
            get => _filtroNome;
            set 
            { 
                _filtroNome = value; 
                OnChanged(nameof(FiltroNome)); 
                FiltrarPessoas(); 
            }
        }

        private string _filtroCPF = "";
        public string FiltroCPF
        {
            get => _filtroCPF;
            set 
            { 
                _filtroCPF = value; 
                OnChanged(nameof(FiltroCPF)); 
                FiltrarPessoas(); 
            }
        }

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
                OnChanged(nameof(Nome));
                OnChanged(nameof(CPF));
                OnChanged(nameof(Endereco));
            }
        }

        public object? ConteudoPessoa { get; set; }
        public bool IsNovaPessoaAberta => ConteudoPessoa != null;

        public ICommand AdicionarOuAtualizarCommand { get; }
        public ICommand AbrirNovaPessoaCommand { get; }
        public ICommand FecharNovaPessoaCommand { get; }
        public ICommand EditarPessoaCommand { get; }
        public ICommand ExcluirPessoaCommand { get; }
        public ICommand NovoPedidoPessoaCommand { get; }

        public PessoaViewModel()
        {
            Pessoas = new ObservableCollection<Pessoa>(_service.CarregarPessoas());
            FiltrarPessoas();

            AdicionarOuAtualizarCommand = new RelayCommand(_ =>
            {
                if (string.IsNullOrWhiteSpace(Nome))
                {
                    MessageBox.Show("Preencha um nome");
                    return;
                }

                if (string.IsNullOrWhiteSpace(CPF))
                {
                    MessageBox.Show("Preencha um CPF");
                    return;
                }

                if (!IsCpfValido(CPF))
                {
                    MessageBox.Show("CPF inválido");
                    return;
                }

                if (PessoaSelecionada != null)
                {
                    PessoaSelecionada.Nome = Nome;
                    PessoaSelecionada.CPF = CPF;
                    PessoaSelecionada.Endereco = Endereco;

                    _service.SalvarOuAtualizarPessoa(PessoaSelecionada);

                    var index = Pessoas.IndexOf(PessoaSelecionada);
                    if (index >= 0) Pessoas[index] = PessoaSelecionada;
                }
                else
                {
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
                FiltrarPessoas();
            });


            AbrirNovaPessoaCommand = new RelayCommand(_ =>
            {
                PessoaSelecionada = null;
                AbrirEdicao();
            });

            FecharNovaPessoaCommand = new RelayCommand(_ => ConteudoPessoa = null);

            EditarPessoaCommand = new RelayCommand(_ =>
            {
                if (PessoaSelecionada != null)
                    AbrirEdicao();
            });

            ExcluirPessoaCommand = new RelayCommand(_ =>
            {
                if (PessoaSelecionada != null)
                {
                    _service.ExcluirPessoa(PessoaSelecionada.Id);
                    Pessoas.Remove(PessoaSelecionada);
                    LimparCampos();
                    FiltrarPessoas();
                }
            });

            NovoPedidoPessoaCommand = new RelayCommand(_ =>
            {
                if (PessoaSelecionada != null)
                {
                    var pedidosVM = new PedidosViewModel
                    {
                        PessoaSelecionada = PessoaSelecionada
                    };

                    var pedidosView = new PedidoView
                    {
                        DataContext = pedidosVM
                    };

                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    mainWindow?.TrocarConteudo(pedidosView);
                }
            });
        }

        private void AbrirEdicao()
        {
            var window = new NovaPessoaView { DataContext = this };
            window.ShowDialog();
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

        private void FiltrarPessoas()
        {
            var query = Pessoas.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FiltroNome))
                query = query.Where(p => p.Nome.Contains(FiltroNome, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(FiltroCPF))
                query = query.Where(p => p.CPF.Contains(FiltroCPF, StringComparison.OrdinalIgnoreCase));

            PessoasFiltradas = new ObservableCollection<Pessoa>(query);
        }

        private bool IsCpfValido(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;

            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11) return false;
            if (new string(cpf[0], 11) == cpf) return false;

            int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;
            for (int i = 0; i < 9; i++) soma += int.Parse(tempCpf[i].ToString()) * mult1[i];
            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            tempCpf += resto;

            soma = 0;
            for (int i = 0; i < 10; i++) soma += int.Parse(tempCpf[i].ToString()) * mult2[i];
            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            return cpf.EndsWith(resto.ToString());
        }

    }
}
