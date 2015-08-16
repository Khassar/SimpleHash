using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHash
{
    public enum HashType
    {
        MD5,
        SHA1,
        SHA256,
        SHA512
    }

    public static class HashTypeExtentions
    {
        public static HashAlgorithm GetAlgorithm(this HashType type)
        {
            switch (type)
            {
                case HashType.MD5:
                    return MD5.Create();
                case HashType.SHA1:
                    return SHA1.Create();
                case HashType.SHA256:
                    return SHA256.Create();
                case HashType.SHA512:
                    return SHA512.Create();
                default:
                    throw new NotImplementedException(type.ToString());
            }
        }

        public static string GetString(this HashType type)
        {
            switch (type)
            {
                case HashType.MD5:
                    return "md5";
                case HashType.SHA1:
                    return "SHA-1";
                case HashType.SHA256:
                    return "SHA-256";
                case HashType.SHA512:
                    return "SHA-512";
                default:
                    throw new NotImplementedException(type.ToString());
            }
        }

        public static HashType GetHashTypeByString(this string str)
        {
            foreach (var type in ((HashType[])Enum.GetValues(typeof(HashType))))
            {
                if (type.GetString() == str)
                    return type;
            }

            throw new NotImplementedException(str);
        }
    }
}
