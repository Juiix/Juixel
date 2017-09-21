using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clans.Data
{
    public class ObjectData
    {
        public ushort Type;
        
        public string HexType
        {
            get => "0x" + Type.ToString("X");
            set => Type = value.StartsWith("0x") ? ushort.Parse(value.Substring(2, value.Length - 2), NumberStyles.HexNumber) : ushort.Parse(value);
        }
        
        public string Name;
        
        public string Class;
        
        public string FileName;

        public ushort FileIndex;

        public string HexFileIndex
        {
            get => "0x" + FileIndex.ToString("X");
            set => FileIndex = value.StartsWith("0x") ? ushort.Parse(value.Substring(2, value.Length - 2), NumberStyles.HexNumber) : ushort.Parse(value);
        }

        public virtual void Parse(XElement Element)
        {
            HexType = Element.Attribute("type").Value;
            Name = Element.Attribute("name").Value;

            FileName = Element.Element("FileName").Value;
            HexFileIndex = Element.Element("FileIndex").Value;
        }
    }
}
