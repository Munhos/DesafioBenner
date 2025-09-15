using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfApp.Models;
using WpfApp.Services;
using WpfApp.Views.Pedidos;

namespace WpfApp.ViewModels
{
    internal class PedidosViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string nome) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));

        private readonly PessoaService _pessoaService = new();
        private readonly ProdutoService _produtoService = new();
        private readonly PedidosService _pedidosService = new();

        public ObservableCollection<Pessoa> Pessoas { get; set; }
        public ObservableCollection<Pedido> Pedidos { get; set; } = new();
        public ObservableCollection<Pedido> PedidosFiltrados { get; set; } = new();
        public ObservableCollection<Produto> ProdutosDisponiveis { get; set; }
        public ObservableCollection<Produto> ProdutosSelecionados { get; set; } = new();

        private Pessoa? _pessoaSelecionada;
        public Pessoa? PessoaSelecionada
        {
            get => _pessoaSelecionada;
            set
            {
                _pessoaSelecionada = value;
                OnPropertyChanged(nameof(PessoaSelecionada));
                FiltrarPedidosPorPessoa();
            }
        }

        private Pedido? _pedidoSelecionado;
        public Pedido? PedidoSelecionado
        {
            get => _pedidoSelecionado;
            set
            {
                _pedidoSelecionado = value;
                OnPropertyChanged(nameof(PedidoSelecionado));
            }
        }

        public FormasPagamento FormaPagamento { get; set; } = FormasPagamento.Dinheiro;
        public AllStatus Status { get; set; } = AllStatus.Pendente;

        public decimal ValorTotal => PedidosFiltrados.Sum(p => (decimal)p.ValorTotal);
        public decimal ValorSelecionados => ProdutosSelecionados.Sum(p => (decimal)p.Valor);

        public Array FormasPagamentoPossiveis => Enum.GetValues(typeof(FormasPagamento));
        public Array StatusPossiveis => Enum.GetValues(typeof(AllStatus));

        public ICommand SalvarPedidoCommand { get; }
        public ICommand ExcluirPedidoCommand { get; }
        public ICommand AdicionarPedidoCommand { get; }
        public ICommand EditarPedidoCommand { get; }

        public object? ConteudoPedido { get; set; }

        public PedidosViewModel()
        {
            Pessoas = new ObservableCollection<Pessoa>(_pessoaService.CarregarPessoas());
            ProdutosDisponiveis = new ObservableCollection<Produto>(_produtoService.CarregarProdutos());
            AtualizarPedidos();

            SalvarPedidoCommand = new RelayCommand(_ => SalvarPedido());
            ExcluirPedidoCommand = new RelayCommand(obj =>
            {
                if (obj is Pedido pedido)
                {
                    _pedidosService.ExcluirPedido(pedido.Id);
                    AtualizarPedidos();
                }
            });

            AdicionarPedidoCommand = new RelayCommand(_ => AbrirEdicao(null));
            EditarPedidoCommand = new RelayCommand(_ => { if (PedidoSelecionado != null) AbrirEdicao(PedidoSelecionado); });

            ProdutosSelecionados.CollectionChanged += (_, __) => OnPropertyChanged(nameof(ValorSelecionados));
        }

        private void SalvarPedido()
        {
            if (PessoaSelecionada == null)
            {
                MessageBox.Show("Selecione uma pessoa.");
                return;
            }
            if (!ProdutosSelecionados.Any())
            {
                MessageBox.Show("Selecione pelo menos um produto.");
                return;
            }

            Pedido pedidoParaSalvar;

            if (PedidoSelecionado == null) // novo pedido
            {
                pedidoParaSalvar = new Pedido
                {
                    Pessoa = PessoaSelecionada.Nome,
                    Produtos = ProdutosSelecionados.ToList(),
                    ValorTotal = ValorSelecionados,
                    DataVenda = DateTime.Now,
                    FormaPagamento = FormaPagamento,
                    Status = Status
                };
            }
            else // edição
            {
                pedidoParaSalvar = new Pedido
                {
                    Id = PedidoSelecionado.Id, // mantém Id existente
                    Pessoa = PessoaSelecionada.Nome,
                    Produtos = ProdutosSelecionados.ToList(),
                    ValorTotal = ValorSelecionados,
                    DataVenda = DateTime.Now,
                    FormaPagamento = FormaPagamento,
                    Status = Status
                };
            }

            _pedidosService.SalvarOuAtualizarPedido(pedidoParaSalvar);
            AtualizarPedidos();
            LimparSelecoes();

            if (ConteudoPedido is Window w) w.Close();
        }

        private void AtualizarPedidos()
        {
            Pedidos.Clear();
            foreach (var p in _pedidosService.CarregarPedidos())
                Pedidos.Add(p);
            FiltrarPedidosPorPessoa();
        }

        private void FiltrarPedidosPorPessoa()
        {
            var selecionadoId = PedidoSelecionado?.Id;

            PedidosFiltrados.Clear();
            if (PessoaSelecionada == null) return;

            foreach (var p in Pedidos.Where(p => p.Pessoa == PessoaSelecionada.Nome))
                PedidosFiltrados.Add(p);

            // restaura seleção pelo Id
            if (selecionadoId != null)
                PedidoSelecionado = PedidosFiltrados.FirstOrDefault(p => p.Id == selecionadoId);

            OnPropertyChanged(nameof(ValorTotal));
        }

        private void AbrirEdicao(Pedido? pedido)
        {
            PedidoSelecionado = pedido;
            ProdutosSelecionados.Clear();

            if (pedido != null)
            {
                foreach (var p in pedido.Produtos) ProdutosSelecionados.Add(p);
                FormaPagamento = pedido.FormaPagamento;
                Status = pedido.Status;
                PessoaSelecionada = Pessoas.FirstOrDefault(p => p.Nome == pedido.Pessoa);
            }

            ConteudoPedido = new NovoPedidoView { DataContext = this };
            ((Window)ConteudoPedido).ShowDialog();
        }

        private void LimparSelecoes()
        {
            ProdutosSelecionados.Clear();
            PessoaSelecionada = null;
            PedidoSelecionado = null;
            FormaPagamento = FormasPagamento.Dinheiro;
            Status = AllStatus.Pendente;
        }
    }
}
