namespace NN01_app
{
    public abstract class GameObject
    {
        public GameObject Parent { get; set; } = null;
        public List<GameObject> Children { get; set; } = new List<GameObject>();

        public PointF Position { get; set; } = PointF.Zero;
        public PointF Velocity { get; set; } = PointF.Zero;
        public PointF Extent { get; set; } = PointF.Zero;
        public RectF BoundingRect => new RectF(Position.X - Extent.X, Position.Y - Extent.Y, Extent.X * 2f, Extent.Y * 2f);
        public PointF TopLeft => new PointF(Position.X - Extent.X, Position.Y - Extent.Y);
        public PointF BottomLeft => new PointF(Position.X - Extent.X, Position.Y + Extent.Y);
        public PointF TopRight => new PointF(Position.X + Extent.X, Position.Y - Extent.Y);
        public PointF BottomRight => new PointF(Position.X + Extent.X, Position.Y + Extent.Y);
        public float Width => Extent.X * 2;
        public float Height => Extent.Y * 2;


        public GameObject(GameObject parent)
        {
            Parent = parent;
        }
        public GameObject()
        {
        }

        public void Add(params GameObject[] child)
        {
            foreach (GameObject go in child)
            {
                Children.Add(go);
                go.Parent = this;
            }
        }

        public virtual void Render(ICanvas canvas, RectF dirtyRect)
        {
        }

        public virtual void Update(float deltaTime)
        {
        }

        public virtual void OnCollision(GameObject other, PointF intersection, PointF velocity)
        {

        }

        public virtual IEnumerable<Tuple<PointF, PointF>> GetBorders()
        {
            yield return new Tuple<PointF, PointF>(TopLeft, TopRight);
            yield return new Tuple<PointF, PointF>(TopRight, BottomRight);
            yield return new Tuple<PointF, PointF>(BottomRight, BottomLeft);
            yield return new Tuple<PointF, PointF>(BottomLeft, TopLeft);
        }

        public virtual void DoRender(ICanvas canvas, RectF dirtyRect)
        {
            foreach (GameObject go in Children)
            {
                go.DoRender(canvas, dirtyRect);
            }
            Render(canvas, dirtyRect);
        }

        public virtual void DoUpdate(float deltaTime)
        {
            foreach (GameObject go in Children)
            {
                go.DoUpdate(deltaTime);
            }
            Update(deltaTime);
        }
    }
}
