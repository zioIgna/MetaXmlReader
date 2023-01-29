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
        private System.Runtime.Caching.MemoryCache hashesCache;
        private System.Runtime.Caching.MemoryCache connectionsCache;
        CacheItemPolicy cacheItemPolicy;
        private byte[] hash;
        public CachingSystem()
        {
            hashesCache = new System.Runtime.Caching.MemoryCache("HaschesCache");
            connectionsCache = new System.Runtime.Caching.MemoryCache("ConnectionsCache");
            cacheItemPolicy = new CacheItemPolicy() { AbsoluteExpiration = DateTime.MaxValue };
        }

        public bool shouldReadFile(string fileName)
        {
            hash = calculateMD5(fileName);
            return !hashesCache.Contains(fileName) || !filesTheSame((byte[])hashesCache.Get(fileName), hash) ;
        }

        public bool currFileIsNewOrChanged(string fileName)
        {
            hash = calculateMD5(fileName);
            Console.WriteLine("Upserted item, Key: {0} hash: {1}", fileName.Split("\\").Last(), hashToString(hash));
            return !hashesCache.Contains(fileName) || !filesTheSame((byte[])hashesCache.Get(fileName), hash);
        }

        //private bool linkedFilesChanged(string fileName)
        //{
        //    HashSet<string> linkedFiles;
        //    if(connectionsCache.GetCacheItem(fileName) != null && (linkedFiles = (HashSet<string>)connectionsCache.GetCacheItem(fileName).Value) != null)
        //    {
                
        //    }
        //}

        public HashSet<string> getLinkedFilesList(string fileName)
        {
            HashSet<string> linkedFiles = null;
            if (connectionsCache.GetCacheItem(fileName) != null)
            {
                linkedFiles = (HashSet<string>)connectionsCache.GetCacheItem(fileName).Value;
            }
            //TODO remove from here
            Console.WriteLine("Elenco dei files collegati a {0}", fileName.Split('\\').Last());
            if (linkedFiles != null)
            {
                //Console.WriteLine(String.Join(", ", linkedFiles));
                foreach (var linkedFile in linkedFiles)
                {
                    Console.Write(linkedFile.Split("\\").Last() + ", ");
                }
            }
            // to here
            return linkedFiles;
        }

        public void upsertHashToCache(string fileName)
        {
            //bool outcome = false;
            hash = calculateMD5(fileName);
            
            var cacheItem = new CacheItem(fileName, hash);
            hashesCache.Set(cacheItem, cacheItemPolicy);
            //TODO remove writeline
            Console.WriteLine("Upserted item, Key: {0} hash: {1}", fileName.Split('\\').Last(), hashToString(hash));
            //if (!cache.Contains(fileName))
            //{
            //    outcome = cache.Add(cacheItem, cacheItemPolicy);
            //}
            //else
            //{
            //    cache.Set(cacheItem, cacheItemPolicy);
            //}
            //return outcome;
        }

        private string hashToString(byte[] hash)
        {
            return BitConverter.ToString(hash);
        }

        internal void upsertLinkToCache(string fileName, string newFileName)
        {
            if (!connectionsCache.Contains(fileName))
            {
                var cacheItem = new CacheItem(fileName, new HashSet<string>() { newFileName });
                connectionsCache.Add(cacheItem, cacheItemPolicy);
            }
            else
            {
                HashSet<string> linksSet = (HashSet<string>)connectionsCache.GetCacheItem(fileName).Value;
                linksSet.Add(newFileName);
                //TODO: remove console writeline
                Console.WriteLine(String.Join(",",linksSet));
                connectionsCache.Set(new CacheItem(fileName, linksSet), cacheItemPolicy);
            }
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
            return b1.Length == b2.Length && (position == b1.Length || (b1[position] == b2[position] && filesTheSame(b1, b2, ++position)));
        }

    }
}
