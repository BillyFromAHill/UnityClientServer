using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared;

namespace UnityClientServer
{
    class World
    {
        private const int MaxWorldSize = 12;
        private const int MinWorldSize = 7;

        private const int MaxUnitCount = 5;

        private int _tickRate = 30;


        private int _worldSize;
        private Unit[] _units;

        public World()
        {
            Random rnd = new Random();

            _worldSize = rnd.Next(MinWorldSize, MaxWorldSize);

            int unitCount = rnd.Next(1, MaxUnitCount);

            _units = new Unit[unitCount];
            for (int i = 0; i < unitCount; i++)
            {
                var unit = new Unit();
                _units[i] = unit;
                unit.UnitId = Guid.NewGuid();
                unit.Position = new PointF(rnd.Next(_worldSize - 1), rnd.Next(_worldSize - 1));
            }
        }

        public WorldDescription GetCurrentDescription()
        {
            return new WorldDescription()
            {
                WorldSize = _worldSize,
                Units = _units,
            };
        }
    }
}
