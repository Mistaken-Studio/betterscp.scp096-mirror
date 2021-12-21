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
            Exiled.Events.Handlers.Scp096.CalmingDown += this.Scp096_CalmingDown;
            Exiled.Events.Handlers.Server.RestartingRound += this.Server_RestartingRound;
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Server_WaitingForPlayers;
        }

        public override void OnDisable()
        {
            Exiled.Events.Handlers.Scp096.AddingTarget -= this.Scp096_AddingTarget;
            Exiled.Events.Handlers.Scp096.Enraging -= this.Scp096_Enraging;
            Exiled.Events.Handlers.Scp096.CalmingDown -= this.Scp096_CalmingDown;
            Exiled.Events.Handlers.Server.RestartingRound += this.Server_RestartingRound;
            Exiled.Events.Handlers.Server.WaitingForPlayers += this.Server_WaitingForPlayers;
        }

        private void Server_WaitingForPlayers()
        {
            this.forceDisable.Clear();
        }

        private void Server_RestartingRound()
        {
            foreach (var item in Player.Get(RoleType.Scp096))
                this.forceDisable.Add(item);
        }

        private readonly HashSet<Player> forceDisable = new HashSet<Player>();

        private void Scp096_CalmingDown(Exiled.Events.EventArgs.CalmingDownEventArgs ev)
        {
            this.forceDisable.Add(ev.Player);
            ev.Player.SetGUI("scp096", PseudoGUIPosition.TOP, null);
            foreach (var player in ev.Scp096._targets)
                Player.Get(player).SetGUI("scp096", PseudoGUIPosition.TOP, null);
        }

        private void Scp096_Enraging(Exiled.Events.EventArgs.EnragingEventArgs ev)
        {
            this.RunCoroutine(this.RageGUI(ev.Player, ev.Scp096), "RageGUI");
        }

        private void Scp096_AddingTarget(Exiled.Events.EventArgs.AddingTargetEventArgs ev)
        {
            if (ev.Target.GetSessionVariable<bool>(SessionVarType.SPAWN_PROTECT))
                ev.EnrageTimeToAdd = 0;
        }

        private IEnumerator<float> RageGUI(Player scp096, PlayableScps.Scp096 script)
        {
            this.forceDisable.Remove(scp096);
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
                        if (p == null)
                            continue;
                        p.SetGUI("scp096", PseudoGUIPosition.TOP, targetMessage);
                        added.Add(p);
                    }

                    var time = Mathf.RoundToInt(script.EnrageTimeLeft).ToString();
                    scp096?.SetGUI("scp096", PseudoGUIPosition.TOP, string.Format(PluginHandler.Instance.Translation.Inform096, targets, time));
                    added.Add(scp096);
                }
                catch (System.Exception ex)
                {
                    this.Log.Error(ex.Message);
                    this.Log.Error(ex.StackTrace);
                }

                foreach (var player in lastAdded.Where(i => !added.Contains(i)))
                    player?.SetGUI("scp096", PseudoGUIPosition.TOP, null);

                yield return Timing.WaitForSeconds(1f);
            }
            while ((script.Enraging || script.Enraged) && !this.forceDisable.Contains(scp096));

            this.forceDisable.Remove(scp096);

            foreach (var player in lastAdded)
                player?.SetGUI("scp096", PseudoGUIPosition.TOP, null);
        }
    }
}
