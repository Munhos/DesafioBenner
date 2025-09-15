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
                FiltrarPedidos();
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

        public ObservableCollection<AllStatus> TodosStatus { get; set; }

        private AllStatus? _statusFiltro;
        public AllStatus? StatusFiltro
        {
            get => _statusFiltro;
            set
            {
                _statusFiltro = value;
                OnPropertyChanged(nameof(StatusFiltro));
                FiltrarPedidos();
            }
        }

        private AllStatus _statusEdicao = AllStatus.Pendente;
        public AllStatus StatusEdicao
        {
            get => _statusEdicao;
            set
            {
                _statusEdicao = value;
                OnPropertyChanged(nameof(StatusEdicao));
            }
        }

        public FormasPagamento FormaPagamento { get; set; } = FormasPagamento.Dinheiro;

        // usado nos filtros
        public AllStatus? StatusSelecionado { get; set; } = AllStatus.Pendente;

        public decimal ValorTotal => PedidosFiltrados.Sum(p => (decimal)p.ValorTotal);
        public decimal ValorSelecionados => ProdutosSelecionados.Sum(p => (decimal)p.Valor);

        public Array FormasPagamentoPossiveis => Enum.GetValues(typeof(FormasPagamento));

        public ICommand SalvarPedidoCommand { get; }
        public ICommand ExcluirPedidoCommand { get; }
        public ICommand AdicionarOuAtualizarCommand { get; }
        public ICommand EditarPedidoCommand { get; }
        public ICommand AdicionarPedidoCommand { get; }


        public object? ConteudoPedido { get; set; }

        public PedidosViewModel()
        {
            Pessoas = new ObservableCollection<Pessoa>(_pessoaService.CarregarPessoas());
            ProdutosDisponiveis = new ObservableCollection<Produto>(_produtoService.CarregarProdutos());
            TodosStatus = new ObservableCollection<AllStatus>((AllStatus[])Enum.GetValues(typeof(AllStatus)));

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

            AdicionarOuAtualizarCommand = new RelayCommand(_ =>
            {
                if (PedidoSelecionado != null) 
                {
                    var idAntigo = PedidoSelecionado.Id;

                    PedidoSelecionado.Pessoa = PessoaSelecionada?.Nome ?? "";
                    PedidoSelecionado.Produtos = ProdutosSelecionados.ToList();
                    PedidoSelecionado.ValorTotal = ProdutosSelecionados.Sum(p => (decimal)p.Valor);
                    PedidoSelecionado.DataVenda = DateTime.Now;
                    PedidoSelecionado.FormaPagamento = FormaPagamento;
                    PedidoSelecionado.Status = StatusEdicao;

                    PedidoSelecionado.Id = idAntigo; 

                    _pedidosService.SalvarOuAtualizarPedido(PedidoSelecionado);

                    var index = Pedidos.IndexOf(PedidoSelecionado);
                    if (index >= 0)
                        Pedidos[index] = PedidoSelecionado;
                }
                else 
                {
                    var novoPedido = new Pedido
                    {
                        Pessoa = PessoaSelecionada?.Nome ?? "",
                        Produtos = ProdutosSelecionados.ToList(),
                        ValorTotal = ProdutosSelecionados.Sum(p => (decimal)p.Valor),
                        DataVenda = DateTime.Now,
                        FormaPagamento = FormaPagamento,
                        Status = StatusEdicao
                    };

                    _pedidosService.SalvarOuAtualizarPedido(novoPedido);
                    Pedidos.Add(novoPedido);
                }

                LimparSelecoes();
                FiltrarPedidos();
            });

            EditarPedidoCommand = new RelayCommand(_ =>
            {
                if (PedidoSelecionado.Status == AllStatus.Recebido)
                {
                    MessageBox.Show("Pedidos recebidos não podem ser editados.");
                    return;
                }

                if (PedidoSelecionado != null)
                    AbrirEdicao(PedidoSelecionado);
            });

            ProdutosSelecionados.CollectionChanged += (_, __) => OnPropertyChanged(nameof(ValorSelecionados));

            AdicionarPedidoCommand = new RelayCommand(_ =>
            {
                var window = new NovoPedidoView { DataContext = this };
                ConteudoPedido = window;
                window.ShowDialog();
            });
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

            if (PedidoSelecionado == null) 
            {
                var novoPedido = new Pedido
                {
                    Pessoa = PessoaSelecionada.Nome,
                    Produtos = ProdutosSelecionados.ToList(),
                    ValorTotal = ValorSelecionados,
                    DataVenda = DateTime.Now,
                    FormaPagamento = FormaPagamento,
                    Status = StatusEdicao
                };

                _pedidosService.SalvarOuAtualizarPedido(novoPedido);
                Pedidos.Add(novoPedido);
            }
            else 
            {
                var idAntigo = PedidoSelecionado.Id;

                PedidoSelecionado.Pessoa = PessoaSelecionada.Nome;
                PedidoSelecionado.Produtos = ProdutosSelecionados.ToList();
                PedidoSelecionado.ValorTotal = ValorSelecionados;
                PedidoSelecionado.DataVenda = DateTime.Now;
                PedidoSelecionado.FormaPagamento = FormaPagamento;
                PedidoSelecionado.Status = StatusEdicao;
                PedidoSelecionado.Id = idAntigo; 

                _pedidosService.SalvarOuAtualizarPedido(PedidoSelecionado);

                var index = Pedidos.IndexOf(PedidoSelecionado);
                if (index >= 0)
                    Pedidos[index] = PedidoSelecionado;
            }

            FiltrarPedidos();
            if (ConteudoPedido is Window w) w.Close();
        }

        private void AtualizarPedidos()
        {
            Pedidos.Clear();
            foreach (var p in _pedidosService.CarregarPedidos())
                Pedidos.Add(p);
            FiltrarPedidos();
        }

        private void AbrirEdicao(Pedido pedido)
        {
            PessoaSelecionada = Pessoas.FirstOrDefault(p => p.Nome == pedido.Pessoa);
            ProdutosSelecionados.Clear();
            foreach (var produto in pedido.Produtos)
                ProdutosSelecionados.Add(produto);

            FormaPagamento = pedido.FormaPagamento;
            StatusEdicao = pedido.Status;
            PedidoSelecionado = pedido;

            var window = new NovoPedidoView { DataContext = this };
            ConteudoPedido = window;
            window.ShowDialog();
        }

        private void LimparSelecoes()
        {
            ProdutosSelecionados.Clear();
            PessoaSelecionada = null;
            PedidoSelecionado = null;
            FormaPagamento = FormasPagamento.Dinheiro;
            StatusSelecionado = AllStatus.Pendente;
        }

        private void FiltrarPedidos()
        {
            var query = Pedidos.AsEnumerable();

            if (PessoaSelecionada != null)
                query = query.Where(p => p.Pessoa == PessoaSelecionada.Nome);

            if (StatusFiltro.HasValue)
                query = query.Where(p => p.Status == StatusFiltro.Value);

            PedidosFiltrados.Clear();
            foreach (var p in query)
                PedidosFiltrados.Add(p);

            OnPropertyChanged(nameof(ValorTotal));
        }
    }
}
