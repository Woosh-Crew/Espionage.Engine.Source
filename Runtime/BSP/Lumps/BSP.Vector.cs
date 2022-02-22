using System;
using System.IO;
using UnityEngine;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Vector : ILump
        {
            public float X, Y, Z;

            public void Read( BinaryReader reader )
            {
                X = reader.ReadSingle() * Scale;
                Z = reader.ReadSingle() * Scale;
                Y = reader.ReadSingle() * Scale;
            }

            public static Vector Parse( BinaryReader reader ) => new()
            {
                X = reader.ReadSingle() * Scale,
                Y = reader.ReadSingle() * Scale,
                Z = reader.ReadSingle() * Scale
            };

            public static Vector Parse( string value )
            {
                var split = value.Split( " " );

                if ( split.Length < 3 )
                    throw new InvalidCastException();

                return new Vector()
                {
                    X = float.Parse( split[0] ) * Scale,
                    Y = float.Parse( split[2] ) * Scale,
                    Z = float.Parse( split[1] ) * Scale
                };
            }

            public static implicit operator Vector3( Vector vector ) => new( vector.X, vector.Y, vector.Z );
        }
    }
}