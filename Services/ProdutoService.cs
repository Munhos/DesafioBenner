using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WpfApp.Models;

namespace WpfApp.Services
{
    public class ProdutoService
    {
        private readonly string _filePath;

        public ProdutoService()
        {
            string binDir = AppDomain.CurrentDomain.BaseDirectory;
            string projetoDir = Path.Combine(binDir, "..\\..\\..");
            string dataFolder = Path.Combine(projetoDir, "Data");

            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            _filePath = Path.Combine(dataFolder, "produtos.json");
        }

        public List<Produto> CarregarProdutos()
        {
            if (!File.Exists(_filePath))
                return new List<Produto>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Produto>>(json) ?? new List<Produto>();
        }

        public void SalvarOuAtualizarProduto(Produto produto)
        {
            var produtos = CarregarProdutos();
            var existente = produtos.FirstOrDefault(p => p.Id == produto.Id);

            if (existente != null)
            {
                existente.Nome = produto.Nome;
                existente.Codigo = produto.Codigo;
                existente.Valor = produto.Valor;
            }
            else
            {
                produto.Id = produtos.Any() ? produtos.Max(p => p.Id) + 1 : 1;
                produtos.Add(produto);
            }

            File.WriteAllText(_filePath, JsonSerializer.Serialize(produtos, new JsonSerializerOptions { WriteIndented = true }));
        }

        public void ExcluirProduto(int id)
        {
            var produtos = CarregarProdutos();
            var produto = produtos.FirstOrDefault(p => p.Id == id);
            if (produto != null)
            {
                produtos.Remove(produto);
                File.WriteAllText(_filePath, JsonSerializer.Serialize(produtos, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }
}
