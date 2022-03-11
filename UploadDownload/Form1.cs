using System;
using System.IO;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data;

namespace UploadDownload
{
    public partial class Form1 : Form
    {
        public static string ConnectionString= @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=D:\source\repos\UploadDownload\UploadDownload\Database1.mdf;Integrated Security=True";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BindGrid();
            DataGridViewLinkColumn lnkDownload = new DataGridViewLinkColumn();
            lnkDownload.UseColumnTextForLinkValue = true;
            lnkDownload.LinkBehavior = LinkBehavior.SystemDefault;
            lnkDownload.Name = "lnkDownload";
            lnkDownload.HeaderText = "Жүктеу";
            lnkDownload.Text = "Жүктеу";
            dataGridView1.Columns.Insert(2, lnkDownload);
            dataGridView1.CellContentClick += new DataGridViewCellEventHandler(DataGridView_CellClick);
        }

        private void DataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                int ?id=null;
                try
                {
                    id = Convert.ToInt32((row.Cells[0].Value));
                }
                catch (InvalidCastException) {
                    MessageBox.Show("Cіз файлды таңдамадыңыз");
                }
                
                byte[] bytes;
                string fileName, contentType;
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    if (id != null)
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.CommandText = "select Name, Data, ContentType from FileUp where Id=@Id";
                            cmd.Parameters.AddWithValue("@Id", id);
                            cmd.Connection = con;
                            con.Open();
                            using (SqlDataReader sdr = cmd.ExecuteReader())
                            {
                                sdr.Read();
                                bytes = (byte[])sdr["Data"];
                                contentType = sdr["ContentType"].ToString();
                                fileName = sdr["Name"].ToString();

                                Stream stream;
                                SaveFileDialog saveFileDialog = new SaveFileDialog();
                                saveFileDialog.Filter = "All files (.*|*.)";
                                saveFileDialog.FilterIndex = 1;
                                saveFileDialog.RestoreDirectory = true;
                                saveFileDialog.FileName = fileName;
                                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                                {
                                    stream = saveFileDialog.OpenFile();
                                    stream.Write(bytes, 0, bytes.Length);
                                    stream.Close();
                                }
                            }
                        }
                    }
                    con.Close();
                }
            }
        }
        private void BindGrid()
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("select Id, Name from FileUp", con))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        using (DataTable dt = new DataTable())
                        {
                            sda.Fill(dt);
                            dataGridView1.DataSource = dt;
                        }
                    }
                }
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string fileName = openFileDialog1.FileName;
                    byte[] bytes = File.ReadAllBytes(fileName);
                    string contentType = "";
                    //Файл кеңейтімі(расширение) негізінде 

                    
                    switch (Path.GetExtension(fileName))
                    {
                        case ".jpg":
                            contentType = "image/jpeg";
                            break;
                        case ".png":
                            contentType = "image/png";
                            break;
                        case ".gif":
                            contentType = "image/gif";
                            break;
                        case ".bmp":
                            contentType = "image/bmp";
                            break;
                            
                    }

                    using (SqlConnection conn = new SqlConnection(ConnectionString))
                    {
                        string sql = "INSERT INTO FileUp VALUES(@Name, @ContentType, @Data)";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Name", Path.GetFileName(fileName));
                            cmd.Parameters.AddWithValue("@ContentType", contentType);
                            cmd.Parameters.AddWithValue("@Data", bytes);
                            conn.Open();
                            cmd.ExecuteNonQuery();
                            conn.Close();
                        }
                    }
                    this.BindGrid();
                }
            }
        }
    }
}
