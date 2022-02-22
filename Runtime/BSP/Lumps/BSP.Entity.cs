using System.Collections.Generic;
using System.Text;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Entity
        {
            public readonly string Raw;
            public readonly Dictionary<string, string> KeyValues;

            public Entity( string raw )
            {
                Raw = raw;
                KeyValues = new Dictionary<string, string>();

                // Deserialize RAW

                var table = Raw.ToCharArray();
                var insideScope = false;

                StringBuilder key = null;
                StringBuilder value = null;

                for ( var i = 0; i < table.Length; i++ )
                {
                    var character = table[i];

                    // Start of Entity Scope
                    if ( character == '"' && !insideScope )
                    {
                        insideScope = true;

                        // Start new Entry
                        if ( key == null && value == null )
                        {
                            key = new StringBuilder();
                            continue;
                        }

                        // Start processing Value
                        if ( key != null && value == null )
                            value = new StringBuilder();

                        continue;
                    }

                    // End of Entity Scope
                    if ( character == '"' && insideScope )
                    {
                        insideScope = false;

                        // If we're done, store it
                        if ( key != null && value != null )
                        {
                            var builtKey = key.ToString();
                            KeyValues.Add( builtKey, value.ToString() );

                            // In the future we would remove this
                            // And account for Entity IO
                            if ( builtKey == "classname" )
                                break;

                            key = null;
                            value = null;
                        }

                        // Skip space after 
                        if ( key != null && value == null )
                            i++;

                        continue;
                    }

                    if ( key != null && value != null )
                        value.Append( table[i] );
                    else if ( key != null )
                        key.Append( table[i] );
                }
            }
        }
    }
}