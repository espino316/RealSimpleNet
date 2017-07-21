using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Data;

namespace RealSimpleNet.Helpers
{
    class Exporting
    {
        private const string TAB = "\t";
        private const string BR = "\r\n";

        public static SaveFileDialog ExportSaveFileDialog;

        public static void ExportDataGridViewToMSExcel(DataGridView dgv)
        {
            if (ExportSaveFileDialog == null) ExportSaveFileDialog = new SaveFileDialog();

            ExportSaveFileDialog.Title = "Guarde un archivo de MS Excel";
            ExportSaveFileDialog.Filter = "Excel Files|*.xls";
            if (ExportSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(ExportSaveFileDialog.FileName))
                {
                    string ruta = ExportSaveFileDialog.FileName;
                    GridToXls(dgv, ruta);
                }
            }
        }

        public static void ExportDataGridViewToCSV(DataGridView dgv)
        {
            if (ExportSaveFileDialog == null) ExportSaveFileDialog = new SaveFileDialog();

            ExportSaveFileDialog.Title = "Guarde un archivo CSV";
            ExportSaveFileDialog.Filter = "CSV Files|*.csv";
            if (ExportSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(ExportSaveFileDialog.FileName))
                {
                    string ruta = ExportSaveFileDialog.FileName;
                    GridToCSV(dgv, ruta);
                }
            }
        }

        public static void ExportDataGridViewToTXT(DataGridView dgv)
        {
            if (ExportSaveFileDialog == null) ExportSaveFileDialog = new SaveFileDialog();

            ExportSaveFileDialog.Title = "Guarde un archivo TXT";
            ExportSaveFileDialog.Filter = "Archivos de texto|*.txt";
            if (ExportSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(ExportSaveFileDialog.FileName))
                {
                    string ruta = ExportSaveFileDialog.FileName;
                    GridToTXT(dgv, ruta);
                }
            }
        }

        public static void ExportDataGridViewToHTML(DataGridView dgv)
        {
            if (ExportSaveFileDialog == null) ExportSaveFileDialog = new SaveFileDialog();

            ExportSaveFileDialog.Title = "Guarde un archivo HTML";
            ExportSaveFileDialog.Filter = "Paginas web HTML|*.html";
            if (ExportSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(ExportSaveFileDialog.FileName))
                {
                    string ruta = ExportSaveFileDialog.FileName;
                    GridToHTML(dgv, ruta);
                }
            }
        }

        private static void GridToCSV(DataGridView dgv, string ruta)
        {

            StringBuilder sb = new StringBuilder();

            string cols = string.Empty;

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Visible == true && col.IsDataBound)
                {
                    if (col.Index < dgv.Columns.Count - 1)
                    {
                        cols += col.HeaderText + "," + TAB;
                    }
                    else
                    {
                        cols += col.HeaderText;
                    }
                }
            }

            sb.Append(cols + BR);

            foreach (DataGridViewRow row in dgv.Rows)
            {
                cols = string.Empty;
                foreach (DataGridViewCell cel in row.Cells)
                {
                    if (dgv.Columns[cel.ColumnIndex].Visible == true && dgv.Columns[cel.ColumnIndex].IsDataBound)
                    {
                        if (cel.ColumnIndex < row.Cells.Count)
                        {
                            cols += Validations.IsNull(cel.Value, "").ToString() + "," + TAB;
                        }
                        else
                        {
                            cols += Validations.IsNull(cel.Value, "").ToString();
                        }
                    }
                }
                sb.Append(cols + BR);
            }


            StreamWriter sw = new StreamWriter(ruta, false);
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();

            Process.Start(ruta);
        }

        private static void GridToTXT(DataGridView dgv, string ruta)
        {

            StringBuilder sb = new StringBuilder();

            string cols = string.Empty;

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Visible == true && col.IsDataBound)
                {
                    if (col.Index < dgv.Columns.Count - 1)
                    {
                        cols += col.HeaderText + TAB;
                    }
                    else
                    {
                        cols += col.HeaderText;
                    }
                }
            }

            sb.Append(cols + BR);

            foreach (DataGridViewRow row in dgv.Rows)
            {
                cols = string.Empty;
                foreach (DataGridViewCell cel in row.Cells)
                {
                    if (dgv.Columns[cel.ColumnIndex].Visible == true && dgv.Columns[cel.ColumnIndex].IsDataBound)
                    {
                        if (cel.ColumnIndex < row.Cells.Count)
                        {
                            cols += Validations.IsNull(cel.Value, "").ToString() + TAB;
                        }
                        else
                        {
                            cols += Validations.IsNull(cel.Value, "").ToString();
                        }
                    }
                }
                sb.Append(cols + BR);
            }


            StreamWriter sw = new StreamWriter(ruta, false);
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();

            Process.Start(ruta);
        }

        private static void GridToXls(DataGridView dgv, string ruta)
        {

            StringBuilder sb = new StringBuilder();

            sb.Append("<table border=\"1\" cellspacing=\"0\" cellpadding=\"1\" rules=\"rows\">" + BR);

            string cols = string.Empty;

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Visible == true && col.IsDataBound)
                {
                    cols += Th(col.HeaderText);
                }
            }

            sb.Append(Tr(cols));

            foreach (DataGridViewRow row in dgv.Rows)
            {
                cols = string.Empty;

                foreach (DataGridViewCell cel in row.Cells)
                {
                    if (dgv.Columns[cel.ColumnIndex].Visible == true && dgv.Columns[cel.ColumnIndex].IsDataBound)
                    {
                        cols +=
                            Td(
                                Validations.IsNull(
                                    cel.Value,
                                    ""
                                ).ToString(),
                                row.DefaultCellStyle.BackColor.Name
                            );
                    }
                }

                sb.Append(Tr(cols));
            }

            sb.Append(BR + "</table>");

            StreamWriter sw = new StreamWriter(ruta, false);
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();

            Process.Start(ruta);
        }

        public static string DataTableToHTML(DataTable dt)
        {

            StringBuilder sb = new StringBuilder();

            sb.Append("<table border=\"1\" cellspacing=\"0\" cellpadding=\"1\" rules=\"rows\">" + BR);

            string cols = string.Empty;

            foreach (DataColumn col in dt.Columns)
            {
                cols += Th(col.ColumnName);
            }

            sb.Append(Tr(cols));

            foreach (DataRow row in dt.Rows)
            {
                cols = string.Empty;

                foreach (DataColumn cel in dt.Columns)
                {
                    cols += Td(Validations.IsNull(row[cel.ColumnName], "").ToString());
                }

                sb.Append(Tr(cols));
            }

            sb.Append(BR + "</table>");

            return sb.ToString();
        }

        private static void GridToHTML(DataGridView dgv, string ruta)
        {

            StringBuilder sb = new StringBuilder();

            sb.Append("<table border=\"1\" cellspacing=\"0\" cellpadding=\"1\" rules=\"rows\">" + BR);

            string cols = string.Empty;

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Visible == true && col.IsDataBound)
                {
                    cols += Th(col.HeaderText);
                }
            }

            sb.Append(Tr(cols));

            foreach (DataGridViewRow row in dgv.Rows)
            {
                cols = string.Empty;
                foreach (DataGridViewCell cel in row.Cells)
                {
                    if (dgv.Columns[cel.ColumnIndex].Visible == true && dgv.Columns[cel.ColumnIndex].IsDataBound)
                    {
                        cols += Td(Validations.IsNull(cel.Value, "").ToString());
                    }
                }
                sb.Append(Tr(cols));
            }

            sb.Append(BR + "</table>");

            StreamWriter sw = new StreamWriter(ruta, false);
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();

            Process.Start(ruta);
        }

        private static string Th(object dato)
        {
            return "<th>" + Convert.ToString(dato) + "</th>";
        }

        private static string Td(object dato)
        {
            return "<td>" + Convert.ToString(dato) + "</td>";
        }

        private static string Td(object dato, string backcolor)
        {
            return "<td style=\"background-color:" + backcolor + "\">" + Convert.ToString(dato) + "</td>";
        }

        private static string Tr(string celdas)
        {
            return "<tr>" + celdas + "</tr>";
        }

        public static void DataTableToCSV(DataTable dt, string ruta, string delimiter)
        {

            StringBuilder sb = new StringBuilder();

            string cols = string.Empty;

            foreach (DataColumn col in dt.Columns)
            {
                if (col.Ordinal < dt.Columns.Count - 1)
                {
                    cols += col.ColumnName + delimiter + TAB;
                }
                else
                {
                    cols += col.ColumnName;
                }
            }

            sb.Append(cols + BR);

            foreach (DataRow row in dt.Rows)
            {
                cols = string.Empty;
                foreach (DataColumn col in dt.Columns)
                {
                    string val = Validations.IsNull(row[col.ColumnName], "").ToString();


                    if (col.Ordinal < dt.Columns.Count - 1)
                    {
                        cols += val + delimiter + TAB;
                    }
                    else
                    {
                        cols += val;
                    }
                }
                sb.Append(cols + BR);
            }

            StreamWriter sw = new StreamWriter(ruta, false);
            sw.Write(sb.ToString());
            sw.Flush();
            sw.Close();

            //Process.Start(ruta);
        }

    } // end class
} // end namespace
