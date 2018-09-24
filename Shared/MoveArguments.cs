using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shared
{
    [Serializable]
    public class MoveArguments
    {
        public Point Position { get; set; }

        public Guid[] Units { get; set; }
    }
}
