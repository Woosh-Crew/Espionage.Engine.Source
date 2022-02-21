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

            public static implicit operator Vector3( Vector vector ) => new( vector.X, vector.Y, vector.Z );
        }
    }
}