using System;
using System.IO;
using Espionage.Engine.Resources;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Espionage.Engine.Source
{
    public class BSPMapProvider : IMapProvider, ICallbacks
    {
    #if UNITY_EDITOR

        [MenuItem( "Source/Load BSP" )]
        public static void LoadMap()
        {
            using var _ = Debugging.Stopwatch( "Loading BSP" );

            var path = EditorUtility.OpenFilePanel( "BSP", @"D:\Programs\SteamLibrary\steamapps\common\Half-Life 2\hl2\maps", "bsp" );
            var bsp = new BSP( new FileInfo( path ) );

            var map = new Map( new BSPMapProvider( bsp ) );
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

        public BSPMapProvider( BSP bsp )
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
                Gizmos.DrawLine( _bsp.Vertices[edge.VertexIndices[0]], _bsp.Vertices[edge.VertexIndices[1]] );
        }

        public void Load( Action finished )
        {
            IsLoading = true;

            Debugging.Log.Info( $"Loading {_bsp.File.Name}" );
            Debugging.Log.Info( $"Format {_bsp.Reader.Header.Format}" );
            Debugging.Log.Info( $"Version {_bsp.Reader.Header.Version}" );
            Debugging.Log.Info( $"Revision {_bsp.Reader.Header.Revision}" );

            for ( var i = 0; i < _bsp.Reader.Header.Lumps.Length; i++ )
            {
                var lump = _bsp.Reader.Header.Lumps[i];
                Debugging.Log.Info( $"Lump {i} [Start: {lump.Offset} - End: {lump.Offset + lump.Length}] (Length: {lump.Length})" );
            }

            // Create Scene
            Scene = SceneManager.CreateScene( Path.GetFileName( _bsp.File.Name ) );
            SceneManager.SetActiveScene( Scene.Value );

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