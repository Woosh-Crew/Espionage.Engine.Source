using System;
using System.IO;

namespace Espionage.Engine.Source
{
    public partial class BSP
    {
        public struct TexInfo : ILump
        {
            public float[][] TextureVecs;
            public float[][] LightmapVecs;

            public Flag Flags;
            public int TexData;

            [Flags]
            public enum Flag : int
            {
                Light = 0x1,
                Sky2D = 0x2,
                Sky = 0x4,
                Warp = 0x8,
                Trans = 0x10,
                NoPortal = 0x20,
                Trigger = 0x40,
                Nodraw = 0x80,
                Hint = 0x100,
                Skip = 0x200,
                Nolight = 0x400,
                BumpLight = 0x800,
                NoShadows = 0x1000,
                NoDecals = 0x2000,
                NoChop = 0x4000,
                Hitbox = 0x8000
            }

            public void Read( BinaryReader reader )
            {
                TextureVecs = new float[2][]
                {
                    new float[4]
                    {
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()
                    },

                    new float[4]
                    {
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()
                    }
                };

                LightmapVecs = new float[2][]
                {
                    new float[4]
                    {
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()
                    },

                    new float[4]
                    {
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()
                    }
                };

                Flags = (Flag) reader.ReadInt32();
                TexData = reader.ReadInt32();
            }
        }
    }
}