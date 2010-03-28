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
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

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

    public struct AnimRef
    {
        /*0x00*/ public UInt32 Flags;
        /*0x04*/ public UInt32 AnimId;

        public static AnimRef ReadAnimRef(BinaryReader br)
        {
            AnimRef aref = new AnimRef();

            aref.Flags = br.ReadUInt32();
            aref.AnimId = br.ReadUInt32();

            return aref;
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

    public class Model
    {
        public String Name;
        public UInt32 Version;
        public UInt32 Flags;
        public List<Vertex> Vertices;
        public List<Face> Faces;
        public List<Bone> Bones;
        public List<Geoset> Geosets;
        public List<Material> Materials;
        public List<MaterialGroup> MaterialGroups;

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

            TagRef refMatBind = TagRef.ReadTagRef(br);
            lstPos.Add(fs.Position);
            fs.Seek(tags[refMatBind.Tag].Offset, SeekOrigin.Begin);

            for (int i = 0; i < refMatBind.NumEntries; i++)
            {
                // Skip 4 bytes
                br.ReadBytes(4);
                
                // Read geoid
                UInt16 geoid = br.ReadUInt16();

                // Skip 4 bytes
                br.ReadBytes(4);

                // Read material id
                UInt16 matid = br.ReadUInt16();

                // Skip 2 bytes
                br.ReadBytes(2);

                m.Geosets[geoid].MaterialGroup = matid;
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
            br.ReadBytes(0x5C);

            if (type == 23)
                br.ReadBytes(0x08);

            // Materials
            TagRef refMatGroups = TagRef.ReadTagRef(br);

            lstPos.Add(fs.Position);
            fs.Seek(tags[refMatGroups.Tag].Offset, SeekOrigin.Begin);

            m.MaterialGroups = new List<MaterialGroup>();

            for (Int32 i = 0; i < refMatGroups.NumEntries; i++)
            {
                m.MaterialGroups.Add(MaterialGroup.ReadMaterialGroup(br));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

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

        public void ToXML()
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node;

            // XML declaration
            node = doc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
            doc.AppendChild(node);

            // Root node
            XmlElement root = doc.CreateElement("COLLADA");
            doc.AppendChild(root);

            // Model
            XML.Build(doc, this);

            // Set the namespace and version
            root.SetAttribute("xmlns", "http://www.collada.org/2005/11/COLLADASchema");
            root.SetAttribute("version", "1.4.0");

            try
            {
                doc.Save("C:\\Release\\test.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    public static class XML
    {        
        // Extension functions
        public static XmlNode GetElementByAttribute(this XmlNode node, String attribute, String value)
        {
            return node.SelectSingleNode("/*[@" + attribute + "=" + value + "]");
        }
        public static XmlNodeList GetElementsByAttribute(this XmlNode node, String attribute, String value)
        {
            return node.SelectNodes("/*[@" + attribute + "=" + value + "]");
        }

        // Build XML
        public static void Build(XmlDocument doc, Model model)
        {
            // Set locale for correct group separator
            String originalCulture = CultureInfo.CurrentCulture.ToString();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            
            // Do the XML
            XmlElement el;
            XmlNode root = doc.SelectSingleNode("/COLLADA");

            // Write metadata
            XmlNode asset = root.AppendChild(doc.CreateElement("asset"));
            XmlNode created = asset.AppendChild(doc.CreateElement("created"));
            created.AppendChild(doc.CreateTextNode(DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")));
            XmlNode modified = asset.AppendChild(doc.CreateElement("modified"));
            modified.AppendChild(doc.CreateTextNode(DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")));
            XmlNode up_axis = asset.AppendChild(doc.CreateElement("up_axis"));
            up_axis.AppendChild(doc.CreateTextNode("Y_UP"));

            // Write data
            root.AppendChild(doc.CreateElement("library_geometries"));
            root.AppendChild(doc.CreateElement("library_visual_scenes")).AppendChild(doc.CreateElement("visual_scene"));
            root.AppendChild(doc.CreateElement("scene"));

            el = (XmlElement)doc.SelectSingleNode("/COLLADA/library_visual_scenes/visual_scene");
            el.SetAttribute("id", "RootNode");

            Int32 count = 0;
            foreach (Geoset set in model.Geosets)
            {
                XmlElement geometry = (XmlElement)doc.SelectSingleNode("/COLLADA/library_geometries").AppendChild(doc.CreateElement("geometry"));
                geometry.SetAttribute("id", "geoset" + count);
                XmlElement mesh = (XmlElement)geometry.AppendChild(doc.CreateElement("mesh"));
                {
                    // Position
                    {
                        XmlElement source = (XmlElement)mesh.AppendChild(doc.CreateElement("source"));
                        source.SetAttribute("id", "geoset" + count + "_position");
                        XmlElement array = (XmlElement)source.AppendChild(doc.CreateElement("float_array"));
                        array.SetAttribute("id","geoset" + count + "_position-array");
                        array.SetAttribute("count", Convert.ToString(set.NumVertices * 3));
                        XmlText values = (XmlText)array.AppendChild(doc.CreateTextNode("\r\n"));
                        for (Int32 i = set.StartVertex; i < set.StartVertex + set.NumVertices; i++)
                        {
                            values.AppendData(Convert.ToString(model.Vertices[i].Position.X) + " ");
                            values.AppendData(Convert.ToString(model.Vertices[i].Position.Y) + " ");
                            values.AppendData(Convert.ToString(model.Vertices[i].Position.Z) + "\r\n");
                        }
                        XmlElement accessor = (XmlElement)source.AppendChild(doc.CreateElement("technique_common")).AppendChild(doc.CreateElement("accessor"));
                        accessor.SetAttribute("source", "#geoset" + count + "_position-array");
                        accessor.SetAttribute("count", Convert.ToString(set.NumVertices));
                        accessor.SetAttribute("stride", "3");
                        XmlElement param = (XmlElement)accessor.AppendChild(doc.CreateElement("param"));
                        param.SetAttribute("name", "X");
                        param.SetAttribute("type", "float");
                        param = (XmlElement)accessor.AppendChild(doc.CreateElement("param"));
                        param.SetAttribute("name", "Y");
                        param.SetAttribute("type", "float");
                        param = (XmlElement)accessor.AppendChild(doc.CreateElement("param"));
                        param.SetAttribute("name", "Z");
                        param.SetAttribute("type", "float");
                    }

                    // Normal
                    {
                        XmlElement source = (XmlElement)mesh.AppendChild(doc.CreateElement("source"));
                        source.SetAttribute("id", "geoset" + count + "_normal");
                        XmlElement array = (XmlElement)source.AppendChild(doc.CreateElement("float_array"));
                        array.SetAttribute("id", "geoset" + count + "_normal-array");
                        array.SetAttribute("count", Convert.ToString(set.NumVertices * 3));
                        XmlText values = (XmlText)array.AppendChild(doc.CreateTextNode("\r\n"));
                        for (Int32 i = set.StartVertex; i < set.StartVertex + set.NumVertices; i++)
                        {
                            values.AppendData(Convert.ToString(model.Vertices[i].Normal.X) + " ");
                            values.AppendData(Convert.ToString(model.Vertices[i].Normal.Y) + " ");
                            values.AppendData(Convert.ToString(model.Vertices[i].Normal.Z) + "\r\n");
                        }
                        XmlElement accessor = (XmlElement)source.AppendChild(doc.CreateElement("technique_common")).AppendChild(doc.CreateElement("accessor"));
                        accessor.SetAttribute("source", "#geoset" + count + "_normal-array");
                        accessor.SetAttribute("count", Convert.ToString(set.NumVertices));
                        accessor.SetAttribute("stride", "3");
                        XmlElement param = (XmlElement)accessor.AppendChild(doc.CreateElement("param"));
                        param.SetAttribute("name", "X");
                        param.SetAttribute("type", "float");
                        param = (XmlElement)accessor.AppendChild(doc.CreateElement("param"));
                        param.SetAttribute("name", "Y");
                        param.SetAttribute("type", "float");
                        param = (XmlElement)accessor.AppendChild(doc.CreateElement("param"));
                        param.SetAttribute("name", "Z");
                        param.SetAttribute("type", "float");
                    }

                    // UV
                    {
                        XmlElement source = (XmlElement)mesh.AppendChild(doc.CreateElement("source"));
                        source.SetAttribute("id", "geoset" + count + "_uv");
                        XmlElement array = (XmlElement)source.AppendChild(doc.CreateElement("float_array"));
                        array.SetAttribute("id", "geoset" + count + "_uv-array");
                        array.SetAttribute("count", Convert.ToString(set.NumVertices * 2));
                        XmlText values = (XmlText)array.AppendChild(doc.CreateTextNode("\r\n"));
                        for (Int32 i = set.StartVertex; i < set.StartVertex + set.NumVertices; i++)
                        {
                            values.AppendData(Convert.ToString(model.Vertices[i].UV.X) + " ");
                            values.AppendData(Convert.ToString(model.Vertices[i].UV.Y) + "\r\n");
                        }
                        XmlElement accessor = (XmlElement)source.AppendChild(doc.CreateElement("technique_common")).AppendChild(doc.CreateElement("accessor"));
                        accessor.SetAttribute("source", "#geoset" + count + "_uv-array");
                        accessor.SetAttribute("count", Convert.ToString(set.NumVertices));
                        accessor.SetAttribute("stride", "2");
                        XmlElement param = (XmlElement)accessor.AppendChild(doc.CreateElement("param"));
                        param.SetAttribute("name", "S");
                        param.SetAttribute("type", "float");
                        param = (XmlElement)accessor.AppendChild(doc.CreateElement("param"));
                        param.SetAttribute("name", "T");
                        param.SetAttribute("type", "float");
                    }

                    // Tangents
                    {
                        XmlElement source = (XmlElement)mesh.AppendChild(doc.CreateElement("source"));
                        source.SetAttribute("id", "geoset" + count + "_tangent");
                        XmlElement array = (XmlElement)source.AppendChild(doc.CreateElement("float_array"));
                        array.SetAttribute("id", "geoset" + count + "_tangent-array");
                        array.SetAttribute("count", Convert.ToString(set.NumVertices * 2));
                        XmlText values = (XmlText)array.AppendChild(doc.CreateTextNode("\r\n"));
                        for (Int32 i = set.StartVertex; i < set.StartVertex + set.NumVertices; i++)
                        {
                            values.AppendData(Convert.ToString(model.Vertices[i].UV.X) + " ");
                            values.AppendData(Convert.ToString(model.Vertices[i].UV.Y) + "\r\n");
                        }
                        XmlElement accessor = (XmlElement)source.AppendChild(doc.CreateElement("technique_common")).AppendChild(doc.CreateElement("accessor"));
                        accessor.SetAttribute("source", "#geoset" + count + "_tangent-array");
                        accessor.SetAttribute("count", Convert.ToString(set.NumVertices));
                        accessor.SetAttribute("stride", "3");
                        XmlElement param = (XmlElement)accessor.AppendChild(doc.CreateElement("param"));
                        param.SetAttribute("name", "X");
                        param.SetAttribute("type", "float");
                        param = (XmlElement)accessor.AppendChild(doc.CreateElement("param"));
                        param.SetAttribute("name", "Y");
                        param.SetAttribute("type", "float");
                        param = (XmlElement)accessor.AppendChild(doc.CreateElement("param"));
                        param.SetAttribute("name", "Z");
                        param.SetAttribute("type", "float");
                    }

                    XmlElement vertices = (XmlElement)mesh.AppendChild(doc.CreateElement("vertices"));
                    vertices.SetAttribute("id", "#geoset" + count + "_vertex");
                    XmlElement input = (XmlElement)vertices.AppendChild(doc.CreateElement("input"));
                    input.SetAttribute("semantic", "POSITION");
                    input.SetAttribute("source", "#geoset" + count + "_position");
                    XmlElement triangles = (XmlElement)mesh.AppendChild(doc.CreateElement("triangles"));
                    triangles.SetAttribute("count", Convert.ToString(set.NumTriangles));
                    input = (XmlElement)triangles.AppendChild(doc.CreateElement("input"));
                    input.SetAttribute("semantic", "VERTEX");
                    input.SetAttribute("source", "#geoset" + count + "_vertex");
                    input.SetAttribute("offset", "0");
                    input = (XmlElement)triangles.AppendChild(doc.CreateElement("input"));
                    input.SetAttribute("semantic", "NORMAL");
                    input.SetAttribute("source", "#geoset" + count + "_normal");
                    input.SetAttribute("offset", "1");
                    input = (XmlElement)triangles.AppendChild(doc.CreateElement("input"));
                    input.SetAttribute("semantic", "TEXCOORD");
                    input.SetAttribute("source", "#geoset" + count + "_uv");
                    input.SetAttribute("offset", "2");
                    input = (XmlElement)triangles.AppendChild(doc.CreateElement("input"));
                    input.SetAttribute("semantic", "TEXTANGENT");
                    input.SetAttribute("source", "#geoset" + count + "_tangent");
                    input.SetAttribute("offset", "3");
                    XmlText p = (XmlText)triangles.AppendChild(doc.CreateElement("p")).AppendChild(doc.CreateTextNode("\r\n"));
                    for (Int32 i = set.StartTriangle; i < set.StartTriangle + set.NumTriangles; i++)
                    {
                        p.AppendData(Convert.ToString(model.Faces[i].Vertices[0] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i].Vertices[0] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i].Vertices[0] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i].Vertices[0] - set.StartVertex) + " ");

                        p.AppendData(Convert.ToString(model.Faces[i].Vertices[1] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i].Vertices[1] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i].Vertices[1] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i].Vertices[1] - set.StartVertex) + " ");

                        p.AppendData(Convert.ToString(model.Faces[i].Vertices[2] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i].Vertices[2] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i].Vertices[2] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i].Vertices[2] - set.StartVertex) + "\r\n");
                    }
                }
                XmlElement node = (XmlElement)doc.SelectSingleNode("/COLLADA/library_visual_scenes/visual_scene").AppendChild(doc.CreateElement("node"));
                node.SetAttribute("id", "geoset" + count + "_node");
                XmlElement instance_geometry = (XmlElement)node.AppendChild(doc.CreateElement("instance_geometry"));
                instance_geometry.SetAttribute("url", "#geoset" + count);
                count++;
            }

            XmlElement instance_visual_scene = (XmlElement)doc.SelectSingleNode("/COLLADA/scene").AppendChild(doc.CreateElement("instance_visual_scene"));
            instance_visual_scene.SetAttribute("url", "#RootNode");

            // Restore the locale
            Thread.CurrentThread.CurrentCulture = new CultureInfo(originalCulture);
        }
    }

    public class Vertex
    {
        public Vector3F Position;
        public Vector4F Normal;
        public Vector2F UV;
        public Vector4F Tangent;
        public UInt32[] BoneIndex;
        public Single[] BoneWeight;

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
            v.BoneWeight = new Single[4];
            v.BoneIndex = new UInt32[4];
            for (Int32 i = 0; i < 4; i++)
            {
                v.BoneIndex[i] = index[i];
                v.BoneWeight[i] = weight[i] / 255.0f;
            }
            
            // Normal
            v.Normal.X = 2 * br.ReadByte() / 255.0f - 1;
            v.Normal.Y = 2 * br.ReadByte() / 255.0f - 1;
            v.Normal.Z = 2 * br.ReadByte() / 255.0f - 1;
            v.Normal.W = br.ReadByte() / 255.0f;
            
            // UV coords
            v.UV.X =  br.ReadInt16() / 2048.0f;
            v.UV.Y = -br.ReadInt16() / 2048.0f;
            
            // Skip extra value
            if ((flags & 0x40000) != 0)
            {
                // Skip 4 bytes if flags & 0x40000
                br.ReadBytes(4);
            }
            
            // Tangent
            v.Tangent.X = 2 * br.ReadByte() / 255.0f - 1;
            v.Tangent.Y = 2 * br.ReadByte() / 255.0f - 1;
            v.Tangent.Z = 2 * br.ReadByte() / 255.0f - 1;
            v.Tangent.W = br.ReadByte() / 255.0f;
            
            return v;
        }

        public static void WriteVertex(BinaryWriter bw, UInt32 flags, Vertex v)
        {
            // Position vector
            bw.Write(v.Position.X);
            bw.Write(v.Position.Y);
            bw.Write(v.Position.Z);

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
            bw.Write((Byte)(Byte.MaxValue * v.Normal.W));

            // UV coords
            bw.Write((Int16)(v.UV.X * 2048.0d));
            bw.Write((Int16)(-v.UV.Y * 2048.0d));
            
            // Unknown
            if ((flags & 0x40000) != 0)
            {
                bw.Write(0);
            }

            // Tangent
            bw.Write((Byte)(Byte.MaxValue * (v.Tangent.X + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * (v.Tangent.X + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * (v.Tangent.X + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * v.Tangent.X));
        }
    }

    public class Face
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

    public class Bone
    {
        public String Name;
        public UInt32 Flags;
        public Int16 Parent;
        public Vector3D Position;
        public QuaternionD Rotation;
        public Vector3D Scale;

        public AnimRef[] refs;

        public static Bone ReadBone(FileStream fs, BinaryReader br, List<Tag> tags)
        {
            Bone b = new Bone();
            b.refs = new AnimRef[3];
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

            // Skip data
            br.ReadBytes(0x02);

            // Translation data
            b.refs[0] = AnimRef.ReadAnimRef(br);

            // Position
            b.Position.X = br.ReadSingle();
            b.Position.Y = br.ReadSingle();
            b.Position.Z = br.ReadSingle();

            // Skip data
            br.ReadBytes(0x18);

            // Rotation
            b.Rotation.X = br.ReadSingle();
            b.Rotation.Y = br.ReadSingle();
            b.Rotation.Z = br.ReadSingle();
            b.Rotation.W = br.ReadSingle();

            // Skip data
            br.ReadBytes(0x1C);

            // Scale
            b.Scale.X = br.ReadSingle();
            b.Scale.Y = br.ReadSingle();
            b.Scale.Z = br.ReadSingle();

            // Skip data
            br.ReadBytes(0x24);

            return b;
        }
    }

    public class Geoset
    {
        public Int32 Type;
        public Int32 StartVertex;
        public Int32 NumVertices;
        public Int32 StartTriangle;
        public Int32 NumTriangles;
        public Int32 MaterialGroup;

        public static Geoset ReadGeoset(BinaryReader br)
        {
            Geoset gs = new Geoset();

            gs.Type = br.ReadInt32();
            gs.StartVertex = br.ReadUInt16();
            gs.NumVertices = br.ReadUInt16();
            gs.StartTriangle = br.ReadInt32()/3;
            gs.NumTriangles = br.ReadInt32()/3;

            // Skip a lot of data
            br.ReadBytes(0x0C);
            
            return gs;
        }
    }

    public class Material
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

    public class MaterialGroup
    {
        public UInt32 Index;
        public UInt32 Num;

        public static MaterialGroup ReadMaterialGroup(BinaryReader br)
        {
            MaterialGroup matGroup = new MaterialGroup();

            matGroup.Num = br.ReadUInt32();
            matGroup.Index = br.ReadUInt32();

            return matGroup;
        }
    }

    public class Layer
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

    public class Sequence
    {
    }
}
