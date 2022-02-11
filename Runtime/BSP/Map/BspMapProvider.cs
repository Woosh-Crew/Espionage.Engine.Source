using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Espionage.Engine.Resources;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Espionage.Engine.Source
{
    public class BspMapProvider : IMapProvider
    {
#if UNITY_EDITOR

        [MenuItem( "Source/Load Bsp" )]
        public static void Loadmap( )
        {
            var path = EditorUtility.OpenFilePanel( "Load .bsp File", @"D:\Programs\SteamLibrary\steamapps\common\Half-Life 2\hl2\maps", "bsp" );
            var map = new Map( new BspMapProvider( path ) ).Load( );
        }

#endif

        // Id
        public string Identifier => _path;

        // Outcome
        public Scene? Scene { get; private set; }

        // Loading Meta
        public float Progress => 0;
        public bool IsLoading { get; private set; }

        public BspMapProvider( string path )
        {
            if ( !File.Exists( path ) && Path.GetExtension( path ) != "bsp" )
                throw new DirectoryNotFoundException( $"Unable to BSP map {path}, Invalid Map Path" );

            _path = path;
            _file = new FileInfo( path );
        }

        //
        // Resource
        //

        private readonly FileInfo _file;
        private readonly string _path;

        public void Load( Action finished )
        {
            IsLoading = true;

            Debugging.Log.Info( "Loading BSP" );
            Scene = SceneManager.CreateScene( Path.GetFileName( _path ) );

            // Just some testing shit
            using ( var fileStream = _file.Open( FileMode.Open ) )
            using ( var reader = new BinaryReader( fileStream ) )
            {
                reader.BaseStream.Position = 4;
                var version = reader.Read( );

                Debugging.Log.Info( $"{version}" );
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