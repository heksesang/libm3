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
using System.IO;
using System.Linq;
using System.Text;

namespace libm3.net
{
    public struct MD33
    {
        /*0x00*/ public String Magic;
        /*0x04*/ public UInt32 OfsRefs;
        /*0x08*/ public UInt32 NumRefs;
        /*0x0C*/ public TagRef Model;

        public static MD33 ReadMD33(BinaryReader br)
        {
            MD33 head = new MD33();
            head.Magic = Encoding.ASCII.GetString(br.ReadBytes(4));
            head.OfsRefs = br.ReadUInt32();
            head.NumRefs = br.ReadUInt32();
            head.Model = TagRef.ReadTagRef(br);
            return head;
        }
    };

    public struct TagRef
    {
        /*0x00*/ public UInt32 NumEntries;
        /*0x04*/ public UInt32 Tag;

        public static TagRef ReadTagRef(BinaryReader br)
        {
            TagRef t = new TagRef();
            t.NumEntries = br.ReadUInt32();
            t.Tag = br.ReadUInt32();
            return t;
        }
    };

    public struct Tag
    {
        /*0x00*/ public String Id;
        /*0x04*/ public UInt32 Offset;
        /*0x08*/ public UInt32 NumEntries;
        /*0x0C*/ public UInt32 Type;

        public static Tag ReadTag(BinaryReader br)
        {
            Tag t = new Tag();
            t.Id = Encoding.ASCII.GetString(br.ReadBytes(4));
            t.Offset = br.ReadUInt32();
            t.NumEntries = br.ReadUInt32();
            t.Type = br.ReadUInt32();
            return t;
        }
    };

    public struct Model
    {
        public TagRef Name;
        public UInt32 Version;
        public UInt32 Flags;
        public TagRef Bones;
        public TagRef Vertices;
        public TagRef Views;

        public Vector3D[] VertexExtents;
        public Double VertexRadius;

        public static Model ReadModel(BinaryReader br, UInt32 type)
        {
            Model m = new Model();
            return m;
        }
    };

    public struct Vertex
    {
        public Vector3D Position;
        public Vector4D Normal;
        public Vector2D UV;
        public Vector4D Tangent;
        public Dictionary<UInt32, Double> Bones;

        public static Vertex ReadVertex(BinaryReader br, UInt32 flags)
        {
            Vertex v = new Vertex();
            
            v.Position.X = br.ReadSingle();
            v.Position.Y = br.ReadSingle();
            v.Position.Z = br.ReadSingle();
            
            Byte[] index = br.ReadBytes(4);
            Byte[] weight = br.ReadBytes(4);
            for (UInt32 i = 0; i < 4; i++)
            {
                v.Bones.Add(index[i], weight[i] / Byte.MaxValue);
            }
            
            v.Normal.X = 2 * br.ReadByte() / 255.0d - 1;
            v.Normal.Y = 2 * br.ReadByte() / 255.0d - 1;
            v.Normal.Z = 2 * br.ReadByte() / 255.0d - 1;
            v.Normal.W = br.ReadByte() / 255.0d;
            
            v.UV.X =  br.ReadInt16() / 2048.0d;
            v.UV.Y = -br.ReadInt16() / 2048.0d;
            
            if ((flags & 0x40000) != 0)
            {
                // Skip 4 bytes if flags & 0x40000
                br.ReadBytes(4);
            }
            
            v.Tangent.X = 2 * br.ReadByte() / 255.0d - 1;
            v.Tangent.Y = 2 * br.ReadByte() / 255.0d - 1;
            v.Tangent.Z = 2 * br.ReadByte() / 255.0d - 1;
            v.Tangent.W = br.ReadByte() / 255.0d;
            
            return v;
        }

        public override string ToString()
        {
            String str = "";
            str += "Position: " +
                Position.X + ", " +
                Position.Y + ", " +
                Position.Z + "\n";
            str += "Normal: " +
                Normal.X + ", " +
                Normal.Y + ", " +
                Normal.Z + ", " +
                Normal.W + "\n";
            str += "UV: " +
                UV.X + ", " +
                UV.Y + "\n";
            str += "Tangent: " +
                Tangent.X + ", " +
                Tangent.Y + ", " +
                Tangent.Z + ", " +
                Tangent.W + "\n";
            return str;
        }
    };
}
