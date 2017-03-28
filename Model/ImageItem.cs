using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary.Model
{
    public class ImageItem
    {
        public Guid FaceId { get; set; }
        public string FileName { get; set; }
        public string FullPath{ get; set; }
    }
}
