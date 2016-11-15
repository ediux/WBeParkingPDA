using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TCPark_PDA
{
    public partial class FrmKeyBoard : Form
    {
        public string strInput = string.Empty;
        KeyBoardHelper _helper;

        private string _tmpString;

        private string TmpString
        {
            get { return _tmpString; }
            set
            {
                _tmpString = value;

                if (!isPwd)
                    lblInput.Text = value;
                else
                {
                    lblInput.Text = string.Empty.PadLeft(value.Length, '*');
                }
            }
        }

        private bool _IsPassword;
        public bool isPwd
        {
            get { return _IsPassword; }
            set
            {
                _IsPassword = value;

                TmpString = TmpString;
            }
        }

        public FrmKeyBoard(KeyBoardHelper helper, string NowText) : this(helper, NowText, ' ')
        {

        }

        public FrmKeyBoard(KeyBoardHelper helper, string NowText, char c)
        {
            InitializeComponent();

            _helper = helper;
            TmpString = NowText;
            
            strInput = TmpString;

            lblInput.Font = new Font(@"Tahoma", 20, helper.isBoldFont ? FontStyle.Bold : FontStyle.Regular);

            if (c != ' ' && char.IsLetterOrDigit(c))
            {
                NormalKey(c.ToString());
            }

            //bOpenScanner = GBL.Scanner.isOn;
        }



        bool bOpenScanner = false;
        private void FrmKeyBoard_Load(object sender, EventArgs e)
        {
            if (!bOpenScanner) return;
            //GBL.Scanner.ScannerEvent += new ChxUSIScanner.deleMyScanner(Scanner_ScannerEvent);

            //if (GBL.bLogOuting)
            //{
            //    this.DialogResult = DialogResult.Cancel;
            //    this.Close();
            //}
        }

        void Scanner_ScannerEvent(object sender, USICF.USIEventArgs e, string barcodeTrim)
        {
            InputOK(barcodeTrim);
        }

        private void FrmKeyBoard_Closing(object sender, CancelEventArgs e)
        {
            if (!bOpenScanner) return;
            //GBL.Scanner.ScannerEvent -= new ChxUSIScanner.deleMyScanner(Scanner_ScannerEvent);
        }

        bool bNoUse = false;
        private void AllKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (sender == btnConfirm || sender == btnBack || sender == btnExit)
                {
                    bNoUse = true;
                }

                InputOK(TmpString);
            }
            else if (e.KeyChar == (char)Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else if (e.KeyChar == (char)Keys.Back)
            {
                KeyBack();
            }
            else if (char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == '-')
            {
                NormalKey(e.KeyChar.ToString());
            }
            else if (e.KeyChar == '*' || e.KeyChar == '#')
            {
                NormalKey("-");
            }
        }

        private void AllKeyClick(object sender, EventArgs e)
        {
            if (sender == btnConfirm || sender == btnBack || sender == btnExit)
            {
                if (bNoUse)
                {
                    bNoUse = false;
                    return;
                }
            }

            if (sender == btnBack)
            {
                KeyBack();
            }
            else if (sender == btnConfirm)
            {
                InputOK(TmpString);
            }
            else if (sender == btnExit)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else
            {
                Button btn = sender as Button;
                NormalKey(btn.Text);
            }
        }

        private void KeyBack()
        {
            if (TmpString.Length > 0)
            {
                TmpString = TmpString.Substring(0, TmpString.Length - 1);
            }
        }

        private void InputOK(string Input)
        {
            if (!_helper.CheckMethod(Input))
            {
              WBeParkingPDA.Utility.ShowErrMsg(@"¿é¤J®æ¦¡¿ù»~!!");
                return;
            }

            strInput = Input;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void NormalKey(string key)
        {
            if (TmpString.Length >= _helper.nMaxLength) return;
            key = key.ToUpper();

            if (_helper.CantInputString.IndexOf(key) != -1) return;

            if (_helper.okString != string.Empty)
            {
                if (_helper.okString.IndexOf(key) == -1) return;
            }

            if (_helper.CheckInput != null)
            {
                if (!_helper.CheckInput(TmpString + key)) return;
            }

            TmpString += key;
        }
    }
}