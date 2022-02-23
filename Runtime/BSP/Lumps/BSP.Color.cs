using System;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Color
        {
            public float Red, Green, Blue, Alpha;

            public static Color Parse( string value )
            {
                var split = value.Split( " " );

                if ( split.Length < 3 )
                    throw new InvalidCastException();

                return new Color()
                {
                    Red = float.Parse( split[0] ),
                    Green = float.Parse( split[1] ),
                    Blue = float.Parse( split[2] ),
                    Alpha = split.Length > 3 ? float.Parse( split[3] ) : 255
                };
            }

            public static implicit operator UnityEngine.Color( Color col ) => new( col.Red / 255, col.Green / 255, col.Blue / 255, col.Alpha / 255 );
        }
    }
}