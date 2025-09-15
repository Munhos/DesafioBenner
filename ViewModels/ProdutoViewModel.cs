using System.Collections.ObjectModel;
using System.ComponentModel;
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

        // Campos para edição
        public string Nome { get; set; } = "";
        public string Codigo { get; set; } = "";
        public string ValorText { get; set; } = "";
        public double Valor { get; set; } = 0;

        public string BotaoTexto => ProdutoSelecionado != null ? "Atualizar" : "Salvar";

        // Comandos
        public ICommand AdicionarOuAtualizarCommand { get; }
        public ICommand AbrirNovoProdutoCommand { get; }
        public ICommand EditarProdutoCommand { get; }
        public ICommand ExcluirProdutoCommand { get; }

        public ProdutoViewModel()
        {
            Produtos = new ObservableCollection<Produto>(_service.CarregarProdutos());

            AdicionarOuAtualizarCommand = new RelayCommand(_ =>
            {
                if (Nome == "")
                {
                    MessageBox.Show("Preencha um nome");
                    return;
                }

                if (Codigo == "")
                {
                    MessageBox.Show("Preencha um codigo");
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
    }
}
