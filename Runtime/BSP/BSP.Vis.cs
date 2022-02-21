using System.IO;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Vis : ILump
        {
            public int NumClusters;
            public int[][] BytesOf;

            public void Read( BinaryReader reader )
            {
                NumClusters = reader.ReadInt32();

                BytesOf = new int[NumClusters][];

                for ( var i = 0; i < NumClusters; i++ )
                    BytesOf[i] = new[]
                    {
                        reader.ReadInt32(),
                        reader.ReadInt32()
                    };
            }
        }
    }
}