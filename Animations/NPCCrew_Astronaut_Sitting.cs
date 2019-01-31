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
using VRageRender;
using System.Text;
using Sandbox.ModAPI.Interfaces;
using VRage.Game.ObjectBuilders.Definitions;
using Sandbox.Game.EntityComponents;
using VRage.Utils;
using VRage.Game.Entity;

namespace Stollie.NPCCrew
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TimerBlock), false, "NPC_Astronaut")]
    public class NPCCrew_Astronaut_Sitting : MyGameLogicComponent
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

        internal bool initHeadMovement = false;
        internal bool workingStatusChanged = false;
        internal bool playAnimation = true;

        internal Vector3 testColor = Color.Yellow.ColorToHSV();

        IMyTimerBlock npcCrewBlock = null;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            try
            {
                npcCrewBlock = Entity as IMyTimerBlock;
                NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
                npcCrewBlock.CubeGrid.ColorBlocks(npcCrewBlock.SlimBlock.Position, npcCrewBlock.SlimBlock.Position, Color.Red.ColorToHSV());
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
                if (npcCrewBlock.IsWorking && npcCrewBlock.IsFunctional)
                {
                    if (playAnimation)
                    {
                        MoveHead();
                        MoveLeftArm();
                        MoveRightArm();
                        MoveLCD();
                        
                        //Controls Boots Emissive (Green)
                        npcCrewBlock.SetEmissiveParts("Emissive", Color.Green, 1.0f);
                    }
                    else
                    {
                        //Controls Boots Emissive (Red)
                        npcCrewBlock.SetEmissiveParts("Emissive", Color.Red, 1.0f);
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
                MyEntitySubpart subpart;
                npcCrewBlock.TryGetSubpart("NPC_Astronaut_Head", out subpart);
                if (subpart == null) return;

                /*
                if (Math.Abs(subpartAsEntity.Dithering - transparency) > 0.001f)
                {
                    npcCrewBlock.SlimBlock.Dithering = transparency;
                    subpart.SetEmissiveParts("PaintedMetal_Colorable", Color.White, 1); // fixes the screen being non-emissive
                }
                */
                var rotation = 0.001f;
                var initialMatrix = subpart.PositionComp.LocalMatrix;
                if (AnimationLoopHead == 400)
                {
                    AnimationLoopHead = 0;
                    initHeadMovement = false;
                }
                if (AnimationLoopHead == 0) TranslationTimeHead = -1;

                if (initHeadMovement)
                {
                    if (AnimationLoopHead == 160) TranslationTimeHead = 1;

                }

                if (!initHeadMovement && AnimationLoopHead == 200)
                {
                    TranslationTimeHead = 1;
                }

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
                var subpart = npcCrewBlock.GetSubpart("NPC_Astronaut_LeftArm");
                //double rotation = 0.002f;
                var initialMatrix = subpart.PositionComp.LocalMatrix;
                double rotationX = 0.0005f;
                double rotationY = -0.0005f;

                if (AnimationLoopLeftArm == 400) AnimationLoopLeftArm = 0;
                if (AnimationLoopLeftArm == 0) TranslationTimeLeftArm = -1;
                if (AnimationLoopLeftArm == 200) TranslationTimeLeftArm = 1;

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
                var subpart = npcCrewBlock.GetSubpart("NPC_Astronaut_RightArm");
                var rotation = -0.0005f;
                var initialMatrix = subpart.PositionComp.LocalMatrix;

                if (AnimationLoopRightArm == 400) AnimationLoopRightArm = 0;
                if (AnimationLoopRightArm == 0) TranslationTimeRightArm = 1;
                if (AnimationLoopRightArm == 200) TranslationTimeRightArm = -1;

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
                var subpart = npcCrewBlock.GetSubpart("NPC_Astronaut_Cube");
                var initialMatrix = subpart.PositionComp.LocalMatrix;


                double rotationX = 0.005f;
                double rotationY = 0.005f;
                double rotationZ = 0.005f;

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