using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// using UnityEngine.Events;
using KSP.UI.Screens;
using KSP.Localization;

namespace FacilityTree {
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class TreeHandler : MonoBehaviour {
        private int nodeCost;
        public void Start() {
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) {
                if (ResearchAndDevelopment.Instance.GetTechState("KerbalSpaceCenter") == null) NewGameSetup();
                GameEvents.OnKSCStructureRepaired.Add(FacilityRepair);
                RDTechTree.OnTechTreeSpawn.Add(TreeSpawned);
            }
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER) {
                //RDTechTree.OnTechTreeDespawn.Add(TreeDespawn);
                RDNode.OnNodeSelected.Add(RDNodeSelected);
                RDNode.OnNodeUnselected.Add(RDNodeUnselected);
                GameEvents.OnTechnologyResearched.Add(TechResearched);
            }
        }
        public void OnDisable() {
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) {
                GameEvents.OnKSCStructureRepaired.Remove(FacilityRepair);
                RDTechTree.OnTechTreeSpawn.Remove(TreeSpawned);
            }
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER) {
                //RDTechTree.OnTechTreeDespawn.Remove(TreeDespawn);
                RDNode.OnNodeSelected.Remove(RDNodeSelected);
                RDNode.OnNodeUnselected.Remove(RDNodeUnselected);
                GameEvents.OnTechnologyResearched.Remove(TechResearched);
            }
        }
        private void NewGameSetup() {
            ResearchAndDevelopment.Instance.UnlockProtoTechNode(new ProtoTechNode { scienceCost = 999999, techID = "KerbalSpaceCenter" });
            ResearchAndDevelopment.Instance.UnlockProtoTechNode(new ProtoTechNode { scienceCost = 999999, techID = "FlagPole" });
            if (HighLogic.CurrentGame.Parameters.Difficulty.IndestructibleFacilities || !System.IO.Directory.Exists(KSPUtil.ApplicationRootPath + "GameData/KerbalHero"))
                for (int i = 0; i < Enum.GetNames(typeof(SpaceCenterFacility)).Length; i++)
                    ResearchAndDevelopment.Instance.UnlockProtoTechNode(new ProtoTechNode { scienceCost = 999999, techID = ((SpaceCenterFacility)i).ToString() });
            else if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER) ResearchAndDevelopment.Instance.UnlockProtoTechNode(new ProtoTechNode { scienceCost = 999999, techID = "Runway" });
        }
        private void FacilityRepair(DestructibleBuilding building) {
            if (Enum.TryParse(building.id.Split('/')[1], out SpaceCenterFacility facility))
                if (ResearchAndDevelopment.Instance.GetTechState(facility.ToString()) == null)
                    ResearchAndDevelopment.Instance.UnlockProtoTechNode(new ProtoTechNode { scienceCost = 999999, techID = facility.ToString() });
        }
        private void TreeSpawned(RDTechTree tree) { // auto-debug
            // tree.controller.actionButton.onClickState.AddListener(new UnityAction<string>(PurchaseUpgrade));
            List<string> treeTechs = AssetBase.RnDTechTree.GetTreeTechs().Select(t => t.techID).ToList();
            foreach (AvailablePart part in PartLoader.LoadedPartsList.Where(p => !treeTechs.Contains(p.TechRequired) && p.TechRequired != null))
                if (part.TechHidden) { } else if (!part.name.StartsWith("kerbalEVA") && !part.name.StartsWith("Potato") && part.name != "flag") {
                    Debug.Log(part.name + " missing RDNode: " + part.TechRequired);
                    part.TechRequired = "start";
                }
        }
        private void RDNodeSelected(RDNode node) {
            nodeCost = node.tech.scienceCost;
            node.tech.scienceCost = 0;
            if (node.state == RDNode.State.RESEARCHABLE)
                StartCoroutine(CallbackUtil.DelayedCallback(1, delegate {
                    CurrencyModifierQuery query = CurrencyModifierQuery.RunQuery(TransactionReasons.StructureConstruction, nodeCost * -500, 0f, 0f);
                    RDController.Instance.actionButtonText.text = Localizer.Format("#fTreeLOC_010") + " [" + query.GetCostLine() + "]";
                    RDController.Instance.actionButton.Enable(query.CanAfford());
                }));
        }
        private void RDNodeUnselected(RDNode node) => node.tech.scienceCost = nodeCost;
        private void TechResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> action) {
            if (action.target == RDTech.OperationResult.Successful && action.host.scienceCost != 999999)
                Funding.Instance.AddFunds(nodeCost * -500, TransactionReasons.StructureConstruction);
        }
        // private void TreeDespawn(RDTechTree tree) => tree.controller.actionButton.onClickState.RemoveListener(new UnityAction<string>(PurchaseUpgrade));
        // private void PurchaseUpgrade(string state) { if (state == "research") Funding.Instance.AddFunds(nodeCost * -500, TransactionReasons.StructureConstruction); }
    }
}
