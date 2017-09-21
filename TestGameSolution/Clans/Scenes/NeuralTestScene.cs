using Juixel.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Juixel;
using Neural;
using Juixel.Drawing.Textures;
using Juixel.Input;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Utilities.Logging;
using Utilities.Tools;
using System.Threading;

namespace Clans.Scenes
{
    public class NeuralTestScene : Scene, IKeyHandler
    {
        private const int Save_Amount = 20;
        private const int Neural_Network_Count = 100;

        private NeuralSet[] NeuralNetworks = new NeuralSet[Neural_Network_Count];
        private SpriteNode[] VisualParticles = new SpriteNode[Neural_Network_Count];

        private int SelectedNetwork = -1;
        private LabelNode CurrentNetworkLabel;
        private LabelNode InputLabel;

        private int Generation = 1;

        private int[] Layers = new int[] { 2, 10, 2 };

        public NeuralTestScene(Location Size) : base(Size)
        {
            for (int i = 0; i < NeuralNetworks.Length; i++)
                NeuralNetworks[i] = CreateInitialNetwork(0.5f, 0.5f);

            Sprite S = new Sprite(ClansGame.Particles_8x8, new Rectangle(0, 0, 8, 8));
            for (int i = 0; i < VisualParticles.Length; i++)
            {
                SpriteNode P = new SpriteNode(S);
                P.Scale = 1;
                AddChild(P);
                VisualParticles[i] = P;
            }

            CurrentNetworkLabel = new LabelNode(Font.Default, $"All Networks - Generation {Generation}", 20);
            CurrentNetworkLabel.AnchorPoint = 0.5;
            CurrentNetworkLabel.Position = new Location(Size.X / 2, Size.Y * 0.9);
            CurrentNetworkLabel.Layer = 100;
            AddChild(CurrentNetworkLabel);

            InputLabel = new LabelNode(Font.Default, $"", 20);
            InputLabel.AnchorPoint = 0.5;
            InputLabel.Position = new Location(Size.X / 2, Size.Y * 0.9 + 40);
            InputLabel.Layer = 100;
            AddChild(InputLabel);

            Input.ListenForKey(Keys.Left, this);
            Input.ListenForKey(Keys.Right, this);

            Input.ListenForKey(Keys.D0, this);
            Input.ListenForKey(Keys.D1, this);
            Input.ListenForKey(Keys.D2, this);
            Input.ListenForKey(Keys.D3, this);
            Input.ListenForKey(Keys.D4, this);
            Input.ListenForKey(Keys.D5, this);
            Input.ListenForKey(Keys.D6, this);
            Input.ListenForKey(Keys.D7, this);
            Input.ListenForKey(Keys.D8, this);
            Input.ListenForKey(Keys.D9, this);

            Input.ListenForKey(Keys.Back, this);
            Input.ListenForKey(Keys.Enter, this);

            Input.ListenForKey(Keys.M, this);
            Input.ListenForKey(Keys.R, this);
        }

        private NeuralSet CreateInitialNetwork(float StartX, float StartY)
        {
            return new NeuralSet(Layers, StartX, StartY);
        }

        private const float Fast_Iteration_Step = 1 / 60.0f;
        private const float Particle_Move_Speed_PPS = 100;

        public bool Running = false;

        private void RunIterations(int Amount)
        {
            Running = true;
            new Thread(() =>
            {
                for (int i = 0; i < Amount; i++)
                {
                    Parallel.ForEach(NeuralNetworks, _ =>
                    {
                        float X = (float)JRandom.NextDouble();
                        float Y = (float)JRandom.NextDouble();
                        while (!_.Dead)
                            StepIterationSingle(_, Fast_Iteration_Step, X, Y);
                    });
                    Mutate();
                    InputLabel.Text = $"{i} / {Amount}";
                }

                InputLabel.Text = "";
                Running = false;
            }).Start();
        }

        private void StepIteration(float StepTime, float TargetX, float TargetY)
        {
            float XSpeed = (Particle_Move_Speed_PPS / JuixelGame.WindowWidth) * StepTime;
            float YSpeed = (Particle_Move_Speed_PPS / JuixelGame.WindowHeight) * StepTime;
            for (int i = 0; i < NeuralNetworks.Length; i++)
            {
                NeuralSet Set = NeuralNetworks[i];
                Set.Run(TargetX, TargetY, XSpeed, YSpeed, StepTime);
            }
        }

        private void StepIterationSingle(NeuralSet Set, float StepTime, float TargetX, float TargetY)
        {
            float XSpeed = (Particle_Move_Speed_PPS / JuixelGame.WindowWidth) * StepTime;
            float YSpeed = (Particle_Move_Speed_PPS / JuixelGame.WindowHeight) * StepTime;
            Set.Run(TargetX, TargetY, XSpeed, YSpeed, StepTime);
        }

        private void Mutate()
        {
            Array.Sort(NeuralNetworks);
            //Logger.Log(NeuralNetworks[0].Network.Accuracy);
            //Logger.Log(NeuralNetworks[NeuralNetworks.Length - 1].Network.Accuracy);
            for (int i = 0; i < NeuralNetworks.Length; i++)
                if (i >= Save_Amount)
                {
                    NeuralNetworks[i].Network = new NeuralNetwork(NeuralNetworks[i % Save_Amount].Network);
                    NeuralNetworks[i].Network.Mutate();
                }
            Reset();
            Generation++;
        }

        private void Reset()
        {
            for (int i = 0; i < NeuralNetworks.Length; i++)
                NeuralNetworks[i].Reset();
        }

        public override void Update(JuixelTime Time)
        {
            base.Update(Time);

            if (!Running)
                StepIteration((float)Time.ElapsedSec, (float)Input.MouseX / JuixelGame.WindowWidth, (float)Input.MouseY / JuixelGame.WindowHeight);
            for (int i = 0; i < NeuralNetworks.Length; i++)
            {
                NeuralSet Set = NeuralNetworks[i];
                SpriteNode S = VisualParticles[i];
                if (SelectedNetwork < 0 || SelectedNetwork == i)
                {
                    S.Hidden = false;
                    S.Position = new Location(Set.Particle.X * JuixelGame.WindowWidth, Set.Particle.Y * JuixelGame.WindowHeight);
                }
                else
                    S.Hidden = true;
            }

            if (SelectedNetwork < 0)
                CurrentNetworkLabel.Text = $"All Networks - Generation {Generation}";
            else
                CurrentNetworkLabel.Text = $"{SelectedNetwork} / {NeuralNetworks.Length} - Generation {Generation}";
        }

        public void KeyDown(Keys Key)
        {
            if (Running)
                return;

            if (Key == Keys.Left)
                SelectedNetwork--;
            else if (Key == Keys.Right)
                SelectedNetwork++;
            else if (Key == Keys.Back)
            {
                if (InputLabel.Text != "")
                    InputLabel.Text = InputLabel.Text.Substring(0, InputLabel.Text.Length - 1);
            }
            else if (Key == Keys.Enter)
            {
                if (InputLabel.Text != "")
                {
                    int Value = int.Parse(InputLabel.Text);
                    InputLabel.Text = "";
                    RunIterations(Value);
                }
            }
            else if (Key == Keys.M)
                Mutate();
            else if (Key == Keys.R)
                Reset();
            else if (Key != Keys.D0 || InputLabel.Text != "")
            {
                InputLabel.Text += (int)Key - 48;
                return;
            }

            SelectedNetwork = Math.Max(-1, Math.Min(SelectedNetwork, NeuralNetworks.Length));
        }

        public void KeyUp(Keys Key)
        {
            
        }

        private struct NeuralParticle
        {
            public float X;
            public float Y;
        }

        private class NeuralSet : IComparable<NeuralSet>
        {
            public NeuralNetwork Network;
            public NeuralParticle Particle;

            private float StartX;
            private float StartY;

            public float Time = 0;
            public float Accuracy = 0;
            public bool Dead = false;

            public float Fitness => Accuracy / (Time * 2);

            public NeuralSet(int[] Layers, float StartX, float StartY)
            {
                this.StartX = StartX;
                this.StartY = StartY;

                Network = new NeuralNetwork(Layers);
                Particle = new NeuralParticle
                {
                    X = StartX,
                    Y = StartY
                };
            }

            public int CompareTo(NeuralSet other)
            {
                if (Fitness > other.Fitness)
                    return -1;
                else if (Fitness < other.Fitness)
                    return 1;
                else
                    return 0;
            }

            public void Reset()
            {
                Dead = false;
                Time = 0;
                Accuracy = 0;
                Particle.X = StartX;
                Particle.Y = StartY;
            }

            public void Run(float TargetX, float TargetY, float ParticleMoveSpeedX, float ParticleMoveSpeedY, float Time)
            {
                Location Target = new Location(TargetX, TargetY);
                Location ParticleStart = new Location(Particle.X, Particle.Y);
                double StartDistance = Target.Distance(ParticleStart);

                Location Vector = (Target - ParticleStart).Angle.Location;

                float[] Output = Network.FeedForward(new float[] { (float)Vector.X, (float)Vector.Y });

                Particle.X += Output[0] * 2 * ParticleMoveSpeedX;
                Particle.Y += Output[1] * 2 * ParticleMoveSpeedY;

                Location New = new Location(Particle.X, Particle.Y);
                double NewDistance = Target.Distance(New);
                float Accuracy = (float)(StartDistance - NewDistance);

                if (NewDistance >= StartDistance || Time >= 1)
                    Dead = true;
                Time += Time;
                Accuracy += Accuracy * Accuracy * (Accuracy / Math.Abs(Accuracy));
            }
        }
    }
}
