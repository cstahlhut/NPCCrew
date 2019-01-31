using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace Stollie.NPCCrew
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class NPC_SessionCore : MySessionComponentBase
    {
        public static HashSet<IMyEntity> entityList = new HashSet<IMyEntity>();
        public static HashSet<IMyCubeGrid> gridsList = new HashSet<IMyCubeGrid>();
        
        public static List<IMyRemoteControl> npcCombatList = new List<IMyRemoteControl>();
        public static HashSet<IMyRemoteControl> npcCombatListWorking = new HashSet<IMyRemoteControl>();
        public static HashSet<IMyRemoteControl> npcCombatListNotWorking = new HashSet<IMyRemoteControl>();
        
        public static List<IMyUpgradeModule> npcProductionList = new List<IMyUpgradeModule>();
        public static HashSet<IMyUpgradeModule> npcProductionListWorking = new HashSet<IMyUpgradeModule>();
        public static HashSet<IMyUpgradeModule> npcProductionListNotWorking = new HashSet<IMyUpgradeModule>();

        public static List<IMyUpgradeModule> npcEngineerList = new List<IMyUpgradeModule>();
        public static HashSet<IMyUpgradeModule> npcEngineerListWorking = new HashSet<IMyUpgradeModule>();
        public static HashSet<IMyUpgradeModule> npcEngineerListNotWorking = new HashSet<IMyUpgradeModule>();

        internal string combatNpcSubtypeName = "NPC_CombatMobility";
        internal string engineeringNpcSubtypeName = "NPC_Engineering";
        internal string productionNpcSubtypeName = "NPC_Production";

        internal string combatNpcUpgradeDetails = "";
        internal string engineerNpcUpgradeDetails = "";
        internal string productionNpcUpgradeDetails = "";

        internal float thrustOutputMulitplierControl = 3.0f;
        internal float gyroOutputMulitplierControl = 3.0f;

        internal float powerOutputMulitplierControl = 3.0f;
        internal float powerConsumptionDivisionControl = 2.0f;

        internal float gasGeneratorOutputMulitplierControl = 3.0f;
        internal float drillOutputMulitplierControl = 2.0f;

        internal int tickCounter = 0;
        internal int tickCounterIncrement = 1;

        internal bool initalized = false;
        internal bool testColourOn = false;

        internal Vector3 upgradeTestColor = Color.Green.ColorToHSV();
        internal Vector3 downgradeTestColor = Color.Red.ColorToHSV();
        
        public override void UpdateBeforeSimulation()
        {
            if (initalized == false)
            {
                initalized = true;
                Initalize();
            }

            tickCounter += tickCounterIncrement;
            if (tickCounter < 60)
            {
                return;
            }
            tickCounter = 0;

            if (MyAPIGateway.Multiplayer.IsServer == false)
            {
                return;
            }
        }

        public void Initalize()
        {

            NPCCrewConfig loadconfig = NPCCrewConfig.LoadConfigFile();
            thrustOutputMulitplierControl = loadconfig.thrustOutputMulitplierControl;
            gyroOutputMulitplierControl = loadconfig.gyroOutputMulitplierControl;

            powerOutputMulitplierControl = loadconfig.powerOutputMulitplierControl;
            powerConsumptionDivisionControl = loadconfig.powerConsumptionDivisionControl;

            gasGeneratorOutputMulitplierControl = loadconfig.gasGeneratorOutputMulitplierControl;
            drillOutputMulitplierControl = loadconfig.drillOutputMulitplierControl;

            entityList.Clear();
            gridsList.Clear();
            
            npcCombatList.Clear();
            npcCombatListWorking.Clear();
            npcCombatListNotWorking.Clear();

            npcProductionList.Clear();
            npcProductionListWorking.Clear();
            npcProductionListNotWorking.Clear();

            npcEngineerList.Clear();
            npcEngineerListWorking.Clear();
            npcEngineerListNotWorking.Clear();

            MyAPIGateway.Entities.GetEntities(entityList);
            foreach (var entity in entityList)
            {
                if (entity as IMyCubeGrid != null)
                {
                    gridsList.Add(entity as IMyCubeGrid);
                }
            }
            foreach (var grid in gridsList)
            {
                List<IMySlimBlock> gridBlocks = new List<IMySlimBlock>();
                gridBlocks.Clear();
                grid.GetBlocks(gridBlocks);
                foreach (var block in gridBlocks)
                {
                    if (block.BlockDefinition.Id.SubtypeName == combatNpcSubtypeName)
                    {
                        var blockAsRemoteControl = block.FatBlock as IMyRemoteControl;
                        npcCombatList.Add(blockAsRemoteControl);
                        blockAsRemoteControl.IsWorkingChanged += WorkingChanged;
                        blockAsRemoteControl.AppendingCustomInfo += AppendingCustomInfo;
                        combatNpcUpgradeDetails = "\nValues Upgraded\n=============\n Thrust Ouput: " + (thrustOutputMulitplierControl * 100).ToString() +
                                              "%\n Gyro Outout: " + (gyroOutputMulitplierControl * 100).ToString() + "%";
                        blockAsRemoteControl.RefreshCustomInfo();
                        TriggerTerminalRefresh(blockAsRemoteControl as MyCubeBlock);
                        /*
                        if (npcCombatList.Count > 0)
                        {
                            ProcessCombatMobilityUpgradeList(blockAsRemoteControl.CubeGrid as MyCubeGrid);
                        }
                        */
                    }
                    if (block.BlockDefinition.Id.SubtypeName == engineeringNpcSubtypeName)
                    {
                        var engineerNPC = block.FatBlock as IMyUpgradeModule;
                        if (engineerNPC != null)
                        {
                            npcEngineerList.Add(engineerNPC);
                            engineerNPC.IsWorkingChanged += WorkingChanged;
                            engineerNPC.AppendingCustomInfo += AppendingCustomInfo;
                            engineerNpcUpgradeDetails =
                                "\nValues Upgraded\n=============\n Power Ouput: " + (powerOutputMulitplierControl * 100).ToString() +
                                "%\n Power Consumption: " + (100 / powerConsumptionDivisionControl).ToString() + "%";
                            engineerNPC.RefreshCustomInfo();
                            TriggerTerminalRefresh(engineerNPC as MyCubeBlock);
                            /*
                            if (npcEngineerList.Count > 0)
                            {
                                ProcessEngineeringUpgradeList(engineerNPC.CubeGrid as MyCubeGrid);
                            }
                            */
                        }
                    }
                    if (block.BlockDefinition.Id.SubtypeName == productionNpcSubtypeName)
                    {
                        var productionNPC = block.FatBlock as IMyUpgradeModule;
                        if (productionNPC != null)
                        {
                            npcProductionList.Add(productionNPC);
                            productionNPC.IsWorkingChanged += WorkingChanged;
                            productionNPC.AppendingCustomInfo += AppendingCustomInfo;
                            productionNpcUpgradeDetails = "\nValues Upgraded\n=============\n Gas Ouput: " + (gasGeneratorOutputMulitplierControl * 100).ToString() +
                                                          "%\n Drill Outout: " + (drillOutputMulitplierControl * 100).ToString() + "%";
                            productionNPC.RefreshCustomInfo();
                            TriggerTerminalRefresh(productionNPC as MyCubeBlock);
                            /*
                            if (npcProductionList.Count == 1)
                            {
                                ProcessProductionUpgradeList(productionNPC.CubeGrid as MyCubeGrid);
                            }
                            */
                        }
                    }
                }
                grid.OnBlockAdded += BlockAddedToGrid;
                grid.OnBlockRemoved += BlockRemovedFromGrid;
                //MyVisualScriptLogicProvider.ShowNotificationToAll(npcCombatList.Count.ToString(), 1000, "Red");
                
            }
            MyAPIGateway.Entities.OnEntityAdd += EntityAdded;
            MyAPIGateway.Entities.OnEntityRemove += EntityRemoved;
            //MyAPIGateway.Utilities.ShowMessage("DEBUG", "Init Ran");
        }

        public void EntityAdded(IMyEntity entity)
        {
            if (entity as IMyCubeGrid != null)
            {
                var grid = entity as IMyCubeGrid;
                List<IMySlimBlock> gridBlocks = new List<IMySlimBlock>();
                gridBlocks.Clear();
                grid.GetBlocks(gridBlocks);
                foreach (var block in gridBlocks)
                {
                    if (block.BlockDefinition.Id.SubtypeName == combatNpcSubtypeName)
                    {
                        var combatNPCBlock = block.FatBlock as IMyRemoteControl;
                        npcCombatList.Add(combatNPCBlock);
                        combatNPCBlock.IsWorkingChanged += WorkingChanged;
                        combatNPCBlock.AppendingCustomInfo += AppendingCustomInfo;
                        combatNpcUpgradeDetails = "\nValues Upgraded\n=============\n Thrust Ouput: " + (thrustOutputMulitplierControl * 100).ToString() +
                                              "%\n Gyro Outout: " + (gyroOutputMulitplierControl * 100).ToString() + "%";
                        combatNPCBlock.RefreshCustomInfo();
                        TriggerTerminalRefresh(combatNPCBlock as MyCubeBlock);
                        if (combatNPCBlock.IsWorking && combatNPCBlock.IsFunctional)
                        {
                            ProcessCombatMobilityUpgradeList(combatNPCBlock.CubeGrid as MyCubeGrid);
                            if (!npcCombatListWorking.Contains(combatNPCBlock))
                            {
                                npcCombatListWorking.Add(combatNPCBlock);
                            }
                        }
                    }
                    if (block.BlockDefinition.Id.SubtypeName == engineeringNpcSubtypeName)
                    {
                        var engineerNPCBlock = block.FatBlock as IMyUpgradeModule;
                        if (engineerNPCBlock != null)
                        {
                            npcEngineerList.Add(engineerNPCBlock);
                            engineerNPCBlock.IsWorkingChanged += WorkingChanged;
                            engineerNPCBlock.AppendingCustomInfo += AppendingCustomInfo;
                            engineerNpcUpgradeDetails =
                                "\nValues Upgraded\n=============\n Power Ouput: " + (powerOutputMulitplierControl * 100).ToString() +
                                "%\n Power Consumption: " + (100 / powerConsumptionDivisionControl).ToString() + "%";
                            engineerNPCBlock.RefreshCustomInfo();
                            TriggerTerminalRefresh(engineerNPCBlock as MyCubeBlock);
                            if (engineerNPCBlock.IsWorking && engineerNPCBlock.IsFunctional)
                            {
                                ProcessEngineeringUpgradeList(engineerNPCBlock.CubeGrid as MyCubeGrid);
                                if (!npcEngineerListWorking.Contains(engineerNPCBlock))
                                {
                                    npcEngineerListWorking.Add(engineerNPCBlock);
                                }
                            }
                        }
                    }
                    if (block.BlockDefinition.Id.SubtypeName == productionNpcSubtypeName)
                    {
                        var productionNPCBlock = block.FatBlock as IMyUpgradeModule;
                        if (productionNPCBlock != null)
                        {
                            npcProductionList.Add(productionNPCBlock);
                            productionNPCBlock.IsWorkingChanged += WorkingChanged;
                            productionNPCBlock.AppendingCustomInfo += AppendingCustomInfo;
                            productionNpcUpgradeDetails = "\nValues Upgraded\n=============\n Gas Ouput: " + (gasGeneratorOutputMulitplierControl * 100).ToString() +
                                                          "%\n Drill Outout: " + (drillOutputMulitplierControl * 100).ToString() + "%";
                            productionNPCBlock.RefreshCustomInfo();
                            TriggerTerminalRefresh(productionNPCBlock as MyCubeBlock);
                            if (productionNPCBlock.IsWorking && productionNPCBlock.IsFunctional)
                            {
                                ProcessProductionUpgradeList(productionNPCBlock.CubeGrid as MyCubeGrid);
                                if (!npcProductionListWorking.Contains(productionNPCBlock))
                                {
                                    npcProductionListWorking.Add(productionNPCBlock);
                                }
                            }
                        }
                    }
                }
                grid.OnBlockAdded += BlockAddedToGrid;
                grid.OnBlockRemoved += BlockRemovedFromGrid;
            }
        }

        public void EntityRemoved(IMyEntity entity)
        {
            if (entity as IMyCubeGrid != null && gridsList.Contains(entity as IMyCubeGrid))
            {
                var grid = (entity as IMyCubeGrid);
                gridsList.Remove(grid);
                grid.OnBlockAdded -= BlockAddedToGrid;
                grid.OnBlockRemoved -= BlockRemovedFromGrid;
            }
        }

        public void BlockAddedToGrid(IMySlimBlock slimBlock)
        {
            try
            {
                if (slimBlock.FatBlock != null && slimBlock != null)
                {
                    if (slimBlock.FatBlock.BlockDefinition.SubtypeName == combatNpcSubtypeName)
                    {
                        var combatNPC = slimBlock.FatBlock as IMyRemoteControl;
                        if (combatNPC != null)
                        {

                            npcCombatList.Add(combatNPC);
                            combatNPC.IsWorkingChanged += WorkingChanged;
                            combatNPC.AppendingCustomInfo += AppendingCustomInfo;
                            combatNpcUpgradeDetails = "\nValues Upgraded\n=============\n Thrust Ouput: " +
                                                      (thrustOutputMulitplierControl * 100).ToString() +
                                                      "%\n Gyro Outout: " + (gyroOutputMulitplierControl * 100).ToString() +
                                                      "%";
                            combatNPC.RefreshCustomInfo();
                            TriggerTerminalRefresh(combatNPC as MyCubeBlock);

                            if (combatNPC.IsWorking && combatNPC.IsFunctional)
                            {
                                ProcessCombatMobilityUpgradeList(slimBlock.CubeGrid as MyCubeGrid);
                                if (!npcCombatListWorking.Contains(combatNPC))
                                {
                                    npcCombatListWorking.Add(combatNPC);
                                }
                            }
                            //MyVisualScriptLogicProvider.ShowNotificationToAll("Combat NPC Count: " + npcCombatList.Count.ToString(), 1000, "Red");
                        }
                    }

                    if (slimBlock.FatBlock.BlockDefinition.SubtypeName == engineeringNpcSubtypeName)
                    {
                        var engineerNPC = slimBlock.FatBlock as IMyUpgradeModule;
                        if (engineerNPC != null)
                        {
                            npcEngineerList.Add(slimBlock.FatBlock as IMyUpgradeModule);
                            engineerNPC.IsWorkingChanged += WorkingChanged;
                            engineerNPC.AppendingCustomInfo += AppendingCustomInfo;
                            engineerNpcUpgradeDetails =
                                "\nValues Upgraded\n=============\n Power Ouput: " +
                                (powerOutputMulitplierControl * 100).ToString() +
                                "%\n Power Consumption: " + (100 / powerConsumptionDivisionControl).ToString() + "%";
                            engineerNPC.RefreshCustomInfo();
                            TriggerTerminalRefresh(engineerNPC as MyCubeBlock);

                            if (engineerNPC.IsWorking && engineerNPC.IsFunctional)
                            {
                                ProcessEngineeringUpgradeList(slimBlock.CubeGrid as MyCubeGrid);
                                if (!npcEngineerListWorking.Contains(engineerNPC))
                                {
                                    npcEngineerListWorking.Add(engineerNPC);
                                }
                            }
                            //MyVisualScriptLogicProvider.ShowNotificationToAll("Engineer NPC Count: " + npcEngineerList.Count.ToString(), 1000, "Red");
                        }
                    }

                    if (slimBlock.FatBlock.BlockDefinition.SubtypeName == productionNpcSubtypeName)
                    {
                        var productionNPC = slimBlock.FatBlock as IMyUpgradeModule;
                        if (productionNPC != null)
                        {
                            npcProductionList.Add(slimBlock.FatBlock as IMyUpgradeModule);
                            productionNPC.IsWorkingChanged += WorkingChanged;
                            productionNPC.AppendingCustomInfo += AppendingCustomInfo;
                            productionNpcUpgradeDetails = "\nValues Upgraded\n=============\n Gas Ouput: " +
                                                          (gasGeneratorOutputMulitplierControl * 100).ToString() +
                                                          "%\n Drill Outout: " +
                                                          (drillOutputMulitplierControl * 100).ToString() + "%";
                            productionNPC.RefreshCustomInfo();
                            TriggerTerminalRefresh(productionNPC as MyCubeBlock);

                            
                            if (productionNPC.IsWorking && productionNPC.IsFunctional)
                            {
                                ProcessProductionUpgradeList(slimBlock.CubeGrid as MyCubeGrid);
                                if (!npcProductionListWorking.Contains(productionNPC))
                                {
                                    npcProductionListWorking.Add(productionNPC);
                                }
                            }

                            //MyVisualScriptLogicProvider.ShowNotificationToAll("Production NPC Count: " + npcProductionList.Count.ToString(), 1000, "Red");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                  MyAPIGateway.Utilities.ShowMessage("BlockAdditionError", e.ToString());
            }
        }

        public void BlockRemovedFromGrid(IMySlimBlock slimBlock)
        {
            try
            {
                if (slimBlock.FatBlock != null && slimBlock != null)
                {
                    if (slimBlock.FatBlock.BlockDefinition.SubtypeName == combatNpcSubtypeName)
                    {
                        var npcCombatBlock = slimBlock.FatBlock as IMyRemoteControl;
                        npcCombatList.Remove(npcCombatBlock);
                        npcCombatListWorking.Remove(npcCombatBlock);
                        npcCombatListNotWorking.Remove(npcCombatBlock);
                        if (npcCombatList.Count == 0)
                        {
                            ProcessCombatMobilityDowngradeList(slimBlock.CubeGrid as MyCubeGrid);
                        }
                        //MyVisualScriptLogicProvider.ShowNotificationToAll("Combat NPC Count: " + npcCombatList.Count.ToString(), 1000, "Red");
                    }
                    if (slimBlock.FatBlock.BlockDefinition.SubtypeName == engineeringNpcSubtypeName)
                    {
                        var npcEngineerBlock = slimBlock.FatBlock as IMyUpgradeModule;
                        npcEngineerList.Remove(npcEngineerBlock);
                        npcEngineerListWorking.Remove(npcEngineerBlock);
                        npcEngineerListNotWorking.Remove(npcEngineerBlock);
                        if (npcEngineerList.Count == 0)
                        {
                            ProcessEngineeringDowngradeList(slimBlock.CubeGrid as MyCubeGrid);
                        }
                        //MyVisualScriptLogicProvider.ShowNotificationToAll("Engineer NPC Count: " + npcEngineerList.Count.ToString(), 1000, "Red");
                    }
                    if (slimBlock.FatBlock.BlockDefinition.SubtypeName == productionNpcSubtypeName)
                    {
                        var npcProductionBlock = slimBlock.FatBlock as IMyUpgradeModule;
                        npcProductionList.Remove(npcProductionBlock);
                        npcProductionListWorking.Remove(npcProductionBlock);
                        npcProductionListNotWorking.Remove(npcProductionBlock);
                        if (npcProductionList.Count == 0)
                        {
                            ProcessProductionDowngradeList(slimBlock.CubeGrid as MyCubeGrid);
                        }
                        //MyVisualScriptLogicProvider.ShowNotificationToAll("Production NPC Count: " + npcProductionList.Count.ToString(), 1000, "Red");
                    }
                }
            }
            catch(Exception e)
            {
                MyAPIGateway.Utilities.ShowMessage("BlockRemovalError", e.ToString());
            }
        }

        public void WorkingChanged(IMyCubeBlock cubeBlock)
        {
            if (cubeBlock.BlockDefinition.SubtypeName != null)
            {
                
                //Combat NPC
                if (cubeBlock.BlockDefinition.SubtypeName == combatNpcSubtypeName && cubeBlock.IsWorking)
                {
                    var combatNPC = cubeBlock as IMyRemoteControl;
                    if (npcCombatListNotWorking.Contains(combatNPC))
                    {
                        npcCombatListNotWorking.Remove(combatNPC);
                    }
                    npcCombatListWorking.Add(combatNPC);
                    combatNpcUpgradeDetails = "\nValues Upgraded\n=============\n Thrust Ouput: " + (thrustOutputMulitplierControl * 100).ToString() + "%\n Gyro Outout: " + (gyroOutputMulitplierControl * 100).ToString() + "%";
                    combatNPC.RefreshCustomInfo();
                    TriggerTerminalRefresh(combatNPC as MyCubeBlock);
                    if (npcCombatListWorking.Count == 0)
                    {
                        ProcessCombatMobilityDowngradeList(cubeBlock.CubeGrid as MyCubeGrid);
                    }
                    else
                    {
                        ProcessCombatMobilityUpgradeList(cubeBlock.CubeGrid as MyCubeGrid);
                    }
                }
                else if (cubeBlock.BlockDefinition.SubtypeName == combatNpcSubtypeName && !cubeBlock.IsWorking)
                {
                    var combatNPC = cubeBlock as IMyRemoteControl;
                    if (npcCombatListWorking.Contains(combatNPC))
                    {
                        npcCombatListWorking.Remove(combatNPC);
                    }
                    npcCombatListNotWorking.Add(combatNPC);
                    combatNpcUpgradeDetails = "Block disabled";
                    combatNPC.RefreshCustomInfo();
                    TriggerTerminalRefresh(combatNPC as MyCubeBlock);
                    if (npcCombatListWorking.Count == 0)
                    {
                        ProcessCombatMobilityDowngradeList(cubeBlock.CubeGrid as MyCubeGrid);
                    }
                    else
                    {
                        ProcessCombatMobilityUpgradeList(cubeBlock.CubeGrid as MyCubeGrid);
                    }
                }

                //Engineering NPC
                if (cubeBlock.BlockDefinition.SubtypeName == engineeringNpcSubtypeName && cubeBlock.IsWorking)
                {
                    var engineerNPC = cubeBlock as IMyUpgradeModule;
                    if (npcEngineerListNotWorking.Contains(engineerNPC))
                    {
                        npcEngineerListNotWorking.Remove(engineerNPC);
                    }
                    npcEngineerListWorking.Add(engineerNPC);
                    engineerNpcUpgradeDetails = "\nValues Upgraded\n=============\n Power Ouput: " + (powerOutputMulitplierControl * 100).ToString() + "%\n Power Consumption: " + (100 / powerConsumptionDivisionControl).ToString() + "%";
                    engineerNPC.RefreshCustomInfo();
                    TriggerTerminalRefresh(engineerNPC as MyCubeBlock);
                    if (npcEngineerListWorking.Count == 0)
                    {
                        ProcessEngineeringDowngradeList(cubeBlock.CubeGrid as MyCubeGrid);
                    }
                    else
                    {
                        ProcessEngineeringUpgradeList(cubeBlock.CubeGrid as MyCubeGrid);
                    }


                }
                else if (cubeBlock.BlockDefinition.SubtypeName == engineeringNpcSubtypeName && !cubeBlock.IsWorking)
                {
                    var engineerNPC = cubeBlock as IMyUpgradeModule;
                    if (npcEngineerListWorking.Contains(engineerNPC))
                    {
                        npcEngineerListWorking.Remove(engineerNPC);
                    }
                    npcEngineerListNotWorking.Add(engineerNPC);
                    engineerNpcUpgradeDetails = "Block disabled";
                    engineerNPC.RefreshCustomInfo();
                    TriggerTerminalRefresh(engineerNPC as MyCubeBlock);
                    if (npcEngineerListWorking.Count == 0)
                    {
                        ProcessEngineeringDowngradeList(cubeBlock.CubeGrid as MyCubeGrid);
                    }
                    else
                    {
                        ProcessEngineeringUpgradeList(cubeBlock.CubeGrid as MyCubeGrid);
                    }

                }

                //Production NPC
                if (cubeBlock.BlockDefinition.SubtypeName == productionNpcSubtypeName && cubeBlock.IsWorking)
                {
                    var productionNPC = cubeBlock as IMyUpgradeModule;
                    if (npcProductionListNotWorking.Contains(productionNPC))
                    {
                        npcProductionListNotWorking.Remove(productionNPC);
                    }
                    npcProductionListWorking.Add(productionNPC);
                    productionNpcUpgradeDetails = "Values Upgraded\n=============\n Gas Ouput: " + (gasGeneratorOutputMulitplierControl * 100).ToString() +
                                                  "%\n Drill Outout: " + (drillOutputMulitplierControl * 100).ToString() + "%";
                    productionNPC.RefreshCustomInfo();
                    TriggerTerminalRefresh(productionNPC as MyCubeBlock);
                    if (npcProductionListWorking.Count == 0)
                    {
                        ProcessProductionDowngradeList(cubeBlock.CubeGrid as MyCubeGrid);
                    }
                    else
                    {
                        ProcessProductionUpgradeList(cubeBlock.CubeGrid as MyCubeGrid);
                    }
                }
                else if (cubeBlock.BlockDefinition.SubtypeName == productionNpcSubtypeName && !cubeBlock.IsWorking)
                {
                    var productionNPC = cubeBlock as IMyUpgradeModule;
                    if (npcProductionListWorking.Contains(productionNPC))
                    {
                        npcProductionListWorking.Remove(productionNPC);
                    }
                    npcProductionListNotWorking.Add(productionNPC);
                    productionNpcUpgradeDetails = "Block disabled";
                    productionNPC.RefreshCustomInfo();
                    TriggerTerminalRefresh(productionNPC as MyCubeBlock);
                    if (npcProductionListWorking.Count == 0)
                    {
                        ProcessProductionDowngradeList(cubeBlock.CubeGrid as MyCubeGrid);
                    }
                    else
                    {
                        ProcessProductionUpgradeList(cubeBlock.CubeGrid as MyCubeGrid);
                    }
                }
            }
        }

        private void AppendingCustomInfo(IMyTerminalBlock terminalBlock, StringBuilder details)
        {
            if (terminalBlock.BlockDefinition.SubtypeName == combatNpcSubtypeName)
            {
                details.Clear();
                details.Append(combatNpcUpgradeDetails);
            }
            if (terminalBlock.BlockDefinition.SubtypeName == engineeringNpcSubtypeName)
            {
                details.Clear();
                details.Append(engineerNpcUpgradeDetails);
            }
            if (terminalBlock.BlockDefinition.SubtypeName == productionNpcSubtypeName)
            {
                details.Clear();
                details.Append(productionNpcUpgradeDetails);
            }
        }

        public static void TriggerTerminalRefresh(MyCubeBlock block)
        {
            MyOwnershipShareModeEnum shareMode;
            long ownerId;
            if (block.IDModule != null)
            {
                ownerId = block.IDModule.Owner;
                shareMode = block.IDModule.ShareMode;
            }
            else
            {
                return;
            }
            block.ChangeOwner(ownerId, shareMode == MyOwnershipShareModeEnum.None ? MyOwnershipShareModeEnum.Faction : MyOwnershipShareModeEnum.None);
            block.ChangeOwner(ownerId, shareMode);
        }

        public void ProcessCombatMobilityUpgradeList(MyCubeGrid attachedGridAsMyCubeGrid)
        {
            
            foreach (var block in attachedGridAsMyCubeGrid.GetFatBlocks())
            {
                if (block != null)
                {
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Thrust))
                    {
                        var blockAsThruster = block as Sandbox.ModAPI.IMyThrust;
                        if (blockAsThruster != null)
                        {
                            blockAsThruster.ThrustMultiplier = 1.0f;
                            blockAsThruster.ThrustMultiplier = (blockAsThruster.ThrustMultiplier * thrustOutputMulitplierControl);
                            if (testColourOn)
                            {
                                blockAsThruster.CubeGrid.ColorBlocks(block.Position, block.Position, upgradeTestColor);
                            }
                        }
                    }
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Gyro))
                    {
                        var blockAsGryo = block as IMyGyro;
                        if (blockAsGryo != null)
                        {
                            blockAsGryo.GyroStrengthMultiplier = 1.0f;
                            blockAsGryo.PowerConsumptionMultiplier = 1.0f;

                            blockAsGryo.GyroStrengthMultiplier = (blockAsGryo.GyroStrengthMultiplier * gyroOutputMulitplierControl);
                            if (testColourOn)
                            {
                                blockAsGryo.CubeGrid.ColorBlocks(block.Position, block.Position, upgradeTestColor);
                            }
                        }
                    }
                }
            }
        }

        public void ProcessCombatMobilityDowngradeList(MyCubeGrid attachedGridAsMyCubeGrid)
        {
            foreach (var block in attachedGridAsMyCubeGrid.GetFatBlocks())
            {
                if (block != null)
                {
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Thrust))
                    {
                        var blockAsThruster = block as Sandbox.ModAPI.IMyThrust;
                        if (blockAsThruster != null)
                        {
                            blockAsThruster.ThrustMultiplier = 1.0f;
                        }
                        if (testColourOn)
                        {
                            blockAsThruster.CubeGrid.ColorBlocks(block.Position, block.Position, downgradeTestColor);
                        }
                    }
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Gyro))
                    {
                        var blockAsGryo = block as Sandbox.ModAPI.IMyGyro;
                        if (blockAsGryo != null)
                        {
                            blockAsGryo.GyroStrengthMultiplier = 1.0f;
                            blockAsGryo.PowerConsumptionMultiplier = 1.0f;
                        }
                        if (testColourOn)
                        {
                            blockAsGryo.CubeGrid.ColorBlocks(block.Position, block.Position, downgradeTestColor);
                        }
                    }
                }
            }
        }

        public void ProcessEngineeringUpgradeList(MyCubeGrid attachedGridAsMyCubeGrid)
        {
            foreach (var block in attachedGridAsMyCubeGrid.GetFatBlocks())
            {
                if (block != null)
                {
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Thrust))
                    {
                        var blockAsThruster = block as IMyThrust;
                        if (blockAsThruster != null)
                        {
                            blockAsThruster.PowerConsumptionMultiplier = 1.0f;
                            blockAsThruster.PowerConsumptionMultiplier = (blockAsThruster.PowerConsumptionMultiplier / powerConsumptionDivisionControl);
                            if (testColourOn)
                            {
                                blockAsThruster.CubeGrid.ColorBlocks(block.Position, block.Position, upgradeTestColor);
                            }

                        }
                    }
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Gyro))
                    {
                        var blockAsGryo = block as IMyGyro;
                        if (blockAsGryo != null)
                        {
                            blockAsGryo.PowerConsumptionMultiplier = 1.0f;
                            blockAsGryo.PowerConsumptionMultiplier = (blockAsGryo.PowerConsumptionMultiplier / powerConsumptionDivisionControl);
                            if (testColourOn)
                            {
                                blockAsGryo.CubeGrid.ColorBlocks(block.Position, block.Position, upgradeTestColor);
                            }
                        }
                    }
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Reactor))
                    {
                        var blockAsReactor = block as IMyReactor;
                        if (blockAsReactor != null)
                        {
                            blockAsReactor.PowerOutputMultiplier = 1.0f;
                            blockAsReactor.PowerOutputMultiplier = (blockAsReactor.PowerOutputMultiplier * powerOutputMulitplierControl);
                            if (testColourOn)
                            {
                                blockAsReactor.CubeGrid.ColorBlocks(block.Position, block.Position, upgradeTestColor);
                            }
                        }
                    }
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Drill))
                    {
                        var blockAsShipDrill = block as IMyShipDrill;
                        if (blockAsShipDrill != null)
                        {
                            blockAsShipDrill.PowerConsumptionMultiplier = 1.0f;
                            blockAsShipDrill.PowerConsumptionMultiplier = (blockAsShipDrill.PowerConsumptionMultiplier / powerConsumptionDivisionControl);
                            if (testColourOn)
                            {
                                blockAsShipDrill.CubeGrid.ColorBlocks(block.Position, block.Position, upgradeTestColor);
                            }
                        }
                    }
                }
            }
        }

        public void ProcessEngineeringDowngradeList(MyCubeGrid attachedGridAsMyCubeGrid)
        {
            foreach (var block in attachedGridAsMyCubeGrid.GetFatBlocks())
            {
                if (block != null)
                {
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Thrust))
                    {
                        var blockAsThruster = block as IMyThrust;
                        if (blockAsThruster != null)
                        {
                            blockAsThruster.PowerConsumptionMultiplier = 1.0f;
                            if (testColourOn)
                            {
                                blockAsThruster.CubeGrid.ColorBlocks(block.Position, block.Position, downgradeTestColor);
                            }

                        }
                    }
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Gyro))
                    {
                        var blockAsGryo = block as IMyGyro;
                        if (blockAsGryo != null)
                        {
                            blockAsGryo.PowerConsumptionMultiplier = 1.0f;
                            if (testColourOn)
                            {
                                blockAsGryo.CubeGrid.ColorBlocks(block.Position, block.Position, downgradeTestColor);
                            }
                        }
                    }
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Reactor))
                    {
                        var blockAsReactor = block as IMyReactor;
                        if (blockAsReactor != null)
                        {
                            //MyAPIGateway.Utilities.ShowMessage("Downgrade Calc", blockAsReactor.PowerOutputMultiplier.ToString() + " " +
                            //                                                     powerOutputMulitplierControl.ToString() + " " + 
                            //                                                     (blockAsReactor.PowerOutputMultiplier / powerOutputMulitplierControl).ToString());
                            blockAsReactor.PowerOutputMultiplier = 1.0f;
                            if (testColourOn)
                            {
                                blockAsReactor.CubeGrid.ColorBlocks(block.Position, block.Position, downgradeTestColor);
                            }
                        }
                    }
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Drill))
                    {
                        var blockAsShipDrill = block as IMyShipDrill;
                        if (blockAsShipDrill != null)
                        {
                            blockAsShipDrill.PowerConsumptionMultiplier = 1.0f;
                            if (testColourOn)
                            {
                                blockAsShipDrill.CubeGrid.ColorBlocks(block.Position, block.Position, downgradeTestColor);
                            }
                        }
                    }
                }
            }
        }

        public void ProcessProductionUpgradeList(MyCubeGrid attachedGridAsMyCubeGrid)
        {
            try
            {
                foreach (var block in attachedGridAsMyCubeGrid.GetFatBlocks())
                {
                    if (block != null)
                    {
                        if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_OxygenGenerator))
                        {
                            var blockAsOxyGen = block as IMyGasGenerator;
                            if (blockAsOxyGen != null)
                            {
                                blockAsOxyGen.ProductionCapacityMultiplier = 1.0f;
                                blockAsOxyGen.ProductionCapacityMultiplier = (blockAsOxyGen.ProductionCapacityMultiplier / gasGeneratorOutputMulitplierControl);
                                if (testColourOn)
                                {
                                    blockAsOxyGen.CubeGrid.ColorBlocks(block.Position, block.Position, upgradeTestColor);
                                }
                            }
                        }
                        if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Drill))
                        {
                            var blockAsShipDrill = block as IMyShipDrill;
                            if (blockAsShipDrill != null)
                            {
                                blockAsShipDrill.DrillHarvestMultiplier = 1.0f;
                                blockAsShipDrill.DrillHarvestMultiplier = (blockAsShipDrill.DrillHarvestMultiplier * drillOutputMulitplierControl);
                                if (testColourOn)
                                {
                                    blockAsShipDrill.CubeGrid.ColorBlocks(block.Position, block.Position, upgradeTestColor);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //MyVisualScriptLogicProvider.ShowNotificationToAll("Upgrade Error " + e, 10000, "Red");
            }
        }

        public void ProcessProductionDowngradeList(MyCubeGrid attachedGridAsMyCubeGrid)
        {
            foreach (var block in attachedGridAsMyCubeGrid.GetFatBlocks())
            {
                if (block != null)
                {
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_OxygenGenerator))
                    {
                        var blockAsOxyGen = block as IMyGasGenerator;
                        if (blockAsOxyGen != null)
                        {
                            blockAsOxyGen.ProductionCapacityMultiplier = 1.0f;
                            if (testColourOn)
                            {
                                blockAsOxyGen.CubeGrid.ColorBlocks(block.Position, block.Position, downgradeTestColor);
                            }
                        }

                    }
                    if (block.BlockDefinition.Id.TypeId == typeof(MyObjectBuilder_Drill))
                    {
                        var blockAsShipDrill = block as IMyShipDrill;
                        if (blockAsShipDrill != null)
                        {
                            blockAsShipDrill.DrillHarvestMultiplier = 1.0f;
                            if (testColourOn)
                            {
                                blockAsShipDrill.CubeGrid.ColorBlocks(block.Position, block.Position, downgradeTestColor);
                            }
                        }
                    }
                }
            }
        }

        public void Close()
        {
            MyAPIGateway.Entities.OnEntityAdd -= EntityAdded;
            MyAPIGateway.Entities.OnEntityRemove -= EntityRemoved;
        }
    }
}
