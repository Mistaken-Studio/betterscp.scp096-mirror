// -----------------------------------------------------------------------
// <copyright file="Translation.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Mistaken.BetterSCP.SCP096
{
    internal class Translation : Exiled.API.Interfaces.ITranslation
    {
        public string Inform096Target { get; set; } = "You are <color=yellow><b>Target</b></color> for <color=red>SCP 096</color><br>Targets: <color=yellow>{0}</color>";

        public string Inform096 { get; set; } = "You have <color=yellow>{0}</color> target(s)<br><color=yellow>{1}</color>s rage left";
    }
}
