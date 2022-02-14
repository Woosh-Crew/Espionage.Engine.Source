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
            TexDatas = Read( reader, Head.Lumps[2], 32, e => new TexData( e ) );
            Vertices = Read( reader, Head.Lumps[3], 12, e => e.ReadSourceVec3() );
            Faces = Read( reader, Head.Lumps[7], 56, e => new Face( e ) );
            Edges = Read( reader, Head.Lumps[12], 4, e => new Edge( e ) );
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
        public readonly TexData[] TexDatas; // LUMP 2
        public readonly Vector3[] Vertices; // LUMP 3s
        public readonly Face[] Faces; // LUMP 7
        public readonly Edge[] Edges; // LUMP 12
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

        public readonly struct TexData
        {
            public TexData( BinaryReader reader )
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

        public readonly struct SurfEdge
        {
            public SurfEdge( BinaryReader reader )
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