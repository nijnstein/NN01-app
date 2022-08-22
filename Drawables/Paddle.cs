using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NN01_app
{
    public class Paddle : GameObject
    {
        public Color Color { get; set; } = Colors.Green;
        public int HitHighlightFrames { get; set; } = 10;
        public Color HitColor { get; set; } = Colors.White;

        public override void Render(ICanvas canvas, RectF dirty)
        {
            if (collisionFrameCountDown > 0)
            {
                collisionFrameCountDown--;
                canvas.FillColor = HitColor;
            }
            else
            {
                canvas.FillColor = Color;
            }


            canvas.FillRectangle(Position.X - Extent.X, Position.Y - Extent.Y, Extent.X * 2, Extent.Y * 2);
        }


        private PointF lastPosition = new PointF(float.NaN, float.NaN);
        public override void Update(float deltaTime)
        {
            // dont update position with velocity, instead calculate velocity from position change and time delta
            if (!float.IsNaN(lastPosition.X))
            {
                float timeInverse = 1f / deltaTime;
                Velocity = new PointF((Position.X - lastPosition.X) * timeInverse, (Position.Y - lastPosition.Y) * timeInverse);
            }
            lastPosition = Position;
        }

        private int collisionFrameCountDown = 0;

        public override void OnCollision(GameObject other, PointF intersection, PointF velocity)
        {
            collisionFrameCountDown = HitHighlightFrames;
        }
    }
}
