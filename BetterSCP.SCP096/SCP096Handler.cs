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
            Exiled.Events.Handlers.Server.RoundStarted += this.Handle(() => this.Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Scp096.AddingTarget += this.Handle<Exiled.Events.EventArgs.AddingTargetEventArgs>((ev) => this.Scp096_AddingTarget(ev));
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Handle(() => this.Server_RoundStarted(), "RoundStart");
            Exiled.Events.Handlers.Scp096.AddingTarget -= this.Handle<Exiled.Events.EventArgs.AddingTargetEventArgs>((ev) => this.Scp096_AddingTarget(ev));
        }

        private void Scp096_AddingTarget(Exiled.Events.EventArgs.AddingTargetEventArgs ev)
        {
            if (ev.Target.GetSessionVar<bool>(API.SessionVarType.SPAWN_PROTECT))
                ev.EnrageTimeToAdd = 0;
        }

        private void Server_RoundStarted()
        {
            this.RunCoroutine(this.Inform096Target(), "Inform096Target");
        }

        private IEnumerator<float> Inform096Target()
        {
            yield return Timing.WaitForSeconds(1f);
            HashSet<Player> added = new HashSet<Player>();
            Player[] lastAdded;
            int rid = RoundPlus.RoundId;
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                lastAdded = added.ToArray();
                added.Clear();
                foreach (var scp096 in RealPlayers.Get(RoleType.Scp096))
                {
                    try
                    {
                        var scp096script = scp096.CurrentScp as PlayableScps.Scp096;
                        if (scp096script.Enraged || scp096script.Enraging)
                        {
                            string targetMessage = string.Format(PluginHandler.Instance.Translation.Inform096Target, scp096script._targets.Count);
                            foreach (var item in scp096script._targets.ToArray())
                            {
                                var p = Player.Get(item);
                                p.SetGUI("scp096", PseudoGUIPosition.TOP, targetMessage);
                                added.Add(p);
                            }

                            var time = Mathf.RoundToInt(scp096script.EnrageTimeLeft).ToString();
                            if (time == "0")
                                time = "[REDACTED]";
                            scp096.SetGUI("scp096", PseudoGUIPosition.TOP, string.Format(PluginHandler.Instance.Translation.Inform096, scp096script._targets.Count, time));
                            added.Add(scp096);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        this.Log.Error(ex.Message);
                        this.Log.Error(ex.StackTrace);
                    }
                }

                foreach (var player in lastAdded.Where(i => !added.Contains(i)))
                    player.SetGUI("scp096", PseudoGUIPosition.TOP, null);

                yield return Timing.WaitForSeconds(1f);
            }
        }
    }
}
