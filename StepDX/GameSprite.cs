﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace StepDX
{
    public class GameSprite : PolygonTextured
    {

        protected Vector2 p = new Vector2(0, 0);  // Position
        protected Vector2 v = new Vector2(0, 0);  // Velocity
        protected Vector2 a = new Vector2(0, 0);  // Acceleration
        

        public int health = 3;

        public float initialY;


        public Vector2 P { set { p = value; initialY = value.Y; } get { return p; } }
        public Vector2 V { set { v = value; } get { return v; } }
        public Vector2 A { set { a = value; } get { return a; } }

        private Vector2 pSave;  // Position
        private Vector2 vSave;  // Velocity
        private Vector2 aSave;  // Acceleration

        public enum SpriteType { One, Two, Three, Four, Player };

        protected SpriteType type = SpriteType.One;

        public SpriteType T { set { type = value; } get { return type; } }

        public void SaveState()
        {
            pSave = p;
            vSave = v;
            aSave = a;
        }

        public void RestoreState()
        {
            p = pSave;
            v = vSave;
            a = aSave;
        }

        protected List<Vector2> verticesM = new List<Vector2>();  // The vertices

        public override List<Vector2> Vertices { get { return verticesM; } }

        public override void Advance(float dt)
        {
            initialY = initialY / 2 + 1;
            // Euler steps

            v.X += a.X * dt;
            v.Y += a.Y * dt;
            p.X += v.X * dt;
            p.Y += v.Y * dt;

            if (type == SpriteType.Two)
            {
                p.Y = initialY + (float)(Math.Sin( p.X * 1.3 ));
            }
            else if (type == SpriteType.Three)
            {
                p.Y = initialY + (float)(1.5 * Math.Sin( p.X * 3.3 ));
            }

            // Move the vertices
            verticesM.Clear();
            foreach (Vector2 x in verticesB)
            {
                verticesM.Add(new Vector2(x.X + p.X, x.Y + p.Y));
            }
        }
    }
}
