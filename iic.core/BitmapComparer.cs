using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DupImageLib;
using ImageMagick;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace iic.core
{
    public static class BitmapComparer
    {
        static readonly ImageHashes Hasher = new ImageHashes(new ImageMagickTransformer());
        static Dictionary<string, ulong[]> _diffHashCache = new Dictionary<string, ulong[]>();

        public static long GetPermutationCount()
        {
            return (long)Math.Pow(_diffHashCache.Count, 2);
        }

        public static IDictionary<Tuple<string,string>, float> CreateDiffMapFromDCTCache(int MaxCount, int startIdx)
        {
            if(_diffHashCache.Count == 0)
            {
                return new Dictionary<Tuple<string, string>, float>();
            }

            bool skippedWith = false;

            long permutations = GetPermutationCount();
            int skipAgainst = startIdx / _diffHashCache.Count;
            int skipWith = startIdx % _diffHashCache.Count;
            var result = new ConcurrentDictionary<Tuple<string, string>, float>();

            long maxAgainst = permutations / MaxCount;
            if(permutations < MaxCount)
            {
                maxAgainst = permutations;
            }
            foreach (string fileAgainst in _diffHashCache.Skip(skipAgainst).Select((kvp) => kvp.Key).Take((int)maxAgainst))
            {
                ulong[] hashAgainst = _diffHashCache[fileAgainst];
                int skipCount = 0;
                if(!skippedWith)
                {
                    skipCount = skipWith;
                    skippedWith = true;
                }

                ParallelOptions opts = new ParallelOptions();
                opts.MaxDegreeOfParallelism = (Environment.ProcessorCount - 1);

                Parallel.ForEach(_diffHashCache.Skip(skipCount + skipAgainst).Select((kvp) => kvp.Key), opts, (fileWith) =>
                {
                    if (fileAgainst == fileWith)
                    {
                        return;
                    }
                    ulong[] hashWith = _diffHashCache[fileWith];
                    float diff = ImageHashes.CompareHashes(hashAgainst, hashWith);
                    result.TryAdd(new Tuple<string, string>(fileAgainst, fileWith), diff);
                });
            }
            return result;
        }

        public static void CacheDiff(string path)
        {
            try
            {
                ulong[] dct = Hasher.CalculateDifferenceHash256(path);
                _diffHashCache.Add(path, dct);
            }
            catch(Exception)
            {
                return;
            }
        }

        public static void SaveDiffCacheToDisk(string path)
        {
            File.Delete(path);
            using (var fswriter = new StreamWriter(File.OpenWrite(path)))
            {
                foreach (string key in _diffHashCache.Keys)
                {
                    string line = GetLineFromDiffCacheEntry(key, _diffHashCache[key]);
                    fswriter.WriteLine(line);
                }
            }
        }

        public static void RestoreDiffCacheFromDisk(string path)
        {
            _diffHashCache.Clear();
            using (var freader = new StreamReader(File.OpenRead(path)))
            {
                string line = freader.ReadLine();
                while(line != null)
                {
                    var kvp = GetDiffCacheEntryFromLine(line);
                    _diffHashCache.Add(kvp.Key, kvp.Value);
                    line = freader.ReadLine();
                }
            }
        }

        private static string GetLineFromDiffCacheEntry(string key, ulong[] value)
        {
            string valuestr = "";
            foreach(ulong v in value)
            {
                valuestr += v + "||";
            }
            string result = "$:" + key + ":>>" + valuestr;
            return result;
        }

        private static KeyValuePair<string, ulong[]> GetDiffCacheEntryFromLine(string text)
        {
            string[] segments = text.Split(new string[] { ":>>" }, StringSplitOptions.RemoveEmptyEntries);

            string keyseg = segments[0];
            string key = keyseg.Split(new string[] { "$:" }, StringSplitOptions.None)[1];

            string valseg = segments[1];
            string[] valvals = valseg.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);

            ulong[] val = new ulong[valvals.Length];
            for(int i = 0; i < valvals.Length; i++)
            {
                val[i] = ulong.Parse(valvals[i]);
            }

            return new KeyValuePair<string, ulong[]>(key, val);
        }
    }
}
