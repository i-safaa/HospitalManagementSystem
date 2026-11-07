using System;
using System.Windows.Forms;

namespace HospitalManagementSystem
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string Username = txtUsername.Text;
            string password = txtPassword.Text;

            if (Username == "Ibrahim" && password == "1234")
            {
                MessageBox.Show("تم تسجيل الدخول بنجاح", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Hide();

                DashboardForm dashboardForm = new DashboardForm();
                dashboardForm.Show();
            }
            else
            {
                MessageBox.Show("خطا في اسم المستخدم او كلمة المرور", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
