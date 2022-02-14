using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Espionage.Engine.Resources;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Espionage.Engine.Source
{
    public class BspMapProvider : IMapProvider, ICallbacks
    {
    #if UNITY_EDITOR

        [MenuItem( "Source/Load BSP" )]
        public static void LoadMap()
        {
            var path = EditorUtility.OpenFilePanel( "BSP", @"D:\Programs\SteamLibrary\steamapps\common\Half-Life 2\hl2\maps", "bsp" );
            var bsp = new BSP( new FileInfo( path ) );

            var map = new Map( new BspMapProvider( bsp ) );
            map.Load();
        }


        [MenuItem( "Source/Load VTF" )]
        public static void LoadTexture()
        {
            var path = EditorUtility.OpenFilePanel( "VTF", @"D:\Programs\SteamLibrary\steamapps\common\Half-Life 2\hl2\maps", "vtf" );
            var vtf = new VTF( new FileInfo( path ) );

            Debugging.Log.Info( vtf.Head.Format );
            Debugging.Log.Info( $"{vtf.Head.Version[0]}.{vtf.Head.Version[1]}" );
            Debugging.Log.Info( vtf.Head.Width );
            Debugging.Log.Info( vtf.Head.Height );
            Debugging.Log.Info( vtf.Head.Flags.ToString() );
        }

    #endif

        // Id
        public string Identifier => _bsp.File.FullName;

        // Outcome
        public Scene? Scene { get; private set; }

        // Loading Meta
        public float Progress => 0;
        public bool IsLoading { get; private set; }

        public BspMapProvider( BSP bsp )
        {
            // BSP Map Provider
            // Is just an Espionage.Engine
            // Wrapper for maps.
            _bsp = bsp;
            Callback.Register( this );
        }

        //
        // Resource
        //

        private readonly BSP _bsp;

        [Callback( "debug.gizmos" )]
        public void Shit()
        {
            foreach ( var edge in _bsp.Edges )
                Gizmos.DrawLine( _bsp.Vertices[edge.VertexIndices[0]] * BSP.Scale, _bsp.Vertices[edge.VertexIndices[1]] * BSP.Scale );
        }

        public void Load( Action finished )
        {
            IsLoading = true;

            Debugging.Log.Info( $"Loading {_bsp.File.Name}" );
            Debugging.Log.Info( $"Format {_bsp.Head.Format}" );
            Debugging.Log.Info( $"Version {_bsp.Head.Version}" );
            Debugging.Log.Info( $"Revision {_bsp.Head.Revision}" );

            for ( var i = 0; i < _bsp.Head.Lumps.Length; i++ )
            {
                var lump = _bsp.Head.Lumps[i];
                Debugging.Log.Info( $"Lump {i} [{lump.Offset} / {lump.Offset + lump.Length}] ({lump.Length})" );
            }

            Scene = SceneManager.CreateScene( Path.GetFileName( _bsp.File.Name ) );
            SceneManager.SetActiveScene( Scene.Value );

            // Place Cubemaps
            foreach ( var item in _bsp.Cubemaps )
            {
                var go = new GameObject( "env_cubemap" );
                var probe = go.AddComponent<ReflectionProbe>();
                probe.resolution = item.Size;
                probe.size = Vector3.one * 50;
                go.transform.position = item.Origin * 0.0333f;
            }

            // Create Mesh
            var gameObject = new GameObject( "World" );
            var meshFilter = gameObject.AddComponent<MeshFilter>();

            var mesh = new Mesh();
            mesh.vertices = _bsp.Vertices;

            // Build TRI Tree
            var tris = new int[_bsp.Edges.Length * 2];
            for ( var i = 0; i < _bsp.Edges.Length; i++ )
            {
                tris[i] = _bsp.Edges[i].VertexIndices[0];
                tris[i + 1] = _bsp.Edges[i].VertexIndices[1];
            }

            mesh.triangles = tris;
            meshFilter.mesh = mesh;

            IsLoading = false;
            finished?.Invoke();
        }

        public void Unload( Action finished )
        {
            IsLoading = true;

            Scene?.Unload();
            Debugging.Log.Info( "Finished Unloading BSP" );

            IsLoading = false;
            finished?.Invoke();
        }
    }
}