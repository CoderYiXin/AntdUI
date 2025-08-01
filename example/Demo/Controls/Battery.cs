﻿// COPYRIGHT (C) Tom. ALL RIGHTS RESERVED.
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
    public partial class Battery : UserControl
    {
        Form form;
        public Battery(Form _form)
        {
            form = _form;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) => SetBattery(5, Controls);

        private void button2_Click(object sender, EventArgs e) => SetBattery(-5, Controls);

        void SetBattery(int value, ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is AntdUI.Battery battery) battery.Value += value;
                if (control.Controls.Count > 0) SetBattery(value, control.Controls);
            }
        }
    }
}