// ReSharper disable StringLiteralTypo

using UnityEngine;
using UnityEngine.Rendering;

namespace Espionage.Engine.Source
{
    [Library( "light_spot" ), Title( "Spot Light" )]
    public class LightSpot : SourceEntity, BSP.IPointEntity
    {
        [Property( "pitch" )]
        public float Pitch { get; set; }

        [Property( "_cone" )]
        public float Cone { get; set; }

        [Property( "angles" )]
        public BSP.Angles Angles { get; set; }

        [Property( "_light" )]
        public BSP.Color Color { get; set; }

        [Property( "style" )]
        public string Style { get; set; }

        public void OnRead( BSP.Entity ent )
        {
            // Create Directional Light
            var light = gameObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.shadows = LightShadows.Soft;
            light.spotAngle = Cone * 2;
            light.color = Color;
            light.intensity = Color.Alpha / 100 / 3;
            light.range = 20;
            light.bounceIntensity = 0;

            light.transform.rotation = Quaternion.Euler( -Pitch, Angles.Yaw + 120, Angles.Roll );
            light.transform.localPosition = BSP.Vector.Parse( ent.KeyValues["origin"] );
        }
    }
}