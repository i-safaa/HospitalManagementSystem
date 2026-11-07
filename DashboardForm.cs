using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace HospitalManagementSystem
{
    public partial class DashboardForm : Form
    {
        //!!
        // يفضل استخدام |DataDirectory| ليتحرك ملف القاعدة تلقائياً مع المشروع
        private readonly string con = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\Hospital_db.mdf;Integrated Security=True;Connect Timeout=30";

        public DashboardForm()
        {
            InitializeComponent();
        }

        private void DashboardForm_Load(object sender, EventArgs e)
        {
            pictureBox1.Visible = true;
            panel1.Visible = false;
            panel2.Visible = false;
            panel3.Visible = false;
            pictureBox3.Visible = false;

            populate();
        }

        // دالة موحدة لتنسيق الأزرار واللوحات بدلاً من تكرار الكود
        private void SetActiveTab(Label activeLabel, Panel activePanel)
        {
            label3.ForeColor = Color.Black;
            label4.ForeColor = Color.Black;
            label5.ForeColor = Color.Black;
            label6.ForeColor = Color.Black;

            if (activeLabel != null) activeLabel.ForeColor = Color.Red;

            panel1.Visible = (activePanel == panel1);
            panel2.Visible = (activePanel == panel2);
            panel3.Visible = (activePanel == panel3);

            pictureBox3.Visible = (activeLabel == label6);
        }

        private void btnAddPatient_Click(object sender, EventArgs e)
        {
            SetActiveTab(label3, panel1);
        }

        private void btnAddDiagnosis_Click(object sender, EventArgs e)
        {
            SetActiveTab(label4, panel2);
        }

        private void btnFullHistory_Click(object sender, EventArgs e)
        {
            SetActiveTab(label5, panel3);
            populate();
        }

        private void btnHospital_Click(object sender, EventArgs e)
        {
            SetActiveTab(label6, null);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) ||
                string.IsNullOrWhiteSpace(txtAddress.Text) ||
                string.IsNullOrWhiteSpace(txtNumber.Text) ||
                string.IsNullOrWhiteSpace(txtAge.Text) ||
                string.IsNullOrWhiteSpace(comGender.Text) ||
                string.IsNullOrWhiteSpace(txtBlood.Text) ||
                string.IsNullOrWhiteSpace(txtAnyMajor.Text) ||
                string.IsNullOrWhiteSpace(txtPatientID.Text))
            {
                MessageBox.Show("يرجى ملء جميع الحقول، حيث أن قاعدة البيانات لا تقبل قيماً فارغة.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!long.TryParse(txtPatientID.Text.Trim(), out long pid))
            {
                MessageBox.Show("معرف المريض (Pid) يجب أن يكون رقماً صحيحاً.", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!long.TryParse(txtNumber.Text.Trim(), out long contact))
            {
                MessageBox.Show("رقم الهاتف (Contact) يجب أن يتكون من أرقام فقط دون مسافات.", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(txtAge.Text.Trim(), out int age))
            {
                MessageBox.Show("العمر (Age) يجب أن يكون رقماً صحيحاً.", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string query = @"INSERT INTO Addp_tbl (Name, Full_Address, Contact, Age, Gender, Blood_Group, Major_D, Pid) 
                     VALUES (@Name, @Full_Address, @Contact, @Age, @Gender, @Blood_Group, @Major_D, @Pid)";

            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Name", txtName.Text.Trim());
                        command.Parameters.AddWithValue("@Full_Address", txtAddress.Text.Trim());
                        command.Parameters.AddWithValue("@Contact", contact); // استخدام المتغير الرقمي bigint
                        command.Parameters.AddWithValue("@Age", age);         // استخدام المتغير الرقمي int
                        command.Parameters.AddWithValue("@Gender", comGender.Text.Trim());
                        command.Parameters.AddWithValue("@Blood_Group", txtBlood.Text.Trim());
                        command.Parameters.AddWithValue("@Major_D", txtAnyMajor.Text.Trim());
                        command.Parameters.AddWithValue("@Pid", pid);         // استخدام المتغير الرقمي bigint

                        command.ExecuteNonQuery();

                        MessageBox.Show("تمت الإضافة بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearInputs();
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    MessageBox.Show("رقم المريض (Pid) مسجل مسبقاً، يرجى استخدام رقم آخر.", "تكرار معرف المريض", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("خطأ في قاعدة البيانات: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "حدث خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ClearInputs()
        {
            txtName.Clear();
            txtAddress.Clear();
            txtNumber.Clear();
            txtAge.Clear();
            txtAnyMajor.Clear();
            txtPatientID.Clear();
            txtBlood.Clear();
            comGender.SelectedIndex = -1;
            comGender.ResetText();
        }

        private void txtPid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // كتم صوت التنبيه الافتراضي

                if (string.IsNullOrWhiteSpace(txtPid.Text))
                {
                    dataGridView1.DataSource = null;
                    return;
                }

                string query = "SELECT * FROM Addp_tbl WHERE Pid = @Pid";

                try
                {
                    using (SqlConnection connection = new SqlConnection(con))
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            // Pid نوعه رقمي في جدولك
                            command.Parameters.AddWithValue("@Pid", Convert.ToInt64(txtPid.Text.Trim()));

                            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                            {
                                DataTable dataTable = new DataTable();
                                adapter.Fill(dataTable);
                                dataGridView1.DataSource = dataTable;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ أثناء البحث: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void txtPid_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            // السماح بالأرقام وزر Backspace فقط
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // إلغاء الحرف
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPid.Text) ||
                string.IsNullOrWhiteSpace(txtDi.Text) ||
                string.IsNullOrWhiteSpace(txtMe.Text) ||
                string.IsNullOrWhiteSpace(txtSy.Text) ||
                string.IsNullOrWhiteSpace(comWard.Text) ||
                string.IsNullOrWhiteSpace(comTypeWard.Text))
            {
                MessageBox.Show("يرجى ملء جميع الحقول، حيث أن قاعدة البيانات لا تقبل قيماً فارغة.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!long.TryParse(txtPid.Text.Trim(), out long pid))
            {
                MessageBox.Show("معرف المريض (Pid) يجب أن يكون رقماً صحيحاً.", "خطأ في الإدخال", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string query = "INSERT INTO PatientMore_Tbl (Pid, Symptoms, Diagnosis, Medicines, Ward, Ward_Type) VALUES (@Pid, @Symptoms, @Diagnosis, @Medicines, @Ward, @Ward_Type)";

            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Pid", pid);
                        command.Parameters.AddWithValue("@Symptoms", txtSy.Text.Trim());
                        command.Parameters.AddWithValue("@Diagnosis", txtDi.Text.Trim());
                        command.Parameters.AddWithValue("@Medicines", txtMe.Text.Trim());
                        command.Parameters.AddWithValue("@Ward", comWard.Text.Trim());
                        command.Parameters.AddWithValue("@Ward_Type", comTypeWard.Text.Trim());

                        command.ExecuteNonQuery();

                        MessageBox.Show("تمت الإضافة بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearInputs2();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearInputs2()
        {
            txtPid.Clear();
            txtDi.Clear();
            txtMe.Clear();
            txtSy.Clear();

            comWard.SelectedIndex = -1;
            comWard.ResetText();

            comTypeWard.SelectedIndex = -1;
            comTypeWard.ResetText();
        }


        public void populate()
        {
            // دمج الجدولين بالاعتماد على رقم المريض المشترك (Pid)
            string query = @"SELECT 
                        A.Pid, 
                        A.Name, 
                        A.Full_Address,
                        A.Contact, 
                        A.Age, 
                        A.Gender, 
                        A.Blood_Group, 
                        A.Major_D,
                        P.Symptoms, 
                        P.Diagnosis, 
                        P.Medicines, 
                        P.Ward, 
                        P.Ward_Type 
                    FROM Addp_tbl A 
                    INNER JOIN PatientMore_Tbl P ON A.Pid = P.Pid";

            try
            {
                using (SqlConnection connection = new SqlConnection(con))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            dataAdapter.Fill(dataTable);

                            dataGridView2.DataSource = dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء جلب السجل الطبي: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}