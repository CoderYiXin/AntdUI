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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AntdUI.Chat
{
    /// <summary>
    /// ChatList 气泡聊天列表
    /// </summary>
    /// <remarks>气泡聊天列表。</remarks>
    [Description("ChatList 气泡聊天列表")]
    [ToolboxItem(true)]
    public class ChatList : IControl
    {
        #region 属性

        ChatItemCollection? items;
        /// <summary>
        /// 数据集合
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Description("数据集合"), Category("数据")]
        public ChatItemCollection Items
        {
            get
            {
                items ??= new ChatItemCollection(this);
                return items;
            }
            set => items = value.BindData(this);
        }

        [Description("Emoji字体"), Category("外观"), DefaultValue("Segoe UI Emoji")]
        public string EmojiFont { get; set; } = "Segoe UI Emoji";

        /// <summary>
        /// 滚动条
        /// </summary>
        [Browsable(false)]
        public ScrollBar ScrollBar;

        #endregion

        #region 方法

        public bool AddToBottom(IChatItem it, bool force = false)
        {
            if (force)
            {
                Items.Add(it);
                ToBottom();
                return true;
            }
            else
            {
                bool isbutt = IsBottom;
                Items.Add(it);
                if (isbutt) ToBottom();
                return isbutt;
            }
        }

        public bool IsBottom
        {
            get
            {
                if (ScrollBar.Show) return ScrollBar.Value == ScrollBar.VrValueI;
                else return true;
            }
        }

        public void ToBottom()
        {
            ScrollBar.Value = ScrollBar.VrValueI;
        }

        #endregion

        #region 渲染

        protected override void OnDraw(DrawEventArgs e)
        {
            if (items == null || items.Count == 0)
            {
                base.OnDraw(e);
                return;
            }
            var g = e.Canvas;
            float sy = ScrollBar.Value, radius = Config.Dpi * 8F;
            g.TranslateTransform(0, -sy);
            foreach (var it in items) PaintItem(g, it, e.Rect, sy, radius);
            g.ResetTransform();
            ScrollBar.Paint(g);
            base.OnDraw(e);
        }

        StringFormat SFL = Helper.SF(tb: StringAlignment.Near);

        void PaintItem(Canvas g, IChatItem it, Rectangle rect, float sy, float radius)
        {
            it.show = it.Show && it.rect.Y > sy - rect.Height - it.rect.Height && it.rect.Bottom < ScrollBar.Value + ScrollBar.ReadSize + it.rect.Height;
            if (it.show)
            {
                if (it is TextChatItem text)
                {
                    using (var path = text.rect_read.RoundPath(radius))
                    {
                        using (var brush = new SolidBrush(Colour.TextTertiary.Get("ChatList", ColorScheme)))
                        {
                            g.String(text.Name, Font, brush, text.rect_name, SFL);
                        }
                        if (text.Me)
                        {
                            g.Fill(Color.FromArgb(0, 153, 255), path);
                            if (text.selectionLength > 0) g.Fill(Color.FromArgb(0, 134, 224), path);
                            using (var brush = new SolidBrush(Color.White))
                            {
                                PaintItemText(g, text, brush);
                            }
                        }
                        else
                        {
                            g.Fill(Brushes.White, path);
                            if (text.selectionLength > 0) g.Fill(Colour.FillQuaternary.Get("ChatList", ColorScheme), path);
                            using (var brush = new SolidBrush(Color.Black))
                            {
                                PaintItemText(g, text, brush);
                            }
                        }
                    }
                    if (text.Icon != null) g.Image(text.rect_icon, text.Icon, TFit.Cover, 0, true);

                }
            }
        }

        void PaintItemText(Canvas g, TextChatItem text, SolidBrush fore)
        {
            if (text.selectionLength > 0)
            {
                int end = text.selectionStartTemp + text.selectionLength - 1;
                if (end > text.cache_font.Length - 1) end = text.cache_font.Length - 1;
                CacheFont first = text.cache_font[text.selectionStartTemp];
                using (var brush = new SolidBrush(Color.FromArgb(text.Me ? 255 : 60, 96, 165, 250)))
                {
                    for (int i = text.selectionStartTemp; i <= end; i++)
                    {
                        var last = text.cache_font[i];
                        if (i == end) g.Fill(brush, new Rectangle(first.rect.X, first.rect.Y, last.rect.Right - first.rect.X, first.rect.Height));
                        else if (first.rect.Y != last.rect.Y || last.retun)
                        {
                            last = text.cache_font[i - 1];
                            g.Fill(brush, new Rectangle(first.rect.X, first.rect.Y, last.rect.Right - first.rect.X, first.rect.Height));
                            first = text.cache_font[i];
                        }
                    }
                }
            }
            if (text.HasEmoji)
            {
                using (var font = new Font(EmojiFont, Font.Size))
                {
                    foreach (var it in text.cache_font)
                    {
                        switch (it.type)
                        {
                            case GraphemeSplitter.STRE_TYPE.STR:
                                if (it.emoji)
                                {
                                    if (SvgDb.Emoji.TryGetValue(it.text, out var svg)) SvgExtend.GetImgExtend(g, svg, it.rect, fore.Color);
                                    else g.String(it.text, font, fore, new Rectangle(it.rect.X - 20, it.rect.Y - 20, it.rect.Width + 40, it.rect.Height + 40));
                                }
                                else g.String(it.text, Font, fore, it.rect);
                                break;
                            case GraphemeSplitter.STRE_TYPE.SVG:
                                using (var bmp_svg = SvgExtend.SvgToBmp(it.text))
                                {
                                    if (bmp_svg != null) g.Image(it.rect, bmp_svg, TFit.Cover, 0, false);
                                }
                                break;
                            case GraphemeSplitter.STRE_TYPE.BASE64IMG:
                                using (var ms = new MemoryStream(Convert.FromBase64String(it.text.Substring(it.text.IndexOf(";base64,") + 8))))
                                using (var bmp_base64 = Image.FromStream(ms))
                                {
                                    g.Image(it.rect, bmp_base64, TFit.Contain, 0, false);
                                }
                                break;
                        }
                    }
                }
            }
            else
            {
                foreach (var it in text.cache_font)
                {
                    switch (it.type)
                    {
                        case GraphemeSplitter.STRE_TYPE.STR:
                            g.String(it.text, Font, fore, it.rect);
                            break;
                        case GraphemeSplitter.STRE_TYPE.SVG:
                            using (var bmp_svg = SvgExtend.SvgToBmp(it.text))
                            {
                                if (bmp_svg != null) g.Image(it.rect, bmp_svg, TFit.Cover, 0, false);
                            }
                            break;
                        case GraphemeSplitter.STRE_TYPE.BASE64IMG:
                            using (var ms = new MemoryStream(Convert.FromBase64String(it.text.Substring(it.text.IndexOf(";base64,") + 8))))
                            using (var bmp_base64 = Image.FromStream(ms))
                            {
                                g.Image(it.rect, bmp_base64, TFit.Contain, 0, false);
                            }
                            break;
                    }
                }
            }

            if (text.showlinedot)
            {
                int size = (int)(2 * Config.Dpi), w = size * 3;
                if (text.cache_font.Length > 0)
                {
                    var rect = text.cache_font[text.cache_font.Length - 1].rect;
                    using (var brush = new SolidBrush(Color.FromArgb(0, 153, 255)))
                    {
                        g.Fill(brush, new Rectangle(rect.Right - w / 2, rect.Bottom - size, w, size));
                    }
                }
                else
                {
                    using (var brush = new SolidBrush(Color.FromArgb(0, 153, 255)))
                    {
                        g.Fill(brush, new Rectangle(text.rect_read.X + (text.rect_read.Width - w) / 2, text.rect_read.Bottom - size, w, size));
                    }
                }
            }

        }

        public ChatList() { ScrollBar = new ScrollBar(this); }

        protected override void Dispose(bool disposing)
        {
            ScrollBar.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #region 鼠标

        TextChatItem? mouseDown;
        Point oldMouseDown;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (ScrollBar.MouseDown(e.X, e.Y))
            {
                if (items == null || items.Count == 0) return;
                Focus();
                int scrolly = ScrollBar.Value;
                foreach (IChatItem it in Items)
                {
                    if (it.show && it.Contains(e.Location, 0, scrolly))
                    {
                        if (it is TextChatItem text)
                        {
                            text.SelectionLength = 0;
                            if (e.Button == MouseButtons.Left && text.ContainsRead(e.X, e.Y, 0, scrolly))
                            {
                                oldMouseDown = e.Location;
                                text.SelectionStart = GetCaretPostion(text, e.X, e.Y + scrolly);

                                mouseDown = text;
                            }
                        }
                        ItemClick?.Invoke(this, new ChatItemEventArgs(it, e));
                    }
                    else if (it is TextChatItem text) text.SelectionLength = 0;
                }
            }
        }

        bool mouseDownMove = false;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int scrolly = ScrollBar.Value;
            if (mouseDown != null)
            {
                mouseDownMove = true;
                SetCursor(CursorType.IBeam);
                var index = GetCaretPostion(mouseDown, oldMouseDown.X + (e.X - oldMouseDown.X), oldMouseDown.Y + scrolly + (e.Y - oldMouseDown.Y));
                mouseDown.SelectionLength = Math.Abs(index - mouseDown.selectionStart);
                if (index > mouseDown.selectionStart) mouseDown.selectionStartTemp = mouseDown.selectionStart;
                else mouseDown.selectionStartTemp = index;
                Invalidate();
            }
            else if (ScrollBar.MouseMove(e.X, e.Y))
            {
                if (items == null || items.Count == 0) return;
                int count = 0, hand = 0, ibeam = 0;
                foreach (IChatItem it in Items)
                {
                    if (it.show && it.Contains(e.Location, 0, scrolly))
                    {
                        if (it is TextChatItem text)
                        {
                            if (text.ContainsRead(e.X, e.Y, 0, scrolly)) ibeam++;
                        }
                        //if (it.Contains(e.Location, 0, (int)scrollY.Value, out var change))
                        //{
                        //    hand++;
                        //}
                        //if (change) count++;
                    }
                }
                if (ibeam > 0) SetCursor(CursorType.IBeam);
                else SetCursor(hand > 0);
                if (count > 0) Invalidate();
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (mouseDown != null && mouseDownMove)
            {
                int scrolly = ScrollBar.Value;
                var index = GetCaretPostion(mouseDown, e.X, e.Y + scrolly);
                if (mouseDown.selectionStart == index) mouseDown.SelectionLength = 0;
                else if (index > mouseDown.selectionStart)
                {
                    mouseDown.SelectionLength = Math.Abs(index - mouseDown.selectionStart);
                    mouseDown.SelectionStart = mouseDown.selectionStart;
                }
                else
                {
                    mouseDown.SelectionLength = Math.Abs(index - mouseDown.selectionStart);
                    mouseDown.SelectionStart = index;
                }
                Invalidate();
            }
            mouseDown = null;
            ScrollBar.MouseUp();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            ScrollBar.Leave();
            SetCursor(false);
            base.OnMouseLeave(e);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            ILeave();
            base.OnLostFocus(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            ILeave();
            base.OnLeave(e);
        }

        void ILeave()
        {
            ScrollBar.Leave();
            SetCursor(false);
            if (items == null || items.Count == 0) return;
            int count = 0;
            foreach (IChatItem it in Items)
            {
                if (it is TextChatItem text) text.SelectionLength = 0;
            }
            if (count > 0) Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            ScrollBar.MouseWheel(e);
            base.OnMouseWheel(e);
        }

        #endregion

        #region 事件

        /// <summary>
        /// 单击时发生
        /// </summary>
        [Description("单击时发生"), Category("行为")]
        public event ClickEventHandler? ItemClick;

        #endregion

        #region 键盘

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.A:
                    SelectAll();
                    break;
                case Keys.Control | Keys.C:
                    Copy();
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        void SelectAll()
        {
            foreach (IChatItem it in Items)
            {
                if (it is TextChatItem text && text.SelectionLength > 0)
                {
                    text.SelectionStart = 0;
                    text.selectionLength = text.cache_font.Length;
                    return;
                }
            }
        }

        void Copy()
        {
            foreach (IChatItem it in Items)
            {
                if (it is TextChatItem text && text.SelectionLength > 0)
                {
                    var _text = GetSelectionText(text);
                    if (_text == null) return;
                    this.ClipboardSetText(_text);
                    return;
                }
            }
        }
        string? GetSelectionText(TextChatItem text)
        {
            if (text.cache_font == null) return null;
            else
            {
                if (text.selectionLength > 0)
                {
                    int start = text.selectionStart, end = text.selectionLength;
                    int end_temp = start + end;
                    var texts = new List<string>(end);
                    foreach (var it in text.cache_font)
                    {
                        if (it.i >= start && end_temp > it.i) texts.Add(it.text);
                    }
                    return string.Join("", texts);
                }
                return null;
            }
        }

        #endregion

        #region 文本

        /// <summary>
        /// 通过坐标系查找光标位置
        /// </summary>
        int GetCaretPostion(TextChatItem item, int x, int y)
        {
            foreach (var it in item.cache_font)
            {
                if (it.rect.X <= x && it.rect.Right >= x && it.rect.Y <= y && it.rect.Bottom >= y)
                {
                    if (x > it.rect.X + it.rect.Width / 2) return it.i + 1;
                    else return it.i;
                }
            }
            var nearest = FindNearestFont(x, y, item.cache_font);
            if (nearest == null)
            {
                if (x > item.cache_font[item.cache_font.Length - 1].rect.Right) return item.cache_font.Length;
                else return 0;
            }
            else
            {
                if (x > nearest.rect.X + nearest.rect.Width / 2) return nearest.i + 1;
                else return nearest.i;
            }
        }

        /// <summary>
        /// 寻找最近的矩形和距离的辅助方法
        /// </summary>
        CacheFont? FindNearestFont(int x, int y, CacheFont[] cache_font)
        {
            CacheFont first = cache_font[0], last = cache_font[cache_font.Length - 1];
            if (x < first.rect.X && y < first.rect.Y) return first;
            else if (x > last.rect.X && y > last.rect.Y) return last;
            double minDistance = int.MaxValue;
            CacheFont? result = null;
            for (int i = 0; i < cache_font.Length; i++)
            {
                var it = cache_font[i];
                // 计算点到矩形四个边的最近距离，取最小值作为当前矩形的最近距离
                int distanceToLeft = Math.Abs(x - (it.rect.Left + it.rect.Width / 2)),
                    distanceToTop = Math.Abs(y - (it.rect.Top + it.rect.Height / 2));
                double currentMinDistance = new int[] { distanceToLeft, distanceToTop }.Average();

                // 如果当前矩形的最近距离比之前找到的最近距离小，更新最近距离和最近矩形信息
                if (currentMinDistance < minDistance)
                {
                    minDistance = currentMinDistance;
                    result = it;
                }
            }
            return result;
        }

        #endregion

        #region 布局

        protected override void OnFontChanged(EventArgs e)
        {
            ChangeList();
            base.OnFontChanged(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            ChangeList();
            base.OnSizeChanged(e);
        }

        internal void ChangeList()
        {
            var rect = ClientRectangle;
            if (items == null || items.Count == 0 || (rect.Width == 0 || rect.Height == 0)) return;
            int y = 0;
            Helper.GDI(g =>
            {
                var size = g.MeasureString(Config.NullText, Font).Height;
                int item_height = (int)Math.Ceiling(size * 1.714),
                    gap = (int)Math.Round(item_height * .75),
                    spilt = item_height - gap, spilt2 = spilt * 2, max_width = (int)(rect.Width * .8F) - item_height;
                y = spilt;
                foreach (var it in items)
                {
                    it.PARENT = this;
                    if (it is TextChatItem text) y += text.SetRect(rect, y, g, Font, FixFontWidth(g, Font, text, max_width, spilt2), size, spilt, spilt2, item_height) + gap;
                }
            });
            ScrollBar.SetVrSize(y);
            ScrollBar.SizeChange(rect);
            return;
        }

        #region 字体

        internal Size FixFontWidth(Canvas g, Font Font, TextChatItem item, int max_width, int spilt)
        {
            item.HasEmoji = false;
            int font_height = 0;
            var font_widths = new List<CacheFont>(item.Text.Length);
            GraphemeSplitter.EachT(item.Text, 0, (str, type, nStart, nLen, nType) =>
            {
                string it = str.Substring(nStart, nLen);
                switch (type)
                {
                    case GraphemeSplitter.STRE_TYPE.BASE64IMG:
                        using (var ms = new MemoryStream(Convert.FromBase64String(it.Substring(it.IndexOf(";base64,") + 8))))
                        using (var image = Image.FromStream(ms))
                        {
                            int imgWidth = image.Width;
                            int imgHeight = image.Height;
                            if (imgWidth > max_width)
                            {
                                float scaleRatio = (float)max_width / imgWidth;
                                imgWidth = max_width;
                                imgHeight = (int)(imgHeight * scaleRatio);
                            }
                            font_widths.Add(new CacheFont(it, false, imgWidth, type)
                            {
                                imageHeight = imgHeight
                            });
                        }
                        break;
                    case GraphemeSplitter.STRE_TYPE.SVG:
                        using (var svgImage = SvgExtend.SvgToBmp(it, font_height, font_height, null))
                        {
                            if (svgImage != null)
                            {
                                int svgWidth = svgImage.Width;
                                int svgHeight = svgImage.Height;
                                font_widths.Add(new CacheFont(it, false, svgWidth, type)
                                {
                                    imageHeight = svgHeight
                                });
                            }
                        }
                        break;
                    default:
                        if (nType == 18)
                        {
                            item.HasEmoji = true;
                            font_widths.Add(new CacheFont(it, true, 0, type));
                        }
                        else
                        {
                            if (it == "\t" || it == "\n" || it == "\r\n")
                            {
                                var sizefont = g.MeasureString(" ", Font);
                                if (font_height < sizefont.Height) font_height = sizefont.Height;
                                font_widths.Add(new CacheFont(it, false, (int)Math.Ceiling(sizefont.Width * 8F), type));
                            }
                            else
                            {
                                var sizefont = g.MeasureString(it, Font);
                                if (font_height < sizefont.Height) font_height = sizefont.Height;
                                font_widths.Add(new CacheFont(it, false, sizefont.Width, type));
                            }
                        }
                        break;
                }
                return true;
            });

            if (item.HasEmoji)
            {
                using (var font = new Font(EmojiFont, Font.Size))
                {
                    foreach (var it in font_widths)
                    {
                        if (it.emoji)
                        {
                            var sizefont = g.MeasureString(it.text, font);
                            if (font_height < sizefont.Height) font_height = sizefont.Height;
                            it.width = sizefont.Width;
                        }
                    }
                }
            }

            for (int j = 0; j < font_widths.Count; j++) { font_widths[j].i = j; }
            item.cache_font = font_widths.ToArray();

            int usex = 0, usey = 0, maxx = 0, maxy = 0;
            foreach (var it in item.cache_font)
            {
                if (it.text == "\r")
                {
                    it.retun = true;
                    continue;
                }
                if (it.text == "\n" || it.text == "\r\n")
                {
                    it.retun = true;
                    usey += font_height;
                    usex = 0;
                    continue;
                }
                else if (usex + it.width > max_width)
                {
                    usey += font_height;
                    usex = 0;
                }
                if (it.imageHeight.HasValue) it.rect = new Rectangle(usex, usey, it.width, it.imageHeight.Value);
                else it.rect = new Rectangle(usex, usey, it.width, font_height);
                if (maxx < it.rect.Right) maxx = it.rect.Right;
                if (maxy < it.rect.Bottom) maxy = it.rect.Bottom;
                usex += it.width;
            }

            return new Size(maxx + spilt, maxy + spilt);
        }

        #endregion

        #endregion
    }

    public class ChatItemCollection : iCollection<IChatItem>
    {
        public ChatItemCollection(ChatList it)
        {
            BindData(it);
        }

        internal ChatItemCollection BindData(ChatList it)
        {
            action = render =>
            {
                if (render) it.ChangeList();
                it.Invalidate();
            };
            return this;
        }
    }

    public class TextChatItem : IChatItem
    {
        public TextChatItem(string text)
        {
            _text = text;
        }
        public TextChatItem(string text, Image? icon)
        {
            _text = text;
            _icon = icon;
        }
        public TextChatItem(string text, Image? icon, string name)
        {
            _text = text;
            _name = name;
            _icon = icon;
        }

        /// <summary>
        /// ID
        /// </summary>
        [Description("ID"), Category("数据"), DefaultValue(null)]
        public string? ID { get; set; }

        /// <summary>
        /// 本人
        /// </summary>
        [Description("本人"), Category("行为"), DefaultValue(false)]
        public bool Me { get; set; }

        Image? _icon;
        /// <summary>
        /// 图标
        /// </summary>
        [Description("图标"), Category("外观"), DefaultValue(null)]
        public Image? Icon
        {
            get => _icon;
            set
            {
                if (_icon == value) return;
                _icon = value;
                Invalidates();
            }
        }

        string? _name;
        /// <summary>
        /// 名称
        /// </summary>
        [Description("名称"), Category("外观"), DefaultValue(null)]
        public string? Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                Invalidate();
            }
        }

        string _text;
        /// <summary>
        /// 文本
        /// </summary>
        [Description("文本"), Category("外观")]
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                _text = value;
                Invalidates();
            }
        }

        ITask? task;
        internal bool showlinedot = false;
        bool loading;
        public bool Loading
        {
            get => loading;
            set
            {
                if (loading == value) return;
                loading = value;
                task?.Dispose();
                task = null;
                showlinedot = false;
                if (value && PARENT != null)
                {
                    task = new ITask(PARENT, () =>
                    {
                        showlinedot = !showlinedot;
                        Invalidate();
                        return loading;
                    }, 200);
                }
                else Invalidate();
            }
        }

        #region 布局

        internal int SetRect(Rectangle _rect, int y, Canvas g, Font font, Size msglen, int gap, int spilt, int spilt2, int image_size)
        {
            if (string.IsNullOrEmpty(_name))
            {
                rect = new Rectangle(_rect.X, _rect.Y + y, _rect.Width, msglen.Height);
                if (Me)
                {
                    rect_icon = new Rectangle(rect.Right - gap - image_size, rect.Y, image_size, image_size);
                    rect_read = new Rectangle(rect_icon.X - spilt - msglen.Width, rect_icon.Y, msglen.Width, msglen.Height);
                }
                else
                {
                    rect_icon = new Rectangle(rect.X + gap, rect.Y, image_size, image_size);
                    rect_read = new Rectangle(rect_icon.Right + spilt, rect_icon.Y, msglen.Width, msglen.Height);
                }
            }
            else
            {
                rect = new Rectangle(_rect.X, _rect.Y + y, _rect.Width, msglen.Height + gap);
                var size_name = g.MeasureString(_name, font).Width;
                if (Me)
                {
                    rect_icon = new Rectangle(rect.Right - gap - image_size, rect.Y, image_size, image_size);
                    rect_name = new Rectangle(rect_icon.X - spilt - msglen.Width + msglen.Width - size_name, rect_icon.Y, size_name, gap);
                    rect_read = new Rectangle(rect_icon.X - spilt - msglen.Width, rect_name.Bottom, msglen.Width, msglen.Height);
                }
                else
                {
                    rect_icon = new Rectangle(rect.X + gap, rect.Y, image_size, image_size);
                    rect_name = new Rectangle(rect_icon.Right + spilt, rect_icon.Y, size_name, gap);
                    rect_read = new Rectangle(rect_name.X, rect_name.Bottom, msglen.Width, msglen.Height);
                }
            }
            rect_text = new Rectangle(rect_read.X + spilt, rect_read.Y + spilt, msglen.Width - spilt2, msglen.Height - spilt2);

            foreach (var it in cache_font)
            {
                it.SetOffset(rect_text.Location);
            }

            Show = true;
            return rect.Height;
        }

        internal Rectangle rect_read { get; set; }
        internal Rectangle rect_name { get; set; }
        internal Rectangle rect_text { get; set; }
        internal Rectangle rect_icon { get; set; }

        internal bool ContainsRead(int x, int y, int sx, int sy) => rect_text.Contains(new Point(x + sx, y + sy));

        #endregion

        #region 字体相关

        internal bool HasEmoji = false;
        internal CacheFont[] cache_font = new CacheFont[0];

        internal int selectionStart = 0, selectionStartTemp = 0, selectionLength = 0;
        /// <summary>
        /// 所选文本的起点
        /// </summary>
        [Browsable(false), DefaultValue(0)]
        public int SelectionStart
        {
            get => selectionStart;
            set
            {
                if (value < 0) value = 0;
                else if (value > 0)
                {
                    if (cache_font == null) value = 0;
                    else if (value > cache_font.Length) value = cache_font.Length;
                }
                if (selectionStart == value) return;
                selectionStart = selectionStartTemp = value;
                Invalidate();
            }
        }

        /// <summary>
        /// 所选文本的长度
        /// </summary>
        [Browsable(false), DefaultValue(0)]
        public int SelectionLength
        {
            get => selectionLength;
            set
            {
                if (selectionLength == value) return;
                selectionLength = value;
                Invalidate();
            }
        }

        #endregion

        #region 设置

        public TextChatItem SetID(string? value)
        {
            ID = value;
            return this;
        }

        public TextChatItem SetName(string? value)
        {
            _name = value;
            return this;
        }

        public TextChatItem SetIcon(Image? img)
        {
            _icon = img;
            return this;
        }

        public TextChatItem SetLoading(bool value = true)
        {
            loading = value;
            return this;
        }

        public TextChatItem SetMe(bool value = true)
        {
            Me = value;
            return this;
        }

        public TextChatItem SetTag(object? value)
        {
            Tag = value;
            return this;
        }

        #endregion
    }
    public class IChatItem
    {
        public IChatItem() { }

        /// <summary>
        /// 用户定义数据
        /// </summary>
        [Description("用户定义数据"), Category("数据"), DefaultValue(null)]
        public object? Tag { get; set; }

        internal void Invalidate() => PARENT?.Invalidate();
        internal void Invalidates()
        {
            if (PARENT == null) return;
            PARENT.ChangeList();
            PARENT.Invalidate();
        }

        internal bool show { get; set; }
        internal bool Show { get; set; }

        internal ChatList? PARENT { get; set; }

        internal Rectangle rect { get; set; }

        internal bool Contains(Point point, int x, int y) => rect.Contains(new Point(point.X + x, point.Y + y));
    }

    internal class CacheFont
    {
        public CacheFont(string _text, bool _emoji, int _width, GraphemeSplitter.STRE_TYPE Type)
        {
            text = _text;
            emoji = _emoji;
            width = _width;
            type = Type;
        }
        public int i { get; set; }
        public string text { get; set; }
        Rectangle _rect;
        public Rectangle rect
        {
            get => _rect;
            set { _rect = value; }
        }
        internal void SetOffset(Point point)
        {
            _rect.Offset(point);
        }
        public bool emoji { get; set; }
        public bool retun { get; set; }
        public int width { get; set; }
        public GraphemeSplitter.STRE_TYPE type { get; set; }
        public int? imageHeight { get; set; }
    }
}