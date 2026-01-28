using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Problems.Mario
{
    public class PipeComponent : MonoBehaviour
    {
        public PipeSize PipeSize;
    }

    public enum PipeSize
    {
        Small,
        Medium,
        Large
    }
}
