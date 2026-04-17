using ChilliSource.Cloud.Core;
using System;

namespace ChilliCoreTemplate.Models;

public static class MyFileStorageHelper
{
    private static string __secret = "e62de33b9bf6416596a27a3b6fffd6a0";
    private static string salt = "c78c38301a2e4f09a7844c2a18e1ae3b";

    public static StorageEncryptionOptions GetEncryption(string filename, Guid guid)
    {
        return GetEncryption(filename, guid.ToString("N"));
    }

    public static StorageEncryptionOptions GetEncryption(string filename, string fileSalt = null)
    {
        return new StorageEncryptionOptions(__secret + filename, fileSalt ?? salt);
    }

    public static StorageEncryptionKeys GetEncryptionKeys(string filename, Guid guid)
    {
        return GetEncryptionKeys(filename, guid.ToString("N"));
    }

    public static StorageEncryptionKeys GetEncryptionKeys(string filename, string fileSalt = null)
    {
        return new StorageEncryptionOptions(__secret + filename, fileSalt ?? salt).GetKeys();
    }
}
