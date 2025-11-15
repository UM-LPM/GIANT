using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace Problems.MicroRTS.Core
{
    [Serializable]
    public class UnitTypeTable
    {
        public const int EMPTY_TYPE_TABLE = -1;
        public const int VERSION_ORIGINAL = 1;
        public const int VERSION_ORIGINAL_FINETUNED = 2;
        public const int VERSION_NON_DETERMINISTIC = 3;
        public const int MOVE_CONFLICT_RESOLUTION_CANCEL_BOTH = 1;
        public const int MOVE_CONFLICT_RESOLUTION_CANCEL_RANDOM = 2;
        public const int MOVE_CONFLICT_RESOLUTION_CANCEL_ALTERNATING = 3;

        [SerializeField] private List<UnitType> unitTypes = new();
        [SerializeField] private int moveConflictResolutionStrategy = MOVE_CONFLICT_RESOLUTION_CANCEL_BOTH;

        public UnitTypeTable()
        {
            SetUnitTypeTable(VERSION_ORIGINAL, MOVE_CONFLICT_RESOLUTION_CANCEL_BOTH);
        }

        public UnitTypeTable(int version, int conflictResolutionStrategy)
        {
            SetUnitTypeTable(version, conflictResolutionStrategy);
        }

        public void SetUnitTypeTable(int version, int conflictResolutionStrategy)
        {
            moveConflictResolutionStrategy = conflictResolutionStrategy;
            if (version == EMPTY_TYPE_TABLE) return;

            CreateResource();
            CreateBase(version);
            CreateBarracks(version);
            CreateWorker();
            CreateLight();
            CreateHeavy(version);
            CreateRanged();
            SetupProductionRelationships();
        }

        private void CreateResource()
        {
            UnitType resource = new("Resource");
            resource.isResource = true;
            resource.isStockpile = false;
            resource.canHarvest = false;
            resource.canMove = false;
            resource.canAttack = false;
            resource.sightRadius = 0;
            AddUnitType(resource);
        }

        private void CreateBase(int version)
        {
            UnitType baseType = new("Base");
            baseType.cost = 10;
            baseType.HP = 10;
            baseType.produceTime = version == VERSION_ORIGINAL ? 250 : 200;
            baseType.isResource = false;
            baseType.isStockpile = true;
            baseType.canHarvest = false;
            baseType.canMove = false;
            baseType.canAttack = false;
            baseType.sightRadius = 5;
            AddUnitType(baseType);
        }

        private void CreateBarracks(int version)
        {
            UnitType barracks = new("Barracks");
            barracks.cost = 5;
            barracks.HP = 4;
            barracks.produceTime = version == VERSION_ORIGINAL ? 200 : 100;
            barracks.isResource = false;
            barracks.isStockpile = false;
            barracks.canHarvest = false;
            barracks.canMove = false;
            barracks.canAttack = false;
            barracks.sightRadius = 3;
            AddUnitType(barracks);
        }

        private void CreateWorker()
        {
            UnitType worker = new("Worker");
            worker.cost = 1;
            worker.HP = 1;
            worker.minDamage = worker.maxDamage = 1;
            worker.attackRange = 1;
            worker.produceTime = 50;
            worker.moveTime = 10;
            worker.attackTime = 5;
            worker.harvestTime = 20;
            worker.returnTime = 10;
            worker.harvestAmount = 1;
            worker.sightRadius = 3;
            worker.isResource = false;
            worker.isStockpile = false;
            worker.canHarvest = true;
            worker.canMove = true;
            worker.canAttack = true;
            AddUnitType(worker);
        }

        private void CreateLight()
        {
            UnitType light = new("Light");
            light.cost = 2;
            light.HP = 4;
            light.minDamage = 2;
            light.maxDamage = 2;
            light.attackRange = 1;
            light.produceTime = 80;
            light.moveTime = 8;
            light.attackTime = 5;
            light.harvestTime = 20;
            light.returnTime = 10;
            light.harvestAmount = 1;
            light.sightRadius = 2;
            light.isResource = false;
            light.isStockpile = false;
            light.canHarvest = false;
            light.canMove = true;
            light.canAttack = true;
            AddUnitType(light);
        }

        private void CreateHeavy(int version)
        {
            UnitType heavy = new("Heavy");

            switch (version)
            {
                case VERSION_ORIGINAL:
                    heavy.moveTime = 12;
                    heavy.HP = 4;
                    heavy.cost = 2;
                    heavy.minDamage = heavy.maxDamage = 4;
                    break;
                case VERSION_ORIGINAL_FINETUNED:
                    heavy.moveTime = 10;
                    heavy.HP = 8;
                    heavy.cost = 3;
                    heavy.minDamage = heavy.maxDamage = 4;
                    break;
                case VERSION_NON_DETERMINISTIC:
                    heavy.moveTime = 10;
                    heavy.HP = 8;
                    heavy.cost = 3;
                    heavy.minDamage = 0;
                    heavy.maxDamage = 6;
                    break;
            }

            heavy.attackRange = 1;
            heavy.produceTime = 120;
            heavy.attackTime = 5;
            heavy.harvestTime = 20;
            heavy.returnTime = 10;
            heavy.harvestAmount = 1;
            heavy.sightRadius = 2;
            heavy.isResource = false;
            heavy.isStockpile = false;
            heavy.canHarvest = false;
            heavy.canMove = true;
            heavy.canAttack = true;
            AddUnitType(heavy);
        }

        private void CreateRanged()
        {
            UnitType ranged = new("Ranged");
            ranged.cost = 2;
            ranged.HP = 1;
            ranged.minDamage = 1;
            ranged.maxDamage = 1;
            ranged.attackRange = 3;
            ranged.produceTime = 100;
            ranged.moveTime = 10;
            ranged.attackTime = 5;
            ranged.harvestTime = 20;
            ranged.returnTime = 10;
            ranged.harvestAmount = 1;
            ranged.sightRadius = 3;
            ranged.isResource = false;
            ranged.isStockpile = false;
            ranged.canHarvest = false;
            ranged.canMove = true;
            ranged.canAttack = true;
            AddUnitType(ranged);
        }

        private void SetupProductionRelationships()
        {
            var baseType = GetUnitType("Base");
            var barracks = GetUnitType("Barracks");
            var worker = GetUnitType("Worker");
            var light = GetUnitType("Light");
            var heavy = GetUnitType("Heavy");
            var ranged = GetUnitType("Ranged");

            baseType.Produces(worker);
            barracks.Produces(light);
            barracks.Produces(heavy);
            barracks.Produces(ranged);
            worker.Produces(baseType);
            worker.Produces(barracks);
        }

        public void AddUnitType(UnitType unitType)
        {
            unitType.ID = unitTypes.Count;
            unitTypes.Add(unitType);
        }

        public UnitType GetUnitType(int id)
        {
            if (id >= 0 && id < unitTypes.Count)
            {
                return unitTypes[id];
            }
            return null;
        }

        public UnitType GetUnitType(string name)
        {
            return unitTypes.FirstOrDefault(ut => ut.name.Equals(name));
        }

        public List<UnitType> GetUnitTypes()
        {
            return unitTypes;
        }

        public int GetMoveConflictResolutionStrategy()
        {
            return moveConflictResolutionStrategy;
        }

        public int GetMaxAttackRange()
        {
            int maxAttackRange = 0;
            foreach (UnitType unitType in unitTypes)
            {
                if (unitType.attackRange > maxAttackRange)
                {
                    maxAttackRange = unitType.attackRange;
                }
            }
            return maxAttackRange;
        }

        public static UnitTypeTable FromXML(XmlElement element)
        {
            UnitTypeTable unitTypeTable = new(EMPTY_TYPE_TABLE, MOVE_CONFLICT_RESOLUTION_CANCEL_BOTH);
            unitTypeTable.moveConflictResolutionStrategy = int.Parse(element.GetAttribute("moveConflictResolutionStrategy"));

            XmlNodeList unitTypeNodes = element.SelectNodes("*");
            foreach (XmlElement unitTypeElement in unitTypeNodes)
            {
                UnitType unitType = UnitType.CreateStub(unitTypeElement);
                unitTypeTable.AddUnitType(unitType);
            }

            foreach (XmlElement unitTypeElement in unitTypeNodes)
            {
                string name = unitTypeElement.GetAttribute("name");
                UnitType unitType = unitTypeTable.GetUnitType(name);
                if (unitType != null)
                {
                    unitType.UpdateFromXML(unitTypeElement, unitTypeTable);
                }
            }

            return unitTypeTable;
        }

        public override string ToString()
        {
            return $"UnitTypeTable({unitTypes.Count} types, conflict resolution: {moveConflictResolutionStrategy})";
        }
    }
}

