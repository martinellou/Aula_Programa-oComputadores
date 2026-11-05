using BoletimColecoes.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BoletimColecoes
{
    public partial class Form1 : Form
    {
        // Array unidimensional de string com as disciplinas (quantidade fixa)
        private readonly string[] disciplinas =
        {
            "Algoritmos", "Programação C#", "Banco de Dados"
        };

        // Lista dinâmica para armazenar os registros em memória
        private readonly List<RegistroNota> registros = new List<RegistroNota>();

        public Form1()
        {
            InitializeComponent();

            // Limpa itens prévios do designer para evitar duplicidades
            cboDisciplina.Items.Clear();

            // FOR percorre o array e adiciona cada disciplina ao combobox
            for (int i = 0; i < disciplinas.Length; i++)
            {
                cboDisciplina.Items.Add(disciplinas[i]);
            }

            if (cboDisciplina.Items.Count > 0)
            {
                cboDisciplina.SelectedIndex = 0;
            }

            AtualizarTela();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void btnAdicionar_Click(object sender, EventArgs e)
        {
            string nome = txtNome.Text.Trim();
            string disciplina = cboDisciplina.SelectedItem?.ToString() ?? string.Empty;
            decimal nota = nudNota.Value;

            // Validação de preenchimento
            if (string.IsNullOrWhiteSpace(nome))
            {
                MessageBox.Show("Informe o nome do estudante.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNome.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(disciplina))
            {
                MessageBox.Show("Selecione uma disciplina.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboDisciplina.Focus();
                return;
            }

            // Cria o novo registro e adiciona na coleção List<RegistroNota>
            RegistroNota novoRegistro = new RegistroNota
            {
                Estudante = nome,
                Disciplina = disciplina,
                Nota = nota
            };

            registros.Add(novoRegistro);

            // Limpa e prepara os campos para a próxima digitação
            txtNome.Clear();
            nudNota.Value = 0;
            txtNome.Focus();

            AtualizarTela();
        }

        private void btnRemover_Click(object sender, EventArgs e)
        {
            // Verifica se há alguma linha selecionada no DataGridView
            if (dgvNotas.SelectedRows.Count > 0)
            {
                int indiceLinha = dgvNotas.SelectedRows[0].Index;

                // Garante que o índice selecionado é válido dentro da List
                if (indiceLinha >= 0 && indiceLinha < registros.Count)
                {
                    registros.RemoveAt(indiceLinha);
                    AtualizarTela();
                    return;
                }
            }

            MessageBox.Show("Selecione um registro na tabela para remover.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnLimpar_Click(object sender, EventArgs e)
        {
            if (registros.Count == 0) return;

            DialogResult confirmacao = MessageBox.Show(
                "Deseja realmente apagar todos os registros?",
                "Confirmar Limpeza",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmacao == DialogResult.Yes)
            {
                registros.Clear();
                AtualizarTela();
            }
        }

        private void AtualizarTela()
        {
            // Apaga visualmente as linhas que aparecem na tela
            dgvNotas.Rows.Clear();

            // For each percorre a lista de registros e adiciona cada registro ao datagridview
            foreach (RegistroNota registro in registros)
            {
                // Row.Add = Adiciona 1 linha visual
                // ToString = Converte o valor da nota para string com 1 casa decimal
                dgvNotas.Rows.Add(registro.Estudante, registro.Disciplina, registro.Nota.ToString("0.0"));
            }

            dgvNotas.ClearSelection(); // Remove a seleção das linhas do datagridview

            AtualizarResumo(); // Atualiza o resumo das notas
        }

        private void AtualizarResumo()
        {
            if (registros.Count == 0)
            {
                txtResumo.Text = "Nenhum registro cadastrado no momento.";
                return;
            }

            StringBuilder sb = new StringBuilder();

            // 1. Cálculos estatísticos gerais
            int total = registros.Count;
            decimal somaGeral = 0;
            decimal maiorNota = registros[0].Nota;
            decimal menorNota = registros[0].Nota;

            foreach (var r in registros)
            {
                somaGeral += r.Nota;
                if (r.Nota > maiorNota) maiorNota = r.Nota;
                if (r.Nota < menorNota) menorNota = r.Nota;
            }

            decimal mediaGeral = somaGeral / total;

            sb.AppendLine("=== RESUMO GERAL ===");
            sb.AppendLine($"Total de Avaliações: {total}");
            sb.AppendLine($"Média Geral: {mediaGeral:F2} | Maior Nota: {maiorNota:F1} | Menor Nota: {menorNota:F1}");
            sb.AppendLine(new string('-', 55));

            // 2. Agrupamento por Disciplina (Coleções / LINQ)
            sb.AppendLine("MÉDIAS POR DISCIPLINA:");
            var porDisciplina = registros
                .GroupBy(r => r.Disciplina)
                .OrderBy(g => g.Key);

            foreach (var grupo in porDisciplina)
            {
                decimal mediaDisciplina = grupo.Average(x => x.Nota);
                sb.AppendLine($" • {grupo.Key,-16}: Média = {mediaDisciplina:F2} (Qtd: {grupo.Count()})");
            }

            sb.AppendLine(new string('-', 55));

            // 3. Agrupamento por Estudante (com status Aprovado/Reprovado)
            sb.AppendLine("SITUAÇÃO POR ESTUDANTE (Média >= 6.0 Aprovado):");
            var porEstudante = registros
                .GroupBy(r => r.Estudante, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key);

            foreach (var grupo in porEstudante)
            {
                decimal mediaAluno = grupo.Average(x => x.Nota);
                string situacao = mediaAluno >= 6.0m ? "Aprovado" : "Reprovado";
                sb.AppendLine($" • {grupo.Key,-20}: Média = {mediaAluno:F2} [{situacao}]");
            }

            txtResumo.Text = sb.ToString();
        }
    }
}