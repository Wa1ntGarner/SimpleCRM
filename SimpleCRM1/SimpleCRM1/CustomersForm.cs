using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SimpleCRM1
{
    public partial class CustomersForm : UserControl
    {
        public event EventHandler DataChanged;
        private DataTable dataTable;
        private SqlDataAdapter adapter;

        public CustomersForm()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SimpleCRM;Integrated Security=True");
        }

        private void LoadCustomers()
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    string query = "SELECT * FROM Customers";
                    dataTable = new DataTable();
                    adapter = new SqlDataAdapter(query, connection);
                    new SqlCommandBuilder(adapter);

                    adapter.Fill(dataTable);
                    dataGridView1.DataSource = dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }
        
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите имя клиента");
                return;
            }

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    // Используем параметризованный запрос для правильной кодировки
                    string insertQuery = @"
                INSERT INTO Customers (Name, Email, Phone, Address, CreatedDate) 
                VALUES (@Name, @Email, @Phone, @Address, @CreatedDate)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", txtName.Text);
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                        cmd.Parameters.AddWithValue("@Phone", txtPhone.Text);
                        cmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        cmd.ExecuteNonQuery();
                    }

                    // Перезагружаем данные
                    LoadCustomers();
                    ClearFields();
                    MessageBox.Show("Клиент добавлен");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Выберите клиента для обновления");
                return;
            }

            try
            {
                adapter.Update(dataTable);
                MessageBox.Show("Данные обновлены");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Выберите клиента для удаления");
                return;
            }

            if (MessageBox.Show("Удалить клиента?", "Подтверждение",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.CurrentRow.Index);
                    adapter.Update(dataTable);
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentRow != null && dataGridView1.CurrentRow.DataBoundItem != null)
            {
                DataRowView row = (DataRowView)dataGridView1.CurrentRow.DataBoundItem;
                txtName.Text = row["Name"].ToString();
                txtEmail.Text = row["Email"].ToString();
                txtPhone.Text = row["Phone"].ToString();
                txtAddress.Text = row["Address"].ToString();
            }
        }
        private void ClearFields()
        {
            txtName.Text = "";
            txtEmail.Text = "";
            txtPhone.Text = "";
            txtAddress.Text = "";
        }

        private void CustomersForm_Load(object sender, EventArgs e)
        {

        }
    }
}
