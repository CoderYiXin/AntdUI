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
using System.Drawing;

namespace AntdUI
{
    partial class CellButton
    {
        public override void Paint(Canvas g, Font font, bool enable, SolidBrush fore) => Table.PaintButton(g, font, PARENT.PARENT.Gaps, Rect, this, enable, PARENT.PARENT.ColorScheme);

        #region GetSize

        public override Size GetSize(Canvas g, Font font, TableGaps gap)
        {
            if (Gap.HasValue)
            {
                int sp = (int)(Gap.Value * Config.Dpi);
                return GetSizeCore(g, font, sp, sp * 2);
            }
            else if (PARENT.PARENT.GapCell.HasValue)
            {
                int sp = (int)(PARENT.PARENT.GapCell.Value * Config.Dpi);
                return GetSizeCore(g, font, sp, sp * 2);
            }
            else return GetSizeCore(g, font, gap.x, gap.x2);
        }

        Size GetSizeCore(Canvas g, Font font, int gap, int gap2)
        {
            if (string.IsNullOrEmpty(Text))
            {
                var size = g.MeasureString(Config.NullText, font);
                int sizei = size.Height + gap;
                return new Size(sizei, sizei);
            }
            else
            {
                var size = g.MeasureText(Text ?? Config.NullText, font);
                bool has_icon = HasIcon;
                if (has_icon || ShowArrow)
                {
                    if (has_icon && (IconPosition == TAlignMini.Top || IconPosition == TAlignMini.Bottom))
                    {
                        int size_read = (int)Math.Ceiling(size.Height * 1.2F);
                        return new Size(size.Width + gap2 * 2 + size_read, size.Height + gap + size_read);
                    }
                    int height = size.Height + gap;
                    if (has_icon && ShowArrow) return new Size(size.Width + gap2 + size.Height * 2, height);
                    else if (has_icon) return new Size(size.Width + gap2 + (int)Math.Ceiling(size.Height * 1.2F), height);
                    else return new Size(size.Width + gap2 + (int)Math.Ceiling(size.Height * .8F), height);
                }
                else return new Size(size.Width + gap2, size.Height + gap);
            }
        }

        #endregion

        #region 动画

        internal override bool ExtraMouseHover
        {
            get => _mouseHover;
            set
            {
                if (_mouseHover == value) return;
                _mouseHover = value;
                if (Enabled)
                {
                    Color _back_hover;
                    switch (Type)
                    {
                        case TTypeMini.Default:
                            if (BorderWidth > 0) _back_hover = Colour.PrimaryHover.Get("Button", PARENT.PARENT.ColorScheme);
                            else _back_hover = Colour.FillSecondary.Get("Button", PARENT.PARENT.ColorScheme);
                            break;
                        case TTypeMini.Success:
                            _back_hover = Colour.SuccessHover.Get("Button", PARENT.PARENT.ColorScheme);
                            break;
                        case TTypeMini.Error:
                            _back_hover = Colour.ErrorHover.Get("Button", PARENT.PARENT.ColorScheme);
                            break;
                        case TTypeMini.Info:
                            _back_hover = Colour.InfoHover.Get("Button", PARENT.PARENT.ColorScheme);
                            break;
                        case TTypeMini.Warn:
                            _back_hover = Colour.WarningHover.Get("Button", PARENT.PARENT.ColorScheme);
                            break;
                        case TTypeMini.Primary:
                        default:
                            _back_hover = Colour.PrimaryHover.Get("Button", PARENT.PARENT.ColorScheme);
                            break;
                    }

                    if (BackHover.HasValue) _back_hover = BackHover.Value;
                    if (Config.HasAnimation(nameof(Table)))
                    {
                        if (IconHoverAnimation > 0 && HasIcon && (IconHoverSvg != null || IconHover != null))
                        {
                            ThreadImageHover?.Dispose();
                            AnimationImageHover = true;
                            var t = Animation.TotalFrames(10, IconHoverAnimation);
                            if (value)
                            {
                                ThreadImageHover = new ITask((i) =>
                                {
                                    AnimationImageHoverValue = Animation.Animate(i, t, 1F, AnimationType.Ball);
                                    OnPropertyChanged();
                                    return true;
                                }, 10, t, () =>
                                {
                                    AnimationImageHoverValue = 1F;
                                    AnimationImageHover = false;
                                    OnPropertyChanged();
                                });
                            }
                            else
                            {
                                ThreadImageHover = new ITask((i) =>
                                {
                                    AnimationImageHoverValue = 1F - Animation.Animate(i, t, 1F, AnimationType.Ball);
                                    OnPropertyChanged();
                                    return true;
                                }, 10, t, () =>
                                {
                                    AnimationImageHoverValue = 0F;
                                    AnimationImageHover = false;
                                    OnPropertyChanged();
                                });
                            }
                        }
                        if (_back_hover.A > 0)
                        {
                            int addvalue = _back_hover.A / 12;
                            ThreadHover?.Dispose();
                            AnimationHover = true;
                            if (value)
                            {
                                ThreadHover = new ITask(PARENT.PARENT, () =>
                                {
                                    AnimationHoverValue += addvalue;
                                    if (AnimationHoverValue > _back_hover.A) { AnimationHoverValue = _back_hover.A; return false; }
                                    OnPropertyChanged();
                                    return true;
                                }, 10, () =>
                                {
                                    AnimationHover = false;
                                    OnPropertyChanged();
                                });
                            }
                            else
                            {
                                ThreadHover = new ITask(PARENT.PARENT, () =>
                                {
                                    AnimationHoverValue -= addvalue;
                                    if (AnimationHoverValue < 1) { AnimationHoverValue = 0; return false; }
                                    OnPropertyChanged();
                                    return true;
                                }, 10, () =>
                                {
                                    AnimationHover = false;
                                    OnPropertyChanged();
                                });
                            }
                        }
                        else
                        {
                            AnimationHoverValue = _back_hover.A;
                            OnPropertyChanged();
                        }
                    }
                    else AnimationHoverValue = _back_hover.A;
                    OnPropertyChanged();
                }
            }
        }

        internal override void Click()
        {
            if (Config.HasAnimation(nameof(Table)))
            {
                ThreadClick?.Dispose();
                AnimationClickValue = 0;
                AnimationClick = true;
                ThreadClick = new ITask(PARENT.PARENT, () =>
                {
                    if (AnimationClickValue > 0.6) AnimationClickValue = AnimationClickValue.Calculate(0.04F);
                    else AnimationClickValue += AnimationClickValue = AnimationClickValue.Calculate(0.1F);
                    if (AnimationClickValue > 1) { AnimationClickValue = 0F; return false; }
                    OnPropertyChanged();
                    return true;
                }, 50, () =>
                {
                    AnimationClick = false;
                    OnPropertyChanged();
                });
            }
        }

        #endregion
    }
}