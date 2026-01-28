using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Problems.Mario
{
    public class MysteryBlockComponent: BlockComponent
    {
        public MysteryBlockContent MysteryBlockContent;
        public GameObject EmptyBlockComponent;

        public override void OnHit(MarioAgentComponent agent)
        {
            if (IsHit)
                return;

            switch (MysteryBlockContent)
            {
                case MysteryBlockContent.Coin:
                    MarioEnvironmentController.OnMysteryBlockWithCoinHit(agent);
                    Instantiate(EmptyBlockComponent, transform.position, Quaternion.identity, transform.parent);
                    Destroy(gameObject);
                    break;
                default:
                    throw new Exception("Unknown MysteryBlockContent type!");
            }

            IsHit = true;
        }
    }

    public enum MysteryBlockContent
    {
        Coin
    }
}
