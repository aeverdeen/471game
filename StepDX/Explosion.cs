using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    public class Explosion : GameSprite
    {
        public override List<Vector2> Vertices { get { return verticesM; } }

        private float spriteTime = 0;
        private float spriteRate = 3; // 3 per second

        public bool getRidOf = false;

        public override void Advance(float dt)
        {
            int spriteNum;

            spriteTime += dt;
            spriteNum = (int)(spriteTime * spriteRate) % 4; // 3 images

            if (spriteNum >= 3)
            {
                getRidOf = true;
                spriteNum = 3;
            }

            // Create the texture vertices
            textureC.Clear();
            textureC.Add(new Vector2(spriteNum * 0.333f, 1));
            textureC.Add(new Vector2(spriteNum * 0.333f, 0));
            textureC.Add(new Vector2((spriteNum + 1) * 0.333f, 0));
            textureC.Add(new Vector2((spriteNum + 1) * 0.333f, 1));

            // Move the vertices
            verticesM.Clear();
            foreach (Vector2 x in verticesB)
            {
                verticesM.Add(new Vector2(x.X + p.X, x.Y + p.Y));
            }
        }
    }
}
