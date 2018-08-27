using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;

namespace Shared
{
    [Serializable]
    public class WorldDescription
    {
        public int WorldSize { get; set; }

        public IEnumerable<Unit> Units{
            get;
            set;
        }

    }
}
