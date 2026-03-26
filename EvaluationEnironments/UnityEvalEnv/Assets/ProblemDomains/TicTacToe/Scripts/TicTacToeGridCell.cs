using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Problems.TicTacToe
{
    public class TicTacToeGridCell : MonoBehaviour
    {
        public Vector3Int Position { get; set; }
        public TicTacToeMarker Marker { get; set; }
    }
}
