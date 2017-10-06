using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Clans.Data
{
    public class ItemData : ObjectData
    {
        public string DisplayFile;
        
        public ushort DisplayIndex;
        
        public string HexDisplayIndex
        {
            get => "0x" + DisplayIndex.ToString("X");
            set => DisplayIndex = value.StartsWith("0x") ? ushort.Parse(value.Substring(2, value.Length - 2), System.Globalization.NumberStyles.HexNumber) : ushort.Parse(value);
        }

        public override void Parse(XElement Element)
        {
            base.Parse(Element);

            DisplayFile = Element.Element("DisplayFile").Value;
            HexDisplayIndex = Element.Element("DisplayIndex").Value;
        }
    }
}
