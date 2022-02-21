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
            TextureData = Reader.Read<TexData>( 2, 32 );
            Vertices = Reader.Read<Vector>( 3, 12 );
            Visibility = Reader.Read<Vis>( 4, 12 );
            Nodes = Reader.Read<Node>( 5, 32 );
            TextureInfo = Reader.Read<TexInfo>( 6, 72 );
            Faces = Reader.Read<Face>( 7, 56 );
            Leafs = Reader.Read<Leaf>( 10, 56 );
            Edges = Reader.Read<Edge>( 12, 4 );
            SurfEdges = Reader.Read( 13, 4, e => e.ReadInt32() );
            DisplacementInfo = Reader.Read<DispInfo>( 26, 176 );
            Cubemaps = Reader.Read<Cubemap>( 42, 16 );

            TexdataStringTable = Reader.Read( 44, 4, e => e.ReadInt32() );
        }

        ~BSP()
        {
            // I think we have to do this
            Reader.Dispose();
        }

        public readonly Entity[] Entities; // LUMP 0
        public readonly Plane[] Planes; // LUMP 1
        public readonly TexData[] TextureData; // LUMP 2
        public readonly Vector[] Vertices; // LUMP 3
        public readonly Vis[] Visibility; // LUMP 4
        public readonly Node[] Nodes; // LUMP 5
        public readonly TexInfo[] TextureInfo; // LUMP 6
        public readonly Face[] Faces; // LUMP 7
        public readonly Leaf[] Leafs; // LUMP 10
        public readonly Edge[] Edges; // LUMP 12
        public readonly int[] SurfEdges; // LUMP 13
        public readonly DispInfo[] DisplacementInfo; // LUMP 26
        public readonly Cubemap[] Cubemaps; // LUMP 42

        public readonly int[] TexdataStringTable; // LUMP 44
        public readonly string[] TexdataStringData; // LUMP 43

        //
        // Structs
        //

        public readonly struct Entity
        {
            public readonly string ClassName;
            public readonly Dictionary<string, string> KeyValues;

            public readonly string Raw;
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