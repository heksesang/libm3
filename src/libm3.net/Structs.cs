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
    // Serialization
    public static class XML
    {
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
                        array.SetAttribute("id", "geoset" + count + "_position-array");
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
                        p.AppendData(Convert.ToString(model.Faces[i][0] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i][0] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i][0] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i][0] - set.StartVertex) + " ");

                        p.AppendData(Convert.ToString(model.Faces[i][1] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i][1] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i][1] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i][1] - set.StartVertex) + " ");

                        p.AppendData(Convert.ToString(model.Faces[i][2] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i][2] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i][2] - set.StartVertex) + " ");
                        p.AppendData(Convert.ToString(model.Faces[i][2] - set.StartVertex) + "\r\n");
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

    public static class Serializer
    {
        public static List<SC2List<object>> lists = new List<SC2List<object>>();

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

            types.Add(typeof(UInt16), "_61U");
            types.Add(typeof(UInt32), "_23U");
            types.Add(typeof(Int16), "_61I");
            types.Add(typeof(Int32), "_23I");
            types.Add(typeof(Byte), "__8U");
            types.Add(typeof(Char), "RAHC");

            return types[t];
        }
        public static void Write(FileStream fs, BinaryWriter bw)
        {
            List<Tag> tags = new List<Tag>();
            
            // Write each list
            foreach (SC2List<object> list in lists)
            {
                if (list.Count == 0)
                    continue;

                // Make the tag
                Tag t = new Tag(list[0].GetType(), (Int32)fs.Position, list.Count, 0);

                // Write lists
                foreach (object el in list)
                {
                    if (el as ISerializable != null)
                    {
                        (el as ISerializable).Write(fs, bw);
                    }
                    else
                    {
                        if (el.GetType() == typeof(UInt16))
                        {
                            bw.Write((UInt16)el);
                        }
                        if (el.GetType() == typeof(UInt32))
                        {
                            bw.Write((UInt32)el);
                        }
                        if (el.GetType() == typeof(Int16))
                        {
                            bw.Write((Int16)el);
                        }
                        if (el.GetType() == typeof(Int32))
                        {
                            bw.Write((Int32)el);
                        }
                        if (el.GetType() == typeof(Single))
                        {
                            bw.Write((Single)el);
                        }
                        if (el.GetType() == typeof(Byte))
                        {
                            bw.Write((Byte)el);
                        }
                        if (el.GetType() == typeof(Char))
                        {
                            bw.Write((Char)el);
                        }
                    }
                }

                // Add the tag
                tags.Add(t);

                // Add padding at end of chunk
                if (fs.Position % 0x10 != 0)
                {
                    Int32 numBytes = 0x10 - (Int32)(fs.Position % 0x10);
                    for (Int32 i = 0; i < numBytes; i++)
                    {
                        bw.Write((Byte)0xAA);
                    }
                }
            }

            // Write tags
            foreach (Tag t in tags)
            {
                t.Write(fs, bw);
            }
        }
        public static void RecursiveParse(ISerializable obj)
        {
            PropertyInfo[] props = obj.GetType().GetProperties();
            foreach (PropertyInfo p in props)
            {
                if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(SC2List<>))
                {
                    object o = p.GetValue(obj, null);
                    SC2List<object> list = (o as ISerializableList).GetSerializable();

                    lists.Add(list);

                    foreach (object el in list)
                    {
                        if(el as ISerializable != null)
                            RecursiveParse(el as ISerializable);
                    }
                }
                if (p.PropertyType == typeof(String))
                {
                    object o = p.GetValue(obj, null);
                    SC2List<object> list = new SC2List<object>();

                    foreach (Char el in o as String)
                    {
                        list.Add(el);
                    }

                    list.Add('\0');

                    lists.Add(list);
                }
            }
        }
    }

    // Base objects and interfaces
    public interface ISerializable
    {
        void Write(FileStream fs, BinaryWriter bw);
    }

    public abstract class SC2Object : ISerializable
    {
        public SC2Object() { }

        #region ISerializable Members

        public abstract void Write(FileStream fs, BinaryWriter bw);

        #endregion
    }

    public interface ISerializableList
    {
        SC2List<object> GetSerializable();
    }

    public class SC2List<T> : List<T>, ISerializableList
    {
        public SC2List() { }

        #region ISerializableList Members

        public SC2List<object> GetSerializable()
        {
            SC2List<object> list = new SC2List<object>();
            
            foreach (object obj in this)
            {
                list.Add(obj);
            }

            return list;
        }

        #endregion
    }

    // Main file
    public class M3 : SC2Object
    {
        public SC2List<MD33> Header { get; set; }
        public SC2List<Model> Model { get; set; }

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            throw new NotImplementedException();
        }
    }

    // Chunks
    public class MD33 : SC2Object
    {
        private Char[] _magic = "33DM".ToCharArray();
        public Char[] Magic
        {
            get
            {
                return _magic;
            }
        }
        private Int32 _ofsrefs = 0;
        public Int32 OfsRefs
        {
            get
            {
                return _ofsrefs;
            }
            set
            {
                _ofsrefs = value;
            }
        }
        private Int32 _numrefs = 0;
        public Int32 NumRefs
        {
            get
            {
                return _numrefs;
            }
            set
            {
                _numrefs = value;
            }
        }
        private TagRef _model = new TagRef(1, 1);
        public TagRef Model
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
            }
        }

        public MD33(BinaryReader br)
        {
            if (Encoding.ASCII.GetString(br.ReadBytes(4)) != new String(Magic))
                throw new System.Exception("Incorrect file header.");
            OfsRefs = br.ReadInt32();
            NumRefs = br.ReadInt32();
            Model = new TagRef(br);
        }
        public MD33(Int32 ofsRefs, Int32 numRefs, TagRef model)
        {
            OfsRefs = ofsRefs;
            NumRefs = numRefs;
            Model = model;
        }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            bw.Write(Magic);
            bw.Write(OfsRefs);
            bw.Write(NumRefs);
            bw.Write(Model.NumEntries);
            bw.Write(Model.Tag);
        }

        #endregion
    }

    public class Model : SC2Object
    {
        private String _name = "";
        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        private UInt32 _version = 3411;
        public UInt32 Version
        {
            get
            {
                return _version;
            }
            set
            {
                _version = value;
            }
        }
        private UInt32 _flags = 0;
        public UInt32 Flags
        {
            get
            {
                return _flags;
            }
            set
            {
                _flags = value;
            }
        }
        private SC2List<Vertex> _vertices = new SC2List<Vertex>();
        public SC2List<Vertex> Vertices
        {
            get
            {
                return _vertices;
            }
            private set
            {
                _vertices = value;
            }
        }
        private SC2List<Face> _faces = new SC2List<Face>();
        public SC2List<Face> Faces
        {
            get
            {
                return _faces;
            }
            private set
            {
                _faces = value;
            }
        }
        private SC2List<Bone> _bones = new SC2List<Bone>();
        public SC2List<Bone> Bones
        {
            get
            {
                return _bones;
            }
            private set
            {
                _bones = value;
            }
        }
        private SC2List<UInt16> _boneList = new SC2List<UInt16>();
        public SC2List<UInt16> BoneList
        {
            get
            {
                return _boneList;
            }
            private set
            {
                _boneList = value;
            }
        }
        private SC2List<Geometry> _geometry = new SC2List<Geometry>();
        public SC2List<Geometry> Geometry
        {
            get
            {
                return _geometry;
            }
            private set
            {
                _geometry = value;
            }
        }
        private SC2List<Geoset> _geosets = new SC2List<Geoset>();
        public SC2List<Geoset> Geosets
        {
            get
            {
                return _geosets;
            }
            private set
            {
                _geosets = value;
            }
        }
        private SC2List<Material> _materials = new SC2List<Material>();
        public SC2List<Material> Materials
        {
            get
            {
                return _materials;
            }
            private set
            {
                _materials = value;
            }
        }
        private SC2List<MaterialGroup> _materialGroups = new SC2List<MaterialGroup>();
        public SC2List<MaterialGroup> MaterialGroups
        {
            get
            {
                return _materialGroups;
            }
            private set
            {
                _materialGroups = value;
            }
        }
        private Vector3F[] _extents = new Vector3F[2];
        public Vector3F[] Extents
        {
            get
            {
                return _extents;
            }
            private set
            {
                _extents = value;
            }
        }
        private Double _radius = 0.0;
        public Double Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
            }
        }

        public Model(FileStream fs, BinaryReader br, UInt32 type, List<Tag> tags)
        {
            List<Int64> lstPos = new List<Int64>();

            // Read name
            TagRef refName = new TagRef(br);

            lstPos.Add(fs.Position);
            fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);

            Name = Encoding.ASCII.GetString(br.ReadBytes(refName.NumEntries - 1));

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Read version
            Version = br.ReadUInt32();

            // Skip a lot of data
            br.ReadBytes(0x2C);

            // Read bones
            TagRef refBone = new TagRef(br);

            lstPos.Add(fs.Position);
            fs.Seek(tags[refBone.Tag].Offset, SeekOrigin.Begin);

            for (int i = 0; i < refBone.NumEntries; i++ )
            {
                Bones.Add(new Bone(fs, br, tags));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Skip an integer
            br.ReadBytes(4);

            // Read flags
            Flags = br.ReadUInt32();

            // Read vertices
            TagRef refVertex = new TagRef(br);
            
            lstPos.Add(fs.Position);
            fs.Seek(tags[refVertex.Tag].Offset, SeekOrigin.Begin);

            while (tags[refVertex.Tag].Offset + tags[refVertex.Tag].NumEntries != fs.Position)
            {
                Vertices.Add(new Vertex(br, Flags));
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

            for (int i = 0; i < refFace.NumEntries; i+=3)
            {
                Faces.Add(new Face(br));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            TagRef refGeoset = new TagRef(br);
            lstPos.Add(fs.Position);
            fs.Seek(tags[refGeoset.Tag].Offset, SeekOrigin.Begin);

            for (int i = 0; i < refGeoset.NumEntries; i++)
            {
                Geosets.Add(new Geoset(br));
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

                Geosets[geoid].MaterialGroup = matid;
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Bone lookup
            TagRef refBoneLookup = new TagRef(br);

            lstPos.Add(fs.Position);
            fs.Seek(tags[refBoneLookup.Tag].Offset, SeekOrigin.Begin);

            for (int i = 0; i < refBoneLookup.NumEntries; i++)
            {
                UInt16 bone = br.ReadUInt16();
                BoneList.Add(bone);
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Read vertex extents
            for (Int32 i = 0; i < 2; i++)
            {
                Extents[i].X = br.ReadSingle();
                Extents[i].Y = br.ReadSingle();
                Extents[i].Z = br.ReadSingle();
            }
            Radius = br.ReadSingle();

            // Skip a lot of data
            br.ReadBytes(0x5C);

            if (type == 23)
                br.ReadBytes(0x08);

            // Materials
            TagRef refMatGroups = new TagRef(br);

            lstPos.Add(fs.Position);
            fs.Seek(tags[refMatGroups.Tag].Offset, SeekOrigin.Begin);

            for (Int32 i = 0; i < refMatGroups.NumEntries; i++)
            {
                MaterialGroups.Add(new MaterialGroup(fs, br));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            TagRef refMats = new TagRef(br);
            
            lstPos.Add(fs.Position);
            fs.Seek(tags[refMats.Tag].Offset, SeekOrigin.Begin);

            for (Int32 i = 0; i < refMats.NumEntries; i++)
            {
                Materials.Add(new Material(fs, br, tags));
            }

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);
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
            FileStream fs = new FileStream("D:\\test.m3", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            
            Tag tag;
            TagRef tagref;

            M3 m = new M3();

            m.Header = new SC2List<MD33>();
            m.Header.Add(new MD33(0x20, 1, new TagRef(0, 0)));
            m.Model = new SC2List<Model>();
            m.Model.Add(this);

            Serializer.RecursiveParse(m);
            Serializer.Write(fs, bw);
        }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {

        }

        #endregion
    }

    public class Vertex : SC2Object
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

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {

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

    public class Face : SC2Object
    {
        private Int16[] _vertices = new Int16[3];
        public Int16 this[int index]
        {
            get
            {
                return _vertices[index];
            }

            set
            {
                _vertices[index] = value;
            }
        }

        public Face(BinaryReader br)
        {
            for (Int16 i = 0; i < 3; i++)
            {
                _vertices[i] = br.ReadInt16();
            }
        }
        public Face() { }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            for (Int32 i = 0; i < 3; i++)
            {
                bw.Write(_vertices[i]);
            }
        }

        #endregion
    }

    public class Bone : SC2Object
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

        public Bone(FileStream fs, BinaryReader br, List<Tag> tags)
        {
            Int64 lCurrentPos;

            // Skip first integer
            br.ReadBytes(4);

            // Name
            TagRef refName = new TagRef(br);
            lCurrentPos = fs.Position;
            fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);
            Name = Encoding.ASCII.GetString(br.ReadBytes(refName.NumEntries - 1));
            fs.Seek(lCurrentPos, SeekOrigin.Begin);

            // Flags
            Flags = br.ReadUInt32();

            // Parent
            Parent = br.ReadInt16();

            // Skip data
            br.ReadBytes(0x02);

            // Translation data
            AnimatedPosition = new AnimRef(br);

            // Position
            InitialPosition.X = br.ReadSingle();
            InitialPosition.Y = br.ReadSingle();
            InitialPosition.Z = br.ReadSingle();

            // Skip data
            br.ReadBytes(0x18);

            // Rotation
            InitialRotation.X = br.ReadSingle();
            InitialRotation.Y = br.ReadSingle();
            InitialRotation.Z = br.ReadSingle();
            InitialRotation.W = br.ReadSingle();

            // Skip data
            br.ReadBytes(0x1C);

            // Scale
            InitialScale.X = br.ReadSingle();
            InitialScale.Y = br.ReadSingle();
            InitialScale.Z = br.ReadSingle();

            // Skip data
            br.ReadBytes(0x24);
        }
        public Bone() { }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
        }

        #endregion
    }

    public class Geometry : SC2Object
    {
        public List<Geoset> Geosets;

        public Geometry()
        {
            Geosets = new List<Geoset>();
        }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
        }

        #endregion
    }

    public class Geoset : SC2Object
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

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
        }

        #endregion
    }

    public class Material : SC2Object
    {
        public String Name;
        public List<Layer> Layers;
        public Vector2D Coords;

        public Material(FileStream fs, BinaryReader br, List<Tag> tags)
        {
            Layers = new List<Layer>();
            List<Int64> lstPos = new List<Int64>();

            // Name
            TagRef refName = new TagRef(br);

            lstPos.Add(fs.Position);
            fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);

            Name = Encoding.ASCII.GetString(br.ReadBytes(refName.NumEntries - 1));

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Skip data
            br.ReadBytes(0x20);

            // Coords?
            Coords.X = br.ReadSingle();
            Coords.Y = br.ReadSingle();

            // Layers
            for (Int32 i = 0; i < 13; i++)
            {
                TagRef refLayer = new TagRef(br);

                lstPos.Add(fs.Position);
                fs.Seek(tags[refLayer.Tag].Offset, SeekOrigin.Begin);

                Layers.Add(new Layer(fs, br, tags));

                fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
                lstPos.RemoveAt(lstPos.Count - 1);
            }

            // Skip a lot of data
            br.ReadBytes(0x3C);
        }
        public Material() { Layers = new List<Layer>(); }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
        }

        #endregion
    }

    public class MaterialGroup : SC2Object
    {
        public UInt32 Index;
        public UInt32 Num;

        public MaterialGroup(FileStream fs, BinaryReader br)
        {
            Num = br.ReadUInt32();
            Index = br.ReadUInt32();
        }
        public MaterialGroup() { }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
        }

        #endregion
    }

    public class Layer : SC2Object
    {
        public String Name;

        public Layer(FileStream fs, BinaryReader br, List<Tag> tags)
        {
            List<Int64> lstPos = new List<Int64>();

            // Skip unknown value
            br.ReadInt32();

            // Name
            TagRef refName = new TagRef(br);
            
            lstPos.Add(fs.Position);
            fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);

            if(tags[refName.Tag].Id == "RAHC")
                Name = Encoding.ASCII.GetString(br.ReadBytes(refName.NumEntries - 1));

            fs.Seek(lstPos[lstPos.Count - 1], SeekOrigin.Begin);
            lstPos.RemoveAt(lstPos.Count - 1);

            // Skip a lot of data
            br.ReadBytes(0x154);
        }
        public Layer() { }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
        }

        #endregion
    }


    // References
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

            types.Add(typeof(UInt16), "_61U");
            types.Add(typeof(UInt32), "_23U");
            types.Add(typeof(Int16), "_61I");
            types.Add(typeof(Int32), "_23I");
            types.Add(typeof(Byte), "__8U");
            types.Add(typeof(Char), "RAHC");

            return types[t];
        }
        public static Int32 GetSize(Type t)
        {
            Dictionary<Type, Int32> types = new Dictionary<Type, Int32>();

            types.Add(typeof(MD33), 20);
            types.Add(typeof(Model), 0);
            types.Add(typeof(Vertex), 1);
            types.Add(typeof(Face), 2);
            types.Add(typeof(Bone), 156);
            types.Add(typeof(Geoset), 28);
            types.Add(typeof(Material), 212);
            types.Add(typeof(MaterialGroup), 8);
            types.Add(typeof(Geometry), 32);

            types.Add(typeof(UInt16), 2);
            types.Add(typeof(UInt32), 4);
            types.Add(typeof(Int16), 2);
            types.Add(typeof(Int32), 4);
            types.Add(typeof(Byte), 1);
            types.Add(typeof(Char), 1);

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
