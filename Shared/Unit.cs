using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared
{
    [Serializable]
    public class Unit
    {
        public Unit()
        {
            SpeedCellsInSecond = 1.0f;
        }

        public Guid UnitId { get; set; }

        // Этому хорошо бы быть целочисленым и раздедиться на позицию
        // и "текущую" позицию.
        public Point Position { get; set; }

        public Point? Destination { get; set; }

        public float SpeedCellsInSecond { get; set; }
    }
}
