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

            node = doc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
            doc.AppendChild(node);

            // Root node
            XmlElement root = doc.CreateElement("COLLADA");
            doc.AppendChild(root);

            // Asset
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
        public static void BuildAsset(XmlDocument doc, String source)
        {
            XmlNode root = doc.SelectSingleNode("/COLLADA");
            XmlElement assetElem = doc.CreateElement("asset");
            root.AppendChild(assetElem);

            XmlElement contributorElem = doc.CreateElement("contributor");
            assetElem.AppendChild(contributorElem);

            // Authoring tool
            XmlElement toolElem = doc.CreateElement("authoring_tool");
            XmlText toolText = doc.CreateTextNode("libm3");

            contributorElem.AppendChild(toolElem);
            toolElem.AppendChild(toolText);

            // Source data
            XmlElement sourceElem = doc.CreateElement("source_data");
            XmlText sourceText = doc.CreateTextNode(source);

            contributorElem.AppendChild(sourceElem);
            sourceElem.AppendChild(sourceText);
        }
        private static void BuildVertexSource(XmlDocument doc, KeyValuePair<String, Geoset> geoset, List<Vertex> vertices)
        {
            XmlElement sourceElem;
            XmlElement arrayElem;
            XmlText arrayValues;
            XmlElement tech_common;
            XmlElement accessElem;
            XmlElement param;
            XmlElement vertsElem;
            XmlElement inputElem;

            XmlNode root = doc.SelectSingleNode("/COLLADA/library_geometries/geometry[@id='" + geoset.Key + "']/mesh");

            Int32 first = geoset.Value.StartVertex;
            Int32 count = geoset.Value.NumVertices;
            Int32 last  = first + count;

            // Position
            sourceElem = doc.CreateElement("source");
            sourceElem.SetAttribute("id", geoset.Key + "_position");
            
            arrayElem = doc.CreateElement("float_array");
            arrayElem.SetAttribute("id", geoset.Key + "_position-array");
            arrayElem.SetAttribute("count", Convert.ToString(count*3));

            arrayValues = doc.CreateTextNode("\r\n");

            tech_common = doc.CreateElement("technique_common");

            accessElem = doc.CreateElement("accessor");
            accessElem.SetAttribute("source", "#" + arrayElem.GetAttribute("id"));
            accessElem.SetAttribute("count", Convert.ToString(count));
            accessElem.SetAttribute("stride", "3");

            param = doc.CreateElement("param");
            param.SetAttribute("name", "X");
            param.SetAttribute("type", "float");

            accessElem.AppendChild(param);

            param = doc.CreateElement("param");
            param.SetAttribute("name", "Y");
            param.SetAttribute("type", "float");

            accessElem.AppendChild(param);

            param = doc.CreateElement("param");
            param.SetAttribute("name", "Z");
            param.SetAttribute("type", "float");

            accessElem.AppendChild(param);

            root.AppendChild(sourceElem);
            sourceElem.AppendChild(arrayElem);
            arrayElem.AppendChild(arrayValues);
            sourceElem.AppendChild(tech_common);
            tech_common.AppendChild(accessElem);

            for (Int32 i = first; i < last; i++)
            {
                arrayValues.AppendData(Convert.ToString(vertices[i].Position.X) + " ");
                arrayValues.AppendData(Convert.ToString(vertices[i].Position.Y) + " ");
                arrayValues.AppendData(Convert.ToString(vertices[i].Position.Z) + "\r\n");
            }

            // Normal
            sourceElem = doc.CreateElement("source");
            sourceElem.SetAttribute("id", geoset.Key + "_normal");
            
            arrayElem = doc.CreateElement("float_array");
            arrayElem.SetAttribute("id", geoset.Key + "_normal-array");
            arrayElem.SetAttribute("count", Convert.ToString(count*3));

            arrayValues = doc.CreateTextNode("\r\n");

            tech_common = doc.CreateElement("technique_common");

            accessElem = doc.CreateElement("accessor");
            accessElem.SetAttribute("source", "#" + arrayElem.GetAttribute("id"));
            accessElem.SetAttribute("count", Convert.ToString(count));
            accessElem.SetAttribute("stride", "3");

            param = doc.CreateElement("param");
            param.SetAttribute("name", "X");
            param.SetAttribute("type", "float");

            accessElem.AppendChild(param);

            param = doc.CreateElement("param");
            param.SetAttribute("name", "Y");
            param.SetAttribute("type", "float");

            accessElem.AppendChild(param);

            param = doc.CreateElement("param");
            param.SetAttribute("name", "Z");
            param.SetAttribute("type", "float");

            accessElem.AppendChild(param);

            root.AppendChild(sourceElem);
            sourceElem.AppendChild(arrayElem);
            arrayElem.AppendChild(arrayValues);
            sourceElem.AppendChild(tech_common);
            tech_common.AppendChild(accessElem);

            for (Int32 i = first; i < last; i++)
            {
                arrayValues.AppendData(Convert.ToString(vertices[i].Position.X) + " ");
                arrayValues.AppendData(Convert.ToString(vertices[i].Position.Y) + " ");
                arrayValues.AppendData(Convert.ToString(vertices[i].Position.Z) + "\r\n");
            }

            // UV
            sourceElem = doc.CreateElement("source");
            sourceElem.SetAttribute("id", geoset.Key + "_uv");
            
            arrayElem = doc.CreateElement("float_array");
            arrayElem.SetAttribute("id", geoset.Key + "_uv-array");
            arrayElem.SetAttribute("count", Convert.ToString(count*2));

            arrayValues = doc.CreateTextNode("\r\n");

            tech_common = doc.CreateElement("technique_common");

            accessElem = doc.CreateElement("accessor");
            accessElem.SetAttribute("source", "#" + arrayElem.GetAttribute("id"));
            accessElem.SetAttribute("count", Convert.ToString(count));
            accessElem.SetAttribute("stride", "2");

            param = doc.CreateElement("param");
            param.SetAttribute("name", "S");
            param.SetAttribute("type", "float");

            accessElem.AppendChild(param);

            param = doc.CreateElement("param");
            param.SetAttribute("name", "T");
            param.SetAttribute("type", "float");

            accessElem.AppendChild(param);

            root.AppendChild(sourceElem);
            sourceElem.AppendChild(arrayElem);
            arrayElem.AppendChild(arrayValues);
            sourceElem.AppendChild(tech_common);
            tech_common.AppendChild(accessElem);

            for (Int32 i = first; i < last; i++)
            {
                arrayValues.AppendData(Convert.ToString(vertices[i].Position.X) + " ");
                arrayValues.AppendData(Convert.ToString(vertices[i].Position.Y) + "\r\n");
            }

            // Tangent
            sourceElem = doc.CreateElement("source");
            sourceElem.SetAttribute("id", geoset.Key + "_tangent");
            
            arrayElem = doc.CreateElement("float_array");
            arrayElem.SetAttribute("id", geoset.Key + "_tangent-array");
            arrayElem.SetAttribute("count", Convert.ToString(count*3));

            arrayValues = doc.CreateTextNode("\r\n");

            tech_common = doc.CreateElement("technique_common");

            accessElem = doc.CreateElement("accessor");
            accessElem.SetAttribute("source", "#" + arrayElem.GetAttribute("id"));
            accessElem.SetAttribute("count", Convert.ToString(count));
            accessElem.SetAttribute("stride", "3");

            param = doc.CreateElement("param");
            param.SetAttribute("name", "X");
            param.SetAttribute("type", "float");

            accessElem.AppendChild(param);

            param = doc.CreateElement("param");
            param.SetAttribute("name", "Y");
            param.SetAttribute("type", "float");

            accessElem.AppendChild(param);

            param = doc.CreateElement("param");
            param.SetAttribute("name", "Z");
            param.SetAttribute("type", "float");

            accessElem.AppendChild(param);

            root.AppendChild(sourceElem);
            sourceElem.AppendChild(arrayElem);
            arrayElem.AppendChild(arrayValues);
            sourceElem.AppendChild(tech_common);
            tech_common.AppendChild(accessElem);

            for (Int32 i = first; i < last; i++)
            {
                arrayValues.AppendData(Convert.ToString(vertices[i].Position.X) + " ");
                arrayValues.AppendData(Convert.ToString(vertices[i].Position.Y) + " ");
                arrayValues.AppendData(Convert.ToString(vertices[i].Position.Z) + "\r\n");
            }

            // <vertices>
            vertsElem = doc.CreateElement("vertices");
            vertsElem.SetAttribute("id", geoset.Key + "_vertex");

            root.AppendChild(vertsElem);

            // <vertices> : <input>
            inputElem = doc.CreateElement("input");
            inputElem.SetAttribute("source", "#" + geoset.Key + "_position");
            inputElem.SetAttribute("semantic", "POSITION");

            vertsElem.AppendChild(inputElem);
        }
        private static void BuildTriangles(XmlDocument doc, KeyValuePair<String, Geoset> geoset, List<Face> triangles)
        {
            XmlElement triElem;
            XmlElement triEntryElem;
            XmlText triValues;
            XmlElement inputElem;

            XmlNode root = doc.SelectSingleNode("/COLLADA/library_geometries/geometry[@id='" + geoset.Key + "']/mesh");

            Int32 first = geoset.Value.StartTriangle;
            Int32 count = geoset.Value.NumTriangles;
            Int32 last = first + count;

            // <triangles>
            triElem = doc.CreateElement("triangles");
            triElem.SetAttribute("count", Convert.ToString(count));

            root.AppendChild(triElem);

            // <vertices> : <input>
            inputElem = doc.CreateElement("input");
            inputElem.SetAttribute("source", "#" + geoset.Key + "_vertex");
            inputElem.SetAttribute("semantic", "VERTEX");
            inputElem.SetAttribute("offset", "0");

            triElem.AppendChild(inputElem);
            /*
            inputElem = doc.CreateElement("input");
            inputElem.SetAttribute("source", "#" + geoset.Key + "_normal");
            inputElem.SetAttribute("semantic", "NORMAL");
            inputElem.SetAttribute("offset", "1");

            triElem.AppendChild(inputElem);

            inputElem = doc.CreateElement("input");
            inputElem.SetAttribute("source", "#" + geoset.Key + "_uv");
            inputElem.SetAttribute("semantic", "TEXCOORD");
            inputElem.SetAttribute("offset", "2");

            triElem.AppendChild(inputElem);
            
            inputElem = doc.CreateElement("input");
            inputElem.SetAttribute("source", "#" + geo.Key + "_tangent");
            inputElem.SetAttribute("semantic", "TEXTANGENT");
            inputElem.SetAttribute("offset", "3");
            
            triElem.AppendChild(inputElem);
            */
            triEntryElem = doc.CreateElement("p");
            triValues = doc.CreateTextNode("\r\n");

            triEntryElem.AppendChild(triValues);
            triElem.AppendChild(triEntryElem);

            for (Int32 i = first; i < last; i++)
            {
                triValues.AppendData(Convert.ToString(triangles[i].Vertices[0] - geoset.Value.StartVertex) + " ");

                triValues.AppendData(Convert.ToString(triangles[i].Vertices[1] - geoset.Value.StartVertex) + " ");

                triValues.AppendData(Convert.ToString(triangles[i].Vertices[2] - geoset.Value.StartVertex) + "\r\n");
            }
        }
        public static void BuildGeoset(XmlDocument doc, KeyValuePair<String, Geoset> geoset, List<Vertex> vertices, List<Face> triangles)
        {
            XmlElement elGeometry;

            XmlNode root = doc.SelectSingleNode("/COLLADA/library_geometries");

            // <geometry>
            elGeometry = doc.CreateElement("geometry");
            elGeometry.SetAttribute("id", geoset.Key);

            root.AppendChild(elGeometry);

            // <mesh>
            elGeometry.AppendChild(doc.CreateElement("mesh"));
            XML.BuildVertexSource(doc, geoset, vertices);
            XML.BuildTriangles(doc, geoset, triangles);
        }
        public static void BuildVisualScene(XmlDocument doc)
        {
            XmlElement elVisualScene;
            XmlElement elNode;
            XmlElement elGeometry;

            XmlNode root = doc.SelectSingleNode("/COLLADA").AppendChild(doc.CreateElement("library_visual_scenes"));
            XmlNodeList elements;

            elVisualScene = doc.CreateElement("visual_scene");
            elVisualScene.SetAttribute("id", "RootNode");

            root.AppendChild(elVisualScene);

            elements = doc.SelectNodes("/COLLADA/library_geometries/geometry");
            foreach (XmlElement el in elements)
            {
                elNode = doc.CreateElement("node");
                elVisualScene.AppendChild(elNode);

                elGeometry = doc.CreateElement("instance_geometry");
                elGeometry.SetAttribute("url", "#" + el.GetAttribute("id"));
                elNode.AppendChild(elGeometry);
            }
        }
        public static void BuildScene(XmlDocument doc)
        {
            XmlNodeList VisualSceneList = doc.SelectNodes("/COLLADA/library_visual_scenes/visual_scene");
            XmlNode root = doc.SelectSingleNode("/COLLADA").AppendChild(doc.CreateElement("scene"));

            foreach (XmlElement el in VisualSceneList)
            {
                XmlElement elSceneNode = doc.CreateElement("instance_visual_scene");
                root.AppendChild(elSceneNode);
                elSceneNode.SetAttribute("url", "#" + el.GetAttribute("id"));
            }
        }
        
        // Extension functions
        public static XmlNode GetElementByAttribute(this XmlNode node, String tag, String attribute, String value)
        {
            return node.SelectSingleNode("/" + tag + "[@" + attribute + "=" + value + "]");
        }

        // Build geoset
        public static void Build(XmlDocument doc, Model model)
        {
            XmlElement el;
            XmlNode root = doc.SelectSingleNode("/COLLADA");

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
                geometry.AppendChild(doc.CreateElement("mesh"));
                XmlElement node = (XmlElement)doc.SelectSingleNode("/COLLADA/library_visual_scenes/visual_scene").AppendChild(doc.CreateElement("node"));
                node.SetAttribute("id", "geoset" + count + "_node");
                XmlElement instance_geometry = (XmlElement)node.AppendChild(doc.CreateElement("instance_geometry"));
                instance_geometry.SetAttribute("url", "#geoset" + count);
                count++;
            }

            XmlElement instance_visual_scene = (XmlElement)doc.SelectSingleNode("/COLLADA/scene").AppendChild(doc.CreateElement("instance_visual_scene"));
            instance_visual_scene.SetAttribute("url", "#RootNode");
        }
    }

    public class Vertex
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
