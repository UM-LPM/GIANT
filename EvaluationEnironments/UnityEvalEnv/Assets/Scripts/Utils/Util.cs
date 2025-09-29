using Base;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    [DisallowMultipleComponent]
    public class Util : MonoBehaviour
    {
        [SerializeField] int InitialSeed = 316227711;
        [SerializeField] public System.Random Rnd;
        [SerializeField] public Dictionary<int, System.Random> RndACs = new Dictionary<int, System.Random>(); // AC = Agent Controller

        private void Awake()
        {
            if (Communicator.Instance != null)
            {
                if (Communicator.Instance.RandomSeedMode == RandomSeedMode.Fixed)
                {
                    Rnd = new System.Random(Communicator.Instance.InitialSeed);
                }
                else if (Communicator.Instance.RandomSeedMode == RandomSeedMode.RandomAll)
                {
                    Rnd = new System.Random(Communicator.Instance.InitialSeed);
                }
                else
                { // RandomSeedMode.RandomPerIndividual
                    Rnd = new System.Random();
                }
            }
            else
            {
                Rnd = new System.Random(InitialSeed);
            }
        }

        public double NextDouble()
        {
            return Rnd.NextDouble();
        }

        public float NextFloat(float min, float max)
        {
            double val = (Rnd.NextDouble() * (max - min) + min);
            return (float)val;
        }

        public float NextFloatdAC(int individualInstanceId, float min, float max)
        {
            double val = (GetRndAC(individualInstanceId).NextDouble() * (max - min) + min);
            return (float)val;
        }

        public int NextInt(int min, int max)
        {
            return Rnd.Next(min, max);
        }

        public int NextIntAC(int individualInstanceId, int min, int max)
        {
            return GetRndAC(individualInstanceId).Next(min, max);
        }

        public int NextInt(int max)
        {
            return Rnd.Next(max);
        }

        public int NextIntAC(int individualInstanceId, int max)
        {
            return GetRndAC(individualInstanceId).Next(max);
        }

        private System.Random GetRndAC(int individualInstanceId)
        {
            if (!RndACs.ContainsKey(individualInstanceId))
            {
                InitializeNewRndAC(individualInstanceId);
            }
            return RndACs[individualInstanceId];
        }

        private void InitializeNewRndAC(int individualInstanceId)
        {
            if (Communicator.Instance != null)
            {
                if (Communicator.Instance.RandomSeedMode == RandomSeedMode.Fixed)
                {
                    RndACs[individualInstanceId] = new System.Random(Communicator.Instance.InitialSeed);
                }
                else if (Communicator.Instance.RandomSeedMode == RandomSeedMode.RandomAll)
                {
                    RndACs[individualInstanceId] = new System.Random(Communicator.Instance.InitialSeed);
                }
                else
                { // RandomSeedMode.RandomPerIndividual
                    RndACs[individualInstanceId] = new System.Random();
                }
            }
            else
            {
                RndACs[individualInstanceId] = new System.Random(InitialSeed);
            }
        }
    }
}