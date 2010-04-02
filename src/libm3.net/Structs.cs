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
    public static class Extentions
    {
    }

    public interface ISerializable
    {
        String TagId;
        Int32 TagType;

        void Queue(List<ISerializable> queue);
        void Write(FileStream fs, BinaryWriter bw, List<Tag> taglist);
    }

    public struct MD33 : ISerializable
    {
        /*0x00*/ public String Magic;
        /*0x04*/ public Int32 OfsRefs;
        /*0x08*/ public Int32 NumRefs;
        /*0x0C*/ public TagRef Model;

        public MD33(BinaryReader br)
        {
            Magic = Encoding.ASCII.GetString(br.ReadBytes(4));
            OfsRefs = br.ReadInt32();
            NumRefs = br.ReadInt32();
            Model = new TagRef(br);
        }
        public MD33(Int32 ofsRefs, Int32 numRefs, TagRef model)
        {
            Magic = "33DM";
            OfsRefs = ofsRefs;
            NumRefs = numRefs;
            Model = model;
        }

        #region ISerializable Members

        public void Queue(List<ISerializable> queue)
        {
            queue.Add(this);
        }

        public void Write(FileStream fs, BinaryWriter bw, List<Tag> taglist)
        {
            if (taglist[taglist.Count - 1].Id != "33DM")
            {
                taglist.Add(new Tag("33DM", (Int32)fs.Position, 1, 11));
            }
            else
            {
                Tag t = taglist[taglist.Count - 1];
                t.NumEntries++;
                taglist[taglist.Count - 1] = t;
            }

            bw.Write(Magic.ToCharArray(0, 4));
            bw.Write(OfsRefs);
            bw.Write(NumRefs);
            bw.Write(Model.NumEntries);
            bw.Write(Model.Tag);
        }

        #endregion
    }

    public struct AnimRef : ISerializable
    {
        /*0x00*/ public UInt32 Flags;
        /*0x04*/ public UInt32 AnimId;

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

        public void Queue(List<ISerializable> queue)
        {
            queue.Add(this);
        }

        public void Write(FileStream fs, BinaryWriter bw, List<Tag> taglist)
        {
            bw.Write(Flags);
            bw.Write(AnimId);
        }

        #endregion
    }

    public struct TagRef : ISerializable
    {
        /*0x00*/ public Int32 NumEntries;
        /*0x04*/ public Int32 Tag;

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

        public void Queue(List<ISerializable> queue)
        {
            queue.Add(this);
        }

        public void Write(FileStream fs, BinaryWriter bw, List<Tag> taglist)
        {
            bw.Write(NumEntries);
            bw.Write(Tag);
        }

        #endregion
    }

    public struct Tag : ISerializable
    {
        /*0x00*/ public String Id;
        /*0x04*/ public Int32 Offset;
        /*0x08*/ public Int32 NumEntries;
        /*0x0C*/ public Int32 Type;

        public Tag(BinaryReader br)
        {
            Id = Encoding.ASCII.GetString(br.ReadBytes(4));
            Offset = br.ReadInt32();
            NumEntries = br.ReadInt32();
            Type = br.ReadInt32();
        }
        public Tag(String id, Int32 offset, Int32 numEntries, Int32 type)
        {
            Id = id;
            Offset = offset;
            NumEntries = numEntries;
            Type = type;
        }

        #region ISerializable Members

        public void Queue(List<ISerializable> queue)
        {
            queue.Add(this);
        }

        public void Write(FileStream fs, BinaryWriter bw, List<Tag> taglist)
        {
            bw.Write(Id.ToCharArray(0, 4));
            bw.Write(Offset);
            bw.Write(NumEntries);
            bw.Write(Type);
        }

        #endregion
    }

    public class Model
    {
        public String Name;
        public UInt32 Version;
        public UInt32 Flags;
        public List<Vertex> Vertices;
        public List<Face> Faces;
        public List<Bone> Bones;
        public List<UInt16> BoneList;
        public List<Geoset> Geosets;
        public List<Material> Materials;
        public List<MaterialGroup> MaterialGroups;

        public Vector3D[] VertexExtents;
        public Double VertexRadius;

        public static Model ReadModel(FileStream fs, BinaryReader br, Int32 type, List<Tag> tags)
        {
            Model m = new Model();
            List<Int64> lstPos = new List<Int64>();

            // Read name
            TagRef refName = new TagRef(br);
            
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
            TagRef refBone = new TagRef(br);

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
            TagRef refVertex = new TagRef(br);
            
            lstPos.Add(fs.Position);
            fs.Seek(tags[refVertex.Tag].Offset, SeekOrigin.Begin);

            m.Vertices = new List<Vertex>();

            while (tags[refVertex.Tag].Offset + tags[refVertex.Tag].NumEntries != fs.Position)
            {
                m.Vertices.Add(new Vertex(br, m.Flags));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Geometry
            TagRef refDiv = new TagRef(br);
            
            lstPos.Add(fs.Position);
            fs.Seek(tags[refDiv.Tag].Offset, SeekOrigin.Begin);

            TagRef refFace = new TagRef(br);
            lstPos.Add(fs.Position);
            fs.Seek(tags[refFace.Tag].Offset, SeekOrigin.Begin);

            m.Faces = new List<Face>();

            for (int i = 0; i < refFace.NumEntries; i+=3)
            {
                m.Faces.Add(new Face(br));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            TagRef refGeoset = new TagRef(br);
            lstPos.Add(fs.Position);
            fs.Seek(tags[refGeoset.Tag].Offset, SeekOrigin.Begin);

            m.Geosets = new List<Geoset>();

            for (int i = 0; i < refGeoset.NumEntries; i++)
            {
                m.Geosets.Add(new Geoset(br));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            TagRef refMatBind = new TagRef(br);
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
            TagRef refMatGroups = new TagRef(br);

            lstPos.Add(fs.Position);
            fs.Seek(tags[refMatGroups.Tag].Offset, SeekOrigin.Begin);

            m.MaterialGroups = new List<MaterialGroup>();

            for (Int32 i = 0; i < refMatGroups.NumEntries; i++)
            {
                m.MaterialGroups.Add(MaterialGroup.ReadMaterialGroup(br));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            TagRef refMats = new TagRef(br);
            
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
        public void ToM3()
        {
            FileStream fs = new FileStream("C:\\Release\\test.m3", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            List<Tag> taglist = new List<Tag>();
            List< List<ISerializable> > objects = new List< List<ISerializable> >();
            
            Tag tag;
            TagRef tagref;

            foreach (ISerializable obj in Vertices)
            {
            }

            foreach (ISerializable obj in objects)
            {
                obj.Write(fs, bw, taglist);

                if (fs.Position % 0x10 != 0)
                {
                    Int32 numBytes = 0x10 - (Int32)(fs.Position % 0x10);
                    for (Int32 i = 0; i < numBytes; i++)
                    {
                        bw.Write((Byte)0xAA);
                    }
                }
            }

            foreach (ISerializable obj in objects)
            {
                if (taglist.Count == 0 || taglist[taglist.Count - 1].Id != "__8U")
                {
                    taglist.Add(new Tag("__8U", (Int32)fs.Position, 1, 0));
                }
                else
                {
                    Tag t = taglist[taglist.Count - 1];
                    t.NumEntries++;
                    taglist[taglist.Count - 1] = t;
                }

                obj.Write(fs, bw, taglist);
            }
            
            foreach (Tag t in taglist)
            {
                t.Write(fs, bw, taglist);
            }
        }
    }

    public static class XML
    {
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
            root.AppendChild(doc.CreateElement("library_effects"));
            root.AppendChild(doc.CreateElement("library_materials"));
            root.AppendChild(doc.CreateElement("library_controllers"));

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

    public class Vertex : ISerializable
    {
        public UInt32 Flags;
        public Vector3F Position;
        public Vector4F Normal;
        public Vector2F UV;
        public Vector4F Tangent;
        public UInt32[] BoneIndex;
        public Single[] BoneWeight;

        public Vertex(BinaryReader br, UInt32 flags)
        {
            // Position
            Position.X = br.ReadSingle();
            Position.Y = br.ReadSingle();
            Position.Z = br.ReadSingle();

            // Bones
            Byte[] weight = br.ReadBytes(4);
            Byte[] index = br.ReadBytes(4);
            BoneWeight = new Single[4];
            BoneIndex = new UInt32[4];
            for (Int32 i = 0; i < 4; i++)
            {
                BoneIndex[i] = index[i];
                BoneWeight[i] = weight[i] / 255.0f;
            }

            // Normal
            Normal.X = 2 * br.ReadByte() / 255.0f - 1;
            Normal.Y = 2 * br.ReadByte() / 255.0f - 1;
            Normal.Z = 2 * br.ReadByte() / 255.0f - 1;
            Normal.W = br.ReadByte() / 255.0f;

            // UV coords
            UV.X = br.ReadInt16() / 2048.0f;
            UV.Y = -br.ReadInt16() / 2048.0f;

            // Skip extra value
            if ((flags & 0x40000) != 0)
            {
                // Skip 4 bytes if flags & 0x40000
                br.ReadBytes(4);
            }

            // Tangent
            Tangent.X = 2 * br.ReadByte() / 255.0f - 1;
            Tangent.Y = 2 * br.ReadByte() / 255.0f - 1;
            Tangent.Z = 2 * br.ReadByte() / 255.0f - 1;
            Tangent.W = br.ReadByte() / 255.0f;
        }
        public Vertex() { }

        #region ISerializable Members

        public void Queue(List<ISerializable> queue)
        {
            queue.Add(this);
        }

        public void Write(FileStream fs, BinaryWriter bw, List<Tag> taglist)
        {
            if (taglist.Count == 0 || taglist[taglist.Count - 1].Id != "__8U")
            {
                taglist.Add(new Tag("__8U", (Int32)fs.Position, 1, 0));
            }
            else
            {
                Tag t = taglist[taglist.Count - 1];
                t.NumEntries++;
                taglist[taglist.Count - 1] = t;
            }

            // Position vector
            bw.Write(Position.X);
            bw.Write(Position.Y);
            bw.Write(Position.Z);

            // Bones
            foreach (Double weight in BoneWeight)
            {
                bw.Write((Byte)(weight * Byte.MaxValue));
            }
            foreach (UInt32 index in BoneIndex)
            {
                bw.Write((Byte)index);
            }

            // Normal
            bw.Write((Byte)(Byte.MaxValue * (Normal.X + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * (Normal.Y + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * (Normal.Z + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * Normal.W));

            // UV coords
            bw.Write((Int16)(UV.X * 2048.0d));
            bw.Write((Int16)(-UV.Y * 2048.0d));

            // Unknown
            if ((Flags & 0x40000) != 0)
            {
                bw.Write(0);
            }

            // Tangent
            bw.Write((Byte)(Byte.MaxValue * (Tangent.X + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * (Tangent.X + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * (Tangent.X + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * Tangent.X));
        }

        #endregion
    }

    public class Face : ISerializable
    {
        public Int16[] Vertices;

        public Face(BinaryReader br)
        {
            Vertices = new Int16[3];

            for (Int16 i = 0; i < 3; i++)
            {
                Vertices[i] = br.ReadInt16();
            }
        }
        public Face() { }

        #region ISerializable Members

        public void Queue(List<ISerializable> queue)
        {
            queue.Add(this);
        }

        public void Write(FileStream fs, BinaryWriter bw, List<Tag> taglist)
        {
            if (taglist[taglist.Count - 1].Id != "_61U")
            {
                taglist.Add(new Tag("_61U", (Int32)fs.Position, 3, 0));
            }
            else
            {
                Tag t = taglist[taglist.Count - 1];
                t.NumEntries+=3;
                taglist[taglist.Count - 1] = t;
            }

            for (Int32 i = 0; i < 3; i++)
            {
                bw.Write(Vertices[i]);
            }
        }

        #endregion
    }

    public class Bone
    {
        public String Name;
        public UInt32 Flags;
        public Int16 Parent;
        public Vector3D InitialPosition;
        public AnimRef AnimatedPosition;
        public QuaternionD InitialRotation;
        public AnimRef AnimatedRotation;
        public Vector3D InitialScale;
        public AnimRef AnimatedScale;

        public static Bone ReadBone(FileStream fs, BinaryReader br, List<Tag> tags)
        {
            Bone b = new Bone();
            Int64 lCurrentPos;

            // Skip first integer
            br.ReadBytes(4);

            // Name
            TagRef refName = new TagRef(br);
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
            b.AnimatedPosition = new AnimRef(br);

            // Position
            b.InitialPosition.X = br.ReadSingle();
            b.InitialPosition.Y = br.ReadSingle();
            b.InitialPosition.Z = br.ReadSingle();

            // Skip data
            br.ReadBytes(0x18);

            // Rotation
            b.InitialRotation.X = br.ReadSingle();
            b.InitialRotation.Y = br.ReadSingle();
            b.InitialRotation.Z = br.ReadSingle();
            b.InitialRotation.W = br.ReadSingle();

            // Skip data
            br.ReadBytes(0x1C);

            // Scale
            b.InitialScale.X = br.ReadSingle();
            b.InitialScale.Y = br.ReadSingle();
            b.InitialScale.Z = br.ReadSingle();

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

        public Geoset(BinaryReader br)
        {
            Type = br.ReadInt32();
            StartVertex = br.ReadUInt16();
            NumVertices = br.ReadUInt16();
            StartTriangle = br.ReadInt32() / 3;
            NumTriangles = br.ReadInt32() / 3;

            // Skip a lot of data
            br.ReadBytes(0x0C);
        }
        public Geoset() { }
        
        public void Write(BinaryWriter bw)
        {
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
            TagRef refName = new TagRef(br);

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
                TagRef refLayer = new TagRef(br);

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
            TagRef refName = new TagRef(br);
            
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
}
