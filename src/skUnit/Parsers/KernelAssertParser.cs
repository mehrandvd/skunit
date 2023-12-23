using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using skUnit.Models;

namespace skUnit.Parsers
{
    public class KernelAssertParser
    {
        public static IKernelAssert Parse(string text)
        {
            return new KernelTextAssert() { Text = text };
        }
    }
}
