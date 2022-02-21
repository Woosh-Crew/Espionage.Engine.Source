using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Espionage.Engine.Source
{
    /// <summary>Deserialized BSP Data</summary>
    public partial class BSP
    {
        public const float Scale = 0.0333f;

        public FileInfo File { get; }
        public BSPReader Reader { get; }

        public BSP( FileInfo info )
        {
            File = info;

            // Open Streams for reading the header
            using var fileStream = File.Open( FileMode.Open, FileAccess.Read );
            using var binaryReader = new BinaryReader( fileStream );

            Reader = new BSPReader( binaryReader );

            Planes = Reader.Read<Plane>( 1, 20 );
            TextureDatas = Reader.Read<TextureData>( 2, 32 );
            TextureDatas = Reader.Read<TextureData>( 2, 32 );

            var vertices = Reader.Read<Vector>( 3, 12 );
            Vertices = vertices.Select( e => e.Convert() ).ToArray();

            Edges = Reader.Read<Edge>( 12, 4 );

            /*
            Planes = Read( binaryReader, Head.Lumps[1], 20, e => new Plane( e ) );
            TextureDatas = Read( binaryReader, Head.Lumps[2], 32, e => new TextureData( e ) );
            Vertices = Read( binaryReader, Head.Lumps[3], 12, e => e.ReadSourceVec3() );
            // Visibility = Read( reader, Head.Lumps[4], 12, e => new Vis( e ) );
            Nodes = Read( binaryReader, Head.Lumps[5], 32, e => new Node( e ) );
            TextureInfos = Read( binaryReader, Head.Lumps[6], 72, e => new TextureInfo( e ) );
            Faces = Read( binaryReader, Head.Lumps[7], 56, e => new Face( e ) );
            Edges = Read( binaryReader, Head.Lumps[12], 4, e => new Edge( e ) );
            SurfEdges = Read( binaryReader, Head.Lumps[13], 4, e => e.ReadInt32() );
            Cubemaps = Read( binaryReader, Head.Lumps[42], 16, e => new Cubemap( e ) );
            */
        }

        public readonly Entity[] Entities; // LUMP 0
        public readonly Plane[] Planes; // LUMP 1
        public readonly TextureData[] TextureDatas; // LUMP 2
        public readonly Vector3[] Vertices; // LUMP 3
        public readonly Vis[] Visibility; // LUMP 4
        public readonly Node[] Nodes; // LUMP 5
        public readonly TextureInfo[] TextureInfos; // LUMP 6
        public readonly Face[] Faces; // LUMP 7
        public readonly Edge[] Edges; // LUMP 12
        public readonly int[] SurfEdges; // LUMP 13
        public readonly Cubemap[] Cubemaps; // LUMP 42

        //
        // Structs
        //

        public readonly struct Entity
        {
            public readonly string ClassName;
            public readonly Dictionary<string, string> KeyValues;
        }

        public struct Plane : ILump
        {
            public Vector3 Normal;
            public float Distance; // From Origin
            public int Type; // Axis Identifier

            public void Read( BinaryReader reader )
            {
                Normal = reader.ReadSourceVec3();
                Distance = reader.ReadSingle();
                Type = reader.ReadInt32();
            }

            public UnityEngine.Plane Convert() => new( Normal, Distance );
        }

        public struct Vector : ILump
        {
            public float X, Y, Z;

            public void Read( BinaryReader reader )
            {
                X = reader.ReadSingle();
                Z = reader.ReadSingle();
                Y = reader.ReadSingle();
            }

            public Vector3 Convert() => new( X, Y, Z );
        }

        public struct TextureData : ILump
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

        public struct TextureInfo : ILump
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

        public struct Node : ILump
        {
            public int PlaneNum;
            public int[] Children;

            public short[] Mins;
            public short[] Maxs;

            public ushort FirstFace;
            public ushort NumFaces;

            public short Area;
            public short Padding;

            public void Read( BinaryReader reader )
            {
                PlaneNum = reader.ReadInt32();

                Children = new int[2];
                Children[0] = reader.ReadInt32();
                Children[1] = reader.ReadInt32();

                Mins = new short[3];
                for ( var i = 0; i < 3; i++ )
                    Mins[i] = reader.ReadInt16();

                Maxs = new short[3];
                for ( var i = 0; i < 3; i++ )
                    Maxs[i] = reader.ReadInt16();

                FirstFace = reader.ReadUInt16();
                NumFaces = reader.ReadUInt16();

                Area = reader.ReadInt16();
                Padding = reader.ReadInt16();
            }
        }

        public struct Edge : ILump
        {
            public ushort[] VertexIndices;

            public void Read( BinaryReader reader )
            {
                VertexIndices = new ushort[2];
                VertexIndices[0] = reader.ReadUInt16();
                VertexIndices[1] = reader.ReadUInt16();
            }
        }

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