using System.IO;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Vis : ILump
        {
            public int NumClusters;
            public int[,] BytesOf;

            public void Read( BinaryReader reader )
            {
                NumClusters = reader.ReadInt32();

                BytesOf = new int[NumClusters, 2];

                for ( var i = 0; i < NumClusters; i++ )
                {
                    BytesOf[i, 0] = reader.ReadInt32();
                    BytesOf[i, 1] = reader.ReadInt32();
                }
            }
        }
    }
}