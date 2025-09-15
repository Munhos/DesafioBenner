using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfApp.Models;
using WpfApp.Services;
using WpfApp.Views.Produtos;

namespace WpfApp.ViewModels
{
    public class ProdutoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnChanged(string nome) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));

        private readonly ProdutoService _service = new ProdutoService();

        public ObservableCollection<Produto> Produtos { get; set; } = new();
        private ObservableCollection<Produto> _produtosFiltrados = new();
        public ObservableCollection<Produto> ProdutosFiltrados
        {
            get => _produtosFiltrados;
            set { _produtosFiltrados = value; OnChanged(nameof(ProdutosFiltrados)); }
        }

        private Produto? _produtoSelecionado;
        public Produto? ProdutoSelecionado
        {
            get => _produtoSelecionado;
            set
            {
                _produtoSelecionado = value;
                if (value != null)
                {
                    Nome = value.Nome;
                    Codigo = value.Codigo;
                    ValorText = value.Valor.ToString();
                }
                else
                {
                    Nome = Codigo = ValorText = "";
                }

                OnChanged(nameof(ProdutoSelecionado));
                OnChanged(nameof(BotaoTexto));
                OnChanged(nameof(Nome));
                OnChanged(nameof(Codigo));
                OnChanged(nameof(ValorText));
            }
        }

        public string Nome { get; set; } = "";
        public string Codigo { get; set; } = "";
        public string ValorText { get; set; } = "";
        public double Valor { get; set; } = 0;

        public string BotaoTexto => ProdutoSelecionado != null ? "Atualizar" : "Salvar";

        private string _filtroNome = "";
        public string FiltroNome
        {
            get => _filtroNome;
            set { _filtroNome = value; OnChanged(nameof(FiltroNome)); FiltrarProdutos(); }
        }

        private string _filtroCodigo = "";
        public string FiltroCodigo
        {
            get => _filtroCodigo;
            set { _filtroCodigo = value; OnChanged(nameof(FiltroCodigo)); FiltrarProdutos(); }
        }

        private double? _filtroValorMin;
        public double? FiltroValorMin
        {
            get => _filtroValorMin;
            set { _filtroValorMin = value; OnChanged(nameof(FiltroValorMin)); FiltrarProdutos(); }
        }

        private double? _filtroValorMax;
        public double? FiltroValorMax
        {
            get => _filtroValorMax;
            set { _filtroValorMax = value; OnChanged(nameof(FiltroValorMax)); FiltrarProdutos(); }
        }

        public ICommand AdicionarOuAtualizarCommand { get; }
        public ICommand AbrirNovoProdutoCommand { get; }
        public ICommand EditarProdutoCommand { get; }
        public ICommand ExcluirProdutoCommand { get; }

        public ProdutoViewModel()
        {
            Produtos = new ObservableCollection<Produto>(_service.CarregarProdutos());
            ProdutosFiltrados = new ObservableCollection<Produto>(Produtos);

            AdicionarOuAtualizarCommand = new RelayCommand(_ =>
            {
                if (string.IsNullOrWhiteSpace(Nome))
                {
                    MessageBox.Show("Preencha um nome");
                    return;
                }

                if (string.IsNullOrWhiteSpace(Codigo))
                {
                    MessageBox.Show("Preencha um código");
                    return;
                }

                if (!double.TryParse(ValorText, out double valor))
                {
                    MessageBox.Show("Valor inválido");
                    return;
                }
                Valor = valor;

                if (ProdutoSelecionado != null)
                {
                    ProdutoSelecionado.Nome = Nome;
                    ProdutoSelecionado.Codigo = Codigo;
                    ProdutoSelecionado.Valor = Valor;
                    _service.SalvarOuAtualizarProduto(ProdutoSelecionado);

                    var index = Produtos.IndexOf(ProdutoSelecionado);
                    if (index >= 0) Produtos[index] = ProdutoSelecionado;
                }
                else
                {
                    var novo = new Produto
                    {
                        Nome = Nome,
                        Codigo = Codigo,
                        Valor = Valor
                    };
                    Produtos.Add(novo);
                    _service.SalvarOuAtualizarProduto(novo);
                }

                LimparCampos();
                FiltrarProdutos();
            });

            AbrirNovoProdutoCommand = new RelayCommand(_ =>
            {
                ProdutoSelecionado = null;
                AbrirEdicao();
            });

            EditarProdutoCommand = new RelayCommand(_ =>
            {
                if (ProdutoSelecionado != null)
                    AbrirEdicao();
            });

            ExcluirProdutoCommand = new RelayCommand(_ =>
            {
                if (ProdutoSelecionado != null)
                {
                    _service.ExcluirProduto(ProdutoSelecionado.Id);
                    Produtos.Remove(ProdutoSelecionado);
                    LimparCampos();
                    FiltrarProdutos();
                }
            });
        }

        private void AbrirEdicao()
        {
            var window = new NovoProdutoView { DataContext = this };
            window.ShowDialog();
        }

        private void LimparCampos()
        {
            Nome = Codigo = ValorText = "";
            ProdutoSelecionado = null;
            OnChanged(nameof(Nome));
            OnChanged(nameof(Codigo));
            OnChanged(nameof(ValorText));
        }

        private void FiltrarProdutos()
        {
            var query = Produtos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FiltroNome))
                query = query.Where(p => p.Nome.Contains(FiltroNome, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(FiltroCodigo))
                query = query.Where(p => p.Codigo.Contains(FiltroCodigo, StringComparison.OrdinalIgnoreCase));

            if (FiltroValorMin.HasValue)
                query = query.Where(p => p.Valor >= FiltroValorMin.Value);

            if (FiltroValorMax.HasValue)
                query = query.Where(p => p.Valor <= FiltroValorMax.Value);

            ProdutosFiltrados = new ObservableCollection<Produto>(query);
        }
    }
}
