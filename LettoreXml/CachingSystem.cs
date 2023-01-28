using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;

namespace LettoreXml
{
    internal class CachingSystem
    {
        private System.Runtime.Caching.MemoryCache cache;
        CacheItemPolicy cacheItemPolicy;
        private byte[] hash;
        public CachingSystem()
        {
            cache = new System.Runtime.Caching.MemoryCache("ParserCache");
            cacheItemPolicy = new CacheItemPolicy() { AbsoluteExpiration = DateTime.MaxValue };
        }

        public bool shouldReadFile(string fileName)
        {
            hash = calculateMD5(fileName);
            return !cache.Contains(fileName) || !filesTheSame((byte[])cache.Get(fileName), newHash) ;
        }

        public bool upsertHashToCache(string fileName)
        {
            bool outcome = false;
            var cacheItem = new CacheItem(fileName, calculateMD5(fileName));
            if (!cache.Contains(fileName))
            {
                outcome = cache.Add(cacheItem, cacheItemPolicy);
            }
            else
            {
                cache.Set(cacheItem, cacheItemPolicy);
            }
            return outcome;
        }

        private byte[] calculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return hash;
                    //return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private byte[] fileToBytes(string filePath)
        {
            using (FileStream fsSource = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // Read the source file into a byte array.
                byte[] bytes = new byte[fsSource.Length];
                int numBytesToRead = (int)fsSource.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

                    // Break when the end of the file is reached.
                    if (n == 0)
                        break;

                    numBytesRead += n;
                    numBytesToRead -= n;
                }

                return bytes;
            }
        }

        private bool filesTheSame(byte[] b1, byte[] b2, int position = 0)
        {
            return b1.Length == b2.Length && (position == b1.Length || (b1[position] == b2[position] && filesTheSame(b1, b2, position++)));
        }

    }
}
