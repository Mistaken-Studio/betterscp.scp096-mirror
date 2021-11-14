// -----------------------------------------------------------------------
// <copyright file="SCP096Handler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using MEC;
using Mistaken.API;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using UnityEngine;

namespace Mistaken.BetterSCP.SCP096
{
    internal class SCP096Handler : Module
    {
        public SCP096Handler(PluginHandler p)
            : base(p)
        {
        }

        public override string Name => nameof(SCP096Handler);

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Scp096.AddingTarget += this.Scp096_AddingTarget;
            Exiled.Events.Handlers.Scp096.Enraging += this.Scp096_Enraging;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Scp096.AddingTarget -= this.Scp096_AddingTarget;
            Exiled.Events.Handlers.Scp096.Enraging -= this.Scp096_Enraging;
        }

        private void Scp096_Enraging(Exiled.Events.EventArgs.EnragingEventArgs ev)
        {
            this.RunCoroutine(this.RageGUI(ev.Player, ev.Scp096), "RageGUI");
        }

        private void Scp096_AddingTarget(Exiled.Events.EventArgs.AddingTargetEventArgs ev)
        {
            if (ev.Target.GetSessionVar<bool>(SessionVarType.SPAWN_PROTECT))
                ev.EnrageTimeToAdd = 0;
        }

        private IEnumerator<float> RageGUI(Player scp096, PlayableScps.Scp096 script)
        {
            HashSet<Player> added = new HashSet<Player>();
            Player[] lastAdded;
            do
            {
                lastAdded = added.ToArray();
                added.Clear();
                try
                {
                    int targets = script._targets.Count;
                    string targetMessage = string.Format(PluginHandler.Instance.Translation.Inform096Target, targets);
                    foreach (var item in script._targets.ToArray())
                    {
                        var p = Player.Get(item);
                        p.SetGUI("scp096", PseudoGUIPosition.TOP, targetMessage);
                        added.Add(p);
                    }

                    var time = Mathf.RoundToInt(script.EnrageTimeLeft).ToString();
                    scp096.SetGUI("scp096", PseudoGUIPosition.TOP, string.Format(PluginHandler.Instance.Translation.Inform096, targets, time));
                    added.Add(scp096);
                }
                catch (System.Exception ex)
                {
                    this.Log.Error(ex.Message);
                    this.Log.Error(ex.StackTrace);
                }

                foreach (var player in lastAdded.Where(i => !added.Contains(i)))
                    player.SetGUI("scp096", PseudoGUIPosition.TOP, null);

                yield return Timing.WaitForSeconds(1f);
            }
            while (script.Enraging || script.Enraged);

            foreach (var player in lastAdded)
                player.SetGUI("scp096", PseudoGUIPosition.TOP, null);
        }
    }
}
