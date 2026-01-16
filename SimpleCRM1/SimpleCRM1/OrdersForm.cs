using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace SimpleCRM1
{
    public partial class OrdersForm : UserControl
    {
        public event EventHandler DataChanged;
        private DataTable ordersTable;
        private DataTable customersTable;
        private SqlDataAdapter ordersAdapter;
        private SqlDataAdapter customersAdapter;
        public OrdersForm()
        {
            InitializeComponent();
            LoadCustomers();
            LoadOrders();
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
                    string query = "SELECT CustomerID, Name FROM Customers";
                    customersTable = new DataTable();
                    customersAdapter = new SqlDataAdapter(query, connection);
                    customersAdapter.Fill(customersTable);

                    cmbCustomer.DataSource = customersTable;
                    cmbCustomer.DisplayMember = "Name";
                    cmbCustomer.ValueMember = "CustomerID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки клиентов: " + ex.Message);
            }
        }
        private void LoadOrders()
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    string query = @"SELECT o.OrderID, c.Name as CustomerName, o.OrderDate, 
                                o.TotalAmount, o.Status, o.Description 
                                FROM Orders o 
                                INNER JOIN Customers c ON o.CustomerID = c.CustomerID";

                    ordersTable = new DataTable();
                    ordersAdapter = new SqlDataAdapter(query, connection);
                    new SqlCommandBuilder(ordersAdapter);

                    ordersAdapter.Fill(ordersTable);
                    dataGridViewOrders.DataSource = ordersTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message);
            }
        }


        private void btnAddOrder_Click(object sender, EventArgs e)
        {
            if (cmbCustomer.SelectedValue == null || string.IsNullOrWhiteSpace(txtDescription.Text))
            {
                MessageBox.Show("Заполните все обязательные поля");
                return;
            }

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    string insertQuery = @"INSERT INTO Orders (CustomerID, OrderDate, TotalAmount, Status, Description) 
                                 VALUES (@CustomerID, @OrderDate, @TotalAmount, @Status, @Description)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", cmbCustomer.SelectedValue);
                        cmd.Parameters.AddWithValue("@OrderDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@TotalAmount",
                            string.IsNullOrWhiteSpace(txtTotalAmount.Text) ? 0 : decimal.Parse(txtTotalAmount.Text));
                        cmd.Parameters.AddWithValue("@Status", cmbStatus.SelectedItem?.ToString() ?? "Новый");
                        cmd.Parameters.AddWithValue("@Description", txtDescription.Text);

                        if (connection.State != ConnectionState.Open)
                            connection.Open();

                        cmd.ExecuteNonQuery();
                    }

                    LoadOrders();
                    ClearFields();
                    MessageBox.Show("Заказ добавлен");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
            DataChanged?.Invoke(this, EventArgs.Empty);

        }

        private void btnUpdateOrder_Click(object sender, EventArgs e)
        {
            if (dataGridViewOrders.CurrentRow == null)
            {
                MessageBox.Show("Выберите заказ для обновления");
                return;
            }

            try
            {
                ordersAdapter.Update(ordersTable);
                MessageBox.Show("Заказ обновлен");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void btnDeleteOrder_Click(object sender, EventArgs e)
        {
            if (dataGridViewOrders.CurrentRow == null)
            {
                MessageBox.Show("Выберите заказ для удаления");
                return;
            }

            if (MessageBox.Show("Удалить заказ?", "Подтверждение",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    dataGridViewOrders.Rows.RemoveAt(dataGridViewOrders.CurrentRow.Index);
                    ordersAdapter.Update(ordersTable);
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void dataGridViewOrders_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewOrders.CurrentRow != null && dataGridViewOrders.CurrentRow.DataBoundItem != null)
            {
                DataRowView row = (DataRowView)dataGridViewOrders.CurrentRow.DataBoundItem;
                txtDescription.Text = row["Description"].ToString();
                txtTotalAmount.Text = row["TotalAmount"].ToString();
                cmbStatus.Text = row["Status"].ToString();

                // Найти клиента в комбобоксе
                foreach (DataRowView customer in cmbCustomer.Items)
                {
                    if (customer["Name"].ToString() == row["CustomerName"].ToString())
                    {
                        cmbCustomer.SelectedItem = customer;
                        break;
                    }
                }
            }
        }
        private void ClearFields()
        {
            txtDescription.Text = "";
            txtTotalAmount.Text = "";
            cmbStatus.SelectedIndex = 0;
            if (cmbCustomer.Items.Count > 0)
                cmbCustomer.SelectedIndex = 0;
        }

        private void OrdersForm_Load(object sender, EventArgs e)
        {

        }
    }
}
