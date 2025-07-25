﻿// THIS FILE IS PART OF SVG PROJECT
// THE SVG PROJECT IS AN OPENSOURCE LIBRARY LICENSED UNDER THE MS-PL License.
// COPYRIGHT (C) svg-net. ALL RIGHTS RESERVED.
// GITHUB: https://github.com/svg-net/SVG

using System;
using System.IO;
using System.Text;

namespace AntdUI.Svg.Text
{
    /// <summary>
    /// http://stackoverflow.com/questions/3633000/net-enumerate-winforms-font-styles
    /// </summary>
    public class FontFamily
    {
        #region InstalledFont Parameters

        string _fontName = string.Empty;
        string _fontSubFamily = string.Empty;
        string _fontPath = string.Empty;

        #endregion

        #region InstalledFont Constructor

        private FontFamily(string fontName, string fontSubFamily, string fontPath)
        {
            _fontName = fontName;
            _fontSubFamily = fontSubFamily;
            _fontPath = fontPath;
        }

        #endregion

        #region InstalledFont Properties

        public string FontName { get { return _fontName; } set { _fontName = value; } }
        public string FontSubFamily { get { return _fontSubFamily; } set { _fontSubFamily = value; } }
        public string FontPath { get { return _fontPath; } set { _fontPath = value; } }

        #endregion

        public static FontFamily FromPath(string fontFilePath)
        {
            string FontName = string.Empty;
            string FontSubFamily = string.Empty;
            string encStr = "UTF-8";
            string strRet = string.Empty;

            using (FileStream fs = new FileStream(fontFilePath, FileMode.Open, FileAccess.Read))
            {

                TT_OFFSET_TABLE ttOffsetTable = new TT_OFFSET_TABLE()
                {
                    uMajorVersion = ReadUShort(fs),
                    uMinorVersion = ReadUShort(fs),
                    uNumOfTables = ReadUShort(fs),
                    uSearchRange = ReadUShort(fs),
                    uEntrySelector = ReadUShort(fs),
                    uRangeShift = ReadUShort(fs),
                };

                TT_TABLE_DIRECTORY tblDir = new TT_TABLE_DIRECTORY();
                bool found = false;
                for (int i = 0; i <= ttOffsetTable.uNumOfTables; i++)
                {
                    tblDir = new TT_TABLE_DIRECTORY();
                    tblDir.Initialize();
                    fs.Read(tblDir.szTag, 0, tblDir.szTag.Length);
                    tblDir.uCheckSum = ReadULong(fs);
                    tblDir.uOffset = ReadULong(fs);
                    tblDir.uLength = ReadULong(fs);

                    Encoding enc = Encoding.GetEncoding(encStr);
                    string s = enc.GetString(tblDir.szTag);

                    if (s.CompareTo("name") == 0)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found) return null;

                fs.Seek(tblDir.uOffset, SeekOrigin.Begin);

                TT_NAME_TABLE_HEADER ttNTHeader = new TT_NAME_TABLE_HEADER
                {
                    uFSelector = ReadUShort(fs),
                    uNRCount = ReadUShort(fs),
                    uStorageOffset = ReadUShort(fs)
                };

                TT_NAME_RECORD ttRecord = new TT_NAME_RECORD();

                for (int j = 0; j <= ttNTHeader.uNRCount; j++)
                {
                    ttRecord = new TT_NAME_RECORD()
                    {
                        uPlatformID = ReadUShort(fs),
                        uEncodingID = ReadUShort(fs),
                        uLanguageID = ReadUShort(fs),
                        uNameID = ReadUShort(fs),
                        uStringLength = ReadUShort(fs),
                        uStringOffset = ReadUShort(fs)
                    };

                    if (ttRecord.uNameID > 2) { break; }

                    long nPos = fs.Position;
                    fs.Seek(tblDir.uOffset + ttRecord.uStringOffset + ttNTHeader.uStorageOffset, SeekOrigin.Begin);

                    byte[] buf = new byte[ttRecord.uStringLength];
                    fs.Read(buf, 0, ttRecord.uStringLength);

                    Encoding enc;
                    if (ttRecord.uEncodingID == 3 || ttRecord.uEncodingID == 1)
                    {
                        enc = Encoding.BigEndianUnicode;
                    }
                    else
                    {
                        enc = Encoding.UTF8;
                    }

                    strRet = enc.GetString(buf);
                    if (ttRecord.uNameID == 1) { FontName = strRet; }
                    if (ttRecord.uNameID == 2) { FontSubFamily = strRet; }

                    fs.Seek(nPos, SeekOrigin.Begin);
                }

                return new FontFamily(FontName, FontSubFamily, fontFilePath);
            }
        }

        public struct TT_OFFSET_TABLE
        {
            public ushort uMajorVersion;
            public ushort uMinorVersion;
            public ushort uNumOfTables;
            public ushort uSearchRange;
            public ushort uEntrySelector;
            public ushort uRangeShift;
        }

        public struct TT_TABLE_DIRECTORY
        {
            public byte[] szTag;
            public uint uCheckSum;
            public uint uOffset;
            public uint uLength;
            public void Initialize()
            {
                szTag = new byte[4];
            }
        }

        public struct TT_NAME_TABLE_HEADER
        {
            public ushort uFSelector;
            public ushort uNRCount;
            public ushort uStorageOffset;
        }

        public struct TT_NAME_RECORD
        {
            public ushort uPlatformID;
            public ushort uEncodingID;
            public ushort uLanguageID;
            public ushort uNameID;
            public ushort uStringLength;
            public ushort uStringOffset;
        }

        private static ushort ReadChar(FileStream fs, int characters)
        {
            string[] s = new string[characters];
            byte[] buf = new byte[Convert.ToByte(s.Length)];

            buf = ReadAndSwap(fs, buf.Length);
            return BitConverter.ToUInt16(buf, 0);
        }

        private static ushort ReadByte(FileStream fs)
        {
            byte[] buf = new byte[11];
            buf = ReadAndSwap(fs, buf.Length);
            return BitConverter.ToUInt16(buf, 0);
        }

        private static ushort ReadUShort(FileStream fs)
        {
            byte[] buf = new byte[2];
            buf = ReadAndSwap(fs, buf.Length);
            return BitConverter.ToUInt16(buf, 0);
        }

        private static uint ReadULong(FileStream fs)
        {
            byte[] buf = new byte[4];
            buf = ReadAndSwap(fs, buf.Length);
            return BitConverter.ToUInt32(buf, 0);
        }

        private static byte[] ReadAndSwap(FileStream fs, int size)
        {
            byte[] buf = new byte[size];
            fs.Read(buf, 0, buf.Length);
            Array.Reverse(buf);
            return buf;
        }
    }
}