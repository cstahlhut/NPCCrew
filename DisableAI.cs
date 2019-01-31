using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI;
using IMyRemoteControl = Sandbox.ModAPI.IMyRemoteControl;

namespace Stollie.NPCCrew
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class DisableAI : MySessionComponentBase
    {
        public static HashSet<IMyEntity> EntityList = new HashSet<IMyEntity>();
        private List<IMyPlayer> PlayerList = new List<IMyPlayer>();
        private HashSet<long> PlayerListLong = new HashSet<long>();
        private bool initRun = false;
        private int tick = 0;
        private string blockToCheckFor = "NPC_CombatMobility";
        private bool disableAIModeEnabled = false; 

        public void Init()
        {
            NPCCrewConfig loadconfig = NPCCrewConfig.LoadConfigFile();
            disableAIModeEnabled = loadconfig.disableAIModeEnabled;
            if (disableAIModeEnabled)
            {
                EntityList.Clear();
                PlayerList.Clear();
                MyAPIGateway.Entities.GetEntities(EntityList);
                MyAPIGateway.Players.GetPlayers(PlayerList);
                foreach (var player in PlayerList)
                {
                    PlayerListLong.Add(player.IdentityId);
                }
                MyAPIGateway.Entities.OnEntityAdd += OnEntityAdded;
                MyAPIGateway.Entities.OnEntityRemove += OnEntityRemoved;
                //MyAPIGateway.Utilities.ShowMessage("DEBUG", "DisableAI Mode Running");
                DisableAIFunction();
            }
        }

        public override void UpdateBeforeSimulation()
        {
            if (!initRun)
            {
                Init();
                initRun = true;
            }

            if (initRun && tick == 100 && disableAIModeEnabled)
            {
                DisableAIFunction();
                tick = 0;
            }
            tick++;
        }

        public void DisableAIFunction()
        {
            foreach (var entity in EntityList)
            {
                var cubeGrid = entity as IMyCubeGrid;
                if (cubeGrid == null)
                {
                    continue;
                }

                var entireBlockList = new List<IMySlimBlock>();
                var npcBlockList = new List<IMyRemoteControl>();
                var npcBlocksPresent = 0;
                var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(entity as IMyCubeGrid);
                gts.GetBlocksOfType(npcBlockList);
                cubeGrid.GetBlocks(entireBlockList);
                foreach (var block in npcBlockList)
                {
                    if (block.BlockDefinition.SubtypeName == blockToCheckFor)
                    {
                        npcBlocksPresent++;
                    }
                }
                foreach (var block in entireBlockList)
                {
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_LargeGatlingTurret) &&
                        npcBlocksPresent == 0 && (PlayerListLong.Contains(block.OwnerId) || block.OwnerId == 0))
                    {
                        var blockAsTurret = block.FatBlock as IMyLargeGatlingTurret;
                        if (blockAsTurret != null)
                        {
                            //MyAPIGateway.Utilities.ShowMessage("DEBUG", "Turret AI Disabled");
                            if (blockAsTurret.HasAction("TargetMeteors_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetMeteors_Off");
                            }
                            if (blockAsTurret.HasAction("TargetMissiles_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetMissiles_Off");
                            }
                            if (blockAsTurret.HasAction("TargetSmallShips_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetSmallShips_Off");
                            }
                            if (blockAsTurret.HasAction("TargetLargeShips_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetLargeShips_Off");
                            }
                            if (blockAsTurret.HasAction("TargetCharacters_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetCharacters_Off");
                            }
                            if (blockAsTurret.HasAction("TargetStations_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetStations_Off");
                            }
                            if (blockAsTurret.HasAction("TargetNeutrals_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetNeutrals_Off");
                            }
                        }
                    }
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_LargeGatlingTurret) &&
                        npcBlocksPresent == 0 && (PlayerListLong.Contains(block.OwnerId) || block.OwnerId == 0))
                    {
                        var blockAsTurret = block.FatBlock as IMyLargeGatlingTurret;
                        if (blockAsTurret != null)
                        {
                            //MyAPIGateway.Utilities.ShowMessage("DEBUG", "Turret AI Disabled");
                            if (blockAsTurret.HasAction("TargetMeteors_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetMeteors_Off");
                            }
                            if (blockAsTurret.HasAction("TargetMissiles_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetMissiles_Off");
                            }
                            if (blockAsTurret.HasAction("TargetSmallShips_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetSmallShips_Off");
                            }
                            if (blockAsTurret.HasAction("TargetLargeShips_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetLargeShips_Off");
                            }
                            if (blockAsTurret.HasAction("TargetCharacters_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetCharacters_Off");
                            }
                            if (blockAsTurret.HasAction("TargetStations_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetStations_Off");
                            }
                            if (blockAsTurret.HasAction("TargetNeutrals_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetNeutrals_Off");
                            }
                        }
                    }
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_LargeMissileTurret) &&
                        npcBlocksPresent == 0 && (PlayerListLong.Contains(block.OwnerId) || block.OwnerId == 0))
                    {
                        var blockAsTurret = block.FatBlock as IMyLargeMissileTurret;
                        if (blockAsTurret != null)
                        {
                            //MyAPIGateway.Utilities.ShowMessage("DEBUG", "Turret AI Disabled");
                            if (blockAsTurret.HasAction("TargetMeteors_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetMeteors_Off");
                            }
                            if (blockAsTurret.HasAction("TargetMissiles_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetMissiles_Off");
                            }
                            if (blockAsTurret.HasAction("TargetSmallShips_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetSmallShips_Off");
                            }
                            if (blockAsTurret.HasAction("TargetLargeShips_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetLargeShips_Off");
                            }
                            if (blockAsTurret.HasAction("TargetCharacters_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetCharacters_Off");
                            }
                            if (blockAsTurret.HasAction("TargetStations_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetStations_Off");
                            }
                            if (blockAsTurret.HasAction("TargetNeutrals_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetNeutrals_Off");
                            }
                        }
                    }
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_InteriorTurret) &&
                        npcBlocksPresent == 0 && (PlayerListLong.Contains(block.OwnerId) || block.OwnerId == 0))
                    {
                        var blockAsTurret = block.FatBlock as IMyLargeInteriorTurret;
                        if (blockAsTurret != null)
                        {
                            //MyAPIGateway.Utilities.ShowMessage("DEBUG", "Turret AI Disabled");
                            if (blockAsTurret.HasAction("TargetMeteors_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetMeteors_Off");
                            }
                            if (blockAsTurret.HasAction("TargetMissiles_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetMissiles_Off");
                            }
                            if (blockAsTurret.HasAction("TargetSmallShips_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetSmallShips_Off");
                            }
                            if (blockAsTurret.HasAction("TargetLargeShips_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetLargeShips_Off");
                            }
                            if (blockAsTurret.HasAction("TargetCharacters_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetCharacters_Off");
                            }
                            if (blockAsTurret.HasAction("TargetStations_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetStations_Off");
                            }
                            if (blockAsTurret.HasAction("TargetNeutrals_Off"))
                            {
                                blockAsTurret.ApplyAction("TargetNeutrals_Off");
                            }
                        }
                    }
                }
            }
        }

        public void OnEntityAdded(IMyEntity entity)
        {
            if (!EntityList.Contains(entity))
            {
                EntityList.Add(entity);
            }
        }

        public void OnEntityRemoved(IMyEntity entity)
        {
            if (EntityList.Contains(entity))
            {
                EntityList.Remove(entity);
            }
        }

        public void Close()
        {
            MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdded;
            MyAPIGateway.Entities.OnEntityRemove -= OnEntityRemoved;
        }
    }
}
