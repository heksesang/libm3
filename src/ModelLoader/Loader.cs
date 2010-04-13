using libm3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ModelLoader
{
    class Loader
    {
        static void Main(string[] args)
        {
            FileStream fs = new FileStream(args[0], FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            // Read header
            MD33 head = new MD33(br);

            // Read the tags
            fs.Seek(head.OfsRefs, SeekOrigin.Begin);
            
            List<Tag> lstTag = new List<Tag>();
            for(Int32 i = 0; i < head.NumRefs; i++)
            {
                lstTag.Add(new Tag(br));
            }

            // Read the model
            fs.Seek(lstTag[head.Model.Tag].Offset, SeekOrigin.Begin);

            Model m = new Model(fs, br, lstTag[head.Model.Tag].Type, lstTag);

            DirectoryInfo dir = new DirectoryInfo("D:\\M2\\SC2");
            FileInfo[] files = dir.GetFiles("*.m3");

            foreach (FileInfo file in files)
            {
                FileStream fs2 = new FileStream(file.FullName, FileMode.Open);
                BinaryReader br2 = new BinaryReader(fs2);

                // Read header
                MD33 head2 = new MD33(br2);

                // Read the tags
                fs2.Seek(head2.OfsRefs, SeekOrigin.Begin);
                
                List<Tag> lstTag2 = new List<Tag>();
                for(Int32 i = 0; i < head2.NumRefs; i++)
                {
                    lstTag2.Add(new Tag(br2));
                }

                // Read the model
                fs2.Seek(lstTag2[head2.Model.Tag].Offset, SeekOrigin.Begin);

                Model m2 = new Model(fs2, br2, lstTag2[head2.Model.Tag].Type, lstTag2);

                foreach (Geoset geo in m2.Geometry[0].Geosets)
                {
                    if(geo.NumBones != geo.BoneCount)
                        Console.WriteLine("NumBones != BoneCount");
                }
            }

            //m.ToM3();
        }
    }
}
