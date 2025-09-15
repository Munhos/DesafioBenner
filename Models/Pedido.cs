using System;
using System.Collections.Generic;

namespace WpfApp.Models
{
    public enum FormasPagamento
    {
        Dinheiro,
        Cartao,
        Boleto
    }

    public enum AllStatus
    {
        Pendente,
        Pago,
        Enviado,
        Recebido
    }

    public class Pedido
    {
        public int Id { get; set; }
        public required string Pessoa { get; set; }
        public List<Produto> Produtos { get; set; } = new();
        public decimal ValorTotal { get; set; } = 0; 
        public DateTime DataVenda { get; set; } = DateTime.Now;
        public FormasPagamento FormaPagamento { get; set; }
        public AllStatus Status { get; set; } = AllStatus.Pendente;
    }
}
