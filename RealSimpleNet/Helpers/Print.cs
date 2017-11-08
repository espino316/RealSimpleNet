using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace RealSimpleNet.Helpers
{
    public class PrintHelper
    {
        public static void PrintFile(string filePath)
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                Verb = "print",
                FileName = filePath //put the correct path here
            };
            p.Start();
        }
        class PrintItems
        {
            public enum ItemTypes
            {
                Text,
                Table,
                Blankline
            }

            private ItemTypes _ItemType;
            public ItemTypes ItemType
            {
                get { return _ItemType; }
                set { _ItemType = value; }
            }

            public object _Item;
            public object Item
            {
                get { return _Item; }
                set { _Item = value; }
            }

            private float? _x;
            public float? x
            {
                get { return _x; }
                set { _x = value; }
            }

            private float? _y;
            public float? y
            {
                get { return _y; }
                set { _y = value; }
            }

            public PrintItems()
            {

            }

            public PrintItems(ItemTypes itemtype, object item)
            {
                this.ItemType = itemtype;
                this.Item = item;
                this.x = null;
                this.y = null;
            }

            public PrintItems(ItemTypes itemtype, object item, float x, float y)
            {
                this.ItemType = itemtype;
                this.Item = item;
                this.x = x;
                this.y = y;
            }
        }


        public enum MeasureTypes
        {
            Centimeters,
            Inches,
            Pixels
        }

        private MeasureTypes _MeasureType = MeasureTypes.Pixels;
        public MeasureTypes MeasureType
        {
            get { return _MeasureType; }
            set { _MeasureType = value; }
        }

        private float _Width;
        public float Width
        {
            get { return _Width; }
            set { _Width = value; }
        }

        private float _Height;
        public float Height
        {
            get { return _Height; }
            set { _Height = value; }
        }

        private string _PrinterName;
        public string PrinterName
        {
            get { return _PrinterName; }
            set { _PrinterName = value; }
        }

        private Font _MyFont;
        public Font MyFont
        {
            get { return _MyFont; }
            set { _MyFont = value; }
        }

        public PrintDocument PrintDoc;
        Brush MyBrush;
        Pen MyPen;
        public int X;
        public int Y;
        List<PrintItems> Items;
        int Padding = 2;
        float ColWidth;
        int RowHeight;
        float MaxWidth;

        public PrintHelper()
        {
            X = 0;
            Y = 0;
            MyFont = new Font("Arial", 8);
            MyBrush = Brushes.Black;
            MyPen = Pens.Black;
            PrintDoc = new PrintDocument();
            PrintDoc.DefaultPageSettings.Margins.Top = 0;
            PrintDoc.DefaultPageSettings.Margins.Left = 0;
            PrintDoc.DefaultPageSettings.Margins.Bottom = 0;
            PrintDoc.DefaultPageSettings.Margins.Right = 0;
            PrintDoc.DocumentName = string.Format("Document_{0:yyyyMMddHHmmss}", DateTime.Now);
            Items = new List<PrintItems>();
            this.Height = CentimetersToPixels(17.1F);
            this.Width = CentimetersToPixels(21.4F);
        }

        public void PrintText(string text)
        {
            Items.Add(new PrintItems(PrintItems.ItemTypes.Text, text));
        }

        public void PrintText(string text, float x, float y)
        {
            Items.Add(new PrintItems(PrintItems.ItemTypes.Text, text, x, y));
        }

        public void PrintText(string text, PointF point)
        {
            Items.Add(new PrintItems(PrintItems.ItemTypes.Text, text, point.X, point.Y));
        }

        public void PrintText(string text, params object[] args)
        {
            Items.Add(new PrintItems(PrintItems.ItemTypes.Text, string.Format(text, args)));
        }

        public void PrintTable(DataTable dtable)
        {
            Items.Add(new PrintItems(PrintItems.ItemTypes.Table, dtable));
        }

        public void PrintTable(BindingSource bs)
        {
            Items.Add(new PrintItems(PrintItems.ItemTypes.Table, bs));
        }

        public void PrintTable<T>(List<T> list, PointF point)
        {
            Items.Add(new PrintItems(PrintItems.ItemTypes.Table, list, point.X, point.Y));
        }

        public void PrintLine()
        {
            Items.Add(new PrintItems(PrintItems.ItemTypes.Blankline, null));
        }

        public void PrintCLRF()
        {
            Items.Add(new PrintItems(PrintItems.ItemTypes.Blankline, null));
        }

        public Bitmap PrintToBitmap()
        {
            Bitmap bmp = null, newbmp = null;
            Graphics g = null;

            try
            {
                bmp = new Bitmap(Convert.ToInt32(this.Width), Convert.ToInt32(this.Height));
                g = Graphics.FromImage(bmp);
                g.FillRectangle(Brushes.White, new Rectangle(new Point(0, 0), new Size(500, 500)));
                DrawItems(ref g);
                newbmp = new Bitmap(Convert.ToInt32(MaxWidth + Padding * 4), Convert.ToInt32(Y + Padding * 4));
                g = Graphics.FromImage(newbmp);
                g.DrawImage(bmp, new Point(Padding * 2, Padding * 2));
                return newbmp;
            }
            finally
            {
                if (g != null) g.Dispose();
                if (bmp != null) bmp.Dispose();
            }
        }

        public void Print()
        {
            PrintDoc.PrintPage += this.PrintPage;
            if (!string.IsNullOrEmpty(this.PrinterName))
                PrintDoc.PrinterSettings.PrinterName = this.PrinterName;
            PrintDoc.Print();
        }

        public float CentimetersToPixels(float cm)
        {
            return cm * 37.795F;
        }

        /// <summary>
        /// Devuelve el texto enviado a imprimir
        /// </summary>
        /// <returns></returns>
        public string PrintToText()
        {
            StringBuilder sb = new StringBuilder();

            foreach (PrintItems item in this.Items)
            {
                switch (item.ItemType.ToString())
                {
                    case "Blankline":

                        sb.AppendLine();
                        break;

                    case "Text":

                        sb.AppendLine(item.Item.ToString());

                        break;

                } // end switch

            } // end foreach


            return sb.ToString();

        } // end PrintToText

        private void DrawItems(ref Graphics g)
        {
            foreach (PrintItems item in this.Items)
            {
                switch (item.ItemType.ToString())
                {
                    case "Blankline":

                        g.DrawString("", MyFont, MyBrush, new Point(X, Y));
                        Y += Convert.ToInt32(g.MeasureString("ABCDE", MyFont).Height) + Padding;
                        MaxWidth = X;
                        break;

                    case "Text":

                        string text = item.Item.ToString();

                        if (item.x == null || item.y == null)
                        {
                            if (Validations.IsNumeric(item.Item))
                            {
                                text = string.Format("{0:N2}", item.Item);
                            }

                            g.DrawString(text, MyFont, MyBrush, new Point(X, Y));

                            Y += Convert.ToInt32(g.MeasureString(item.Item.ToString(), MyFont).Height) + Padding;
                            MaxWidth = X + Convert.ToInt32(g.MeasureString(item.Item.ToString(), MyFont).Width) + Padding;
                        }
                        else
                        {
                            if (this.MeasureType == MeasureTypes.Centimeters)
                            {
                                item.x = this.CentimetersToPixels(item.x.Value);
                                item.y = this.CentimetersToPixels(item.y.Value);
                            }

                            g.DrawString(text, MyFont, MyBrush, new PointF(item.x.Value, item.y.Value));
                        }
                        break;

                    case "Table":

                        if (typeof(DataTable) == item.Item.GetType())
                        {
                            DataTable dt = (DataTable)item.Item;

                            if (dt.Rows.Count > 0)
                            {
                                ColWidth = Width / dt.Columns.Count;
                                RowHeight = Convert.ToInt32(g.MeasureString("ABCDF", MyFont).Height);

                                //  Headers
                                foreach (DataColumn col in dt.Columns)
                                {
                                    //  DrawString
                                    int leftmargin = Convert.ToInt32((dt.Columns.IndexOf(col)) * ColWidth);
                                    g.DrawString(col.ColumnName, MyFont, MyBrush, new RectangleF(leftmargin, Y, ColWidth, RowHeight));

                                }
                                Y += RowHeight + Padding;
                                MaxWidth = dt.Columns.Count * ColWidth + Padding;
                                //  DrawLine
                                g.DrawLine(MyPen, new Point(X, Y), new PointF(Width, Y));

                                //  Values
                                foreach (DataRow dr in dt.Rows)
                                {
                                    foreach (DataColumn dc in dt.Columns)
                                    {
                                        //  DrawString
                                        string val;
                                        int leftmargin = Convert.ToInt32((dt.Columns.IndexOf(dc)) * ColWidth);

                                        if (Validations.IsNumeric(dr[dc.ColumnName]))
                                        {
                                            StringFormat sf = new StringFormat();
                                            sf.Alignment = StringAlignment.Far;
                                            val = string.Format("{0:N2}", dr[dc.ColumnName]);
                                            g.DrawString(val, MyFont, MyBrush, new RectangleF(leftmargin, Y, ColWidth, RowHeight), sf);
                                        }
                                        else
                                        {
                                            val = dr[dc.ColumnName].ToString();
                                            g.DrawString(val, MyFont, MyBrush, new RectangleF(leftmargin, Y, ColWidth, RowHeight));
                                        }

                                        //  DrawRectangle
                                    }
                                    Y += RowHeight + Padding;
                                } // end foreach
                            } // end if
                        } // end if
                        else if (item.Item.GetType() == typeof(BindingSource))
                        {
                            BindingSource bs = (BindingSource)item.Item;
                            PropertyInfo[] properties = bs.Current.GetType().GetProperties();
                            int limit = bs.Count;

                            if (bs.Count > 0)
                            {
                                ColWidth = Width / bs.Current.GetType().GetProperties().Length;
                                RowHeight = Convert.ToInt32(g.MeasureString("ABCDF", MyFont).Height);

                                /* NO headers
                                //  Headers
                                foreach (PropertyInfo col in bs.Current.GetType().GetProperties())
                                {
                                    //  DrawString
                                    //int leftmargin = Convert.ToInt32((dt.Columns.IndexOf(col)) * ColWidth);
                                    //g.DrawString(col.ColumnName, MyFont, MyBrush, new RectangleF(leftmargin, Y, ColWidth, RowHeight));

                                }
                                 * */

                                Y += RowHeight + Padding;
                                MaxWidth = bs.Current.GetType().GetProperties().Length * ColWidth + Padding;

                                /* NO LINE
                                //  DrawLine
                                g.DrawLine(MyPen, new Point(X, Y), new PointF(Width, Y));
                                */

                                for (int i = 0; i < limit; i++)
                                {
                                    object val;
                                    int leftmargin = X;

                                    foreach (PropertyInfo prop in properties)
                                    {
                                        val = prop.GetValue(bs.Current, null);
                                        if (Validations.IsNumeric(val))
                                        {
                                            StringFormat sf = new StringFormat();
                                            sf.Alignment = StringAlignment.Far;
                                            string strval = string.Format("{0:N2}", val);
                                            g.DrawString(strval, MyFont, MyBrush, new RectangleF(leftmargin, Y, ColWidth, RowHeight), sf);
                                        }
                                        else
                                        {
                                            string strval = val.ToString();
                                            g.DrawString(strval, MyFont, MyBrush, new RectangleF(leftmargin, Y, ColWidth, RowHeight));
                                        }

                                        leftmargin += Convert.ToInt32(leftmargin + ColWidth);
                                    }
                                    Y += RowHeight + Padding;

                                    bs.MoveNext();
                                } // end for
                            } // end if
                        } // end else if
                        else if (item.Item is IList && item.Item.GetType().IsGenericType)
                        {
                            object obj;
                            BindingSource bs = new BindingSource(item.Item, null);
                            int limit = bs.Count;
                            ColWidth = Convert.ToInt32(g.MeasureString("__" + string.Format("{0:N2}", bs.Current), MyFont).Width);
                            RowHeight = Convert.ToInt32(g.MeasureString(string.Format("{0:N2}", bs.Current), MyFont).Height);

                            float x = (item.x == null) ? X : item.x.Value;
                            float y = (item.x == null) ? Y : item.y.Value;

                            for (int i = 0; i < limit; i++)
                            {
                                obj = bs.Current;

                                if (Validations.IsNumeric(obj))
                                {
                                    StringFormat sf = new StringFormat();
                                    sf.Alignment = StringAlignment.Far;
                                    string strval = string.Format("{0:N2}", obj);
                                    g.DrawString(strval, MyFont, MyBrush, new RectangleF(x, y, ColWidth, RowHeight), sf);
                                }
                                else
                                {
                                    string strval = obj.ToString();
                                    g.DrawString(strval, MyFont, MyBrush, new RectangleF(x, y, ColWidth, RowHeight));
                                }

                                y += RowHeight + Padding;

                                bs.MoveNext();
                            }
                        }

                        break;

                } // End switch
            } // End foreach
        }

        private bool IsList(object val)
        {
            try
            {

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            DrawItems(ref g);
        } // End void
    } // End class
}
