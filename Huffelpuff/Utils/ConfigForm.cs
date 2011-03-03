using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Huffelpuff.Utils
{
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();
        }

        private void ConfigForm_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns.Add("group", "Group");
            dataGridView1.Columns.Add("key", "Key");
            dataGridView1.Columns.Add("value", "Value");

            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[1].ReadOnly = true;

            foreach (var row in PersistentMemory.Instance.RawData.Tables[0].Select())
            {
                dataGridView1.Rows.Add(row.Field<string>(0), row.Field<string>(1), row.Field<string>(2));
            }

            dataGridView1.Update();
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var row = dataGridView1.Rows[e.RowIndex];

            var group = row.Cells[0].Value as string;
            var key = row.Cells[1].Value as string;
            var value = row.Cells[2].Value as string;

            if (group != null && key != null && value != null)
            {
                PersistentMemory.Instance.ReplaceValue(group, key, value);
                PersistentMemory.Instance.Flush();

                row.Cells[2].Style.BackColor = Color.LightGreen;

                var worker = new BackgroundWorker();
                worker.DoWork += (o, args) => { Thread.Sleep(TimeSpan.FromSeconds(2)); row.Cells[2].Style.BackColor = Color.White; };
                worker.RunWorkerAsync();
            }
        }

    }
}
