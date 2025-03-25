using Base;
using UnityEngine;

namespace Utils
{
    [DisallowMultipleComponent]
    public class Util : MonoBehaviour
    {
        [SerializeField] int InitialSeed = 316227711;
        [SerializeField] public System.Random Rnd;
        [SerializeField] public System.Random RndAC; // AC = Agent Controller

        private void Awake()
        {
            if (Communicator.Instance != null)
            {
                if (Communicator.Instance.RandomSeedMode == RandomSeedMode.Fixed)
                {
                    Rnd = new System.Random(Communicator.Instance.InitialSeed);
                    RndAC = new System.Random(Communicator.Instance.InitialSeed);
                }
                else if (Communicator.Instance.RandomSeedMode == RandomSeedMode.RandomAll)
                {
                    Rnd = new System.Random(Communicator.Instance.InitialSeed);
                    RndAC = new System.Random(Communicator.Instance.InitialSeed);
                }
                else
                { // RandomSeedMode.RandomPerIndividual
                    Rnd = new System.Random();
                    RndAC = new System.Random();
                }
            }
            else
            {
                Rnd = new System.Random(InitialSeed);
                RndAC = new System.Random(InitialSeed);
            }
        }

        public double NextDouble()
        {
            return Rnd.NextDouble();
        }

        public double NextDoubleBt()
        {
            return RndAC.NextDouble();
        }

        public float NextFloat(float min, float max)
        {
            double val = (Rnd.NextDouble() * (max - min) + min);
            return (float)val;
        }

        public float NextFloatdAC(float min, float max)
        {
            double val = (RndAC.NextDouble() * (max - min) + min);
            return (float)val;
        }

        public int NextInt(int min, int max)
        {
            return Rnd.Next(min, max);
        }

        public int NextIntAC(int min, int max)
        {
            return RndAC.Next(min, max);
        }

        public int NextInt(int max)
        {
            return Rnd.Next(max);
        }

        public int NextIntAC(int max)
        {
            return RndAC.Next(max);
        }
    }
}