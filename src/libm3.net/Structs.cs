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
    // Base object/interface
    public interface ISerializable
    {
        void Write(FileStream fs, BinaryWriter bw);
    }

    public abstract class SC2Object : ISerializable
    {
        public virtual int ElementCount { get { return 1; } }
        public UInt32 Type = 0;

        public SC2Object() { }

        #region ISerializable Members

        public abstract void Write(FileStream fs, BinaryWriter bw);

        #endregion
    }

    // Primitive objects
    public class SC2UInt16 : SC2Object
    {
        public UInt16 Value { get; set; }

        public SC2UInt16(UInt16 value) { Value = value; }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            bw.Write(Value);
        }

        #endregion
    }

    public class SC2UInt32 : SC2Object
    {
        public UInt32 Value { get; set; }

        public SC2UInt32(UInt32 value) { Value = value; }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            bw.Write(Value);
        }

        #endregion
    }
    public class SC2Int16 : SC2Object
    {
        public Int16 Value { get; set; }

        public SC2Int16(Int16 value) { Value = value; }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            bw.Write(Value);
        }

        #endregion
    }
    public class SC2Int32 : SC2Object
    {
        public Int32 Value { get; set; }

        public SC2Int32(Int32 value) { Value = value; }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            bw.Write(Value);
        }

        #endregion
    }
    public class SC2Byte : SC2Object
    {
        public Byte Value { get; set; }

        public SC2Byte(Byte value) { Value = value; }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            bw.Write(Value);
        }

        #endregion
    }
    public class SC2Char : SC2Object
    {
        public Char Value { get; set; }

        public SC2Char(Char value) { Value = value; }

        #region ISerializable Members

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            bw.Write(Value);
        }

        #endregion
    }

    // List/interface
    public interface ISerializableList
    {
        SC2List<object> GetList();
        TagRef GetRef();
    }

    public class SC2List<T> : List<T>, ISerializableList
    {
        public int ElementCount
        {
            get
            {
                int count = 0;
                foreach (object o in this)
                    if (o as SC2Object != null)
                        count += (o as SC2Object).ElementCount;

                return count;
            }
        }

        public SC2List() { }

        #region ISerializableList Members

        virtual public SC2List<object> GetList()
        {
            SC2List<object> list = new SC2List<object>();
            
            foreach (object obj in this)
            {
                list.Add(obj);
            }            

            return list;
        }

        virtual public TagRef GetRef()
        {
            Int32 count = 0;
            foreach (SC2List<object> list in Serializer.Lists)
            {
                if (list.Count == 0)
                    continue;

                if (Count != 0 && list[0] == this[0] as object)
                {
                    TagRef t = new TagRef();

                    t.NumEntries = list.ElementCount;
                    t.Tag = count;

                    return t;
                }
                count++;
            }

            return new TagRef(0, 0);
        }

        #endregion
    }

    // String
    public class SC2String : SC2List<SC2Char>
    {
        public SC2Char[] Array
        {
            get
            {
                return ToArray();
            }
        }
        public Char[] CharArray
        {
            get
            {
                Char[] arr = new Char[Count];

                Int32 count = 0;
                foreach (SC2Char ch in this)
                {
                    arr[count] = ch.Value;
                    count++;
                }

                return arr;
            }
        }
        public String String
        {
            get
            {
                return new String(CharArray);
            }
        }
        public Int32 Length
        {
            get
            {
                return Count;
            }
        }

        public SC2String() { }
        public SC2String(String str) { Assign(str); }

        public void Assign(String str)
        {
            Clear();
            foreach (Char ch in str)
            {
                Add(new SC2Char(ch));
            }
            Add(new SC2Char('\0'));
        }
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

    // Objects
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
        private SC2String _name = new SC2String("");
        public SC2String Name
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

        private UInt32 _nVisibleBones = 0;
        public UInt32 NumVisibleBones
        {
            get
            {
                return _nVisibleBones;
            }
            set
            {
                _nVisibleBones = value;
            }
        }

        private SC2List<SC2UInt16> _boneList = new SC2List<SC2UInt16>();
        public SC2List<SC2UInt16> BoneList
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

        private Single _radius = 0.0f;
        public Single Radius
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
            Type = type;

            TagRef refName;
            TagRef refVerts;
            TagRef refBones;
            TagRef refBoneList;
            TagRef refGeo;
            TagRef refAtt;
            TagRef refAttList;
            TagRef refLights;
            TagRef refSHBX;
            TagRef refCam;
            TagRef refUnk;
            TagRef refMats;
            TagRef refMatM;

            // Read name ref
            refName = new TagRef(br);

            // Read version
            Version = br.ReadUInt32();

            // Skip SEQS
            br.ReadBytes(0x08);

            // Skip STC
            br.ReadBytes(0x08);

            // Skip STG
            br.ReadBytes(0x08);

            // Unknown #1
            br.ReadBytes(4);

            // Unknown #2
            br.ReadBytes(4);

            // Unknown #3
            br.ReadBytes(4);

            // Skip STS
            br.ReadBytes(0x08);

            // Read bone ref
            refBones = new TagRef(br);

            // Number of visible bones
            NumVisibleBones = br.ReadUInt32();

            // Read flags
            Flags = br.ReadUInt32();

            // Disable the extra 4 bytes in vertices
            Flags &= 0x40000;

            // Read vertices
            refVerts = new TagRef(br);

            // Geometry
            refGeo = new TagRef(br);

            // Bone lookup
            refBoneList = new TagRef(br);

            // Read vertex extents
            for (Int32 i = 0; i < 2; i++)
            {
                Extents[i].X = br.ReadSingle();
                Extents[i].Y = br.ReadSingle();
                Extents[i].Z = br.ReadSingle();
            }
            Radius = br.ReadSingle();

            // Unknown #4
            br.ReadBytes(4);

            // Unknown #5
            br.ReadBytes(4);

            // Unknown #6
            br.ReadBytes(4);

            // Unknown #7
            br.ReadBytes(4);

            // Unknown #8
            br.ReadBytes(4);

            // Unknown #9
            br.ReadBytes(4);

            // Unknown #10
            br.ReadBytes(4);

            // Unknown #11
            br.ReadBytes(4);

            // Unknown #12
            br.ReadBytes(4);

            // Unknown #13
            br.ReadBytes(4);

            // Unknown #14
            br.ReadBytes(4);

            // Unknown #15
            br.ReadBytes(4);

            // Unknown #16
            br.ReadBytes(4);

            // Attachment reference
            refAtt = new TagRef(br);

            // Attachment list reference
            refAttList = new TagRef(br);

            // Light reference
            refLights = new TagRef(br);

            // SHBX reference
            if (Type == 23)
                refSHBX = new TagRef(br);

            // Camera reference
            refCam = new TagRef(br);

            // Unknown reference #1
            refUnk = new TagRef(br);

            // Materials
            refMatM = new TagRef(br);
            refMats = new TagRef(br);

            // Parse references
            long lPos = fs.Position;

            if (refName.NumEntries != 0)
            {
                fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);
                Name.Assign( Encoding.ASCII.GetString(br.ReadBytes(refName.NumEntries - 1)) );
            }

            if (refBones.NumEntries != 0)
            {
                fs.Seek(tags[refBones.Tag].Offset, SeekOrigin.Begin);
                for (int i = 0; i < refBones.NumEntries; i++)
                {
                    Bones.Add(new Bone(fs, br, tags));
                }
            }

            if (refVerts.NumEntries != 0)
            {
                fs.Seek(tags[refVerts.Tag].Offset, SeekOrigin.Begin);
                while (tags[refVerts.Tag].Offset + tags[refVerts.Tag].NumEntries > fs.Position)
                {
                    Vertices.Add(new Vertex(br, Flags));
                }
            }

            if (refGeo.NumEntries != 0)
            {
                fs.Seek(tags[refGeo.Tag].Offset, SeekOrigin.Begin);
                for (int i = 0; i < refGeo.NumEntries; i++)
                {
                    Geometry geo = new Geometry(fs, br, tags);
                    Geometry.Add(geo);
                }
            }

            if (refBoneList.NumEntries != 0)
            {
                fs.Seek(tags[refBoneList.Tag].Offset, SeekOrigin.Begin);
                for (int i = 0; i < refBoneList.NumEntries; i++)
                {
                    UInt16 bone = br.ReadUInt16();
                    BoneList.Add(new SC2UInt16(bone));
                }
            }

            if (refMatM.NumEntries != 0)
            {
                fs.Seek(tags[refMatM.Tag].Offset, SeekOrigin.Begin);
                for (Int32 i = 0; i < refMatM.NumEntries; i++)
                {
                    MaterialGroups.Add(new MaterialGroup(fs, br));
                }
            }

            if (refMats.NumEntries != 0)
            {
                fs.Seek(tags[refMats.Tag].Offset, SeekOrigin.Begin);
                for (Int32 i = 0; i < refMats.NumEntries; i++)
                {
                    Materials.Add(new Material(fs, br, tags));
                }
            }

            fs.Seek(lPos, SeekOrigin.Begin);
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
            Serializer.XML(doc, this);

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

            M3 m = new M3();

            m.Header = new SC2List<MD33>();
            m.Header.Add(new MD33(0x20, 1, new TagRef(1, 1)));
            m.Model = new SC2List<Model>();
            m.Model.Add(this);

            Serializer.RecursiveParse(m);
            Serializer.Write(fs, bw);
        }

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            // Source file
            Name.GetRef().Write(fs, bw);

            // Version
            bw.Write(Version);

            // SEQS
            new TagRef(0, 0).Write(fs, bw);

            // STC
            new TagRef(0, 0).Write(fs, bw);

            // STG
            new TagRef(0, 0).Write(fs, bw);

            // Unknown #1
            bw.Write(0);

            // Unknown #2
            bw.Write(0);

            // Unknown #3
            bw.Write(0);

            // STS
            new TagRef(0, 0).Write(fs, bw);

            // Bones
            Bones.GetRef().Write(fs, bw);

            // Unknown #4
            bw.Write(0);

            // Flags
            bw.Write(Flags);

            // Vertices
            Vertices.GetRef().Write(fs, bw);

            // Geometry
            Geometry.GetRef().Write(fs, bw);

            // Bone table
            BoneList.GetRef().Write(fs, bw);

            // Extents
            bw.Write(Extents[0].X);
            bw.Write(Extents[0].Y);
            bw.Write(Extents[0].Z);
            bw.Write(Extents[1].X);
            bw.Write(Extents[1].Y);
            bw.Write(Extents[1].Z);
            bw.Write(Radius);

            // Unknown #5
            bw.Write(0);

            // Unknown #6
            bw.Write(0);

            // Unknown #7
            bw.Write(0);

            // Unknown #8
            bw.Write(0);

            // Unknown #9
            bw.Write(0);

            // Unknown #10
            bw.Write(0);

            // Unknown #11
            bw.Write(0);

            // Unknown #12
            bw.Write(0);

            // Unknown #13
            bw.Write(0);

            // Unknown #14
            bw.Write(0);

            // Unknown #15
            bw.Write(0);

            // Unknown #16
            bw.Write(0);

            // Unknown #17
            bw.Write(0);

            // Attachments
            new TagRef(0, 0).Write(fs, bw);

            // Attachment table
            new TagRef(0, 0).Write(fs, bw);

            // Lights
            new TagRef(0, 0).Write(fs, bw);

            // SHBX
            // if(type == 23)
            //     new TagRef(0, 0).Write(fs, bw);

            // Cameras
            new TagRef(0, 0).Write(fs, bw);

            // Unknown reference #1
            new TagRef(0, 0).Write(fs, bw);

            // Material groups
            MaterialGroups.GetRef().Write(fs, bw);

            // Materials
            Materials.GetRef().Write(fs, bw);

            // DIS
            new TagRef(0, 0).Write(fs, bw);

            // CMP
            new TagRef(0, 0).Write(fs, bw);

            // TER
            new TagRef(0, 0).Write(fs, bw);

            // VOL
            new TagRef(0, 0).Write(fs, bw);

            // Unknown #18
            bw.Write(0);

            // Unknown #19
            bw.Write(0);

            // CREP
            new TagRef(0, 0).Write(fs, bw);

            // PAR
            new TagRef(0, 0).Write(fs, bw);

            // PARC
            new TagRef(0, 0).Write(fs, bw);

            // RIB
            new TagRef(0, 0).Write(fs, bw);

            // PROJ
            new TagRef(0, 0).Write(fs, bw);

            // FOR
            new TagRef(0, 0).Write(fs, bw);

            // WRP
            new TagRef(0, 0).Write(fs, bw);

            // Unknown #20
            bw.Write(0);

            // Unknown #21
            bw.Write(0);

            // PHRB
            new TagRef(0, 0).Write(fs, bw);

            // Unknown #22
            bw.Write(0);

            // Unknown #23
            bw.Write(0);

            // Unknown #24
            bw.Write(0);

            // Unknown #25
            bw.Write(0);

            // Unknown #26
            bw.Write(0);

            // Unknown #27
            bw.Write(0);

            // IKJT
            new TagRef(0, 0).Write(fs, bw);

            // Unknown #28
            bw.Write(0);

            // Unknown #29
            bw.Write(0);

            // PATU
            new TagRef(0, 0).Write(fs, bw);

            // TRGD
            new TagRef(0, 0).Write(fs, bw);

            // IREF
            new TagRef(0, 0).Write(fs, bw);

            // Unknown reference #2
            new TagRef(0, 0).Write(fs, bw);

            // Floats
            for (Int32 i = 0; i < 23; i++)
            {
                bw.Write(1.0f);
            }

            // SSGS
            new TagRef(0, 0).Write(fs, bw);

            // ATVL
            new TagRef(0, 0).Write(fs, bw);

            // Unknown reference #3
            // if(type == 23)
            //     new TagRef(0, 0).Write(fs, bw);

            // Unknown reference #4
            // if(type == 23)
            //     new TagRef(0, 0).Write(fs, bw);

            // BBSC
            new TagRef(0, 0).Write(fs, bw);

            // TDM
            new TagRef(0, 0).Write(fs, bw);

            // Unknown #30
            bw.Write(0);

            // Unknown #31
            bw.Write(0);

            // Unknown #32
            bw.Write(0);
        }
    }

    public class Vertex : SC2Object
    {
        public Vector3F Position;
        public Vector4F Normal;
        public Vector2F UV;
        public Vector4F Tangent;
        public UInt32[] BoneIndex;
        public Single[] BoneWeight;

        public override int ElementCount { get { return 32; } }

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

            // Tangent
            bw.Write((Byte)(Byte.MaxValue * (Tangent.X + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * (Tangent.X + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * (Tangent.X + 1) / 2));
            bw.Write((Byte)(Byte.MaxValue * Tangent.X));
        }
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
            TagRef refName;

            // Skip first integer
            br.ReadBytes(4);

            // Name
            refName = new TagRef(br);

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

            // Parse references
            long lPos = fs.Position;

            fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);
            Name = Encoding.ASCII.GetString(br.ReadBytes(refName.NumEntries - 1));

            fs.Seek(lPos, SeekOrigin.Begin);
        }
        public Bone() { }

        public override void Write(FileStream fs, BinaryWriter bw)
        {
        }
    }

    public class Geometry : SC2Object
    {
        private SC2List<SC2UInt16> _faces = new SC2List<SC2UInt16>();
        public SC2List<SC2UInt16> Faces
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

        private SC2List<RenderParams> _render = new SC2List<RenderParams>();
        public SC2List<RenderParams> RenderSettings
        {
            get
            {
                return _render;
            }
            private set
            {
                _render = value;
            }
        }

        private SC2List<MSEC> _msec = new SC2List<MSEC>();
        public SC2List<MSEC> MSEC
        {
            get
            {
                return _msec;
            }
            private set
            {
                _msec = value;
            }
        }

        public Geometry(FileStream fs, BinaryReader br, List<Tag> tags)
        {
            TagRef refFaces = new TagRef(br);
            TagRef refGeosets = new TagRef(br);
            TagRef refRender = new TagRef(br);
            TagRef refMSEC = new TagRef(br);

            // Read refs
            long lPos = fs.Position;

            fs.Seek(tags[refFaces.Tag].Offset, SeekOrigin.Begin);
            for (int i = 0; i < refFaces.NumEntries; i++)
            {
                SC2UInt16 face = new SC2UInt16(br.ReadUInt16());
                Faces.Add(face);
            }

            fs.Seek(tags[refGeosets.Tag].Offset, SeekOrigin.Begin);
            for (int i = 0; i < refGeosets.NumEntries; i++)
            {
                Geoset geo = new Geoset(fs, br);
                Geosets.Add(geo);
            }

            fs.Seek(tags[refRender.Tag].Offset, SeekOrigin.Begin);
            for (int i = 0; i < refRender.NumEntries; i++)
            {
                RenderParams param = new RenderParams(fs, br);
                RenderSettings.Add(param);
            }

            fs.Seek(tags[refMSEC.Tag].Offset, SeekOrigin.Begin);
            for (int i = 0; i < refMSEC.NumEntries; i++)
            {
                MSEC msec = new MSEC(fs, br);
                MSEC.Add(msec);
            }

            fs.Seek(lPos, SeekOrigin.Begin);
        }
        public Geometry() { }

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            Faces.GetRef().Write(fs, bw);
            Geosets.GetRef().Write(fs, bw);
            RenderSettings.GetRef().Write(fs, bw);
            MSEC.GetRef().Write(fs, bw);
        }
    }

    public class Geoset : SC2Object
    {
        public UInt32 Unknown;
        public UInt16 StartVertex;
        public UInt16 NumVertices;
        public UInt32 StartTriangle;
        public UInt32 NumTriangles;
        public UInt16 BoneCount;
        public UInt16 StartBone;
        public UInt16 NumBones;
        public UInt16 RootBone;
        public Byte[] Unknowns = new Byte[4];
        
        public Geoset(FileStream fs, BinaryReader br)
        {
            Unknown = br.ReadUInt32();
            StartVertex = br.ReadUInt16();
            NumVertices = br.ReadUInt16();
            StartTriangle = br.ReadUInt32();
            NumTriangles = br.ReadUInt32();
            BoneCount = br.ReadUInt16();
            StartBone = br.ReadUInt16();
            NumBones = br.ReadUInt16();
            RootBone = br.ReadUInt16();

            for (UInt32 i = 0; i < 4; i++)
            {
                Unknowns[i] = br.ReadByte();
            }
        }
        public Geoset() { }

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            bw.Write(Unknown);
            bw.Write(StartVertex);
            bw.Write(NumVertices);
            bw.Write(StartTriangle);
            bw.Write(NumTriangles);
            bw.Write(BoneCount);
            bw.Write(StartBone);
            bw.Write(NumBones);
            bw.Write(RootBone);

            foreach (Byte b in Unknowns)
            {
                bw.Write(b);
            }
        }
    }

    public class RenderParams : SC2Object
    {
        private Int16[] _params = new Int16[7];
        public Int16 this[int index]
        {
            get
            {
                return _params[index];
            }
            set
            {
                _params[index] = value;
            }
        }

        public RenderParams(FileStream fs, BinaryReader br)
        {
            for (Int32 i = 0; i < 7; i++)
            {
                _params[i] = br.ReadInt16();
            }
        }
        public RenderParams() { }

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            foreach (Int16 param in _params)
            {
                bw.Write(param);
            }
        }
    }

    public class MSEC : SC2Object
    {
        private AnimRef _extents = new AnimRef();
        public AnimRef AnimatedExtents
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
        
        private Vector3F[] _initExtents = new Vector3F[2];
        private Single _radius = 0.0f;
        public Vector3F[] InitialExtents
        {
            get
            {
                return _initExtents;
            }
            private set
            {
                _initExtents = value;
            }
        }
        public Single Radius
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

        private UInt32[] _unknowns = new UInt32[8];
        public UInt32[] Unknowns
        {
            get
            {
                return _unknowns;
            }
            private set
            {
                _unknowns = value;
            }
        }

        public MSEC(FileStream fs, BinaryReader br)
        {
            // Skip first few bytes
            br.ReadBytes(4);

            // Animated extents
            AnimatedExtents = new AnimRef(br);

            // Initial extents
            InitialExtents[0][0] = br.ReadSingle();
            InitialExtents[0][1] = br.ReadSingle();
            InitialExtents[0][2] = br.ReadSingle();
            InitialExtents[1][0] = br.ReadSingle();
            InitialExtents[1][1] = br.ReadSingle();
            InitialExtents[1][2] = br.ReadSingle();
            Radius = br.ReadSingle();

            for (Int32 i = 0; i < 8; i++)
            {
                _unknowns[i] = br.ReadUInt32();
            }
        }
        public MSEC() { }

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            throw new NotImplementedException();
        }
    }

    public class Material : SC2Object
    {
        private SC2String _name = new SC2String();
        public SC2String Name
        {
            get
            {
                return _name;
            }
            private set
            {
                _name = value;
            }
        }

        private SC2List<Layer> _layers = new SC2List<Layer>();
        public SC2List<Layer> Layers
        {
            get
            {
                return _layers;
            }
            private set
            {
                _layers = value;
            }
        }

        public Vector2F _scale = new Vector2F();
        public Vector2F Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
            }
        }

        public Material(FileStream fs, BinaryReader br, List<Tag> tags)
        {            
            TagRef refName;
            TagRef[] refLayers = new TagRef[13];

            // Name
            refName = new TagRef(br);

            // Skip data
            br.ReadBytes(0x20);

            // Coords?
            Vector2F scale = new Vector2F();
            scale.X = br.ReadSingle();
            scale.Y = br.ReadSingle();

            Scale = scale;

            // Layers
            for (Int32 i = 0; i < 13; i++)
            {
                refLayers[i] = new TagRef(br);
            }

            // Skip a lot of data
            br.ReadBytes(0x3C);

            // Parse references
            long lPos = fs.Position;

            if (refName.NumEntries != 0)
            {
                fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);
                Name.Assign(Encoding.ASCII.GetString(br.ReadBytes(refName.NumEntries - 1)));
            }

            foreach (TagRef layer in refLayers)
            {
                if (layer.NumEntries != 0)
                {
                    fs.Seek(tags[layer.Tag].Offset, SeekOrigin.Begin);
                    Layers.Add(new Layer(fs, br, tags));
                }
            }

            fs.Seek(lPos, SeekOrigin.Begin);
        }
        public Material() { }

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            throw new NotImplementedException();
        }
    }

    public class MaterialGroup : SC2Object
    {
        private UInt32 _index = 0;
        public UInt32 Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
            }
        }

        private UInt32 _num = 0;
        public UInt32 Num
        {
            get
            {
                return _num;
            }
            set
            {
                _num = value;
            }
        }

        public MaterialGroup(FileStream fs, BinaryReader br)
        {
            Num = br.ReadUInt32();
            Index = br.ReadUInt32();
        }
        public MaterialGroup() { }

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            throw new NotImplementedException();
        }
    }

    public class Layer : SC2Object
    {
        private SC2String _name = new SC2String();
        public SC2String Name
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

        public Layer(FileStream fs, BinaryReader br, List<Tag> tags)
        {
            TagRef refName;

            // Skip unknown value
            br.ReadInt32();

            // Name
            refName = new TagRef(br);
            
            // Skip a lot of data
            br.ReadBytes(0x154);

            // Parse references
            long lPos = fs.Position;

            if (refName.NumEntries != 0)
            {
                fs.Seek(tags[refName.Tag].Offset, SeekOrigin.Begin);
                Name.Assign( Encoding.ASCII.GetString(br.ReadBytes(refName.NumEntries - 1)) );
            }

            fs.Seek(lPos, SeekOrigin.Begin);
        }
        public Layer() { }

        public override void Write(FileStream fs, BinaryWriter bw)
        {
            throw new NotImplementedException();
        }
    }
}
