using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using VRage.ObjectBuilders;
using System.Collections.Generic;
using VRage.ModAPI;
using Sandbox.ModAPI;
using VRageMath;
using Sandbox.Game.Entities;
using System;
using Sandbox.Game;
using Sandbox.Definitions;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage;
using VRage.Game.ModAPI;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using Sandbox.Game.EntityComponents;
using System.Text;
using VRage.Utils;

namespace Stollie.NPCCrew
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_UpgradeModule), false, "NPC_Production")]
    public class NPCCrew_ProductionNPC : MyGameLogicComponent
    {
        internal int TranslationTimeHead = 0;
        internal int TranslationTimeLeftArm = 0;
        internal int TranslationTimeRightArm = 0;
        internal int TranslationTimeLCD2 = 0;

        internal int RotationTimeHead = 0;
        internal int RotationTimeLeftArm = 0;
        internal int RotationTimeRightArm = 0;
        internal int RotationTimeLCD = 0;
        internal int RotationTimeLCD3 = 0;

        internal int AnimationLoopHead = 0;
        internal int AnimationLoopLeftArm = 0;
        internal int AnimationLoopRightArm = 0;
        internal int AnimationLoopLCD = 0;
        internal int AnimationLoopLCD2 = 0;
        internal int AnimationLoopLCD3 = 0;

        internal bool playAnimation = true;
        internal bool forceColor = true;

        internal static readonly MyDefinitionId GId = new MyDefinitionId(typeof(MyObjectBuilder_GasProperties), "Electricity");
        internal const float Power = 0.01f;
        internal MyResourceSinkComponent Sink;
        internal MyResourceSinkInfo ResourceInfo;

        IMyUpgradeModule npcCrewBlock = null;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            try
            {
                npcCrewBlock = Entity as IMyUpgradeModule;
                //MyAPIGateway.Utilities.ShowMessage("DEBUG", "Init Ran");
                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
                npcCrewBlock.CubeGrid.ColorBlocks(npcCrewBlock.SlimBlock.Position, npcCrewBlock.SlimBlock.Position, Color.Red.ColorToHSV());

                NPCCrewConfig loadconfig = NPCCrewConfig.LoadConfigFile();
                forceColor = loadconfig.forceColor;

                if (Sink == null)
                {
                    Sink = new MyResourceSinkComponent();
                }
                ResourceInfo = new MyResourceSinkInfo()
                {
                    ResourceTypeId = GId,
                    MaxRequiredInput = 0.02f,
                    RequiredInputFunc = () => Power
                };
                Sink.Init(MyStringHash.GetOrCompute("Utility"), ResourceInfo);
                Sink.AddType(ref ResourceInfo);
                Entity.Components.Add(Sink);
                Sink.Update();
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Init Error" + e, 10000, "Red");
            }
        }
        
        public override void UpdateAfterSimulation()
        {
            try
            {
                if (npcCrewBlock.GetDiffuseColor() != Color.SteelBlue.ColorToHSV() && forceColor)
                {
                    npcCrewBlock.CubeGrid.ColorBlocks(npcCrewBlock.SlimBlock.Position, npcCrewBlock.SlimBlock.Position, Color.SteelBlue.ColorToHSV());
                }
                if (npcCrewBlock.IsWorking && npcCrewBlock.IsFunctional && Sink.IsPowerAvailable(GId, 0.01f))
                {
                    if (playAnimation)
                    {
                        MoveHead();
                        MoveLeftArm();
                        MoveRightArm();
                        MoveLCD();
                        MoveLCD2();
                        MoveLCD3();
                    }
                }
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Update Error" + e, 2500, "Red");
            }
        }

        #region Animation
        public void MoveHead()
        {
            try
            {
                var subpart = npcCrewBlock.GetSubpart("NPC_Production_Head");
                var rotation = 0.002f;
                var initialMatrix = subpart.PositionComp.LocalMatrix;

                if (AnimationLoopHead == 300)
                {
                    AnimationLoopHead = 0;
                    TranslationTimeHead = -1;
                }

                if (AnimationLoopHead == 150)
                {
                    TranslationTimeHead = 1 ;
                }

                var rotationMatrix = MatrixD.CreateRotationY(rotation * TranslationTimeHead);
                //subpart.Value.PositionComp.LocalMatrix = rotationMatrix;
                var matrix = rotationMatrix * initialMatrix;
                subpart.PositionComp.LocalMatrix = matrix;
                AnimationLoopHead++;

                /*var subparts = (cubeBlock as MyEntity).Subparts;
                foreach (var subpart in subparts)
                {
                    Random rnd = new Random();
                    double rotation = NextDouble(0.0003, 0.0010);
                    var initialMatrix = subpart.Value.PositionComp.LocalMatrix;

                    if (AnimationLoop == 300) AnimationLoop = 0;
                    if (AnimationLoop == 0) TranslationTime = -1;
                    if (AnimationLoop == 150) TranslationTime = 1;

                    var rotationMatrix = MatrixD.CreateRotationX(rotation * TranslationTime) * MatrixD.CreateRotationY(rotation * TranslationTime);
                    //subpart.Value.PositionComp.LocalMatrix = rotationMatrix;
                    var matrix = rotationMatrix * initialMatrix;
                    subpart.Value.PositionComp.LocalMatrix = matrix;
                    AnimationLoop++;
                }
                */
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Update Error" + e, 2500, "Red");
            }
        }

        public void MoveLeftArm()
        {
            try
            {
                var subpart = npcCrewBlock.GetSubpart("NPC_Production_LeftArm");
                //double rotation = 0.002f;
                var initialMatrix = subpart.PositionComp.LocalMatrix;
                if (AnimationLoopLeftArm == 300) AnimationLoopLeftArm = 0;
                double rotationX = 0.001f;
                double rotationY = 0.001f;

                if (AnimationLoopLeftArm == 0) TranslationTimeLeftArm = -1;
                if (AnimationLoopLeftArm == 150) TranslationTimeLeftArm = 1;

                var rotationMatrix = MatrixD.CreateRotationX(rotationX * TranslationTimeLeftArm) * MatrixD.CreateRotationY(rotationY * TranslationTimeLeftArm);
                var matrix = rotationMatrix * initialMatrix;
                subpart.PositionComp.LocalMatrix = matrix;
                AnimationLoopLeftArm++;
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Update Error" + e, 2500, "Red");
            }
        }

        public void MoveRightArm()
        {
            try
            {
                var subpart = npcCrewBlock.GetSubpart("NPC_Production_RightArm");
                var rotation = -0.001f;
                var initialMatrix = subpart.PositionComp.LocalMatrix;

                if (AnimationLoopRightArm == 700) AnimationLoopRightArm = 0;
                if (AnimationLoopRightArm == 0) TranslationTimeRightArm = -1;
                if (AnimationLoopRightArm == 350) TranslationTimeRightArm = 1;

                var rotationMatrix = MatrixD.CreateRotationX(rotation * TranslationTimeRightArm);
                var matrix = rotationMatrix * initialMatrix;
                subpart.PositionComp.LocalMatrix = matrix;
                AnimationLoopRightArm++;
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Update Error" + e, 2500, "Red");
            }
        }

        public void MoveLCD()
        {
            try
            {
                var subpart = npcCrewBlock.GetSubpart("NPC_Production_LCDRotation");
                subpart.SetEmissiveParts("Emissive", Color.Cyan, 0.4f);
                var initialMatrix = subpart.PositionComp.LocalMatrix;

                double rotationX = 0.0f;
                double rotationY = 0.0f;
                double rotationZ = 0.01;

                if (AnimationLoopLCD == 200) AnimationLoopLCD = 0;
                if (AnimationLoopLCD == 0) RotationTimeLCD = -1;
                if (AnimationLoopLCD == 100) RotationTimeLCD = 1;

                var rotationMatrix = MatrixD.CreateRotationX(rotationX) * MatrixD.CreateRotationY(rotationY) * MatrixD.CreateRotationZ(rotationZ);
                var matrix = rotationMatrix * initialMatrix;
                subpart.PositionComp.LocalMatrix = matrix;
                AnimationLoopLCD++;
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Update Error" + e, 2500, "Red");
            }
        }

        public void MoveLCD2()
        {
            try
            {
                var subpart = npcCrewBlock.GetSubpart("NPC_Production_LCDRotation2");
                var initialMatrix = subpart.PositionComp.LocalMatrix;
                subpart.SetEmissiveParts("Emissive", Color.Cyan, 0.4f);

                if (AnimationLoopLCD2 == 1000) AnimationLoopLCD2 = 0;
                if (AnimationLoopLCD2 == 0) TranslationTimeLCD2 = -1;
                if (AnimationLoopLCD2 == 500) TranslationTimeLCD2 = 1;
                

                var matrix = MatrixD.CreateTranslation(0.0004 * TranslationTimeLCD2, 0, 0) * initialMatrix;
                subpart.PositionComp.LocalMatrix = matrix;
                AnimationLoopLCD2++;
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Update Error" + e, 2500, "Red");
            }
        }

        public void MoveLCD3()
        {
            try
            {
                var subpart = npcCrewBlock.GetSubpart("NPC_Production_LCDRotation3");
                subpart.SetEmissiveParts("Emissive", Color.Cyan, 0.4f);
                var initialMatrix = subpart.PositionComp.LocalMatrix;

                double rotationX = 0.0f;
                double rotationY = 0.0f;
                double rotationZ = 0.01;

                if (AnimationLoopLCD3 == 200) AnimationLoopLCD3 = 0;
                if (AnimationLoopLCD3 == 0) RotationTimeLCD3 = -1;
                if (AnimationLoopLCD3 == 100) RotationTimeLCD3 = 1;

                var rotationMatrix = MatrixD.CreateRotationX(rotationX) * MatrixD.CreateRotationY(rotationY) * MatrixD.CreateRotationZ(rotationZ);
                var matrix = rotationMatrix * initialMatrix;
                subpart.PositionComp.LocalMatrix = matrix;
                AnimationLoopLCD3++;
            }
            catch (Exception e)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Update Error" + e, 2500, "Red");
            }
        }

        #endregion

    }
}