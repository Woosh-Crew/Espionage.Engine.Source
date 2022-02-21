using System;
using System.IO;

namespace Espionage.Engine.Source
{
    public class BSPReader : IDisposable
    {
        public BinaryReader Reader { get; }
        public BSP.Header Header { get; }

        public BSPReader( BinaryReader reader )
        {
            Reader = reader;
            Header = new BSP.Header( reader );
        }

        public void Dispose() => Reader?.Dispose();

        // Lumps

        public T[] Read<T>( int lumpIndex, int size ) where T : ILump, new()
        {
            var lump = Header.Lumps[lumpIndex];

            Reader.BaseStream.Seek( lump.Offset, SeekOrigin.Begin );

            var final = new T[lump.Length / size];
            for ( var i = 0; i < lump.Length / size; i++ )
            {
                var item = new T();
                item.Read( Reader );
                final[i] = item;
            }

            return final;
        }

        public T[] Read<T>( int lumpIndex, int size, Func<BinaryReader, T> item )
        {
            var lump = Header.Lumps[lumpIndex];
            Reader.BaseStream.Seek( lump.Offset, SeekOrigin.Begin );

            var final = new T[lump.Length / size];
            for ( var i = 0; i < lump.Length / size; i++ )
                final[i] = item.Invoke( Reader );

            return final;
        }

        public T[] Read<T>( int lumpIndex, Func<BinaryReader, BSP.Header.Lump, T[]> item )
        {
            var lump = Header.Lumps[lumpIndex];
            Reader.BaseStream.Seek( lump.Offset, SeekOrigin.Begin );

            return item.Invoke( Reader, lump );
        }
    }
}