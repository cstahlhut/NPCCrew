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
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TextPanel), false, "NPC_Astronaut_BlankLCD")]
    public class NPCCrew_Astronaut_Standing_BlankLCD : MyGameLogicComponent
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

        internal bool initHeadMovement = true;
        internal bool playAnimation = true;

        IMyTextPanel npcCrewBlock = null;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            try
            {
                npcCrewBlock = Entity as IMyTextPanel;
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
                if (npcCrewBlock == null || npcCrewBlock.Render == null || !npcCrewBlock.InScene || npcCrewBlock.MarkedForClose)
                    return;
                
                if (npcCrewBlock.IsWorking && npcCrewBlock.IsFunctional)
                {
                    if (playAnimation)
                    {
                        MoveHead();
                        MoveLeftArm();
                        MoveRightArm();
                        
                        //Controls Boots Emissive (Green)
                        npcCrewBlock.SetEmissivePartsForSubparts("Emissive", Color.Green, 1.0f);
                    }
                }
                else
                {
                    //Controls Boots Emissive (Red)
                    npcCrewBlock.SetEmissivePartsForSubparts("Emissive", Color.Red, 1.0f);
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
                MyEntitySubpart subpartAstronaut;
                MyEntitySubpart subpart;

                npcCrewBlock.TryGetSubpart("NPC_Astronaut_Standing_BlankLCD_Test", out subpartAstronaut);
                subpartAstronaut.TryGetSubpart("NPC_Astronaut_Standing_Head_BlankLCD", out subpart);

                if (subpart == null) return;
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
                MyEntitySubpart subpartAstronaut;
                MyEntitySubpart subpart;

                npcCrewBlock.TryGetSubpart("NPC_Astronaut_Standing_BlankLCD_Test", out subpartAstronaut);
                subpartAstronaut.TryGetSubpart("NPC_Astronaut_Standing_LeftArm_BlankLCD", out subpart);

                if (subpart == null) return;
                //double rotation = 0.002f;
                var initialMatrix = subpart.PositionComp.LocalMatrix;
                double rotationX = -0.0005f;
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
                MyEntitySubpart subpartAstronaut;
                MyEntitySubpart subpart;

                npcCrewBlock.TryGetSubpart("NPC_Astronaut_Standing_BlankLCD_Test", out subpartAstronaut);
                subpartAstronaut.TryGetSubpart("NPC_Astronaut_Standing_RightArm_BlankLCD", out subpart);

                if (subpart == null) return;
                var rotation = -0.001f;
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
        
        #endregion

    }
}