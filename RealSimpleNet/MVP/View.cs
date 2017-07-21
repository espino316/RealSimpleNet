using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static RealSimpleNet.Helpers.Validations;

namespace RealSimpleNet.MVP
{
    public class View : Form
    {
        public DialogResult ShowInfo(string text, string title = "Info")
        {
            return MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        } // end DialogResult

        public DialogResult ShowWarning(string text, string title = "Advertencia")
        {
            return MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        } // end DialogResult

        public DialogResult ShowError(string text, string title = "Error")
        {
            return MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        } // end DialogResult

        /// <summary>
        /// Devuelve un cuadro de dialogo de confirmacion
        /// </summary>
        /// <param name="msg">El mensaje a desplegar</param>
        /// <returns>DialogResult</returns>
        public DialogResult Confirm(string msg)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = "Confirm";
            label.Text = msg;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            return dialogResult;
        }

        public DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        /// <summary>
        /// Sets a binding
        /// </summary>
        /// <param name="control"></param>
        /// <param name="controlProperty"></param>
        /// <param name="dataSource"></param>
        /// <param name="dataMember"></param>
        public void SetBinding(
            Control control,
            string controlProperty,
            object dataSource,
            string dataMember
        )
        {
            control.DataBindings.Add(
                new Binding(
                    controlProperty,
                    dataSource,
                    dataMember,
                    true,
                    DataSourceUpdateMode.OnPropertyChanged
                ) // end new binding
            ); // end control databindings add            
        } // end void SetBinding

        public void ValidInputOnlyNumbers_Keypress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) && !char.IsPunctuation(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        public void ValidInputOnlyDigits_Keypress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        public void ValidInputOnlyChars_Keypress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        public void AddTextBoxOnlyDigitsValidation(ref TextBox txt)
        {
            txt.KeyPress += ValidInputOnlyDigits_Keypress;
        }

        public void AddTextBoxOnlyNumbersValidation(ref TextBox txt)
        {
            txt.KeyPress += ValidInputOnlyNumbers_Keypress;
        }

        public void AddTextBoxesOnlyNumbersValidation(params TextBox[] textboxes)
        {
            foreach (TextBox txt in textboxes)
            {
                txt.KeyPress += ValidInputOnlyNumbers_Keypress;
            }
        }

        public void AddTextBoxOnlyCharsValidation(ref TextBox txt)
        {
            txt.KeyPress += ValidInputOnlyChars_Keypress;
        }

        public void AddTextBoxesOnlyCharsValidation(params TextBox[] textboxes)
        {
            foreach (TextBox txt in textboxes)
            {
                txt.KeyPress += ValidInputOnlyChars_Keypress;
            }
        }

        /// <summary>
        /// Realiza la sumatoria de todos los valores de una columna en un DataGridView
        /// </summary>
        /// <param name="dgv">El DataGridView sobre el cual se realizará la operación</param>
        /// <param name="column">La columna a sumar</param>
        /// <returns></returns>
        public decimal DGVSum(DataGridView dgv, string column)
        {
            decimal ret = 0;

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.Cells[column].Value != null &&
                        !Convert.IsDBNull(row.Cells[column]))
                {
                    ret += Convert.ToDecimal(row.Cells[column].Value);
                }
            }

            return ret;
        }

        /// <summary>
        /// Pone en blanco los controles de tipo TextBox pertenecientes al control especificado
        /// </summary>
        /// <param name="ctrl">El control al que se le limpiaran las cajas de texto</param>
        public void ClearControl(Control ctrl)
        {
            foreach (Control c in ctrl.Controls)
            {
                if (c.GetType() == typeof(System.Windows.Forms.TextBox))
                {
                    c.Text = "";
                }

                if (c.GetType() == typeof(System.Windows.Forms.ComboBox))
                {
                    ComboBox cbo = (ComboBox)c;
                    cbo.SelectedIndex = 0;
                }

                if (c.GetType() == typeof(System.Windows.Forms.ListBox))
                {
                    ListBox list = (ListBox)c;
                    if (!Helpers.Validations.IsNullOrEmpty(list.DataSource))
                    {
                        if (list.DataSource.GetType() != typeof(System.Windows.Forms.BindingSource))
                        {
                            list.DataSource = null;
                            list.Items.Clear();
                        }
                    }
                }

                ClearControl(c);
            }
        }

        /// <summary>
        /// Aplica opciones de validación a un control
        /// </summary>
        /// <param name="ctrl">El control a validar</param>
        /// <param name="args">Los argumentos de validacion</param>
        public static void ValidateControl(Control ctrl, params ValidateRule[] args)
        {
            if (ctrl.GetType() == typeof(TextBox))
            {
                TextBox txt = (TextBox)ctrl;

                foreach (ValidateRule rule in args)
                {
                    switch (rule)
                    {
                        case ValidateRule.Numeric:
                            if (!IsNumeric(txt.Text))
                            {
                                throw new Exception("El valor debe ser numérico!");
                            }
                            break;
                        case ValidateRule.Required:
                            if (txt.Text == "")
                            {
                                throw new Exception(String.Format("Debe capturar un valor para el control {0}", txt.Name));
                            }
                            break;
                    }
                }
            }
            else if (ctrl.GetType() == typeof(ComboBox))
            {
                ComboBox cbo = (ComboBox)ctrl;

                foreach (ValidateRule rule in args)
                {
                    switch (rule)
                    {
                        case ValidateRule.Required:
                            if (cbo.SelectedItem == null)
                            {
                                throw new Exception(String.Format("Debe capturar un valor para el control {0}", cbo.Name));
                            }

                            if (cbo.SelectedIndex < 0)
                            {
                                throw new Exception(String.Format("Debe capturar un valor para el control {0}", cbo.Name));
                            }
                            break;
                    }
                }
            }
            else if (ctrl.GetType() == typeof(CheckBox))
            {
                CheckBox check = (CheckBox)ctrl;

                foreach (ValidateRule rule in args)
                {
                    switch (rule)
                    {
                        case ValidateRule.Required:
                            if (check.CheckState == CheckState.Indeterminate)
                            {
                                throw new Exception(String.Format("Debe capturar un valor para el control {0}", check.Name));
                            }
                            break;
                    }
                }
            }
            else if (ctrl.GetType() == typeof(DateTimePicker))
            {
                DateTimePicker dtpick = (DateTimePicker)ctrl;

                foreach (ValidateRule rule in args)
                {
                    switch (rule)
                    {
                        case ValidateRule.Required:
                            if (!dtpick.Checked)
                            {
                                throw new Exception(String.Format("Debe capturar un valor para el control {0}", dtpick.Name));
                            }
                            break;
                    }
                }
            }
        } // end void ValidateControl
    } // end class View
} // end namaspace MVP
