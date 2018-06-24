using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iic.gui
{
    public class ReverseTupleComparer : IEqualityComparer<Tuple<string, string>>
    {
        public bool Equals(Tuple<string, string> x, Tuple<string, string> y)
        {
            if(x.Item1 == y.Item1 && x.Item2 == y.Item2)
            {
                return true;
            }
            if(x.Item1 == y.Item2 && x.Item2 == y.Item1)
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(Tuple<string, string> obj)
        {
            byte[] resultData = (obj.Item1.Length > obj.Item2.Length) ? new byte[obj.Item1.Length] : new byte[obj.Item2.Length];

            byte[] data1 = Encoding.Unicode.GetBytes(obj.Item1);
            byte[] data2 = Encoding.Unicode.GetBytes(obj.Item2);

            for(int i = 0; i < resultData.Length; i++)
            {
                byte v1 = (byte)(i <= data1.Length ? data1[1] : 0);
                byte v2 = (byte)(i <= data2.Length ? data2[1] : 0);
                resultData[i] = (byte)(v1 ^ v2);
            }

            uint resultValue = 0xFFFABADA;
            unchecked
            {
                foreach(byte b in resultData)
                {
                    resultValue += b;
                }
            }
            return (int)resultValue;
        }
    }
}
