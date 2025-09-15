using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WpfApp.Models;

namespace WpfApp.Services
{
    internal class PessoaService
    {
        private readonly string _filePath;

        public PessoaService()
        {
            string binDir = AppDomain.CurrentDomain.BaseDirectory;
            string projetoDir = Path.GetFullPath(Path.Combine(binDir, "..\\..\\.."));
            string dataFolder = Path.Combine(projetoDir, "Data");

            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            _filePath = Path.Combine(dataFolder, "pessoas.json");

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        public void SalvarOuAtualizarPessoa(Pessoa pessoa)
        {
            var pessoas = CarregarPessoas();

            // Atualiza se existir
            var existente = pessoas.FirstOrDefault(p => p.Id == pessoa.Id);
            if (existente != null)
            {
                existente.Nome = pessoa.Nome;
                existente.CPF = pessoa.CPF;
                existente.Endereco = pessoa.Endereco;
            }
            else
            {
                // Novo Id
                pessoa.Id = pessoas.Any() ? pessoas.Max(p => p.Id) + 1 : 1;
                pessoas.Add(pessoa);
            }

            SalvarLista(pessoas);
        }

        public void ExcluirPessoa(int id)
        {
            var pessoas = CarregarPessoas();
            var pessoa = pessoas.FirstOrDefault(p => p.Id == id);
            if (pessoa != null)
            {
                pessoas.Remove(pessoa);
                SalvarLista(pessoas);
            }
        }

        public List<Pessoa> CarregarPessoas()
        {
            if (!File.Exists(_filePath))
                return new List<Pessoa>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Pessoa>>(json) ?? new List<Pessoa>();
        }

        private void SalvarLista(List<Pessoa> pessoas)
        {
            var json = JsonSerializer.Serialize(pessoas, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
    }
}
