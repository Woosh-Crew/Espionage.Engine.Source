using System.Collections.Generic;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Entity
        {
            public readonly string Raw;
            public readonly Dictionary<string, string> Data;

            public Entity( string raw )
            {
                Raw = raw;
                Data = new Dictionary<string, string>();

                // Load KeyValues
                var lines = Raw.Split( '\n' );
                
                foreach ( var line in lines )
                {
                    
                    Data.Add( line.SplitArguments()[0], line.SplitArguments()[1] );
                }
            }
        }
    }
}