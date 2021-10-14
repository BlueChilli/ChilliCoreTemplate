using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChilliCoreTemplate.Models
{

    public static class MyServer
    {
        public static string MapPath(string path)
        {
            return Path.Combine((string)AppDomain.CurrentDomain.GetData("ContentRootPath"), path);
        }
    }
}
