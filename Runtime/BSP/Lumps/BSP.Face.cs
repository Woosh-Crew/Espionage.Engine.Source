using System.IO;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct Face : ILump
        {
            public ushort PlaneNum;
            public byte Side;
            public bool OnNode;
            public int FirstEdge;
            public short NumEdges;
            public short TexInfo;
            public short DisplacementInfo;
            public short SurfaceFogVolumeID;
            public byte[] Styles;
            public int LightOffset;
            public float Area;
            public int[] LightmapTextureMinsInLuxels;
            public int[] LightmapTextureSizeInLuxels;
            public int OriginalFace;
            public ushort NumPrims;
            public ushort FirstPrimID;
            public uint SmoothingGroups;

            public void Read( BinaryReader reader )
            {
                PlaneNum = reader.ReadUInt16();
                Side = reader.ReadByte();
                OnNode = reader.ReadBoolean();
                FirstEdge = reader.ReadInt32();
                NumEdges = reader.ReadInt16();
                TexInfo = reader.ReadInt16();
                DisplacementInfo = reader.ReadInt16();
                SurfaceFogVolumeID = reader.ReadInt16();
                Styles = reader.ReadBytes( 4 );
                LightOffset = reader.ReadInt32();
                Area = reader.ReadSingle();

                LightmapTextureMinsInLuxels = new int[2];
                LightmapTextureMinsInLuxels[0] = reader.ReadInt32();
                LightmapTextureMinsInLuxels[1] = reader.ReadInt32();

                LightmapTextureSizeInLuxels = new int[2];
                LightmapTextureSizeInLuxels[0] = reader.ReadInt32();
                LightmapTextureSizeInLuxels[1] = reader.ReadInt32();

                OriginalFace = reader.ReadInt32();
                NumPrims = reader.ReadUInt16();
                FirstPrimID = reader.ReadUInt16();
                SmoothingGroups = reader.ReadUInt32();
            }
        }
    }
}