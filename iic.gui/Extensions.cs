using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iic.gui
{
    public static class Extensions
    {
        public static bool EndsWith(this string input, string ocurrence, bool ignoreCaps = true)
        {
            if(ocurrence.Length == 0)
            {
                return true;
            }

            int endIdx = (input.Length - ocurrence.Length);
            string ending = input.Substring(endIdx, ocurrence.Length);

            return (string.Compare(ending, ocurrence, true) == 0);
        }
    }
}
