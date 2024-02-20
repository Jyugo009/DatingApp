using System.Formats.Asn1;
using System.Globalization;
using System.Xml.Linq;
using CsvHelper;
using Newtonsoft.Json;

namespace DatingApp
{
    public partial class Form1 : Form
    {
        public List<Operator> operators = new List<Operator>();

        public List<Record> records = new List<Record>();

        public Form1()
        {
            InitializeComponent();
        }

        private void textOutput(object sender, EventArgs e)
        {

        }

        private void BrowseCsv_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    RefreshListBox1();

                    foreach (string filePath in openFileDialog.FileNames)
                    {
                        using (var reader = new StreamReader(filePath))
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))

                        {
                            var newRecords = csv.GetRecords<Record>().ToList();
                            records.AddRange(newRecords);
                        }
                    }

                    var consolidatedRecords = records
                        .GroupBy(r => r.id)
                        .Select(grp => new Record
                        {
                            id = grp.Key,
                            lady = grp.First().lady,
                            bonuses = grp.Sum(r => r.bonuses)
                        })
                         .ToList();

                    foreach (Operator op in operators)
                    {

                        var opRecords = consolidatedRecords.Where(r => op.Ids.Contains(r.id)).ToList();

                        listBox1.Items.Add($"�������� {op.Name}:\n");

                        foreach (var record in opRecords)
                        {
                            listBox1.Items.Add($"Id: {record.id}, ����: {record.lady}, ������: {record.bonuses:F2}");
                        }

                        listBox1.Items.Add($" ����� �������: {opRecords.Sum(record => record.bonuses):F2}\n");

                    }

                }

            }

        }

        private void RefreshListBox1()
        {
            listBox1.Items.Clear();


        }

        private void RefreshListBox()
        {
            listBox2.Items.Clear();

            foreach (Operator op in operators)
            {
                listBox2.Items.Add(op.Name + ": " + string.Join(", ", op.Ids));
            }
        }

        private void AddOperator_Click(object sender, EventArgs e)
        {
            AddOperatorForm addOpForm = new AddOperatorForm(operators);

            if (addOpForm.ShowDialog() == DialogResult.OK)
            {
                RefreshListBox();
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            if (File.Exists("operators.json"))
            {
                string jsonFromFile = File.ReadAllText("operators.json");
                if (jsonFromFile != null)
                {
                    operators = JsonConvert.DeserializeObject<List<Operator>>(jsonFromFile);
                    if (operators != null)
                        RefreshListBox();
                    else
                    {
                        MessageBox.Show("���-�� ����� �� ���.");
                    }
                }
                else
                {

                    MessageBox.Show("������ � ����������� ������ �������! ��������, ��� ������ ���� operators.json!");
                }

            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            string json = JsonConvert.SerializeObject(operators);
            File.WriteAllText("operators.json", json);
        }

        private void DeleteOperator_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {
                string selectedItem = listBox2.SelectedItem.ToString();
                if (selectedItem != null)
                {
                    string opName = selectedItem.Split(':')[0];

                    MessageBox.Show("�������� " + opName + " ������.");

                    var opToRemove = operators.FirstOrDefault(op => op.Name == opName);

                    if (opToRemove != null)
                        operators.Remove(opToRemove);
                    else
                        MessageBox.Show($"�������� {opName} �� ������!");
                }
                else
                {
                    MessageBox.Show("���-�� ����� �� ���.");
                }

                RefreshListBox();
            }
            else
            {
                MessageBox.Show("����������, �������� ��������� �� ������, ����� ���������.");
            }



        }

        private void EditOperator_Click(object sender, EventArgs e)
        {
            if (listBox2.SelectedItem != null)
            {
                string selectedItem = listBox2.SelectedItem.ToString();

                string opName = selectedItem.Split(':')[0];

                Operator opToEdit = operators.Find(op => op.Name == opName);

                if (opToEdit != null)
                {

                    if (opToEdit != null)
                    {
                        EditOperatorForm editOpForm = new EditOperatorForm(opToEdit, operators);

                        if (editOpForm.ShowDialog() == DialogResult.OK)
                        {
                            RefreshListBox();
                        }
                    }

                    else
                    {
                        MessageBox.Show("��������� �������� �� ������.");
                    }
                }
                else
                {
                    MessageBox.Show("���-�� ����� �� ���.");
                }

            }
            else
            {
                MessageBox.Show("����������, �������� ��������� ��� ���������.");
            }

        }


        private void MakeReport_Click(object sender, EventArgs e)
        {
            ReportForm reportForm = new ReportForm(records, operators);

            if (reportForm.ShowDialog() == DialogResult.OK)
            {
                records.Clear();
                listBox1.Items.Clear();
            }

        }

        private void EarseListBoxButton_Click(object sender, EventArgs e)
        {
            records.Clear();

            listBox1.Items.Clear();
        }
    }
}
