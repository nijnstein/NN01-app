using Microsoft.Maui.Animations;
using NN01;

namespace NN01_app
{
    public class Bal : GameObject
    {
        public const float HitImpactBoost = 1.3f;
        public const float ImpactFactor = 1.5f;
        public const float DragFactor = 0.9f;

        public PointF MinVelocity = new PointF(130, 100);
        public PointF MaxVelocity = new PointF(300, 200);
        public Color Color { get; set; } = Colors.Yellow;
        public PointF Margin { get; set; } = new PointF(5f, 6f);
        public List<GameObject> Obstacles { get; set; } = new List<GameObject>();

        public override void Render(ICanvas canvas, RectF dirty)
        {
            canvas.StrokeColor = Colors.Yellow;
            canvas.FillColor = Colors.Yellow;
            canvas.FillCircle(Position, Extent.X * 2);
        }
        public override void Update(float deltaTime)
        {
            PointF newPosition = new PointF(Position.X + deltaTime * Velocity.X, Position.Y + deltaTime * Velocity.Y);
            PointF intersection;
            PointF bounce;
            int bounceCount = 0;
            GameObject hit; 

            // bounce of corners, handles corner case of 2 bounces within 1 timestep 
            // -> note that we could allow a non-rectangular polygon as border using this full segment scan method
            do
            {
                hit = null; 
                bounce = PointF.Zero;
                intersection = PointF.Zero;

                // check bouncing of any obstacle 
                for (int iObstacle = 0; iObstacle < Obstacles.Count && bounce.IsEmpty; iObstacle++)
                {
                    GameObject obstacle = Obstacles[iObstacle]; 

                    foreach (Tuple<PointF, PointF> segment in obstacle.GetBorders())
                    {
                        // only check if clockwise order in line points, otherwise we are hitting from the inside 
                        bool isCW = false;  
                        float cx = (segment.Item1.X + segment.Item2.X) / 2f;
                        float cy = (segment.Item1.Y + segment.Item2.Y) / 2f;

                        // as seen from starting position
                        if(segment.Item1.Y <= Position.Y && segment.Item2.Y <= Position.Y)
                        {
                            // segment above us, cw if p1x < p1.y
                            isCW = segment.Item1.X > segment.Item2.X;
                        }
                        else
                        {
                            // below us
                            isCW = segment.Item1.X < segment.Item2.X; 
                        }
                        if (!isCW)
                        {
                            if (segment.Item1.X <= Position.X && segment.Item2.X <= Position.X)
                            {
                                // segment to left
                                isCW = segment.Item1.Y < segment.Item2.Y;
                            }
                            else
                            {
                                // to right
                                isCW = segment.Item1.Y > segment.Item2.Y;
                            }
                        }

                        if (isCW)
                        {
                            // point on ball in direction of linesegment checked
                            PointF p = MathEx.ClosestPointOnLine(segment.Item1, segment.Item2, Position, true);
                            float d = Math.Abs(p.Distance(Position));

                            if (d > 0 && (d < Extent.X || d < Extent.Y))
                            {
                                // normalized direction to point on line segment from position 
                                //PointF n = new PointF(p.X - Position.X, p.Y - Position.Y).Normalize();

                                // point on sphere closest to linesegment 
                                //PointF o0 = new PointF(Position.X + n.X * Extent.X, Position.Y + n.Y * Extent.Y);

                                //o = Position;
                                //PointF o1 = new PointF(o0.X + deltaTime * Velocity.X, o0.Y + deltaTime * Velocity.Y);

                                PointF newPosition2 = new PointF(Position.X + deltaTime * 2f * Velocity.X, Position.Y + deltaTime * 2f * Velocity.Y);

                                // on intersect bounce normals 
                                if (MathEx.TryGetSegmentIntersection(Position, newPosition2, segment.Item1, segment.Item2, out intersection, bounceCount == 0))
                                {
                                    obstacle.OnCollision(this, intersection, Velocity);

                                    hit = obstacle;

                                    // reflect of collision using normal 
                                    PointF normal = MathEx.Normal(segment.Item1, segment.Item2).Normalize();
                                    {
                                        bounce = normal; 
                                    }
                                    break;
                                }
                            }
                        }
                    }
           
                    // alter direction if bounced and calculate new position and a new target to move from with the remaining distance to travel after the collision
                    if (!bounce.IsEmpty)
                    {
                        if (bounce.X > 0)
                        {
                            if (Velocity.X < 0)
                            {
                                bounce.X = -bounce.X;
                            }
                            else
                            if (Velocity.X > 0)
                            {
                                bounce.X = -bounce.X;
                            }
                        }
                        if (bounce.Y > 0)
                        {
                            if (Velocity.Y < 0)
                            {
                                bounce.Y = -bounce.Y;
                            }
                            else
                            if (Velocity.Y > 0)
                            {
                                bounce.Y = -bounce.Y;
                            }
                        }
                        if (!bounce.IsEmpty)
                        {
                            float dx = newPosition.X - intersection.X;
                            float dy = newPosition.Y - intersection.Y;

                            float tx = newPosition.X - Position.X;
                            float ty = newPosition.Y - Position.Y;

                            float rx = -(dx - tx) * bounce.X;
                            float ry = -(dy - ty) * bounce.Y;

                            Position = intersection;
                            newPosition = new PointF(intersection.X + rx, intersection.Y + ry);

                            bounce = new PointF(bounce.X == 0 ? 1 : bounce.X, bounce.Y == 0 ? 1 : bounce.Y);

                            Velocity = new PointF(Velocity.X * bounce.X * HitImpactBoost, Velocity.Y * bounce.Y * HitImpactBoost);

                            // transfer velocity from hit 
                            Velocity = new PointF(Velocity.X + hit.Velocity.X * ImpactFactor, Velocity.Y + MathF.Abs(hit.Velocity.Y) * ImpactFactor);

                            // clamp speed between minmax 
                            Velocity = new PointF(
                                Velocity.X > 0 ? Math.Max(MinVelocity.X, Velocity.X.Clamp(MinVelocity.X, MaxVelocity.X)) : Math.Max(-MinVelocity.X, Velocity.X.Clamp(-MinVelocity.X, -MaxVelocity.X)),
                                Velocity.Y > 0 ? Math.Max(MinVelocity.Y, Velocity.Y.Clamp(MinVelocity.Y, MaxVelocity.Y)) : Math.Max(-MinVelocity.Y, Velocity.Y.Clamp(-MinVelocity.Y, -MaxVelocity.Y))); 

                            // Position = newPosition;
                            // we might bounce again in a corner, so we re-check with the remaining distance to travel 
                            // - on rebounce, dont check on endpoints of linesegments as we move position to the point of intersection 
                            // - dont keep bouncing if stuck  
                            bounceCount++;
                        }
                    }
                }
            }
            while (!bounce.IsEmpty && bounceCount < 10f);

            // update final position 
            if(newPosition.X > 400)
            {
                
            }

            Position = newPosition;

            // apply some drag to the bal above a threshold 
            Velocity = Velocity.Lerp( 
                new PointF(  
                    Velocity.X > 0 ? Math.Max(MinVelocity.X, Velocity.X.Clamp(MinVelocity.X, MaxVelocity.X)) : Math.Max(-MinVelocity.X, Velocity.X.Clamp(-MinVelocity.X, -MaxVelocity.X)),
                    Velocity.Y > 0 ? Math.Max(MinVelocity.Y, Velocity.Y.Clamp(MinVelocity.Y, MaxVelocity.Y)) : Math.Max(-MinVelocity.Y, Velocity.Y.Clamp(-MinVelocity.Y, -MaxVelocity.Y)))
                , DragFactor * deltaTime);
        }
    }
}
