using System.IO;
using UnityEngine;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Plane : ILump
        {
            public Vector3 Normal;
            public float Distance; // From Origin
            public int Type; // Axis Identifier

            public void Read( BinaryReader reader )
            {
                Normal = reader.ReadSourceVec3();
                Distance = reader.ReadSingle();
                Type = reader.ReadInt32();
            }

            public UnityEngine.Plane Convert() => new( Normal, Distance );
        }
    }
}