using System.IO;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Model : ILump
        {
            public Vector Mins;
            public Vector Maxs;
            public Vector Origin;

            public int Headnode;
            public int FirstFace;
            public int NumFaces;

            public void Read( BinaryReader reader )
            {
                Mins = Vector.Parse( reader );
                Maxs = Vector.Parse( reader );
                Origin = Vector.Parse( reader );

                Headnode = reader.ReadInt32();
                FirstFace = reader.ReadInt32();
                NumFaces = reader.ReadInt32();
            }
        }
    }
}