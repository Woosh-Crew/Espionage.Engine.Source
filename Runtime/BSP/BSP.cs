using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Espionage.Engine.Source
{
    /// <summary>Deserialized BSP Data</summary>
    public class BSP
    {
        public const float Scale = 0.0333f;

        public Header Head { get; }
        public FileInfo File { get; }

        public BSP( FileInfo info )
        {
            File = info;

            // Open Streams for reading the header
            using var fileStream = File.Open( FileMode.Open, FileAccess.Read );
            using var reader = new BinaryReader( fileStream );

            Head = new Header( reader );

            Planes = Read( reader, Head.Lumps[1], 20, e => new Plane( e ) );
            TextureDatas = Read( reader, Head.Lumps[2], 32, e => new TextureData( e ) );
            Vertices = Read( reader, Head.Lumps[3], 12, e => e.ReadSourceVec3() );
            // Visibility = Read( reader, Head.Lumps[4], 12, e => new Vis( e ) );
            Nodes = Read( reader, Head.Lumps[5], 32, e => new Node( e ) );
            TextureInfos = Read( reader, Head.Lumps[6], 72, e => new TextureInfo( e ) );
            Faces = Read( reader, Head.Lumps[7], 56, e => new Face( e ) );
            Edges = Read( reader, Head.Lumps[12], 4, e => new Edge( e ) );
            SurfEdges = Read( reader, Head.Lumps[13], 4, e => e.ReadInt32() );
            Cubemaps = Read( reader, Head.Lumps[42], 16, e => new Cubemap( e ) );
        }

        //
        // Header
        //

        public readonly struct Header
        {
            public string Format => Encoding.UTF8.GetString( Indent );

            public Header( BinaryReader reader )
            {
                Indent = reader.ReadBytes( 4 );
                Version = reader.ReadInt32();

                // Read Lumps
                Lumps = new Lump[64];
                for ( var i = 0; i < 64; i++ )
                {
                    var lump = Lumps[i];

                    lump.Offset = reader.ReadInt32();
                    lump.Length = reader.ReadInt32();
                    lump.Version = reader.ReadInt32();
                    lump.Indent = reader.ReadBytes( 4 );

                    Lumps[i] = lump;
                }

                Revision = reader.ReadInt32();
            }

            public readonly byte[] Indent;
            public readonly Lump[] Lumps;
            public readonly int Version;
            public readonly int Revision;

            public struct Lump
            {
                public int Offset;
                public int Length;
                public int Version;
                public byte[] Indent;
            }
        }

        // Lumps

        private static T[] Read<T>( BinaryReader reader, Header.Lump lump, int size, Func<BinaryReader, T> item )
        {
            reader.BaseStream.Seek( lump.Offset, SeekOrigin.Begin );

            var final = new T[lump.Length / size];
            for ( var i = 0; i < lump.Length / size; i++ )
                final[i] = item.Invoke( reader );

            return final;
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

        public readonly struct Plane
        {
            public Plane( BinaryReader reader )
            {
                Normal = reader.ReadSourceVec3();
                Distance = reader.ReadSingle();
                Type = reader.ReadInt32();
            }

            public readonly Vector3 Normal;
            public readonly float Distance; // From Origin
            public readonly int Type; // Axis Identifier
        }

        public readonly struct TextureData
        {
            public TextureData( BinaryReader reader )
            {
                Reflectivity = reader.ReadSourceVec3();
                NameID = reader.ReadInt32();

                Width = reader.ReadInt32();
                Height = reader.ReadInt32();

                ViewWidth = reader.ReadInt32();
                ViewHeight = reader.ReadInt32();
            }

            public readonly Vector3 Reflectivity;
            public readonly int NameID;
            public readonly int Width, Height;
            public readonly int ViewWidth, ViewHeight; // Tf are these for?
        }

        public readonly struct Vis
        {
            public Vis( BinaryReader reader )
            {
                NumClusters = reader.ReadInt32();

                BytesOf = new int[NumClusters, 2];

                for ( var i = 0; i < NumClusters; i++ )
                {
                    BytesOf[i, 0] = reader.ReadInt32();
                    BytesOf[i, 1] = reader.ReadInt32();
                }
            }

            public readonly int NumClusters;
            public readonly int[,] BytesOf;
        }

        public readonly struct TextureInfo
        {
            public TextureInfo( BinaryReader reader )
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

            public readonly float[,] TextureVecs;
            public readonly float[,] LightmapVecs;
            public readonly int Flags;
            public readonly int TexData;
        }

        public readonly struct Node
        {
            public Node( BinaryReader reader )
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

            public readonly int PlaneNum;
            public readonly int[] Children;

            public readonly short[] Mins;
            public readonly short[] Maxs;

            public readonly ushort FirstFace;
            public readonly ushort NumFaces;

            public readonly short Area;
            public readonly short Padding;
        }

        public readonly struct Edge
        {
            public Edge( BinaryReader reader )
            {
                VertexIndices = new ushort[2];
                VertexIndices[0] = reader.ReadUInt16();
                VertexIndices[1] = reader.ReadUInt16();
            }

            public readonly ushort[] VertexIndices;
        }

        public readonly struct Cubemap
        {
            public Cubemap( BinaryReader reader )
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

            public readonly Vector3 Origin;
            public readonly int Size;
        }

        public readonly struct Face
        {
            public Face( BinaryReader reader )
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

            public readonly ushort PlaneNum;
            public readonly byte Side;
            public readonly bool OnNode;
            public readonly int FirstEdge;
            public readonly short NumEdges;
            public readonly short TexInfo;
            public readonly short DisplacementInfo;
            public readonly short SurfaceFogVolumeID;
            public readonly byte[] Styles;
            public readonly int LightOffset;
            public readonly float Area;
            public readonly int[] LightmapTextureMinsInLuxels;
            public readonly int[] LightmapTextureSizeInLuxels;
            public readonly int OriginalFace;
            public readonly ushort NumPrims;
            public readonly ushort FirstPrimID;
            public readonly uint SmoothingGroups;
        }
    }
}