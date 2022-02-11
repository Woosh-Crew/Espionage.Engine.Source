using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        public static void LoadMap( )
        {
            var path = EditorUtility.OpenFilePanel( "BSP", @"D:\Programs\SteamLibrary\steamapps\common\Half-Life 2\hl2\maps", "bsp" );
            var bsp = new BSP( new FileInfo( path ) );

            var map = new Map( new BspMapProvider( bsp ) );
            map.Load( );
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
        }

        //
        // Resource
        //

        private readonly BSP _bsp;

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

            foreach ( var item in _bsp.Vertices )
            {
                var go = GameObject.CreatePrimitive( PrimitiveType.Cube );
                go.transform.localScale = Vector3.one * 0.4f;
                go.transform.position = item * 0.0333f;
            }

            // Place Cubemaps
            foreach ( var item in _bsp.Cubemaps )
            {
                var go = new GameObject( "env_cubemap" );
                var probe = go.AddComponent<ReflectionProbe>( );
                probe.resolution = item.Size;
                probe.size = Vector3.one * 50;
                go.transform.position = item.Origin * 0.0333f;
            }

            IsLoading = false;
            finished?.Invoke( );
        }

        public void Unload( Action finished )
        {
            IsLoading = true;

            Scene?.Unload( );
            Debugging.Log.Info( "Finished Unloading BSP" );

            IsLoading = false;
            finished?.Invoke( );
        }
    }
}