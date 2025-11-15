using System;
using System.Xml;
using UnityEngine;

namespace Problems.MicroRTS.Core
{
    [Serializable]
    public class Unit
    {
        [SerializeField] private UnitType type;
        [SerializeField] private long id;
        [SerializeField] private int player;
        [SerializeField] private int x, y;
        [SerializeField] private int resources;
        [SerializeField] private int hitpoints = 0;

        public static long nextID = 0;

        public Unit(long id, int player, UnitType type, int x, int y, int resources)
        {
            this.player = player;
            this.type = type;
            this.x = x;
            this.y = y;
            this.resources = resources;
            hitpoints = type.HP;
            this.id = id;
            if (id >= nextID)
            {
                nextID = id + 1;
            }
        }

        public Unit(int player, UnitType type, int x, int y, int resources)
        {
            this.player = player;
            this.type = type;
            this.x = x;
            this.y = y;
            this.resources = resources;
            hitpoints = type.HP;
            id = nextID++;
        }

        public Unit(int player, UnitType type, int x, int y)
        {
            this.player = player;
            this.type = type;
            this.x = x;
            this.y = y;
            resources = 0;
            hitpoints = type.HP;
            id = nextID++;
        }

        public Unit(Unit other)
        {
            player = other.player;
            type = other.type;
            x = other.x;
            y = other.y;
            resources = other.resources;
            hitpoints = other.hitpoints;
            id = other.id;
        }

        public int Player => player;
        public UnitType Type => type;
        public long ID => id;
        public int X => x;
        public int Y => y;
        public int Resources => resources;
        public int HitPoints => hitpoints;
        public int MaxHitPoints => type.HP;

        public void SetType(UnitType newType)
        {
            type = newType;
        }

        public void SetID(long newId)
        {
            id = newId;
        }

        public void SetX(int newX)
        {
            x = newX;
        }

        public void SetY(int newY)
        {
            y = newY;
        }

        public void SetResources(int newResources)
        {
            resources = newResources;
        }

        public void SetHitPoints(int newHitPoints)
        {
            hitpoints = newHitPoints;
        }

        public int Cost => type.cost;
        public int MoveTime => type.moveTime;
        public int AttackTime => type.attackTime;
        public int AttackRange => type.attackRange;
        public int MinDamage => type.minDamage;
        public int MaxDamage => type.maxDamage;
        public int HarvestAmount => type.harvestAmount;
        public int HarvestTime => type.harvestTime;

        public Unit Clone()
        {
            return new Unit(this);
        }

        public override string ToString()
        {
            return type.name + "(" + id + ")";
        }

        public static Unit FromXML(XmlElement element, UnitTypeTable unitTypeTable)
        {
            string typeName = element.GetAttribute("type");
            string idStr = element.GetAttribute("ID");
            string playerStr = element.GetAttribute("player");
            string xStr = element.GetAttribute("x");
            string yStr = element.GetAttribute("y");
            string resourcesStr = element.GetAttribute("resources");
            string hitpointsStr = element.GetAttribute("hitpoints");

            long id = long.Parse(idStr);
            if (id >= nextID)
            {
                nextID = id + 1;
            }
            UnitType type = unitTypeTable.GetUnitType(typeName);
            int player = int.Parse(playerStr);
            int x = int.Parse(xStr);
            int y = int.Parse(yStr);
            int resources = int.Parse(resourcesStr);
            int hitpoints = int.Parse(hitpointsStr);

            Unit unit = new(id, player, type, x, y, resources);
            unit.hitpoints = hitpoints;
            return unit;
        }
    }
}

