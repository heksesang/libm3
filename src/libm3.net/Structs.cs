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
    }

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
    }

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
    }

    public struct Model
    {
        public String Name;
        public UInt32 Version;
        public UInt32 Flags;
        public List<Vertex> Vertices;
        public List<Face> Faces;
        public List<Bone> Bones;
        public List<Geoset> Geosets;
        public List<Material> Materials;

        public Vector3D[] VertexExtents;
        public Double VertexRadius;

        public static Model ReadModel(FileStream fs, BinaryReader br, UInt32 type, List<Tag> tags)
        {
            Model m = new Model();
            List<Int64> lstPos = new List<Int64>();

            // Read name
            TagRef refName = TagRef.ReadTagRef(br);
            
            lstPos.Add(fs.Position);
            fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);

            m.Name = Encoding.ASCII.GetString(br.ReadBytes(refName.NumEntries - 1));

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Read version
            m.Version = br.ReadUInt32();

            // Skip a lot of data
            br.ReadBytes(0x2C);

            // Read bones
            TagRef refBone = TagRef.ReadTagRef(br);

            lstPos.Add(fs.Position);
            fs.Seek(tags[refBone.Tag].Offset, SeekOrigin.Begin);

            m.Bones = new List<Bone>();

            for (int i = 0; i < refBone.NumEntries; i++ )
            {
                m.Bones.Add(Bone.ReadBone(fs, br, tags));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Skip an integer
            br.ReadBytes(4);

            // Read flags
            m.Flags = br.ReadUInt32();

            // Read vertices
            TagRef refVertex = TagRef.ReadTagRef(br);
            
            lstPos.Add(fs.Position);
            fs.Seek(tags[refVertex.Tag].Offset, SeekOrigin.Begin);

            m.Vertices = new List<Vertex>();

            while (tags[refVertex.Tag].Offset + tags[refVertex.Tag].NumEntries != fs.Position)
            {
                m.Vertices.Add(Vertex.ReadVertex(br, m.Flags));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Geometry
            TagRef refDiv = TagRef.ReadTagRef(br);
            
            lstPos.Add(fs.Position);
            fs.Seek(tags[refDiv.Tag].Offset, SeekOrigin.Begin);

            TagRef refFace = TagRef.ReadTagRef(br);
            lstPos.Add(fs.Position);
            fs.Seek(tags[refFace.Tag].Offset, SeekOrigin.Begin);

            m.Faces = new List<Face>();

            for (int i = 0; i < refFace.NumEntries; i+=3)
            {
                m.Faces.Add(Face.ReadFace(br));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            TagRef refGeoset = TagRef.ReadTagRef(br);
            lstPos.Add(fs.Position);
            fs.Seek(tags[refGeoset.Tag].Offset, SeekOrigin.Begin);

            m.Geosets = new List<Geoset>();

            for (int i = 0; i < refGeoset.NumEntries; i++)
            {
                m.Geosets.Add(Geoset.ReadGeoset(br));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Unknown 16-bit integers
            br.ReadBytes(0x08);

            // Read vertex extents
            m.VertexExtents = new Vector3D[2];
            for (Int32 i = 0; i < 2; i++)
            {
                m.VertexExtents[i].X = br.ReadSingle();
                m.VertexExtents[i].Y = br.ReadSingle();
                m.VertexExtents[i].Z = br.ReadSingle();
            }
            m.VertexRadius = br.ReadSingle();

            // Skip a lot of data
            br.ReadBytes(0x64);

            if (type == 23)
                br.ReadBytes(0x08);

            // Materials
            TagRef refMats = TagRef.ReadTagRef(br);
            
            lstPos.Add(fs.Position);
            fs.Seek(tags[refMats.Tag].Offset, SeekOrigin.Begin);

            m.Materials = new List<Material>();

            for (Int32 i = 0; i < refMats.NumEntries; i++)
            {
                m.Materials.Add(Material.ReadMaterial(fs, br, tags));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            return m;
        }
    }

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

    public struct Face
    {
        public Int16[] Vertices;

        public static Face ReadFace(BinaryReader br)
        {
            Face f = new Face();
            f.Vertices = new Int16[3];

            for (Int16 i = 0; i < 3; i++)
            {
                f.Vertices[i] = br.ReadInt16();
            }

            return f;
        }
    }

    public struct Bone
    {
        public String Name;
        public UInt32 Flags;
        public Int16 Parent;

        public static Bone ReadBone(FileStream fs, BinaryReader br, List<Tag> tags)
        {
            Bone b = new Bone();
            Int64 lCurrentPos;

            // Skip first integer
            br.ReadBytes(4);

            // Name
            TagRef refName = TagRef.ReadTagRef(br);
            lCurrentPos = fs.Position;
            fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);
            b.Name = Encoding.ASCII.GetString(br.ReadBytes(refName.NumEntries - 1));
            fs.Seek(lCurrentPos, SeekOrigin.Begin);

            // Flags
            b.Flags = br.ReadUInt32();

            // Parent
            b.Parent = br.ReadInt16();

            // Skip a lot of data
            br.ReadBytes(0x8A);

            return b;
        }
    }

    public struct Geoset
    {
        public UInt32 Type;
        public UInt32 StartVertex;
        public UInt32 NumVertices;
        public UInt32 StartTriangle;
        public UInt32 NumTriangles;

        public static Geoset ReadGeoset(BinaryReader br)
        {
            Geoset gs = new Geoset();

            gs.Type = br.ReadUInt32();
            gs.StartVertex = br.ReadUInt16();
            gs.NumVertices = br.ReadUInt16();
            gs.StartTriangle = br.ReadUInt32()/3;
            gs.NumTriangles = br.ReadUInt32()/3;

            // Skip a lot of data
            br.ReadBytes(0x0C);
            
            return gs;
        }
    }

    public struct Material
    {
        public String Name;
        public List<Layer> Layers;
        public Vector2D Coords;

        public static Material ReadMaterial(FileStream fs, BinaryReader br, List<Tag> tags)
        {
            Material m = new Material();
            m.Layers = new List<Layer>();
            List<Int64> lstPos = new List<Int64>();

            // Name
            TagRef refName = TagRef.ReadTagRef(br);

            lstPos.Add(fs.Position);
            fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);

            m.Name = Encoding.ASCII.GetString(br.ReadBytes(refName.NumEntries - 1));

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Skip data
            br.ReadBytes(0x20);

            // Coords?
            m.Coords.X = br.ReadSingle();
            m.Coords.Y = br.ReadSingle();

            // Layers
            for (Int32 i = 0; i < 13; i++)
            {
                TagRef refLayer = TagRef.ReadTagRef(br);

                lstPos.Add(fs.Position);
                fs.Seek(tags[refLayer.Tag].Offset, SeekOrigin.Begin);

                m.Layers.Add(Layer.ReadLayer(fs, br, tags));

                fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
                lstPos.RemoveAt(lstPos.Count - 1);
            }

            // Skip a lot of data
            br.ReadBytes(0x3C);

            return m;
        }
    }

    public struct Layer
    {
        public String Name;

        public static Layer ReadLayer(FileStream fs, BinaryReader br, List<Tag> tags)
        {
            Layer l = new Layer();
            List<Int64> lstPos = new List<Int64>();

            // Skip unknown value
            br.ReadInt32();

            // Name
            TagRef refName = TagRef.ReadTagRef(br);
            
            lstPos.Add(fs.Position);
            fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);

            if(tags[refName.Tag].Id == "RAHC")
                l.Name = Encoding.ASCII.GetString(br.ReadBytes(refName.NumEntries - 1));

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Skip a lot of data
            br.ReadBytes(0x154);

            return l;
        }
    }

    public struct Sequence
    {
    }
}
