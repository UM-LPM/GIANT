using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Problems.MicroRTS.Core
{
    [Serializable]
    public class UnitType
    {
        [SerializeField] public int ID = 0;
        [SerializeField] public string name;
        [SerializeField] public int cost = 1;
        [SerializeField] public int HP = 1;
        [SerializeField] public int minDamage = 1;
        [SerializeField] public int maxDamage = 1;
        [SerializeField] public int attackRange = 1;
        [SerializeField] public int produceTime = 10;
        [SerializeField] public int moveTime = 10;
        [SerializeField] public int attackTime = 10;
        [SerializeField] public int harvestTime = 10;
        [SerializeField] public int returnTime = 10;
        [SerializeField] public int harvestAmount = 1;
        [SerializeField] public int sightRadius = 4;
        [SerializeField] public bool isResource = false;
        [SerializeField] public bool isStockpile = false;
        [SerializeField] public bool canHarvest = false;
        [SerializeField] public bool canMove = true;
        [SerializeField] public bool canAttack = true;
        [NonSerialized] public List<UnitType> produces = new();
        [NonSerialized] public List<UnitType> producedBy = new();

        public UnitType()
        {
        }

        public UnitType(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), "Unit type name cannot be null or empty");
            this.name = name;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UnitType))
                return false;
            return name.Equals(((UnitType)obj).name);
        }

        public void Produces(UnitType unitType)
        {
            Debug.Assert(unitType != null, "Cannot add null unit type to produces list");
            produces.Add(unitType);
            unitType.producedBy.Add(this);
        }

        public static UnitType CreateStub(XmlElement element)
        {
            Debug.Assert(element != null, "XML element cannot be null");
            UnitType unitType = new();
            unitType.ID = int.Parse(element.GetAttribute("ID"));
            unitType.name = element.GetAttribute("name");
            Debug.Assert(!string.IsNullOrEmpty(unitType.name), "Unit type name from XML cannot be null or empty");
            return unitType;
        }

        public void UpdateFromXML(XmlElement element, UnitTypeTable unitTypeTable)
        {
            cost = int.Parse(element.GetAttribute("cost"));
            HP = int.Parse(element.GetAttribute("hp"));
            minDamage = int.Parse(element.GetAttribute("minDamage"));
            maxDamage = int.Parse(element.GetAttribute("maxDamage"));
            attackRange = int.Parse(element.GetAttribute("attackRange"));

            produceTime = int.Parse(element.GetAttribute("produceTime"));
            moveTime = int.Parse(element.GetAttribute("moveTime"));
            attackTime = int.Parse(element.GetAttribute("attackTime"));
            harvestTime = int.Parse(element.GetAttribute("harvestTime"));
            returnTime = int.Parse(element.GetAttribute("returnTime"));

            harvestAmount = int.Parse(element.GetAttribute("harvestAmount"));
            sightRadius = int.Parse(element.GetAttribute("sightRadius"));

            isResource = bool.Parse(element.GetAttribute("isResource"));
            isStockpile = bool.Parse(element.GetAttribute("isStockpile"));
            canHarvest = bool.Parse(element.GetAttribute("canHarvest"));
            canMove = bool.Parse(element.GetAttribute("canMove"));
            canAttack = bool.Parse(element.GetAttribute("canAttack"));

            XmlNodeList producesNodes = element.SelectNodes("produces");
            foreach (XmlElement producesElement in producesNodes)
            {
                string typeName = producesElement.GetAttribute("type");
                UnitType producedType = unitTypeTable.GetUnitType(typeName);
                if (producedType != null)
                {
                    produces.Add(producedType);
                }
            }

            XmlNodeList producedByNodes = element.SelectNodes("producedBy");
            foreach (XmlElement producedByElement in producedByNodes)
            {
                string typeName = producedByElement.GetAttribute("type");
                UnitType producerType = unitTypeTable.GetUnitType(typeName);
                if (producerType != null)
                {
                    producedBy.Add(producerType);
                }
            }
        }

        public static UnitType FromXML(XmlElement element, UnitTypeTable unitTypeTable)
        {
            UnitType unitType = new();
            unitType.UpdateFromXML(element, unitTypeTable);
            return unitType;
        }

        public override string ToString()
        {
            return name;
        }
    }
}

