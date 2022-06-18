
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace OpenTree {
    public class OpenTreeSettings : GameParameters.CustomParameterNode {
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.CAREER | GameParameters.GameMode.SCIENCE; } }
        public override string Section { get { return "OpenTree"; } }
        public override string DisplaySection { get { return "OpenTree"; } }
        public override bool HasPresets { get { return false; } }
        public override int SectionOrder { get { return 0; } }
        public override string Title { get { return "#autoLOC_190662"; } }
        [GameParameters.CustomParameterUI("#autoLOC_900350", newGameOnly = true)]
        public string start = "Uncrewed";
        public override IList ValidValues(MemberInfo member) => new string[2] { "Uncrewed", "Crewed" };
    }
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class OpenTreeSetup : MonoBehaviour {
        public void Start() {
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) {
                GameEvents.OnTechnologyResearched.Add(TechResearched);
                string startTech = HighLogic.CurrentGame.Parameters.CustomParams<OpenTreeSettings>().start.ToLower() + "Tech";
                if (ResearchAndDevelopment.Instance.GetTechState(startTech) == null)
                    ResearchAndDevelopment.Instance.UnlockProtoTechNode(new ProtoTechNode { scienceCost = 5, techID = startTech });
            }
        }
        public void OnDisable() => GameEvents.OnTechnologyResearched.Remove(TechResearched);
        private void TechResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> action) {
            if (action.host.techID == "structuralII" && action.target == RDTech.OperationResult.Successful) {
                ResearchAndDevelopment.Instance.UnlockProtoTechNode(new ProtoTechNode { scienceCost = 1, techID = "generalConstruction" });
            }
        }
    }
}
