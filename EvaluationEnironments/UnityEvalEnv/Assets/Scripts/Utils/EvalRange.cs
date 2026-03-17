using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class EvalRange
    {
        public int Start { get; set; }
        public int End { get; set; }
        public EvalRange(int evalRangeStart, int evalRangeEnd)
        {
            Start = evalRangeStart;
            End = evalRangeEnd;
        }
    }
}