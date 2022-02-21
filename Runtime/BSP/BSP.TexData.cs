using System.IO;
using UnityEngine;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct TexData : ILump
        {
            public Vector3 Reflectivity;
            public int NameID;
            public int Width, Height;
            public int ViewWidth, ViewHeight; // Tf are these for?

            public void Read( BinaryReader reader )
            {
                Reflectivity = reader.ReadSourceVec3();
                NameID = reader.ReadInt32();

                Width = reader.ReadInt32();
                Height = reader.ReadInt32();

                ViewWidth = reader.ReadInt32();
                ViewHeight = reader.ReadInt32();
            }
        }
    }
}