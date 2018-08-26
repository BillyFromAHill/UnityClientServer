using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;

namespace Shared
{
    [Serializable]
    public class WorldDescriptioin
    {
        public Size WorldSize { get; set; }

        public IEnumerable<Unit> Units{
            get;
            set;
        }

    }
}
