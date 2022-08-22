using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NN01_app
{
    public class Obstacle : GameObject
    {
        public Color Color { get; set; } = Colors.Green;
        public int HitHighlightFrames { get; set; } = 10;
        public Color HitColor { get; set; } = Colors.White;

        public override void Render(ICanvas canvas, RectF dirty)
        {
            if (collisionFrameCountDown > 0)
            {
                collisionFrameCountDown--;
                canvas.StrokeColor = HitColor;
            }
            else
            {
                canvas.StrokeColor = Color;
            }
            canvas.StrokeSize = 1; 

            canvas.DrawRectangle(Position.X - Extent.X, Position.Y - Extent.Y, Extent.X * 2, Extent.Y * 2);
        }

        private int collisionFrameCountDown = 0;

        public override void OnCollision(GameObject other, PointF intersection, PointF velocity)
        {
            collisionFrameCountDown = HitHighlightFrames;
        }
    }
}
