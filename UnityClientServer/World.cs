using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared;
using Shared.AStar;

namespace UnityClientServer
{
    class World
    {
        private const int MaxWorldSize = 12;
        private const int MinWorldSize = 7;

        private const int MaxUnitCount = 5;

        private int _worldSize;

        private Unit[] _units;

        private int _ticksPerSecond = 30;

        private Task _tickingTask;

        // Конечная точка маршрута может не соответствовать назначению.
        private Dictionary<Unit, RouteToPoint> _unitRoutes = new Dictionary<Unit, RouteToPoint>();

        public World(CancellationToken token)
        {
            Random rnd = new Random();

            _worldSize = rnd.Next(MinWorldSize, MaxWorldSize);

            int unitCount = rnd.Next(1, MaxUnitCount);

            _units = new Unit[unitCount];
            for (int i = 0; i < unitCount; i++)
            {
                var unit = new Unit();

                unit.UnitId = Guid.NewGuid();

                Point position = new Point(rnd.Next(_worldSize - 1), rnd.Next(_worldSize - 1));

                while(_units.Any(u => u != null && u.Position.Equals(position)))
                {
                    position = new Point(rnd.Next(_worldSize - 1), rnd.Next(_worldSize - 1));
                }

                unit.Position = position;
                _units[i] = unit;
            }

            _tickingTask = Task.Factory.StartNew(
                () => WorldTicking(token),
                TaskCreationOptions.LongRunning);
        }

        public WorldDescription GetCurrentDescription()
        {
            return new WorldDescription()
            {
                WorldSize = _worldSize,
                Units = _units,
            };
        }

        public void SendTo(IEnumerable<Guid> units, Point position)
        {
            foreach (var unit in units)
            {
                Unit worldUnit = _units.FirstOrDefault(u => u.UnitId == unit);
                if (worldUnit != null)
                {
                    worldUnit.Destination = position;
                }
            }
        }

        private async void WorldTicking(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (var unit in _units)
                {
                    if (unit.Destination.HasValue && !unit.Position.Equals(unit.Destination.Value))
                    {
                        unit.Position = GetNextPosition(unit);
                    }
                    else if (unit.Destination.HasValue && unit.Position.Equals(unit.Destination.Value))
                    {
                        unit.Destination = null;
                        _unitRoutes.Remove(unit);
                    }
                }

                await Task.Delay(1000 / _ticksPerSecond);
            }
        }

        private Point GetNextPosition(Unit unit)
        {
            if (!unit.Destination.HasValue)
            {
                return GetRounded(unit.Position);
            }

            int[,] worldMap = GetWorldMap(unit);

            if (_unitRoutes.ContainsKey(unit))
            {
                if (!_unitRoutes[unit].Points.Any() || !_unitRoutes[unit].Destination.Equals(unit.Destination))
                {
                    _unitRoutes.Remove(unit);
                }
                else
                {
                    Point nextDest = _unitRoutes[unit].Points.First();

                    if (worldMap[(int)nextDest.X, (int)nextDest.Y] > 0)
                    {
                        _unitRoutes.Remove(unit);
                    }
                }
            }

           if (!_unitRoutes.ContainsKey(unit))
           {
                List<Point> route = GetNearestRoute(unit);

                if (route != null)
                {
                    _unitRoutes.Add(unit, new RouteToPoint() { Points = route.Skip(1), Destination = unit.Destination.Value });
                }
            }

            if (!_unitRoutes.ContainsKey(unit) || !_unitRoutes[unit].Points.Any())
            {
                return GetRounded(unit.Position);
            }

            Point current = _unitRoutes[unit].Points.First();

            Point moveVector = current - unit.Position;

            float distance = _ticksPerSecond * unit.SpeedCellsInSecond / 1000.0f;
            if (moveVector.Length < distance)
            {
                distance = moveVector.Length;

                if (_unitRoutes.ContainsKey(unit))
                {
                    _unitRoutes[unit].Points = _unitRoutes[unit].Points.Skip(1);
                }
            }

            return unit.Position + moveVector.GetNormalized() * distance;
        }

        private List<Point> GetNearestRoute(Unit unit)
        {
            int[,] map = GetWorldMap(unit);

            List<Point> route = AStarSolver.FindPath(
                map,
                unit.Position,
                unit.Destination.Value);

            int levelAround = 1;
            float sortDistance = float.MaxValue;

            while (route == null && levelAround < _worldSize)
            {
                int levelSide = (levelAround * 2 + 1);

                Point startPoint = unit.Destination.Value - new Point(levelAround, levelAround);
                for (int i = 0; i < levelSide; i++)
                {
                    Point currentDestination = new Point(startPoint.X + i, startPoint.Y);
                    sortDistance = SortDistance(unit, map, currentDestination, sortDistance, ref route);

                    currentDestination = new Point(startPoint.X + i, startPoint.Y + levelSide - 1);
                    sortDistance = SortDistance(unit, map, currentDestination, sortDistance, ref route);

                    currentDestination = new Point(startPoint.X, startPoint.Y + i);
                    sortDistance = SortDistance(unit, map, currentDestination, sortDistance, ref route);

                    currentDestination = new Point(startPoint.X + levelSide - 1, startPoint.Y + i);
                    sortDistance = SortDistance(unit, map, currentDestination, sortDistance, ref route);
                }

                levelAround++;
            }

            return route;
        }

        private static float SortDistance(
            Unit unit,
            int[,] map,
            Point currentDestination,
            float sortDistance,
            ref List<Point> route)
        {
            List<Point> currentRoute = AStarSolver.FindPath(
                map,
                unit.Position,
                currentDestination);

            // Будем искать первую точку, ближайшую к назначению.
            if (currentRoute != null && sortDistance >
                currentDestination.GetDistanceTo(unit.Destination.Value))
            {
                route = currentRoute;
                sortDistance = currentDestination.GetDistanceTo(unit.Destination.Value);
            }

            return sortDistance;
        }

        private Point GetRounded(Point point)
        {
            return new Point(
                (int)Math.Round(point.X),
                (int)Math.Round(point.Y));
        }

        private int[,] GetWorldMap(Unit selectedUnit)
        {
            int[,] map = new int[_worldSize, _worldSize];

            foreach (var unit in _units)
            {
                if (unit == selectedUnit)
                {
                    continue;
                }

                Point rounded = GetRounded(unit.Position);

                map[(int)rounded.X, (int)rounded.Y] = 1;

                if (unit.Destination.HasValue)
                {
                    Point movingVector = (rounded - unit.Position).GetNormalized();

                    if (movingVector.Length > float.Epsilon)
                    {
                        Point moved = GetRounded(rounded + movingVector * 0.5f);

                        if (moved.X >= 0 && moved.Y >= 0 && moved.X < _worldSize && moved.Y < _worldSize)
                        {
                            map[(int)Math.Round(moved.X),
                                (int)Math.Round(moved.Y)] = 1;
                        }
                    }
                }
            }

            return map;
        }
    }
}
