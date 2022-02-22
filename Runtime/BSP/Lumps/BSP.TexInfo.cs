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
                /// <summary> Value will hold the light strength </summary>
                Light = 0x1,

                /// <summary> Don't draw, indicates we should skylight + draw 2d sky but not draw the 3D skybox </summary>
                Sky2D = 0x2,

                /// <summary> Don't draw, but add to skybox </summary>
                Sky = 0x4,

                /// <summary> Turbulent water warp </summary>
                Warp = 0x8,

                /// <summary> Texture is translucent </summary>
                Trans = 0x10,

                /// <summary> The surface can not have a portal placed on it </summary>
                NoPortal = 0x20,

                /// <summary> FIXME: This is an xbox hack to work around elimination of trigger surfaces, which breaks occluders </summary>
                Trigger = 0x40,

                /// <summary> don't bother referencing the texture </summary>
                Nodraw = 0x80,

                /// <summary> make a primary bsp splitter </summary>
                Hint = 0x100,

                /// <summary> completely ignore, allowing non-closed brushes </summary>
                Skip = 0x200,

                /// <summary> Don't calculate light </summary>
                Nolight = 0x400,

                /// <summary> calculate three lightmaps for the surface for bumpmapping </summary>
                BumpLight = 0x800,

                /// <summary> Don't receive shadows </summary>
                NoShadows = 0x1000,

                /// <summary> Don't receive decals </summary>
                NoDecals = 0x2000,

                /// <summary> Don't subdivide patches on this surface </summary>
                NoChop = 0x4000,

                /// <summary> surface is part of a hitbox </summary>
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