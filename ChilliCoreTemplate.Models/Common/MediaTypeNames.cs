using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;

namespace ChilliCoreTemplate.Models
{
    //System.Net.Mime.MediaTypeNames
    public static class MyMediaTypeNames
    {
        public static class Text
        {
            public static string Csv = "text/csv";
        }
        public static class Application
        {
            public static string ExcelX = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }
    }

}
