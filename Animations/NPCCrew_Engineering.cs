﻿using VRage.Game.Components;
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
using VRageRender;
using System.Text;
using Sandbox.ModAPI.Interfaces;
using VRage.Game.ObjectBuilders.Definitions;
using Sandbox.Game.EntityComponents;
using VRage.Utils;

namespace Stollie.NPCCrew
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_UpgradeModule), false, "NPC_Engineering")]
    public class NPCCrew_Engineering : MyGameLogicComponent
    {
        internal int TranslationTimeHead = 0;
        internal int TranslationTimeLeftArm = 0;
        internal int TranslationTimeRightArm = 0;

        internal int RotationTimeHead = 0;
        internal int RotationTimeLeftArm = 0;
        internal int RotationTimeRightArm = 0;
        internal int RotationTimeLCD = 0;

        internal int AnimationLoopHead = 0;
        internal int AnimationLoopLeftArm = 0;
        internal int AnimationLoopRightArm = 0;
        internal int AnimationLoopLCD = 0;

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
                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
                npcCrewBlock.CubeGrid.ColorBlocks(npcCrewBlock.SlimBlock.Position, npcCrewBlock.SlimBlock.Position, Color.Red.ColorToHSV());

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
                if (npcCrewBlock.GetDiffuseColor() != Color.DarkOrange.ColorToHSV() && forceColor)
                {
                    npcCrewBlock.CubeGrid.ColorBlocks(npcCrewBlock.SlimBlock.Position, npcCrewBlock.SlimBlock.Position, Color.DarkOrange.ColorToHSV());
                }
                if (npcCrewBlock.IsWorking && npcCrewBlock.IsFunctional && Sink.IsPowerAvailable(GId, 0.01f))
                {
                    if (playAnimation)
                    {
                        MoveHead();
                        MoveLeftArm();
                        MoveRightArm();
                        MoveLCD();
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
                var subpart = npcCrewBlock.GetSubpart("NPC_Engineer_Head");
                var rotation = 0.003f;
                var initialMatrix = subpart.PositionComp.LocalMatrix;

                if (AnimationLoopHead == 200) AnimationLoopHead = 0;
                if (AnimationLoopHead == 0) TranslationTimeHead = -1;
                if (AnimationLoopHead == 100) TranslationTimeHead = 1;

                var rotationMatrix = MatrixD.CreateRotationY(rotation * TranslationTimeHead);
                var matrix = rotationMatrix * initialMatrix;
                subpart.PositionComp.LocalMatrix = matrix;
                AnimationLoopHead++;
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
                var subpart = npcCrewBlock.GetSubpart("NPC_Engineer_LeftArm");
                var initialMatrix = subpart.PositionComp.LocalMatrix;
                double rotationX = 0.001f;
                double rotationY = 0.001f;

                if (AnimationLoopLeftArm == 200) AnimationLoopLeftArm = 0;
                if (AnimationLoopLeftArm == 0) TranslationTimeLeftArm = -1;
                if (AnimationLoopLeftArm == 100) TranslationTimeLeftArm = 1;

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
                var subpart = npcCrewBlock.GetSubpart("NPC_Engineer_RightArm");
                var rotation = -0.001f;
                var initialMatrix = subpart.PositionComp.LocalMatrix;

                if (AnimationLoopRightArm == 500) AnimationLoopRightArm = 0;
                if (AnimationLoopRightArm == 0) TranslationTimeRightArm = -1;
                if (AnimationLoopRightArm == 250) TranslationTimeRightArm = 1;

                var rotationMatrix = MatrixD.CreateRotationY(rotation * TranslationTimeRightArm);
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
                var subpart = npcCrewBlock.GetSubpart("NPC_Engineer_LCDRotation");
                subpart.SetEmissiveParts("CleanEmissive", Color.OrangeRed, 0.4f);
                var initialMatrix = subpart.PositionComp.LocalMatrix;
                double rotationX = 0.01f;
                double rotationY = 0.01f;
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


        #endregion
    }
}