using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityClientServer
{
    class RouteToPoint
    {
        public Point Destination { get; set; }

        public IEnumerable<Point> Points { get; set; }
    }
}
