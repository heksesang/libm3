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
    public static class Serializer
    {
        private static List<SC2List<object>> _lists = new List<SC2List<object>>();
        public static List<SC2List<object>> Lists
        {
            get
            {
                return _lists;
            }
            private set
            {
                _lists = value;
            }
        }

        public static void Write(FileStream fs, BinaryWriter bw)
        {
            List<Tag> tags = new List<Tag>();

            // Write each list
            foreach (SC2List<object> list in Lists)
            {
                if (list.Count == 0)
                    continue;

                // Make the tag
                Tag t = new Tag(list[0].GetType(), (Int32)fs.Position, list.ElementCount, 0);

                // Write lists
                foreach (object el in list)
                {
                    if (el as ISerializable != null)
                    {
                        (el as ISerializable).Write(fs, bw);
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

            // Update the file header
            foreach (SC2List<object> list in Lists)
            {
                if (list.Count == 0)
                    continue;

                if (Tag.GetId(list[0].GetType()) == "LDOM")
                {
                    Int64 lPos = fs.Position;

                    MD33 head = new MD33((Int32)fs.Position, Lists.Count, new TagRef(1, 1));

                    fs.Seek(0, SeekOrigin.Begin);
                    head.Write(fs, bw);
                    fs.Seek(lPos, SeekOrigin.Begin);
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
                    SC2List<object> list = (o as ISerializableList).GetList();

                    Lists.Add(list);

                    foreach (object el in list)
                    {
                        if (el as ISerializable != null)
                            RecursiveParse(el as ISerializable);
                    }
                }
                if (p.PropertyType == typeof(SC2String))
                {
                    object o = p.GetValue(obj, null);
                    SC2List<object> list = new SC2List<object>();

                    foreach (SC2Char el in o as SC2String)
                    {
                        list.Add(el);
                    }

                    Lists.Add(list);
                }
            }
        }

        public static void XML(XmlDocument doc, Model model)
        {/*
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
            Thread.CurrentThread.CurrentCulture = new CultureInfo(originalCulture);*/
        }
    }
}