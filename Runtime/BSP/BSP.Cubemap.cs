using System.IO;
using UnityEngine;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Cubemap : ILump
        {
            public Vector3 Origin;
            public int Size;

            public void Read( BinaryReader reader )
            {
                // We cant use ReadVec3 here, cause
                // its actually 3 ints.

                var x = reader.ReadInt32();
                var z = reader.ReadInt32();
                var y = reader.ReadInt32();

                Origin = new Vector3( x, y, z );

                var res = reader.ReadInt32();
                Size = res == 0 ? 256 : res;
            }
        }
    }
}