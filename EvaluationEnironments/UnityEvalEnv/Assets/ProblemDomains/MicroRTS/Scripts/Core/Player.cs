using System;
using System.Xml;
using UnityEngine;

namespace Problems.MicroRTS.Core
{
    [Serializable]
    public class Player
    {
        [SerializeField] private int id = 0;
        [SerializeField] private int resources = 0;

        public Player(int id, int resources)
        {
            this.id = id;
            this.resources = resources;
        }

        public int ID => id;
        public int Resources => resources;

        public void SetResources(int newResources)
        {
            resources = newResources;
        }

        public Player Clone()
        {
            return new Player(id, resources);
        }

        public override string ToString()
        {
            return $"player {id}({resources})";
        }

        public static Player FromXML(XmlElement element)
        {
            int playerId = int.Parse(element.GetAttribute("ID"));
            int playerResources = int.Parse(element.GetAttribute("resources"));
            return new Player(playerId, playerResources);
        }
    }
}

