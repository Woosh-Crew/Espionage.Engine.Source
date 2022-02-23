// ReSharper disable StringLiteralTypo

namespace Espionage.Engine.Source
{
    [Library( "worldspawn" ), Spawnable]
    public class SourceWorld : World, BSP.IPointEntity
    {
        [Property( "coldword" )]
        public bool IsCold { get; set; }

        [Property( "skyname" )]
        public string SkyName { get; set; }
    }
}