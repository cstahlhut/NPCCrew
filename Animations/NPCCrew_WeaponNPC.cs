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
using VRage.Game;
using VRage.Game.ModAPI;
using Sandbox.Game.EntityComponents;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Utils;
using System.Text;


namespace Stollie.NPCCrew
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_RemoteControl), false, "NPC_CombatMobility")]
    public class NPCCrew_WeaponNPC : MyGameLogicComponent
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
        Vector3 scaleDirection = new Vector3(-1, -1, -1);

        internal bool initalized = false;
        internal bool playAnimation = true;
        internal bool forceColor = true;

        internal IMyRemoteControl npcCrewBlock = null;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            try
            {
                npcCrewBlock = Entity as IMyRemoteControl;
                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
                //MyAPIGateway.Utilities.ShowMessage("DEBUG", "Init Ran");
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
                if (npcCrewBlock.GetDiffuseColor() != Color.Red.ColorToHSV() && forceColor)
                {
                    npcCrewBlock.CubeGrid.ColorBlocks(npcCrewBlock.SlimBlock.Position, npcCrewBlock.SlimBlock.Position, Color.Red.ColorToHSV());
                }
                if (npcCrewBlock.IsWorking && npcCrewBlock.IsFunctional)
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
                var subpart = npcCrewBlock.GetSubpart("NPC_Weapons_Head");
                var rotation = 0.005f;
                var initialMatrix = subpart.PositionComp.LocalMatrix;

                if (AnimationLoopHead == 200) AnimationLoopHead = 0;
                if (AnimationLoopHead == 0) TranslationTimeHead = -1;
                if (AnimationLoopHead == 100) TranslationTimeHead = 1;

                var rotationMatrix = MatrixD.CreateRotationY(rotation * TranslationTimeHead);
                //subpart.Value.PositionComp.LocalMatrix = rotationMatrix;
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
                var subpart = npcCrewBlock.GetSubpart("NPC_Weapons_LeftArm");
                //double rotation = 0.002f;
                var initialMatrix = subpart.PositionComp.LocalMatrix;
                if (AnimationLoopLeftArm == 140) AnimationLoopLeftArm = 0;
                double rotationX = 0.001f;
                double rotationY = 0.001f;

                if (AnimationLoopLeftArm == 0) TranslationTimeLeftArm = -1;
                if (AnimationLoopLeftArm == 70) TranslationTimeLeftArm = 1;

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
                var subpart = npcCrewBlock.GetSubpart("NPC_Weapons_RightArm");
                var rotation = -0.001f;
                var initialMatrix = subpart.PositionComp.LocalMatrix;

                if (AnimationLoopRightArm == 500) AnimationLoopRightArm = 0;
                if (AnimationLoopRightArm == 0) TranslationTimeRightArm = -1;
                if (AnimationLoopRightArm == 250) TranslationTimeRightArm = 1;

                var rotationMatrix = MatrixD.CreateRotationX(rotation * TranslationTimeRightArm) * MatrixD.CreateRotationY(rotation * TranslationTimeRightArm);
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
                var subpart = npcCrewBlock.GetSubpart("NPC_Weapons_LCDRotation");
                subpart.SetEmissiveParts("Emissive", Color.Red, 0.4f);

                var initialMatrix = subpart.PositionComp.LocalMatrix;
                double rotationX = 0.0f;
                double rotationY = 0.0f;
                double rotationZ = 0.01;
                Vector3 scaleDown = new Vector3D(0.998, 0.998, 1);
                Vector3 scaleUp = new Vector3D(1.002, 1.002, 1);
                
                if (AnimationLoopLCD == 1500)
                {
                    AnimationLoopLCD = 0;
                    scaleDirection = scaleDown;
                }
                if (AnimationLoopLCD == 0)
                {
                    RotationTimeLCD = -1;
                    scaleDirection = scaleDown;
                }
                if (AnimationLoopLCD == 750)
                {
                    RotationTimeLCD = 1;
                    scaleDirection = scaleUp;
                }
                var rotationAndScaleMatrix = MatrixD.CreateRotationX(rotationX) * MatrixD.CreateRotationY(rotationY) *
                                             MatrixD.CreateRotationZ(rotationZ) * MatrixD.CreateScale(scaleDirection);

                var translationMatrix = MatrixD.CreateTranslation(0.0010 * RotationTimeLCD, 0.0001 * RotationTimeLCD, -0.00003 * RotationTimeLCD);

                var matrix = rotationAndScaleMatrix * initialMatrix * translationMatrix;
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