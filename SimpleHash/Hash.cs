using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleHash
{
    public class Hash
    {
        public static string Calculate(string path, HashType hashType, Action<int> callBack, CancellationToken token)
        {
            var hashAlgorithm = hashType.GetAlgorithm();

            // 64 kb
            const int STREAM_BUFFER_SIZE = 4096 * 16;
            const int HASH_BUFFER_SIZE = 4096;

            var buffer = new byte[HASH_BUFFER_SIZE];
            var lastPercent = -1;

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, STREAM_BUFFER_SIZE))
            {
                var fileSize = fs.Length;
                long readed = 0;

                while (true)
                {
                    if (token.IsCancellationRequested)
                        return null;

                    var len = fs.Read(buffer, 0, buffer.Length);

                    if (readed + HASH_BUFFER_SIZE >= fileSize)
                    {
                        hashAlgorithm.TransformFinalBlock(buffer, 0, len);

                        return GetStringFromBytes(hashAlgorithm.Hash);
                    }

                    var handler = callBack;
                    if (handler != null)
                    {
                        var percent = (int)(100 * (readed / (float)fileSize));

                        if (percent != lastPercent)
                        {
                            lastPercent = percent;
                            handler(percent);
                        }
                    }

                    hashAlgorithm.TransformBlock(buffer, 0, len, null, 0);

                    readed += len;
                }
            }
        }

        private static string GetStringFromBytes(IEnumerable<byte> bytes)
        {
            return string.Join("", bytes.Select(e => e.ToString("x2")));
        }
    }
}
