using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WpfApp.Models;

namespace WpfApp.Services
{
    internal class PedidosService
    {
        private readonly string _filePath;

        public PedidosService()
        {
            string binDir = AppDomain.CurrentDomain.BaseDirectory;
            string projetoDir = Path.GetFullPath(Path.Combine(binDir, "..\\..\\.."));
            string dataFolder = Path.Combine(projetoDir, "Data");

            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            _filePath = Path.Combine(dataFolder, "pedidos.json");

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        // Salva ou atualiza um pedido existente
        public void SalvarOuAtualizarPedido(Pedido pedido)
        {
            var pedidos = CarregarPedidos();

            // Remove o antigo caso exista
            var existente = pedidos.FirstOrDefault(p => p.Id == pedido.Id);
            if (existente != null)
                pedidos.Remove(existente);

            // Se o Id for zero, define um novo Id
            if (pedido.Id == 0)
                pedido.Id = pedidos.Any() ? pedidos.Max(p => p.Id) + 1 : 1;

            // Adiciona o pedido atualizado
            pedidos.Add(pedido);

            SalvarLista(pedidos);
        }

        // Exclui um pedido pelo Id
        public void ExcluirPedido(int id)
        {
            var pedidos = CarregarPedidos();
            var pedido = pedidos.FirstOrDefault(p => p.Id == id);
            if (pedido != null)
            {
                pedidos.Remove(pedido);
                SalvarLista(pedidos);
            }
        }

        // Carrega todos os pedidos do JSON
        public List<Pedido> CarregarPedidos()
        {
            if (!File.Exists(_filePath))
                return new List<Pedido>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Pedido>>(json) ?? new List<Pedido>();
        }

        // Método privado para salvar a lista no arquivo
        private void SalvarLista(List<Pedido> pedidos)
        {
            var json = JsonSerializer.Serialize(pedidos, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}
