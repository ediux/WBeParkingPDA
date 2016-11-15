using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Media;

namespace TCPark_PDA
{
    public partial class FrmMsg : Form
    {
        public FrmMsg(string Title, string Msg, ChxMsgBtnType MsgBtnType, MessageBoxDefaultButton DefaultBtn,
            string LeftBtn, string RigthBtn, string MiddleBtn, ChxMsgType SoundType)
        {
            InitializeComponent();

            this.Text = Title;
            lblMsg.Text = Msg;

            SetBtns(MsgBtnType, LeftBtn, RigthBtn, MiddleBtn);
            SetDefaultBtn(MsgBtnType, DefaultBtn);

            PlayMsgSound(SoundType);
        }

        private void SetBtns(ChxMsgBtnType MsgBtnType, string LeftBtn, string RigthBtn, string MiddleBtn)
        {
            switch (MsgBtnType)
            {
                case ChxMsgBtnType.OK:
                    btnConfirm.Visible = false;
                    btnCancel.Text = "確認";
                    break;
                case ChxMsgBtnType.OKCancel:
                    break;
                case ChxMsgBtnType.Two:
                    if (LeftBtn != string.Empty) btnConfirm.Text = LeftBtn;
                    if (RigthBtn != string.Empty) btnCancel.Text = RigthBtn;
                    break;
                case ChxMsgBtnType.Three:
                    if (LeftBtn != string.Empty) btnConfirm.Text = LeftBtn;
                    if (RigthBtn != string.Empty) btnCancel.Text = RigthBtn;
                    if (MiddleBtn != string.Empty) btnMid.Text = MiddleBtn;
                    btnMid.Visible = true;
                    break;
                default:
                    throw new Exception("無法取得對話視窗類型!!");
            }
        }

        private void SetDefaultBtn(ChxMsgBtnType MsgBtnType, MessageBoxDefaultButton DefaultBtn)
        {
            switch (DefaultBtn)
            {
                case MessageBoxDefaultButton.Button1:
                    btnConfirm.Focus();
                    if (MsgBtnType == ChxMsgBtnType.OK) btnCancel.Focus();
                    break;
                case MessageBoxDefaultButton.Button2:
                    btnCancel.Focus();
                    if (MsgBtnType == ChxMsgBtnType.Three) btnMid.Focus();
                    break;
                case MessageBoxDefaultButton.Button3:
                    btnCancel.Focus();
                    break;
                default:
                    throw new Exception("無法取得預設按鈕!!");
            }
        }

        private void PlayMsgSound(ChxMsgType SoundType)
        {
            SoundPlayer player = new SoundPlayer();

            switch (SoundType)
            {
                case ChxMsgType.Ask:
                    //player.PlayWarnSound();
                    break;
                case ChxMsgType.Error:
                    //player.PlayErrSound();
                    break;
                case ChxMsgType.None:
                    break;
                case ChxMsgType.OK:
                    //player.PlayOKSound();
                    break;
                default:
                    throw new Exception("無法取得欲播放音效!!");
            }
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void btnMid_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }

        #region ShowDialogFunction

        private readonly int GWL_STYLE = (-16);

        private bool centered = true;

        public bool CenterFormOnScreen
        {
            get
            {
                return centered;
            }
            set
            {
                centered = value;

                if (centered)
                {
                    CenterWithinScreen();
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            UInt32 WS_CAPTION = 0x00C00000;     /* WS_BORDER | WS_DLGFRAME  */
            UInt32 WS_BORDER = 0x00800000;
            UInt32 WS_POPUP = 0x80000000;

            UInt32 SHDB_SHOW = 0x0001;
            UInt32 SHDB_HIDE = 0x0002;
            //if (!this.InputBoxVisible)
            //FATool.MSAPI.SetWindowLong(this.Handle, GWL_STYLE, WS_BORDER | WS_CAPTION | WS_POPUP);

            //FATool.MSAPI.SHDoneButton(this.Handle, this.ControlBox ? SHDB_SHOW : SHDB_HIDE);

            if (centered)
            {
                CenterWithinScreen();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (centered)
            {
                CenterWithinScreen();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (this.ControlBox)
            {
                if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Escape)
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
        }

        private void CenterWithinScreen()
        {

            int x = (Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2;
            int y = (Screen.PrimaryScreen.WorkingArea.Height - this.Height) / 2;

            this.Location = new Point(x, y);
        }
        #endregion
    }

    public enum ChxMsgBtnType
    {
        OK = 0,
        OKCancel = 1,
        Two = 2,
        Three = 3,
    }

    public enum ChxMsgType
    {
        OK = 0,
        Error = 1,
        Ask = 2,
        None = 3,
    }
}