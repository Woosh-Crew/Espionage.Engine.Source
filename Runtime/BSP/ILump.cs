using System.IO;

namespace Espionage.Engine.Source
{
    public interface ILump
    {
        void Read( BinaryReader reader );
    }
}