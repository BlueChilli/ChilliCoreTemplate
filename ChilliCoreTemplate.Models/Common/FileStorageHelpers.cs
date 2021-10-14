using ChilliSource.Cloud.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodel.Models
{
    public static class MyFileStorageHelper
    {
        private static string __secret = "TODO_XXXXXXXXXX";
        private static string salt = "TODO_XXXXXXXXXX";

        public static StorageEncryptionOptions GetEncryption(string filename)
        {
            return new StorageEncryptionOptions(__secret + filename, salt);
        }
    }
}
