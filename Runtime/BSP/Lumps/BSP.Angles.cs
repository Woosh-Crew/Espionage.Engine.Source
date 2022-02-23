using System;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Angles
        {
            public float Pitch, Yaw, Roll;

            public static Angles Parse( string value )
            {
                var split = value.Split( " " );

                if ( split.Length < 3 )
                    throw new InvalidCastException();

                return new Angles()
                {
                    Pitch = float.Parse( split[0] ),
                    Yaw = float.Parse( split[1] ),
                    Roll = float.Parse( split[2] )
                };
            }
        }
    }
}