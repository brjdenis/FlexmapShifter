using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FlexmapShifter
{
    public partial class Form1 : Form
    {
        public string FlexmapPath = "";
        public List<string> FlexmapContent = new List<string>() { };
        public string[] FlexmapHeader = new string[]{ };
        public DataSet FlexmapDataSet;


        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            CreateDataset();
        }

        private void SetDataGridViewProperties()
        {
            this.dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            this.dataGridView1.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Flexmap files (*.flexmap)|*.flexmap|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    this.FlexmapPath = openFileDialog.FileName;
                    ReadFlexmap();
                    AddToDataset();
                    UpdateCharts();
                }
            }
        }

        private void ReadFlexmap()
        {
            this.label4.Text = this.FlexmapPath;
            List<string> lines = new List<string>() { };

            using (FileStream fileStream = new FileStream(this.FlexmapPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    while (!reader.EndOfStream)
                    {
                        lines.Add(reader.ReadLine());
                    }
                }
            }
            
            this.FlexmapContent = lines;

            GetFlexmapData();

            this.richTextBox1.Lines = this.FlexmapHeader;
        }


        private void GetFlexmapData()
        {
            List<string> header = new List<string>() { };
            
            int i;
            for (i=0; i<this.FlexmapContent.Count; i++)
            {
                if (this.FlexmapContent[i].Contains("Angle"))
                {
                    header.Add(this.FlexmapContent[i]);
                    break;
                }
                else
                {
                    header.Add(this.FlexmapContent[i]);
                }
            }
            this.FlexmapHeader = header.ToArray();

            //Continue with collecting data
            ResetDataset();
            var data = this.FlexmapDataSet.Tables["data"];

            int j;
            for (j = i+1; j<this.FlexmapContent.Count; j++)
            {
                string line = this.FlexmapContent[j];
                List<string> anglexy = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries).ToList();
                DataRow row = data.NewRow();
                row["Angle"] = decimal.Parse(anglexy[0], CultureInfo.InvariantCulture);
                row["X"] = decimal.Parse(anglexy[1], CultureInfo.InvariantCulture);
                row["Y"] = decimal.Parse(anglexy[2], CultureInfo.InvariantCulture);
                data.Rows.Add(row);
            }

        }

        private void CreateDataset()
        {
            DataSet Flexmap = new DataSet("Flexmap");
            DataTable data = Flexmap.Tables.Add("data");

            data.Columns.Add("Angle", typeof(decimal));
            data.Columns.Add("X", typeof(decimal));
            data.Columns.Add("Y", typeof(decimal));

            this.FlexmapDataSet = Flexmap;
        }

        private void ResetDataset()
        {
            this.FlexmapDataSet.Clear();
            this.FlexmapDataSet.Tables["data"].DefaultView.RowFilter = string.Empty;
        }

        private void AddToDataset()
        {
            this.dataGridView1.DataSource = this.FlexmapDataSet.Tables["data"];
            SetDataGridViewProperties();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Move button
            decimal dx;
            decimal dy;

            if (!decimal.TryParse(this.textBox1.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out dx) |
                !decimal.TryParse(this.textBox2.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out dy) |
                this.textBox1.Text.Contains(",") | this.textBox2.Text.Contains(","))
            {
                MessageBox.Show("Invalid input, dx, dy.", "Error");
                return;
            }
            else
            {
                MoveColumns(dx, dy);

            }
        }


        private void MoveColumns(decimal dx, decimal dy)
        {
            for (int i=0; i < this.FlexmapDataSet.Tables["data"].Rows.Count; i++)
            {
                decimal valX = (decimal)this.FlexmapDataSet.Tables["data"].Rows[i]["X"];
                this.FlexmapDataSet.Tables["data"].Rows[i]["X"] = Decimal.Round(Decimal.Add(valX, dx), 3);
                decimal valY = (decimal)this.FlexmapDataSet.Tables["data"].Rows[i]["Y"];
                this.FlexmapDataSet.Tables["data"].Rows[i]["Y"] = Decimal.Round(Decimal.Add(valY, dy), 3);
            }

            UpdateCharts();
        }


        private bool ValidateInput(string text)
        {
            decimal dx;

            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out dx) & !text.Contains(","))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (ValidateInput(this.textBox1.Text))
            {
                this.textBox1.ForeColor = Color.Black;
            }
            else
            {
                this.textBox1.ForeColor = Color.Red;
            }
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (ValidateInput(this.textBox2.Text))
            {
                this.textBox2.ForeColor = Color.Black;
            }
            else
            {
                this.textBox2.ForeColor = Color.Red;
            }
        }

        private void ClearAll()
        {
            this.FlexmapPath = "";
            this.FlexmapContent = new List<string>() { };
            this.FlexmapHeader = new  string[] { };
            this.label4.Text = "";
            this.richTextBox1.Text = "";
            ResetDataset();
            ClearAllCharts();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ClearAll();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Save button
            if (this.FlexmapDataSet.Tables["data"].Rows.Count == 0)
            {
                MessageBox.Show("Cannot save empty dataset.", "Error");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "Flexmap files (*.flexmap)|*.flexmap|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    String path = saveFileDialog.FileName;
                    using (StreamWriter stream = new StreamWriter(path))
                    {
                        for (int i = 0; i < this.FlexmapHeader.Count(); i++)
                        {
                            stream.WriteLine(this.FlexmapHeader[i]);
                        }

                        var table = this.FlexmapDataSet.Tables["data"];
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            string angle = ((decimal)table.Rows[i]["Angle"]).ToString("000.000", CultureInfo.CreateSpecificCulture("en-GB"));
                            string X = ((decimal)table.Rows[i]["X"]).ToString("000.000", CultureInfo.CreateSpecificCulture("en-GB"));
                            string Y = ((decimal)table.Rows[i]["Y"]).ToString("000.000", CultureInfo.CreateSpecificCulture("en-GB"));
                            stream.WriteLine(ConvertString(angle) + " " + "\t" +  ConvertString(X) + " " + "\t"  + ConvertString(Y));
                        }
                    }
                    MessageBox.Show("File saved to:\n" +path +"\n\nMake sure that the data structure was preserved!", "File saved");
                }
                catch (Exception f)
                {
                    MessageBox.Show("Cannot save file \n" + f.ToString(), "Error");
                }
            }
        }

        private string ConvertString(string vars)
        {
            // this is a very nasty way of shaping strings 
            // the final shape should match the one Elekta is using.
            // do not change whitespace characters and tabs!
            string result = vars;

            var sep = vars.Split('.').ToList();

            if (sep[0][0] == '-')
            {
                if (sep[0][1] == '0')
                {
                    if (sep[0][2] == '0')
                    {
                        result = " -" + sep[0].Substring(3) + "." + sep[1];
                    }
                    else
                    {
                        result = "-" + sep[0].Substring(2) + "." + sep[1];
                    }
                }
            }

            if (sep[0][0] == '0')
            {
                if(sep[0][1] == '0')
                {
                    result = "  " + sep[0].Substring(2) + "." + sep[1];
                }
                else
                {
                    result = " " + sep[0].Substring(1) + "." + sep[1];
                }
            }
            return result;
        }

        private void UpdateCharts()
        {
            AddPointsChart(this.chart1, "X");
            AddPointsChart(this.chart2, "Y");
            AddPointsChart(this.chart3, "XY");
        }

        private void ClearAllCharts()
        {
            this.chart1.Series.Clear();
            this.chart2.Series.Clear();
            this.chart3.Series.Clear();
        }

        private void AddPointsChart(Chart chart, string variable)
        {
            ChartArea CA = chart.ChartAreas[0];

            CA.AxisX.ScaleView.Zoomable = true;
            CA.CursorX.AutoScroll = false;
            CA.CursorX.IsUserSelectionEnabled = true;
            CA.CursorX.IsUserEnabled = true;
            CA.AxisX.ScrollBar.IsPositionedInside = true;

            CA.AxisY.ScaleView.Zoomable = true;
            CA.CursorY.AutoScroll = false;
            CA.CursorY.IsUserSelectionEnabled = true;
            CA.CursorY.IsUserEnabled = true;
            CA.AxisY.ScrollBar.IsPositionedInside = true;

            CA.AxisX.ScaleView.MinSize = 0.02;
            CA.AxisY.ScaleView.MinSize = 0.02;

            CA.CursorX.Interval = 0.02;
            CA.CursorY.Interval = 0.02;

            CA.AxisX.IntervalType = DateTimeIntervalType.Number;

            CA.AxisX.MajorGrid.LineColor = Color.Gainsboro;
            CA.AxisY.MajorGrid.LineColor = Color.Gainsboro;

            CA.AxisX.IsStartedFromZero = false;
            CA.AxisY.IsStartedFromZero = false;
            CA.RecalculateAxesScale();

            chart.Series.Clear();

            chart.Series.Add(variable);

            //chart.Series[variable].XValueType = ChartValueType.Auto;
            if (variable == "XY")
            {
                chart.Series[variable].XValueMember = "X";
                chart.Series[variable].YValueMembers = "Y";
                chart.Series[variable].ToolTip = "X = #VALX \n Y = #VAL";
            }
            else
            {
                chart.Series[variable].XValueMember = "Angle";
                chart.Series[variable].YValueMembers = variable;
                chart.Series[variable].ToolTip = "#VALX \n " + variable + " = #VAL";
                CA.AxisX.Maximum = 182.0;
                CA.AxisX.Minimum = -182.0;
            }
            
            chart.Series[variable].ChartType = SeriesChartType.Line;
            chart.Series[variable].MarkerStyle = MarkerStyle.Circle;

            chart.Series[variable].Color = Color.Gray;
            chart.Series[variable].MarkerColor = Color.Blue;
            chart.Series[variable].MarkerSize = 4;

            chart.Legends[0].Docking = Docking.Top;
            
            chart.DataSource = this.FlexmapDataSet.Tables["data"].DefaultView.ToTable();
        }
    }
}
