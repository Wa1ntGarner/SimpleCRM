using System;
using System.Data.SqlClient;
using System.Windows.Forms;




namespace SimpleCRM1
{
    public partial class Form1 : Form
    {

        private SqlConnection connection;
        private UserControl currentForm; // ← ДОБАВЬТЕ ЭТУ СТРОКУ
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (TestConnection())
            {
                LoadStatistics();
                ShowCustomersForm();
            }
        }
        private bool TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True"))
                {
                    connection.Open();

                    // Создаем базу данных если ее нет
                    string createDatabase = @"
                IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'SimpleCRM')
                BEGIN
                    CREATE DATABASE SimpleCRM;
                END";

                    using (SqlCommand cmd = new SqlCommand(createDatabase, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                // Создаем таблицы в новой базе
                using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SimpleCRM;Integrated Security=True"))
                {
                    connection.Open();

                    // Создаем таблицы с правильной кодировкой
                    string createTables = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Customers' AND xtype='U')
                CREATE TABLE Customers (
                    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL,
                    Email NVARCHAR(100),
                    Phone NVARCHAR(20),
                    Address NVARCHAR(200),
                    CreatedDate DATETIME DEFAULT GETDATE()
                );
                
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
                CREATE TABLE Orders (
                    OrderID INT IDENTITY(1,1) PRIMARY KEY,
                    CustomerID INT FOREIGN KEY REFERENCES Customers(CustomerID),
                    OrderDate DATETIME DEFAULT GETDATE(),
                    TotalAmount DECIMAL(10,2),
                    Status NVARCHAR(50) DEFAULT N'Новый',
                    Description NVARCHAR(500)
                );";

                    using (SqlCommand cmd = new SqlCommand(createTables, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Добавляем тестовые данные с правильной кодировкой
                    AddTestData(connection);

                    connection.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка");
                return false;
            }
        }
        private void AddTestData(SqlConnection connection)
        {
            try
            {
                // Проверяем есть ли данные в таблице Customers
                string checkCustomers = "SELECT COUNT(*) FROM Customers";
                using (SqlCommand cmd = new SqlCommand(checkCustomers, connection))
                {
                    int count = (int)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        // Добавляем тестовых клиентов с префиксом N для Unicode
                        string insertCustomers = @"
                    INSERT INTO Customers (Name, Email, Phone, Address) VALUES 
                    (N'Иван Петров', N'ivan@mail.ru', N'+79161234567', N'Москва, ул. Ленина 1'),
                    (N'Мария Сидорова', N'maria@mail.ru', N'+79167654321', N'Санкт-Петербург, Невский пр. 100'),
                    (N'Алексей Козлов', N'alex@mail.ru', N'+79031112233', N'Казань, ул. Баумана 50');";

                        using (SqlCommand insertCmd = new SqlCommand(insertCustomers, connection))
                        {
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }

                // Проверяем есть ли данные в таблице Orders
                string checkOrders = "SELECT COUNT(*) FROM Orders";
                using (SqlCommand cmd = new SqlCommand(checkOrders, connection))
                {
                    int count = (int)cmd.ExecuteScalar();
                    if (count == 0)
                    {
                        // Добавляем тестовые заказы с префиксом N для Unicode
                        string insertOrders = @"
                    INSERT INTO Orders (CustomerID, TotalAmount, Status, Description) VALUES 
                    (1, 15000.00, N'Завершен', N'Разработка сайта'),
                    (2, 8000.50, N'В работе', N'Дизайн логотипа'),
                    (3, 25000.00, N'Новый', N'Мобильное приложение');";

                        using (SqlCommand insertCmd = new SqlCommand(insertOrders, connection))
                        {
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки при добавлении тестовых данных
                Console.WriteLine("Ошибка добавления тестовых данных: " + ex.Message);
            }
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            ShowCustomersForm();
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            ShowOrdersForm();
        }
        private void ShowCustomersForm()
        {
            // Сначала закрываем предыдущую форму если есть
            if (currentForm != null)
            {
                panelMain.Controls.Remove(currentForm);
                currentForm.Dispose();
            }

            // Открываем новую форму
            panelMain.Controls.Clear();
            currentForm = new CustomersForm();
            currentForm.Dock = DockStyle.Fill;
            panelMain.Controls.Add(currentForm);

            ((CustomersForm)currentForm).DataChanged += OnDataChanged;
        }

        private void ShowOrdersForm()
        {
            // Сначала закрываем предыдущую форму если есть
            if (currentForm != null)
            {
                panelMain.Controls.Remove(currentForm);
                currentForm.Dispose();
            }

            // Открываем новую форму
            panelMain.Controls.Clear();
            currentForm = new OrdersForm();
            currentForm.Dock = DockStyle.Fill;
            panelMain.Controls.Add(currentForm);

            ((OrdersForm)currentForm).DataChanged += OnDataChanged;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Пересоздать базу данных?", "Подтверждение",
        MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True"))
                    {
                        connection.Open();

                        // Удаляем старую базу
                        string dropDatabase = @"
                    IF EXISTS(SELECT * FROM sys.databases WHERE name = 'SimpleCRM')
                    BEGIN
                        ALTER DATABASE SimpleCRM SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        DROP DATABASE SimpleCRM;
                    END";

                        using (SqlCommand cmd = new SqlCommand(dropDatabase, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        connection.Close();
                    }

                    // Перезапускаем инициализацию
                    if (TestConnection())
                    {
                        ShowCustomersForm();
                        MessageBox.Show("База пересоздана с правильной кодировкой!", "Успех");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
                }
            }
        }

        private void panelMain_Paint(object sender, PaintEventArgs e)
        {

        }
        private void LoadStatistics()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SimpleCRM;Integrated Security=True"))
                {
                    connection.Open();

                    // Получаем общее количество заказов
                    string ordersQuery = "SELECT COUNT(*) FROM Orders";
                    int totalOrders = 0;
                    using (SqlCommand cmd = new SqlCommand(ordersQuery, connection))
                    {
                        totalOrders = (int)cmd.ExecuteScalar();
                    }

                    // Получаем общую сумму всех заказов
                    string moneyQuery = "SELECT ISNULL(SUM(TotalAmount), 0) FROM Orders";
                    decimal totalMoney = 0;
                    using (SqlCommand cmd = new SqlCommand(moneyQuery, connection))
                    {
                        totalMoney = (decimal)cmd.ExecuteScalar();
                    }

                    // Обновляем интерфейс
                    lblTotalMoney.Text = $"{totalMoney:N2} ₽";
                    lblOrdersCount.Text = totalOrders.ToString();

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                lblTotalMoney.Text = "Ошибка";
                lblOrdersCount.Text = "0";
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка");
            }
        }
        private void OnDataChanged(object sender, EventArgs e)
        {
            LoadStatistics();
        }
        private void btnRefreshStats_Click(object sender, EventArgs e)
        {
            LoadStatistics();
        }

        private void guna2ImageButton1_Click(object sender, EventArgs e)
        {
            CloseAllForms();
        }

        private void CloseAllForms()
        {
            if (currentForm != null)
            {
                panelMain.Controls.Remove(currentForm);
                currentForm.Dispose();
                currentForm = null;

                // Показываем сообщение что форма закрыта
                MessageBox.Show("Текущая форма закрыта", "Информация",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
