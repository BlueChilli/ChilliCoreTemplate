using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChilliCoreTemplate.Models
{
    public class PdfModel
    {
        public string Filename { get; set; }

        public byte[] PdfData { get; set; }

        public string Html { get; set; }
    }
}
