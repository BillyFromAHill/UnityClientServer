using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Shared
{
    [Serializable]
    public class Unit
    {
        public Unit()
        {
            SpeedCellsInSecond = 1.0;
        }

        public Guid UnitId { get; set; }

        public PointF Position { get; set; }

        public Point Destination { get; set; }

        public double SpeedCellsInSecond { get; set; }
    }
}
