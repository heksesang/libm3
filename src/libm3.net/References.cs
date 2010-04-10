/****************************************************************************
*                                                                           *
*   libm3.net                                                               *
*   Copyright (C) 2010 Gunnar Lilleaasen                                    *
*                                                                           *
*   This program is free software; you can redistribute it and/or modify    *
*   it under the terms of the GNU General Public License as published by    *
*   the Free Software Foundation; either version 2 of the License, or       *
*   (at your option) any later version.                                     *
*                                                                           *
*   This program is distributed in the hope that it will be useful,         *
*   but WITHOUT ANY WARRANTY; without even the implied warranty of          *
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the           *
*   GNU General Public License for more details.                            *
*                                                                           *
*   You should have received a copy of the GNU General Public License along *
*   with this program; if not, write to the Free Software Foundation, Inc., *
*   51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.             *
*                                                                           *
****************************************************************************/

using Sharp3D.Math.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace libm3
{
    public struct AnimRef
    {
        public UInt32 Flags;
        public UInt32 AnimId;

        public AnimRef(BinaryReader br)
        {
            Flags = br.ReadUInt32();
            AnimId = br.ReadUInt32();
        }
        public AnimRef(UInt32 animId, UInt32 flags)
        {
            Flags = flags;
            AnimId = animId;
        }

        #region ISerializable Members

        public void Write(FileStream fs, BinaryWriter bw)
        {
            bw.Write(Flags);
            bw.Write(AnimId);
        }

        #endregion
    }

    public struct TagRef
    {
        public Int32 NumEntries;
        public Int32 Tag;

        public TagRef(BinaryReader br)
        {
            NumEntries = br.ReadInt32();
            Tag = br.ReadInt32();
        }
        public TagRef(Int32 numEntries, Int32 tag)
        {
            NumEntries = numEntries;
            Tag = tag;
        }

        #region ISerializable Members

        public void Write(FileStream fs, BinaryWriter bw)
        {
            bw.Write(NumEntries);
            bw.Write(Tag);
        }

        #endregion
    }

    public struct Tag
    {
        public String Id;
        public Int32 Offset;
        public Int32 NumEntries;
        public UInt32 Type;

        public static String GetId(Type t)
        {
            Dictionary<Type, String> types = new Dictionary<Type, String>();

            types.Add(typeof(MD33), "33DM");
            types.Add(typeof(Model), "LDOM");
            types.Add(typeof(Vertex), "__8U");
            types.Add(typeof(Face), "_61U");
            types.Add(typeof(Bone), "ENOB");
            types.Add(typeof(Geoset), "NGER");
            types.Add(typeof(Material), "_TAM");
            types.Add(typeof(MaterialGroup), "MTAM");
            types.Add(typeof(Geometry), "_VID");

            types.Add(typeof(SC2UInt16), "_61U");
            types.Add(typeof(SC2UInt32), "_23U");
            types.Add(typeof(SC2Int16), "_61I");
            types.Add(typeof(SC2Int32), "_23I");
            types.Add(typeof(SC2Byte), "__8U");
            types.Add(typeof(SC2Char), "RAHC");

            return types[t];
        }

        public Tag(BinaryReader br)
        {
            Id = Encoding.ASCII.GetString(br.ReadBytes(4));
            Offset = br.ReadInt32();
            NumEntries = br.ReadInt32();
            Type = br.ReadUInt32();
        }
        public Tag(Type t, Int32 offset, Int32 numEntries, UInt32 type)
        {
            Id = GetId(t);
            Offset = offset;
            NumEntries = numEntries;
            Type = type;
        }

        public void Write(FileStream fs, BinaryWriter bw)
        {
            bw.Write(Id.ToCharArray(0, 4));
            bw.Write(Offset);
            bw.Write(NumEntries);
            bw.Write(Type);
        }
    }
}