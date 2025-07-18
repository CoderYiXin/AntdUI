// COPYRIGHT (C) Tom. ALL RIGHTS RESERVED.
// THE AntdUI PROJECT IS AN WINFORM LIBRARY LICENSED UNDER THE Apache-2.0 License.
// LICENSED UNDER THE Apache License, VERSION 2.0 (THE "License")
// YOU MAY NOT USE THIS FILE EXCEPT IN COMPLIANCE WITH THE License.
// YOU MAY OBTAIN A COPY OF THE LICENSE AT
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// UNLESS REQUIRED BY APPLICABLE LAW OR AGREED TO IN WRITING, SOFTWARE
// DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED.
// SEE THE LICENSE FOR THE SPECIFIC LANGUAGE GOVERNING PERMISSIONS AND
// LIMITATIONS UNDER THE License.
// GITCODE: https://gitcode.com/AntdUI/AntdUI
// GITEE: https://gitee.com/AntdUI/AntdUI
// GITHUB: https://github.com/AntdUI/AntdUI
// CSDN: https://blog.csdn.net/v_132
// QQ: 17379620

using System;
using System.Windows.Forms;

namespace Demo.Controls
{
    public partial class Modal : UserControl
    {
        Form form;
        public Modal(Form _form)
        {
            form = _form;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AntdUI.Modal.open(new AntdUI.Modal.Config(form, "This is a success message", "Some contents...Some contents...Some contents...Some contents...Some contents...Some contents...Some contents...", AntdUI.TType.Success)
            {
                OnButtonStyle = (id, btn) =>
                {
                    btn.BackExtend = "135, #6253E1, #04BEFE";
                },
                CancelText = null,
                OkText = "知道了"
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AntdUI.Modal.open(form, "This is a error message", "Some contents...Some contents...Some contents...Some contents...Some contents...Some contents...Some contents...", AntdUI.TType.Error);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            AntdUI.Modal.open(new AntdUI.Modal.Config(form, "This is a warn message", "Some contents...Some contents...Some contents...Some contents...Some contents...Some contents...Some contents...", AntdUI.TType.Warn)
            {
                Btns = new AntdUI.Modal.Btn[] { new AntdUI.Modal.Btn("按钮Name", "自定义按钮", AntdUI.TTypeMini.Warn) },
                OnBtns = btn =>
                {
                    MessageBox.Show("触发的Name：" + btn.Name);
                    return true;
                },
                OnOk = config =>
                {
                    System.Threading.Thread.Sleep(1000);
                    return true;
                }
            });
            button3.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            AntdUI.Modal.open(form, "Hello, Ant Design!", "Some contents...Some contents...Some contents...Some contents...Some contents...Some contents...Some contents...");
        }
    }
}