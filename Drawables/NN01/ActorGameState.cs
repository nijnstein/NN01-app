using Microsoft.Maui.Animations;
using NN01;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NN01_app 
{
    public class ActorGameState
    {
        public float PaddlyY;        
        public float BallX;
        public float BallY;
        public float BallVelocityX;
        //public float BallVelocityY;
        //public float BallDistance;

        static public int InputCount => 4;
        static public int OutputCount => 3; 

        public float[] Pattern
        {
            get
            {
                return new float[] { PaddlyY, BallX, BallY , BallVelocityX/*, BallVelocityY, BallDistance */ };
            }
        }
        public float[] Target
        {
            get
            {
                float[] target = new float[] { 0, 0, 0 };

                if (BallVelocityX < 0f)
                {
                    if (BallY < PaddlyY) target[0] = 1f;
                    if (BallY > PaddlyY) target[2] = 1f;
                }
                else
                {
                    target[1] = 1f;

                    /* move to middle 
                    if (PaddlyY < 0.5f) 
                    {
                        target[2] = 0.2f;
                    }
                    else
                    if (PaddlyY < 0.2)
                    {
                        target[2] = 0.3f;
                    }

                    if (PaddlyY > 0.7)
                    {
                        target[0] = 0.3f;
                    }
                    else
                    if (PaddlyY > 0.5)
                    {
                        target[0] = 0.2f;
                    } */ 
                }

                return target; 
            }
        }

        public ActorGameState() { }

        public ActorGameState(float[] pattern)
        {
            PaddlyY = pattern[0];
            BallX = pattern[1];
            BallY = pattern[2];
            BallVelocityX = pattern[3];
           // BallVelocityY = pattern[4];
           // BallDistance = pattern[5]; 
        }
    }
}
