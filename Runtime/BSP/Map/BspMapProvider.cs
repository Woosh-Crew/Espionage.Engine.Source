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

namespace Espionage.Engine.Source
{
    public class BspMapProvider : IMapProvider
    {
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

            Debugging.Log.Info( "Loading BSP" );
            Scene = SceneManager.CreateScene( Path.GetFileName( _bsp.File.Name ) );

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