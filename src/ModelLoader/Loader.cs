using libm3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            Model m = Model.ReadModel(fs, br, lstTag[head.Model.Tag].Type, lstTag);

            m.ToM3();
        }
    }
}
