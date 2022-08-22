using System;
using System.Diagnostics;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Graphics;
using NN01;

namespace NN01_app
{


    public class Pong : GameObject, IDrawable
    {
        public const float BallRadius = 5;

        /// <summary>
        /// time ball needs to traverse width of screen in seconds
        /// </summary>
        public const float BallSpeedFactor = 3f;
        public const float PaddleSpeedFactor = 3f; 

        private Paddle leftPaddle = null;
        private Paddle rightPaddle = null;
        private Bal bal = null;
        private GUI gui = null; 

        public Pong() : base(null)
        {
            Reset();
        }
        
        /// <summary>
        /// reset / create environment 
        /// </summary>
        public void Reset()
        {
            // create gameobjects in their initial state 
            Children.Clear();

            Position = new PointF(200, 150);
            Extent = new PointF(200, 150);

            leftPaddle = new Paddle() { Position = new PointF(this.Position.X - this.Extent.X + 20f, this.Position.Y), Extent = new PointF(3f, 40f)};
            rightPaddle = new Paddle() { Position = new PointF(this.Position.X + this.Extent.X - 20f, this.Position.Y), Extent = new PointF(3f, 40f)};

            var obstacle1 = new Obstacle() { Position = new PointF(this.Position.X, this.Position.Y - 70), Extent = new PointF(5f, 25f), Color = Colors.Red, HitColor = Colors.White };
            var obstacle2 = new Obstacle() { Position = new PointF(this.Position.X, this.Position.Y + 70), Extent = new PointF(5f, 25f), Color = Colors.Red, HitColor = Colors.White };
            
            var obstacle3 = new Obstacle() { Position = new PointF(this.Position.X, this.Position.Y), Extent = new PointF(50f, 5f), Color = Colors.Red, HitColor = Colors.White };

            gui = new GUI() { Position = Position, Extent = new Point(Extent.X, Extent.Y), OutlineColor = Colors.Red };

            float ballDirection = Random.Shared.NextSingle() - 0.5f > 0 ? -1 : 1;

            bal = new Bal()
            {
                Position = new PointF(200, 150),
                Extent = new PointF(BallRadius, BallRadius),
                Velocity = new Point(ballDirection * (Extent.X * 2) / BallSpeedFactor, Random.Shared.NextSingle() * Extent.Y / BallSpeedFactor), 
                Obstacles = new List<GameObject>()
                {
                    gui, 
                    leftPaddle, rightPaddle,
                    obstacle1, obstacle2, obstacle3
                }
            };

            Add(leftPaddle, rightPaddle, obstacle1, obstacle2, obstacle3, bal, gui);

            // setup the neural network 
            InitNetwork();
        }

        /// <summary>
        /// update environment 
        /// </summary>
        public override void Update(float deltaTime)
        {
            // control the right paddly directly 
            PointF target = new PointF(rightPaddle.Position.X, Math.Max(Position.Y - Extent.Y + rightPaddle.Extent.Y, Math.Min(Position.Y + Extent.Y - rightPaddle.Extent.Y, bal.Position.Y)));
            rightPaddle.Position = rightPaddle.Position.Lerp(target, deltaTime * PaddleSpeedFactor);

            // the left is for the nn 
            PointF nv = bal.Velocity.Normalize();
            ActorGameState gstate = new ActorGameState()
            {
                BallX = (1f / (Extent.X * 2)) * bal.Position.X,
                BallY = (1f / (Extent.Y * 2)) * bal.Position.Y,
                PaddlyY = (1f / (Extent.Y * 2)) * leftPaddle.Position.Y,
                BallVelocityX = nv.X,
            };

            // determine amount to move from output
            ReadOnlySpan<float> state = FeedState(gstate);

            float moveY = state[0] * -100f + state[2] * 100f * (1.1f - Math.Abs(state[0]));

            switch(state.ArgMax())
            {
                case 0: moveY -= 100f; break;
                case 1: moveY += Position.Y - leftPaddle.Position.Y; break;
                case 2: moveY += 100f; break;
            }

            target = new PointF(
                leftPaddle.Position.X,
                Math.Max(Position.Y - Extent.Y + leftPaddle.Extent.Y, Math.Min(Position.Y + Extent.Y - leftPaddle.Extent.Y, leftPaddle.Position.Y + moveY))
            );
            leftPaddle.Position = leftPaddle.Position.Lerp(target, deltaTime * PaddleSpeedFactor);

            // train state 1 step 
            TrainState(gstate); 
        }

        /// <summary>
        /// render gameobjects
        /// </summary>
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            DoRender(canvas, dirtyRect);
        }


        private NeuralNetwork Network;
        private Trainer trainer;
        
        public int Generation = 0;
        public int Step = 0;
        public float Fittness = 0f;
        public float Cost = 0f;


        /// <summary>
        /// 
        /// </summary>
        public void InitNetwork()
        {
            Network = new NeuralNetwork(
                new int[]
                { 
                    ActorGameState.InputCount,
                    8,
                    4,
                    ActorGameState.OutputCount
                },
                new LayerActivationFunction[]
                {
                    LayerActivationFunction.Sigmoid,
                    LayerActivationFunction.Swish,
                    LayerActivationFunction.LeakyReLU
                });

            trainer = new Trainer();
            trainer.Reset(Network, new Trainer.Settings()
            {
                Population = 100,
                MutationChance = 0.3f,
                LearningRate = 0.01f
            });  

            Generation = 0;
            Step = 0; 
         }

        public void TrainState(ActorGameState state)
        {
            trainer.Step(
                new float[][] { state.Pattern },
                new float[][] { state.Target }, 
                null,
                null);

            Step++;
            Cost = Network.Cost;
            Fittness = Network.Fitness;

            // check for next generation 
            if (trainer.EstimateIfReady()
                ||                
                Step > trainer.settings.Steps)
            {
                Generation++;
                Step = 0; 

                List<NeuralNetwork> best = trainer.Networks.Take(10).ToList();
                if (lastBest != null)
                {
                    best.AddRange(lastBest);
                }
                best.Sort();
                lastBest = best.Take(10).ToList(); 

                trainer.Reset(Network, lastBest, trainer.settings);
            }
        }

        List<NeuralNetwork> lastBest; 

        public ReadOnlySpan<float> FeedState(ActorGameState state)
        {
            // feed gamestate into our best nn 
            Network.FeedForward(state.Pattern);
            return Network.Output.Neurons.AsSpan(); 
        }


        
 

    }


}               