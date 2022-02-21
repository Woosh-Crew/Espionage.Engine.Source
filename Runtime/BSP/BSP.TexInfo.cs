using System.IO;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct TexInfo : ILump
        {
            public float[,] TextureVecs;
            public float[,] LightmapVecs;
            public int Flags;
            public int TexData;

            public void Read( BinaryReader reader )
            {
                TextureVecs = new float[2, 4];
                for ( var x = 0; x < 2; x++ )
                for ( var y = 0; y < 4; y++ )
                    TextureVecs[x, y] = reader.ReadInt32();

                LightmapVecs = new float[2, 4];
                for ( var x = 0; x < 2; x++ )
                for ( var y = 0; y < 4; y++ )
                    LightmapVecs[x, y] = reader.ReadInt32();

                Flags = reader.ReadInt32();
                TexData = reader.ReadInt32();
            }
        }
    }
}