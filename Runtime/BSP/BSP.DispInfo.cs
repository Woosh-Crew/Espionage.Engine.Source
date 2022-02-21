using System.IO;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct DispInfo : ILump
        {
            public Vector StartPosition; // start position used for orientation
            public int DispVertStart; // Index into LUMP_DISP_VERTS.
            public int DispTriStart; // Index into LUMP_DISP_TRIS.
            public int Power; // power - indicates size of surface (2^power	1)
            public int MinTess; // minimum tesselation allowed
            public float SmoothingAngle; // lighting smoothing angle
            public int Contents; // surface contents
            public ushort MapFace; // Which map face this displacement comes from.
            public int LightmapAlphaStart; // Index into ddisplightmapalpha.
            public int LightmapSamplePositionStart; // Index into LUMP_DISP_LIGHTMAP_SAMPLE_POSITIONS.
            public uint AllowedVerts; // active verticies

            public void Read( BinaryReader reader )
            {
                var pos = new Vector();
                pos.Read( reader );
                StartPosition = pos;

                DispVertStart = reader.ReadInt32();
                DispTriStart = reader.ReadInt32();
                Power = reader.ReadInt32();
                MinTess = reader.ReadInt32();
                SmoothingAngle = reader.ReadSingle();
                Contents = reader.ReadInt32();
                MapFace = reader.ReadUInt16();
                LightmapAlphaStart = reader.ReadInt32();
                LightmapSamplePositionStart = reader.ReadInt32();
                AllowedVerts = reader.ReadUInt32();
            }
        }
    }
}