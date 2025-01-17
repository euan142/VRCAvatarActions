﻿#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace VRCAvatarActions
{
    [CreateAssetMenu(fileName = "Visemes", menuName = "VRCAvatarActions/Other Actions/Visemes")]
    public class Visemes : NonMenuActions
    {
        [System.Serializable]
        public class VisemeAction : Action
        {
            //Visemes
            [System.Serializable]
            public struct VisemeTable
            {
                public bool sil;
                public bool pp;
                public bool ff;
                public bool th;
                public bool dd;
                public bool kk;
                public bool ch;
                public bool ss;
                public bool nn;
                public bool rr;
                public bool aa;
                public bool e;
                public bool i;
                public bool o;
                public bool u;

                public bool GetValue(VisemeEnum type)
                {
                    switch (type)
                    {
                        case VisemeEnum.Sil: return sil;
                        case VisemeEnum.PP: return pp;
                        case VisemeEnum.FF: return ff;
                        case VisemeEnum.TH: return th;
                        case VisemeEnum.DD: return dd;
                        case VisemeEnum.KK: return kk;
                        case VisemeEnum.CH: return ch;
                        case VisemeEnum.SS: return ss;
                        case VisemeEnum.NN: return nn;
                        case VisemeEnum.RR: return rr;
                        case VisemeEnum.AA: return aa;
                        case VisemeEnum.E: return e;
                        case VisemeEnum.I: return i;
                        case VisemeEnum.O: return o;
                        case VisemeEnum.U: return u;
                    }
                    return false;
                }

                public void SetValue(VisemeEnum type, bool value)
                {
                    switch (type)
                    {
                        case VisemeEnum.Sil: sil = value; break;
                        case VisemeEnum.PP: pp = value; break;
                        case VisemeEnum.FF: ff = value; break;
                        case VisemeEnum.TH: th = value; break;
                        case VisemeEnum.DD: dd = value; break;
                        case VisemeEnum.KK: kk = value; break;
                        case VisemeEnum.CH: ch = value; break;
                        case VisemeEnum.SS: ss = value; break;
                        case VisemeEnum.NN: nn = value; break;
                        case VisemeEnum.RR: rr = value; break;
                        case VisemeEnum.AA: aa = value; break;
                        case VisemeEnum.E: e = value; break;
                        case VisemeEnum.I: i = value; break;
                        case VisemeEnum.O: o = value; break;
                        case VisemeEnum.U: u = value; break;
                    }
                }

                public bool IsModified() => sil || pp || ff || th || dd || kk || ch || ss || nn || rr || aa || e || i || o || u;
            }

            public VisemeTable visimeTable = new VisemeTable();
        }

        public List<VisemeAction> actions = new List<VisemeAction>();

        public override void GetActions(List<Action> output)
        {
            foreach (var action in actions)
                output.Add(action);
        }

        public override Action AddAction()
        {
            var result = new VisemeAction();
            actions.Add(result);
            return result;
        }

        public override void RemoveAction(Action action) => actions.Remove(action as VisemeAction);
        public override void InsertAction(int index, Action action) => actions.Insert(index, action as VisemeAction);

        public override bool CanUseLayer(AnimationLayer layer) => layer == AnimationLayer.FX;

        public override bool ActionsHaveExit() => false;

        public override void Build(ActionsBuilder builder, MenuActions.MenuAction parentAction)
        {
            //Layer name
            var layerName = name;
            if (parentAction != null)
                layerName = $"{parentAction.name}_{layerName}_SubActions";

            //Build
            BuildNormal(builder, AnimationLayer.FX, layerName, actions, parentAction);
        }

        void BuildNormal(ActionsBuilder builder, AnimationLayer layerType, string layerName, List<VisemeAction> sourceActions, MenuActions.MenuAction parentAction)
        {
            //Find all that affect this layer
            var layerActions = new List<VisemeAction>();
            foreach (var action in sourceActions)
            {
                if (!action.ShouldBuild())
                    continue;
                if (!action.AffectsLayer(layerType))
                    continue;
                layerActions.Add(action);
            }
            if (layerActions.Count == 0)
                return;

            //Build
            BuildLayer(builder, layerType, layerName, layerActions, parentAction);
        }

        void BuildLayer(ActionsBuilder builder, AnimationLayer layerType, string layerName, List<VisemeAction> actions, MenuActions.MenuAction parentAction)
        {
            var controller = builder.GetController(layerType);

            var VisemeValues = System.Enum.GetValues(typeof(VisemeEnum)).Cast<VisemeEnum>();

            //Add parameter
            builder.AddParameter(controller, "Viseme", AnimatorControllerParameterType.Int, 0);

            //Prepare layer
            var layer = builder.GetControllerLayer(controller, layerName);
            layer.stateMachine.entryTransitions = null;
            layer.stateMachine.anyStateTransitions = null;
            layer.stateMachine.states = null;
            layer.stateMachine.entryPosition = builder.StatePosition(-1, 0);
            layer.stateMachine.anyStatePosition = builder.StatePosition(-1, 1);
            layer.stateMachine.exitPosition = builder.StatePosition(-1, 2);

            //Default
            AnimatorState defaultState = null;
            VisemeAction defaultAction = null;
            var unusedValues = new List<VisemeEnum>();
            foreach (var value in VisemeValues)
                unusedValues.Add(value);

            //Build states
            int actionIter = 0;
            foreach (var action in this.actions)
            {
                //Check if valid
                if (!action.visimeTable.IsModified())
                {
                    EditorUtility.DisplayDialog("Build Warning", $"Visemes {action.name} has no selected conditions.", "Okay");
                    continue;
                }

                //Build
                var state = layer.stateMachine.AddState(action.name, builder.StatePosition(0, actionIter + 1));
                state.motion = action.GetAnimation(builder, layerType, true);
                actionIter += 1;

                //Conditions
                foreach (var visime in VisemeValues)
                {
                    if (action.visimeTable.GetValue(visime))
                    {
                        //Transition
                        var transition = layer.stateMachine.AddAnyStateTransition(state);
                        transition.hasExitTime = false;
                        transition.exitTime = 0;
                        transition.duration = action.fadeIn;
                        transition.canTransitionToSelf = false;
                        transition.AddCondition(AnimatorConditionMode.Equals, (int)visime, "Viseme");

                        //Parent
                        if (parentAction != null)
                            parentAction.AddCondition(builder, transition, true);

                        //Cleanup
                        unusedValues.Remove(visime);
                    }
                }

                //Store default
                if (action.visimeTable.sil)
                {
                    defaultState = state;
                    defaultAction = action;
                }
            }

            //Default state
            if (defaultState == null)
                defaultState = layer.stateMachine.AddState("Default", builder.StatePosition(0, 0));
            layer.stateMachine.defaultState = defaultState;

            //Animation Layer Weight
            var layerWeight = defaultState.AddStateMachineBehaviour<VRC.SDK3.Avatars.Components.VRCAnimatorLayerControl>();
            layerWeight.goalWeight = 1;
            layerWeight.layer = builder.GetLayerIndex(controller, layer);
            layerWeight.blendDuration = 0;
            layerWeight.playable = VRC.SDKBase.VRC_AnimatorLayerControl.BlendableLayer.FX;

            //Default transitions
            foreach (var visime in unusedValues)
            {
                //Transition
                var transition = layer.stateMachine.AddAnyStateTransition(defaultState);
                transition.hasExitTime = false;
                transition.exitTime = 0;
                transition.duration = defaultAction != null ? defaultAction.fadeIn : 0f;
                transition.canTransitionToSelf = false;
                transition.AddCondition(AnimatorConditionMode.Equals, (int)visime, "Viseme");
            }

            //Parent
            if (parentAction != null)
            {
                var transition = layer.stateMachine.AddAnyStateTransition(defaultState);
                transition.hasExitTime = false;
                transition.exitTime = 0;
                transition.duration = defaultAction != null ? defaultAction.fadeIn : 0f;
                transition.canTransitionToSelf = false;
                parentAction.AddCondition(builder, transition, false);
            }
        }
    }
}
#endif