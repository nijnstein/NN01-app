using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NN01_app
{
    public class GUI : GameObject
    {
        public Color OutlineColor { get; set; } = Colors.Red;
        public Color HitColor { get; set; } = Colors.Yellow;
        public int HitHighlightFrames { get; set; } = 10;
    
        private int collisionFrameCountDown = 0;

        public override void Render(ICanvas canvas, RectF dirty)
        {
            if (collisionFrameCountDown > 0)
            {
                canvas.StrokeColor = HitColor;
                canvas.StrokeSize = 2;
                collisionFrameCountDown--;
            }
            else
            {
                canvas.StrokeColor = OutlineColor;
                canvas.StrokeSize = 1;
            }
            canvas.DrawRectangle(BoundingRect);
        }

        public override void OnCollision(GameObject other, PointF intersection, PointF velocity)
        {
            collisionFrameCountDown = HitHighlightFrames;
        }

        public override IEnumerable<Tuple<PointF, PointF>> GetBorders()
        {
            // invert borders, we would be bouncing INSIDE the ui
            yield return new Tuple<PointF, PointF>(TopLeft, BottomLeft);
            yield return new Tuple<PointF, PointF>(BottomRight, TopRight);

            yield return new Tuple<PointF, PointF>(BottomLeft, BottomRight);
            yield return new Tuple<PointF, PointF>(TopRight, TopLeft);
        }
    }
}
