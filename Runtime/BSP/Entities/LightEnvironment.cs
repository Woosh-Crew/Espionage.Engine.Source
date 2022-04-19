// ReSharper disable StringLiteralTypo

using UnityEngine;
using UnityEngine.Rendering;

namespace Espionage.Engine.Source
{
    [Library( "light_environment" ), Title( "Sun Light" )]
    public class LightEnvironment : SourceEntity, BSP.IPointEntity
    {
        [Property( "pitch" )]
        public float Pitch { get; set; }

        [Property( "angles" )]
        public BSP.Angles Angles { get; set; }

        [Property( "_light" )]
        public BSP.Color Color { get; set; }

        [Property( "_ambient" )]
        public BSP.Color Ambient { get; set; }

        public void OnRead( BSP.Entity ent )
        {
            if ( _lightEnv != null )
            {
	            GameObject.Destroy( GameObject );
                return;
            }

            _lightEnv = this;

            // Create Directional Light
            _light = GameObject.AddComponent<Light>();
            _light.type = LightType.Directional;
            _light.shadows = LightShadows.Soft;
            _light.color = Color;
            _light.intensity = Color.Alpha / 100 / 3;

            RenderSettings.ambientLight = Ambient;
            RenderSettings.sun = _light;
            RenderSettings.ambientMode = AmbientMode.Flat;

            _light.transform.rotation = Quaternion.Euler( Angles.Pitch + -Pitch, Angles.Yaw + 120, Angles.Roll );
        }

        private Light _light;
        private static LightEnvironment _lightEnv;
    }
}
