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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace AntdUI
{
    public abstract class ILayeredFormAnimate : ILayeredForm
    {
        internal static Dictionary<string, List<ILayeredFormAnimate>> list = new Dictionary<string, List<ILayeredFormAnimate>>();

        internal string key = "";
        public abstract string name { get; }
        internal virtual TAlignFrom Align => TAlignFrom.TR;
        internal virtual bool ActiveAnimation => true;

        int start_X = 0, end_X = 0, start_Y = 0, end_Y = 0;

        #region 位置

        public int ReadY => end_Y;
        public int ReadB => end_Y + TargetRect.Height;

        internal bool SetPosition(Form form, bool InWindow)
        {
            Rectangle workingArea;
            if (InWindow || Config.ShowInWindow) workingArea = new Rectangle(form.Location, form.Size);
            else workingArea = Screen.FromControl(form).WorkingArea;
            key = Align.ToString() + "|" + workingArea.X + "|" + workingArea.Y + "|" + workingArea.Right + "|" + workingArea.Bottom;
            int width = TargetRect.Width, height = TargetRect.Height;
            switch (Align)
            {
                case TAlignFrom.Top:
                    if (TopY(workingArea, out var resultTop)) return true;
                    int xt = start_X = end_X = workingArea.X + (workingArea.Width - width) / 2;
                    end_Y = resultTop;
                    SetLocation(xt, start_Y = end_Y - height / 2);
                    break;
                case TAlignFrom.Bottom:
                    if (BottomY(workingArea, out var resultBottom)) return true;
                    int xb = start_X = end_X = workingArea.X + (workingArea.Width - width) / 2;
                    end_Y = resultBottom;
                    SetLocation(xb, start_Y = end_Y + height / 2);
                    break;
                case TAlignFrom.TL:
                    if (TopY(workingArea, out var resultTL)) return true;
                    end_X = workingArea.X;
                    SetLocation(start_X = end_X - width / 3, start_Y = end_Y = resultTL);
                    break;
                case TAlignFrom.TR:
                    if (TopY(workingArea, out var resultTR)) return true;
                    end_X = workingArea.X + workingArea.Width - width;
                    SetLocation(start_X = end_X + width / 3, start_Y = end_Y = resultTR);
                    break;
                case TAlignFrom.BL:
                    if (BottomY(workingArea, out var resultBL)) return true;
                    end_X = workingArea.X;
                    SetLocation(start_X = end_X - width / 3, start_Y = end_Y = resultBL);
                    break;
                case TAlignFrom.BR:
                    if (BottomY(workingArea, out var resultBR)) return true;
                    end_X = workingArea.X + workingArea.Width - width;
                    SetLocation(start_X = end_X + width / 3, start_Y = end_Y = resultBR);
                    break;
            }
            Add();
            return false;
        }
        void Add()
        {
            if (list.TryGetValue(key, out var its)) its.Add(this);
            else list.Add(key, new List<ILayeredFormAnimate> { this });
        }

        #region 核心

        bool TopY(Rectangle workingArea, out int result)
        {
            switch (name)
            {
                case nameof(Notification):
                    if (Notification.MaxCount.HasValue)
                    {
                        if (list.TryGetValue(key, out var count) && count.Count + 1 > Notification.MaxCount.Value)
                        {
                            if (Config.NoticeOverflowClose) count[0].CloseMe();
                            result = 0;
                            return true;
                        }
                    }
                    break;
                case nameof(Message):
                    if (Message.MaxCount.HasValue)
                    {
                        if (list.TryGetValue(key, out var count) && count.Count + 1 > Message.MaxCount.Value)
                        {
                            if (Config.NoticeOverflowClose) count[0].CloseMe();
                            result = 0;
                            return true;
                        }
                    }
                    break;
            }
            int offset = (int)(Config.NoticeWindowOffsetXY * Config.Dpi);
            var y = TopYCore(workingArea, offset);
            if (y < workingArea.Bottom - TargetRect.Height)
            {
                result = y;
                return false;
            }
            else
            {
                if (Config.NoticeOverflowClose && list.TryGetValue(key, out var count) && count.Count > 0) count[0].CloseMe();
                result = 0;
                return true;
            }
        }
        int TopYCore(Rectangle workingArea, int offset)
        {
            if (list.TryGetValue(key, out var its) && its.Count > 0)
            {
                int tmp = its[its.Count - 1].ReadB;
                for (int i = 0; i < its.Count - 1; i++)
                {
                    var it = its[i];
                    if (it.ReadB > tmp) tmp = it.ReadB;
                }
                return tmp;
            }
            else return workingArea.Y + offset;
        }
        bool BottomY(Rectangle workingArea, out int result)
        {
            switch (name)
            {
                case nameof(Notification):
                    if (Notification.MaxCount.HasValue)
                    {
                        if (list.TryGetValue(key, out var count) && count.Count + 1 > Notification.MaxCount.Value)
                        {
                            if (Config.NoticeOverflowClose) count[0].CloseMe();
                            result = 0;
                            return true;
                        }
                    }
                    break;
                case nameof(Message):
                    if (Message.MaxCount.HasValue)
                    {
                        if (list.TryGetValue(key, out var count) && count.Count + 1 > Message.MaxCount.Value)
                        {
                            if (Config.NoticeOverflowClose) count[0].CloseMe();
                            result = 0;
                            return true;
                        }
                    }
                    break;
            }
            int offset = (int)(Config.NoticeWindowOffsetXY * Config.Dpi);
            var y = BottomYCore(workingArea, offset) - TargetRect.Height;
            if (y >= 0)
            {
                result = y;
                return false;
            }
            else
            {
                if (Config.NoticeOverflowClose && list.TryGetValue(key, out var count) && count.Count > 0) count[0].CloseMe();
                result = 0;
                return true;
            }
        }
        int BottomYCore(Rectangle workingArea, int offset)
        {
            if (list.TryGetValue(key, out var its) && its.Count > 0)
            {
                int tmp = its[its.Count - 1].ReadY;
                for (int i = 0; i < its.Count - 1; i++)
                {
                    var it = its[i];
                    if (it.ReadY < tmp) tmp = it.ReadY;
                }
                return tmp;
            }
            else return workingArea.Bottom - offset;
        }

        #endregion

        internal void SetPositionCenter(int w)
        {
            if (Align == TAlignFrom.Top || Align == TAlignFrom.Bottom)
            {
                int x = TargetRect.X + (w - TargetRect.Width) / 2;
                SetLocationX(x);
                start_X = end_X = x;
            }
            else if (Align == TAlignFrom.TR || Align == TAlignFrom.BR)
            {
                int x = TargetRect.X - (TargetRect.Width - w);
                SetLocationX(x);
                start_X = end_X = x;
            }
            Print(true);
        }
        internal void SetPositionY(int y)
        {
            SetLocationY(y);
            end_Y = y;
            start_Y = y - TargetRect.Height / 2;
        }
        internal void SetPositionYB(int y)
        {
            SetLocationY(y);
            end_Y = y;
            start_Y = y + TargetRect.Height / 2;
        }

        #endregion

        ITask? task_start;
        protected override void OnLoad(EventArgs e)
        {
            if (ActiveAnimation) PlayAnimation();
            base.OnLoad(e);
        }

        #region 动画

        #region 开始动画

        Bitmap? bmp_tmp;
        public void PlayAnimation()
        {
            if (Config.HasAnimation(name))
            {
                var t = Animation.TotalFrames(10, 200);
                task_start = new ITask(start_X == end_X ? i =>
                {
                    var val = Animation.Animate(i, t, 1F, AnimationType.Ball);
                    SetAnimateValueY(start_Y + (int)((end_Y - start_Y) * val), (byte)(240 * val));
                    return true;
                }
                : i =>
                {
                    var val = Animation.Animate(i, t, 1F, AnimationType.Ball);
                    SetAnimateValueX(start_X + (int)((end_X - start_X) * val), (byte)(240 * val));
                    return true;
                }, 10, t, () =>
                {
                    DisposeAnimation();
                    SetAnimateValue(end_X, end_Y, 240);
                    Print(true);
                });
            }
            else
            {
                SetAnimateValue(end_X, end_Y, 240);
                Print(true);
            }
        }

        #endregion

        #region 关闭动画

        internal ITask StopAnimation()
        {
            task_start?.Dispose();
            var t = Animation.TotalFrames(10, 200);
            return new ITask(start_X == end_X ? (i) =>
            {
                var val = Animation.Animate(i, t, 1F, AnimationType.Ball);
                SetAnimateValueY(end_Y - (int)((end_Y - start_Y) * val), (byte)(240 * (1F - val)));
                return true;
            }
            : (i) =>
            {
                var val = Animation.Animate(i, t, 1F, AnimationType.Ball);
                SetAnimateValueX(end_X - (int)((end_X - start_X) * val), (byte)(240 * (1F - val)));
                return true;
            }, 10, t, DisposeAnimation);
        }
        public void DisposeAnimation()
        {
            bmp_tmp?.Dispose();
            bmp_tmp = null;
        }

        #endregion

        #region 设置动画参数

        void SetAnimateValueX(int x, byte _alpha)
        {
            if (TargetRect.X != x || alpha != _alpha)
            {
                SetLocationX(x);
                alpha = _alpha;
                bmp_tmp ??= PrintBit();
                if (bmp_tmp == null) return;
                if (Print(bmp_tmp) == RenderResult.Invalid) bmp_tmp = null;
            }
        }
        void SetAnimateValueY(int y, byte _alpha)
        {
            if (TargetRect.Y != y || alpha != _alpha)
            {
                SetLocationY(y);
                alpha = _alpha;
                bmp_tmp ??= PrintBit();
                if (bmp_tmp == null) return;
                if (Print(bmp_tmp) == RenderResult.Invalid) bmp_tmp = null;
            }
        }
        internal void SetAnimateValueY(int y)
        {
            if (TargetRect.Y != y)
            {
                SetLocationY(y);
                bmp_tmp ??= PrintBit();
                if (bmp_tmp == null) return;
                if (Print(bmp_tmp) == RenderResult.Invalid) bmp_tmp = null;
            }
        }
        void SetAnimateValue(int x, int y, byte _alpha)
        {
            if (TargetRect.X != x || TargetRect.Y != y || alpha != _alpha)
            {
                SetLocation(x, y);
                alpha = _alpha;
                bmp_tmp ??= PrintBit();
                if (bmp_tmp == null) return;
                if (Print(bmp_tmp) == RenderResult.Invalid) bmp_tmp = null;
            }
        }

        #endregion

        #endregion

        #region 关闭

        public void Sleep(int second)
        {
            Thread.Sleep(second * 1000);
            if (Tag is string id)
            {
                while (MsgQueue.expands.TryGetValue(id, out var d))
                {
                    var now = DateTime.Now;
                    MsgQueue.expands.Remove(id);
                    int t = (int)(d.AddSeconds(second) - now).TotalMilliseconds;
                    Thread.Sleep(t);
                }
            }
        }

        DateTime closetime;
        bool handclose = false;
        public void CloseMe()
        {
            var now = DateTime.Now;
            if (handclose && (now - closetime).TotalSeconds < 2) return;
            closetime = now;
            handclose = true;
            task_start?.Dispose();
            MsgQueue.Add(this);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            task_start?.Dispose();
            if (list.TryGetValue(key, out var layeredFormAnimateList) && layeredFormAnimateList.Contains(this))
            {
                if (layeredFormAnimateList.Count == 1) list.Remove(key);
                else layeredFormAnimateList.Remove(this);
            }
            base.Dispose(disposing);
        }
    }

    public static class MsgQueue
    {
        static ManualResetEvent _event = new ManualResetEvent(false);
        internal static ConcurrentQueue<object> queue = new ConcurrentQueue<object>(), queue_cache = new ConcurrentQueue<object>();
        internal static List<string> volley = new List<string>();
        internal static List<string> ids = new List<string>();
        internal static Dictionary<string, DateTime> expands = new Dictionary<string, DateTime>();

        #region 添加队列

        public static void Add(Notification.Config config)
        {
            if (config.ID != null)
            {
                string key = "N" + config.ID;
                ids.Add(key);
            }
            queue.Enqueue(config);
            _event.SetWait();
        }
        public static void Add(Message.Config config)
        {
            if (config.ID != null)
            {
                string key = "M" + config.ID;
                ids.Add(key);
            }
            queue.Enqueue(config);
            _event.SetWait();
        }

        internal static void Add(ILayeredFormAnimate command)
        {
            queue.Enqueue(command);
            _event.SetWait();
        }

        #endregion

        /// <summary>
        /// 判断ID是否存在队列
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="time_expand">是否延长时间</param>
        public static bool contains(string id, bool time_expand = false)
        {
            bool result = ids.Contains(id);
            if (result)
            {
                lock (expands)
                {
                    if (expands.ContainsKey(id)) expands[id] = DateTime.Now;
                    else expands.Add(id, DateTime.Now);
                }
            }
            return result;
        }

        static MsgQueue()
        {
            new Thread(LongTask) { IsBackground = true }.Start();
        }

        static void LongTask()
        {
            while (true)
            {
                if (_event.Wait()) return;
                while (queue.TryDequeue(out var d)) Hand(d);
                volley.Clear();
                expands.Clear();
                _event.ResetWait();
            }
        }

        static void Hand(object d)
        {
            try
            {
                if (d is Notification.Config configNotification)
                {
                    if (Open(configNotification)) _event.ResetWait();
                }
                else if (d is Message.Config configMessage)
                {
                    if (Open(configMessage)) _event.ResetWait();
                }
                else if (d is ILayeredFormAnimate formAnimate)
                {
                    if (formAnimate.Tag is string ID) ids.Remove(ID);
                    if (Config.HasAnimation(formAnimate.name)) formAnimate.StopAnimation().Wait();
                    formAnimate.IClose(true);
                    Close(formAnimate.Align, formAnimate.key, formAnimate.name);
                    if (queue_cache.TryDequeue(out var d_cache)) Hand(d_cache);
                }
            }
            catch { }
        }

        static bool Open(Notification.Config config)
        {
            if (config.Form.IsHandleCreated)
            {
                string? key = null;
                if (config.ID != null)
                {
                    key = "N" + config.ID;
                    if (volley.Contains(key))
                    {
                        ids.Remove(key);
                        volley.Remove(key);
                        return false;
                    }
                }
                bool ishand = false;
                config.Form.Invoke(new Action(() =>
                {
                    var from = new NotificationFrm(config, key);
                    if (from.IInit())
                    {
                        from.Dispose();
                        ishand = true;
                    }
                    else
                    {
                        if (config.TopMost) from.Show();
                        else from.Show(config.Form);
                    }
                }));
                if (ishand)
                {
                    queue_cache.Enqueue(config);
                    return true;
                }
            }
            return false;
        }

        static bool Open(Message.Config config)
        {
            if (config.Form.IsHandleCreated)
            {
                string? key = null;
                if (config.ID != null)
                {
                    key = "M" + config.ID;
                    if (volley.Contains(key))
                    {
                        ids.Remove(key);
                        volley.Remove(key);
                        return false;
                    }
                }
                bool ishand = false;
                config.Form.Invoke(new Action(() =>
                {
                    var from = new MessageFrm(config, key);
                    if (from.IInit())
                    {
                        from.Dispose();
                        ishand = true;
                    }
                    else
                    {
                        if (config.TopMost) from.Show();
                        else from.Show(config.Form);
                    }
                }));
                if (ishand)
                {
                    queue_cache.Enqueue(config);
                    return true;
                }
            }
            return false;
        }

        static void Close(TAlignFrom align, string key, string name)
        {
            try
            {
                if (ILayeredFormAnimate.list.TryGetValue(key, out var list))
                {
                    if (list.Count == 0) return;
                    switch (align)
                    {
                        case TAlignFrom.Bottom:
                        case TAlignFrom.BL:
                        case TAlignFrom.BR:
                            CloseB(key, name, list);
                            break;
                        case TAlignFrom.Top:
                        case TAlignFrom.TL:
                        case TAlignFrom.TR:
                        default:
                            CloseT(key, name, list);
                            break;
                    }
                }
            }
            catch { }
        }

        static void CloseT(string key, string name, List<ILayeredFormAnimate> list)
        {
            var arr = key.Split('|');
            int y = int.Parse(arr[2]);
            int offset = (int)(Config.NoticeWindowOffsetXY * Config.Dpi);
            int y_temp = y + offset;
            var dir = new Dictionary<ILayeredFormAnimate, int[]>(list.Count);
            foreach (var it in list)
            {
                int y2 = it.ReadY;
                if (y2 != y_temp) dir.Add(it, new int[] { y_temp, y2 - y_temp });
                y_temp += it.TargetRect.Height;
            }
            if (dir.Count > 0)
            {
                if (Config.HasAnimation(name))
                {
                    var t = Animation.TotalFrames(10, 200);
                    new ITask(i =>
                    {
                        var val = Animation.Animate(i, t, 1F, AnimationType.Ball);
                        foreach (var it in dir) it.Key.SetAnimateValueY(it.Value[0] + (it.Value[1] - (int)(it.Value[1] * val)));
                        return true;
                    }, 10, t, () =>
                    {
                        y_temp = y + offset;
                        for (int i = 0; i < list.Count; i++)
                        {
                            var it = list[i];
                            it.DisposeAnimation();
                            it.SetAnimateValueY(y_temp);
                            it.SetPositionY(y_temp);
                            y_temp += it.TargetRect.Height;
                        }
                    }).Wait();
                }
                else
                {
                    y_temp = y + offset;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var it = list[i];
                        it.DisposeAnimation();
                        it.SetAnimateValueY(y_temp);
                        it.SetPositionY(y_temp);
                        y_temp += it.TargetRect.Height;
                    }
                }
            }
        }
        static void CloseB(string key, string name, List<ILayeredFormAnimate> list)
        {
            var arr = key.Split('|');
            int b = int.Parse(arr[4]);
            int offset = (int)(Config.NoticeWindowOffsetXY * Config.Dpi);
            int y_temp = b - offset;
            var dir = new Dictionary<ILayeredFormAnimate, int[]>(list.Count);
            foreach (var it in list)
            {
                y_temp -= it.TargetRect.Height;
                int y2 = it.ReadY;
                if (y2 != y_temp) dir.Add(it, new int[] { y_temp, y2 - y_temp });
            }
            if (dir.Count > 0)
            {
                if (Config.HasAnimation(name))
                {
                    var t = Animation.TotalFrames(10, 200);
                    new ITask(i =>
                    {
                        var val = Animation.Animate(i, t, 1F, AnimationType.Ball);
                        foreach (var it in dir) it.Key.SetAnimateValueY(it.Value[0] + (it.Value[1] - (int)(it.Value[1] * val)));
                        return true;
                    }, 10, t, () =>
                    {
                        y_temp = b - offset;
                        for (int i = 0; i < list.Count; i++)
                        {
                            var it = list[i];
                            it.DisposeAnimation();
                            y_temp -= it.TargetRect.Height;
                            it.SetAnimateValueY(y_temp);
                            it.SetPositionY(y_temp);
                        }
                    }).Wait();
                }
                else
                {
                    y_temp = b - offset;
                    for (int i = 0; i < list.Count; i++)
                    {
                        var it = list[i];
                        it.DisposeAnimation();
                        y_temp -= it.TargetRect.Height;
                        it.SetAnimateValueY(y_temp);
                        it.SetPositionY(y_temp);
                    }
                }
            }
        }
    }
}