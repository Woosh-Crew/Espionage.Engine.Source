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
            Vertices = Reader.Read<Vector>( 3, 12 );


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
        public readonly Vector[] Vertices; // LUMP 3
        public readonly Vis[] Visibility; // LUMP 4
        public readonly Node[] Nodes; // LUMP 5
        public readonly TexInfo[] TextureInfos; // LUMP 6
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

        public struct Vector : ILump
        {
            public float X, Y, Z;

            public void Read( BinaryReader reader )
            {
                X = reader.ReadSingle() * Scale;
                Z = reader.ReadSingle() * Scale;
                Y = reader.ReadSingle() * Scale;
            }

            public static implicit operator Vector3( Vector vector ) => new( vector.X, vector.Y, vector.Z );
        }
    }
}