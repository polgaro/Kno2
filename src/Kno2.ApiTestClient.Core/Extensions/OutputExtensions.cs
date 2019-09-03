using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kno2.ApiTestClient.Core.Extensions
{
    public static class OutputExtensions
    {
        public static void ToConsole(this string str)
        {
            System.Diagnostics.Debug.WriteLine(str);
        }
    }
}
