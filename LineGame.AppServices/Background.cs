using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LineGame.AppServices
{
    [DataContract]
    public class Background
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string FileName { get; set; }

        public Background(string name, string fileName)
        {
            Name = name;
            FileName = fileName;
        }
    }
}
