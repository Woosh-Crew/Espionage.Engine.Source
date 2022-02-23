// ReSharper disable StringLiteralTypo

using UnityEngine;
using UnityEngine.Rendering;

namespace Espionage.Engine.Source
{
    [Library( "env_fog_controller" ), Title( "Fog Controller" ), Group( "Entities" ), Spawnable]
    public class EnvFogController : Behaviour, BSP.IPointEntity
    {
        // Colors

        [Property( "fogcolor" )]
        public BSP.Color Color1 { get; set; }

        // State & Distance

        [Property( "fogenable" )]
        public bool Enabled { get; set; }

        [Property( "fogstart" )]
        public float Start { get; set; }

        [Property( "fogend" )]
        public float End { get; set; }

        public void OnRead( BSP.Entity ent )
        {
            // Apply Fog

            RenderSettings.fog = Enabled;
            RenderSettings.fogColor = Color1;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = Start / 100;
            RenderSettings.fogEndDistance = End / 100;
        }
    }
}