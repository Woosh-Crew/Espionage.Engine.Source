using System.Collections.Generic;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Entity
        {
            public readonly string Raw;

            public Entity( string raw ) { Raw = raw; }
        }
    }
}