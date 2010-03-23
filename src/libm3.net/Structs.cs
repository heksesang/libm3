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

namespace libm3
{
    public struct MD33
    {
        /*0x00*/ public String Magic;
        /*0x04*/ public Int32 OfsRefs;
        /*0x08*/ public Int32 NumRefs;
        /*0x0C*/ public TagRef Model;

        public static MD33 ReadMD33(BinaryReader br)
        {
            MD33 head = new MD33();
            head.Magic = Encoding.ASCII.GetString(br.ReadBytes(4));
            head.OfsRefs = br.ReadInt32();
            head.NumRefs = br.ReadInt32();
            head.Model = TagRef.ReadTagRef(br);
            return head;
        }

        public static void WriteMD33(BinaryWriter bw, MD33 head)
        {
            bw.Write(head.Magic.ToCharArray(0, 4));
            bw.Write(head.OfsRefs);
            bw.Write(head.NumRefs);
            bw.Write(head.Model.NumEntries);
            bw.Write(head.Model.Tag);
        }
    };

    public struct TagRef
    {
        /*0x00*/ public Int32 NumEntries;
        /*0x04*/ public Int32 Tag;

        public static TagRef ReadTagRef(BinaryReader br)
        {
            TagRef t = new TagRef();
            t.NumEntries = br.ReadInt32();
            t.Tag = br.ReadInt32();
            return t;
        }

        public static void WriteTagRef(BinaryWriter bw, TagRef tr)
        {
            bw.Write(tr.NumEntries);
            bw.Write(tr.Tag);
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

        public static void WriteTag(BinaryWriter bw, Tag t)
        {
            bw.Write(t.Id.ToCharArray(0, 4));
            bw.Write(t.Offset);
            bw.Write(t.NumEntries);
            bw.Write(t.Type);
        }
    };

    public struct Model
    {
        public String Name;
        public UInt32 Version;
        public UInt32 Flags;
        public List<Vertex> Vertices;

        public Vector3D[] VertexExtents;
        public Double VertexRadius;

        public static Model ReadModel(FileStream fs, BinaryReader br, UInt32 type, List<Tag> tags)
        {
            Model m = new Model();
            Int64 lCurrentPos = fs.Position;

            // Read name
            TagRef refName = TagRef.ReadTagRef(br);
            lCurrentPos = fs.Position;
            fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);

            m.Name = br.ReadString();

            fs.Seek(lCurrentPos, SeekOrigin.Begin);

            // Read version
            m.Version = br.ReadUInt32();

            // Skip a lot of data
            br.ReadBytes(0x38);

            // Read flags
            m.Flags = br.ReadUInt32();

            // Read vertices
            TagRef refVertex = TagRef.ReadTagRef(br);
            Tag tagVertex = tags[refVertex.Tag];
            lCurrentPos = fs.Position;
            fs.Seek(tagVertex.Offset, SeekOrigin.Begin);

            m.Vertices = new List<Vertex>();

            while (tagVertex.Offset + tagVertex.NumEntries != fs.Position)
            {
                m.Vertices.Add(Vertex.ReadVertex(br, m.Flags));
            }

            fs.Seek(lCurrentPos, SeekOrigin.Begin);

            // Skip geometry for now
            br.ReadBytes(0x10);

            // Read vertex extents
            m.VertexExtents = new Vector3D[2];
            for (Int32 i = 0; i < 2; i++)
            {
                m.VertexExtents[i].X = br.ReadSingle();
                m.VertexExtents[i].Y = br.ReadSingle();
                m.VertexExtents[i].Z = br.ReadSingle();
            }
            m.VertexRadius = br.ReadSingle();

            return m;
        }
    };

    public struct Vertex
    {
        public Vector3D Position;
        public Vector4D Normal;
        public Vector2D UV;
        public Vector4D Tangent;
        public UInt32[] BoneIndex;
        public Double[] BoneWeight;

        public static Vertex ReadVertex(BinaryReader br, UInt32 flags)
        {
            Vertex v = new Vertex();
            
            // Position
            v.Position.X = br.ReadSingle();
            v.Position.Y = br.ReadSingle();
            v.Position.Z = br.ReadSingle();

            // Bones
            Byte[] weight = br.ReadBytes(4);
            Byte[] index = br.ReadBytes(4);
            v.BoneWeight = new Double[4];
            v.BoneIndex = new UInt32[4];
            for (Int32 i = 0; i < 4; i++)
            {
                v.BoneIndex[i] = index[i];
                v.BoneWeight[i] = weight[i] / 255.0d;
            }
            
            // Normal
            v.Normal.X = 2 * br.ReadByte() / 255.0d - 1;
            v.Normal.Y = 2 * br.ReadByte() / 255.0d - 1;
            v.Normal.Z = 2 * br.ReadByte() / 255.0d - 1;
            v.Normal.W = br.ReadByte() / 255.0d;
            
            // UV coords
            v.UV.X =  br.ReadInt16() / 2048.0d;
            v.UV.Y = -br.ReadInt16() / 2048.0d;
            
            // Skip extra value
            if ((flags & 0x40000) != 0)
            {
                // Skip 4 bytes if flags & 0x40000
                br.ReadBytes(4);
            }
            
            // Tangent
            v.Tangent.X = 2 * br.ReadByte() / 255.0d - 1;
            v.Tangent.Y = 2 * br.ReadByte() / 255.0d - 1;
            v.Tangent.Z = 2 * br.ReadByte() / 255.0d - 1;
            v.Tangent.W = br.ReadByte() / 255.0d;
            
            return v;
        }

        public static void WriteVertex(BinaryWriter bw, UInt32 flags, Vertex v)
        {
            // Position vector
            bw.Write((Single)v.Position.X);
            bw.Write((Single)v.Position.Y);
            bw.Write((Single)v.Position.Z);

            // Bones
            foreach (Double weight in v.BoneWeight)
            {
                bw.Write((Byte)(weight * Byte.MaxValue));
            }
            foreach (UInt32 index in v.BoneIndex)
            {
                bw.Write((Byte)index);
            }

            // Normal
            bw.Write((Byte)(Byte.MaxValue * (v.Normal.X + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * (v.Normal.Y + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * (v.Normal.Z + 1) / 2));
            bw.Write((Byte)(v.Normal.W * Byte.MaxValue));

            // UV coords
            bw.Write((Int16)(v.UV.X * 2048.0d));
            bw.Write((Int16)(-v.UV.Y * 2048.0d));
            
            // Unknown
            if ((flags & 0x40000) != 0)
            {
                bw.Write(0);
            }

            // Tangent
            bw.Write((Byte)0);
            bw.Write((Byte)0);
            bw.Write((Byte)0);
            bw.Write((Byte)0);
        }
    };
}
