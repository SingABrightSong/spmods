using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using KSP.Localization;

namespace KerbalHero {
    public abstract class KerbalHeroParam : GameParameters.CustomParameterNode {
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.NOTMISSION; } }
        public override string Section { get { return "KerbalHero"; } }
        public override string DisplaySection { get { return "KerbalHero"; } }
        public override bool HasPresets { get { return false; } }
    }
    public class KerbalHeroSettings : KerbalHeroParam {
        public override int SectionOrder { get { return 0; } }
        public override string Title { get { return "#autoLOC_900441"; } }
        [GameParameters.CustomParameterUI("#autoLOC_901054", newGameOnly = true)]
        public string name = "Sally Kerman";
        [GameParameters.CustomParameterUI("#autoLOC_190689", toolTip = "Use Save Name as Hero Name", newGameOnly = true)]
        public bool useSaveName = false;
        [GameParameters.CustomParameterUI("#autoLOC_900447", newGameOnly = true)]
        public ProtoCrewMember.Gender gender = ProtoCrewMember.Gender.Female;
        [GameParameters.CustomParameterUI("#autoLOC_900431", newGameOnly = true)]
        public string trait = KerbalRoster.scientistTrait;
        [GameParameters.CustomParameterUI("#autoLOC_6001968", newGameOnly = true)]
        public ProtoCrewMember.KerbalSuit suit = ProtoCrewMember.KerbalSuit.Default;
        [GameParameters.CustomParameterUI("#autoLOC_900437", toolTip = "Orange Suit Highlight", newGameOnly = true)]
        public bool vet = true;
        public override IList ValidValues(MemberInfo member) {
            if (member.Name == nameof(name)) {
                List<string> names = new List<string> { "Sally Kerman" };
                for (int i = 1; i < 100; i++) names.Add(CrewGenerator.GetRandomName(i < 50 ? ProtoCrewMember.Gender.Female : ProtoCrewMember.Gender.Male, new KSPRandom(i + DateTime.Now.Millisecond)));
                return names;
            } else if (member.Name == nameof(trait)) return new string[3] { KerbalRoster.engineerTrait, KerbalRoster.pilotTrait, KerbalRoster.scientistTrait };
            return null;
        }
    }
    public class KerbalHeroTraits : KerbalHeroParam {
        public override int SectionOrder { get { return 1; } }
        public override string Title { get { return "#autoLOC_900435"; } }
        public override bool HasPresets { get { return true; } }
        [GameParameters.CustomIntParameterUI("#autoLOC_900436", minValue = 0, maxValue = 100, newGameOnly = true)]
        public int courage = 51;
        [GameParameters.CustomIntParameterUI("#autoLOC_900438", minValue = 0, maxValue = 100, newGameOnly = true)]
        public int stupidity = 7;
        [GameParameters.CustomParameterUI("#autoLOC_900440", newGameOnly = true)]
        public bool badS = false;
        public override void SetDifficultyPreset(GameParameters.Preset preset) {
            courage = 12 + (int)preset * 25 + UnityEngine.Random.Range(-12, 12);
            stupidity = 12 + (3 - (int)preset) * 25 + UnityEngine.Random.Range(-12, 12);
            badS = (int)preset * 10 > UnityEngine.Random.Range(0, 100);
        }
        public override bool Interactible(MemberInfo member, GameParameters parameters) => false;
    }
    public class KerbalHeroStatus : KerbalHeroParam {
        public override int SectionOrder { get { return 2; } }
        public override string Title { get { return Localizer.Format("#autoLOC_900174") + " " + Localizer.Format("#autoLOC_475347"); } }
        [GameParameters.CustomParameterUI("Hero", newGameOnly = true)]
        public ProtoCrewMember.RosterStatus hero = ProtoCrewMember.RosterStatus.Missing;
        [GameParameters.CustomParameterUI("#autoLOC_20803", newGameOnly = true)]
        public ProtoCrewMember.RosterStatus jeb = ProtoCrewMember.RosterStatus.Missing;
        [GameParameters.CustomParameterUI("#autoLOC_20827", newGameOnly = true)]
        public ProtoCrewMember.RosterStatus val = ProtoCrewMember.RosterStatus.Missing;
        [GameParameters.CustomParameterUI("#autoLOC_20811", newGameOnly = true)]
        public ProtoCrewMember.RosterStatus bill = ProtoCrewMember.RosterStatus.Missing;
        [GameParameters.CustomParameterUI("#autoLOC_20819", newGameOnly = true)]
        public ProtoCrewMember.RosterStatus bob = ProtoCrewMember.RosterStatus.Missing;
        public override IList ValidValues(MemberInfo member) => new ProtoCrewMember.RosterStatus[3] {
            ProtoCrewMember.RosterStatus.Available,
            ProtoCrewMember.RosterStatus.Missing,
            ProtoCrewMember.RosterStatus.Dead };
    }
    internal enum Mission { Hero, Jeb, Val, Bill, Bob }
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class KerbalHeroAddon : MonoBehaviour {
        KerbalHeroSettings settings;
        KerbalHeroTraits traits;
        KerbalHeroStatus status;
        bool careerMode;
        Dictionary<Mission, KerbalHeroData> data;
        public void Start() {
            careerMode = HighLogic.CurrentGame.Mode == Game.Modes.CAREER;
            if (careerMode || HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) {
                settings = HighLogic.CurrentGame.Parameters.CustomParams<KerbalHeroSettings>();
                traits = HighLogic.CurrentGame.Parameters.CustomParams<KerbalHeroTraits>();
                status = HighLogic.CurrentGame.Parameters.CustomParams<KerbalHeroStatus>();
                data = new Dictionary<Mission, KerbalHeroData> {
                    [Mission.Hero] = new KerbalHeroData(settings.useSaveName ? HighLogic.SaveFolder : settings.name, settings.gender, settings.trait, settings.vet, (float)(traits.courage * 0.01), (float)(traits.stupidity * 0.01), traits.badS,
                        "DTS-51-K", VesselType.Debris, new Orbit(0.01, 0.999, 313000, 105, 91.5, 2.6, Planetarium.GetUniversalTime(), Planetarium.fetch.Home)),
                    [Mission.Jeb] = new KerbalHeroData(Localizer.Format("#autoLOC_20803"), ProtoCrewMember.Gender.Male, KerbalRoster.pilotTrait, true, 0.5f, 0.5f, true,
                        "jex-15", VesselType.Plane, new Orbit(0.01, 0.958, 313300, 105, 105, Math.PI, Planetarium.GetUniversalTime(), Planetarium.fetch.Home)),
                    [Mission.Val] = new KerbalHeroData(Localizer.Format("#autoLOC_20827"), ProtoCrewMember.Gender.Female, KerbalRoster.pilotTrait, true, 0.55f, 0.4f, true,
                        "Valstok 6", VesselType.Ship, new Orbit(65.09, 0, 669500, 0, 0, 0, 0, Planetarium.fetch.Home)),
                    [Mission.Bill] = new KerbalHeroData(Localizer.Format("#autoLOC_20811"), ProtoCrewMember.Gender.Male, KerbalRoster.engineerTrait, true, 0.5f, 0.8f, false,
                        "Abillo 13", VesselType.Ship, new Orbit(0, 0.862, 5059311, 0, 0, 0, 0, Planetarium.fetch.Home)),
                    [Mission.Bob] = new KerbalHeroData(Localizer.Format("#autoLOC_20819"), ProtoCrewMember.Gender.Male, KerbalRoster.scientistTrait, true, 0.3f, 0.1f, false,
                        "Bobslab R", VesselType.Ship, new Orbit(50, 0.002747, 1037950, 0, 0, 0, 0, Planetarium.fetch.Home))};
                StartCoroutine(CallbackUtil.DelayedCallback(7, delegate {
                    //if (!settings.intro && status.hero == ProtoCrewMember.RosterStatus.Assigned && (int)HighLogic.CurrentGame.CrewRoster[data[Mission.Hero].name].rosterStatus < 2) Popup(Mission.Hero);
                    if (FindObjectOfType<ScenarioNewGameIntro>()?.kscComplete == false && !HighLogic.CurrentGame.CrewRoster.Exists(data[Mission.Hero].name)) NewGameSetup();
                    else if (status.hero == ProtoCrewMember.RosterStatus.Assigned && (int)HighLogic.CurrentGame.CrewRoster[data[Mission.Hero].name].rosterStatus < 2 && !HighLogic.CurrentGame.Parameters.Difficulty.IndestructibleFacilities) Popup(Mission.Hero);
                    else if (status.jeb == ProtoCrewMember.RosterStatus.Missing && !HighLogic.CurrentGame.CrewRoster.Exists(data[Mission.Jeb].name)) MissionCheck(Mission.Jeb);
                    else if (status.val == ProtoCrewMember.RosterStatus.Missing && !HighLogic.CurrentGame.CrewRoster.Exists(data[Mission.Val].name)
                        && System.IO.Directory.Exists(KSPUtil.ApplicationRootPath + "GameData/SquadExpansion/MakingHistory")) MissionCheck(Mission.Val);
                    else if (status.bill == ProtoCrewMember.RosterStatus.Missing && !HighLogic.CurrentGame.CrewRoster.Exists(data[Mission.Bill].name)) MissionCheck(Mission.Bill);
                    else if (status.bob == ProtoCrewMember.RosterStatus.Missing && !HighLogic.CurrentGame.CrewRoster.Exists(data[Mission.Bob].name)) MissionCheck(Mission.Bob);
                }));
                // if (!settings.intro) GameEvents.onGUIRecoveryDialogDespawn.Add(VesselRecovered);
                // GameEvents.OnKSCStructureRepaired.Add(TrackingRepairCheck);
            }
        }
        // public void OnDisable() { if (!settings.intro) GameEvents.onGUIRecoveryDialogDespawn.Remove(VesselRecovered); }
        private void NewGameSetup() {
            if (status.hero == ProtoCrewMember.RosterStatus.Missing) StartCoroutine(CallbackUtil.DelayedCallback(51, delegate { SpawnKerbal(Mission.Hero); }));
            else if (status.hero == ProtoCrewMember.RosterStatus.Available) SpawnKerbal(Mission.Hero, false);
            if (status.jeb != ProtoCrewMember.RosterStatus.Available) HighLogic.CurrentGame.CrewRoster.Remove(data[Mission.Jeb].name);
            if (status.val != ProtoCrewMember.RosterStatus.Available) HighLogic.CurrentGame.CrewRoster.Remove(data[Mission.Val].name);
            if (status.bill != ProtoCrewMember.RosterStatus.Available) HighLogic.CurrentGame.CrewRoster.Remove(data[Mission.Bill].name);
            if (status.bob != ProtoCrewMember.RosterStatus.Available) HighLogic.CurrentGame.CrewRoster.Remove(data[Mission.Bob].name);
            if (!HighLogic.CurrentGame.Parameters.Difficulty.IndestructibleFacilities) DemolishKSC();
        }
        private void DemolishKSC() {
            try { FindObjectOfType<ScenarioNewGameIntro>().kscComplete = true; } catch (NullReferenceException) { }
            foreach (DestructibleBuilding building in FindObjectsOfType<DestructibleBuilding>())
                if (Enum.TryParse(building.id.Split('/')[1], out SpaceCenterFacility facility))
                    if (!careerMode || facility != SpaceCenterFacility.Runway) building.Demolish();
        }
        private void MissionCheck(Mission mission) {
            bool timePassed = mission == Mission.Jeb || Planetarium.GetUniversalTime() > ((int)mission - 1) * 21600;
            bool trackingRepaired = !careerMode;
            if (careerMode) trackingRepaired = ScenarioDestructibles.GetFacilityDamage("TrackingStation") < 100f;
            if (timePassed && trackingRepaired) if (mission == Mission.Jeb) Popup(mission); else SpawnKerbal(mission);
        }
        private void Popup(Mission mission = Mission.Hero) {
            List<DialogGUIBase> dialog = new List<DialogGUIBase>();
            if (mission == Mission.Hero) {
                status.hero = ProtoCrewMember.RosterStatus.Available;
                // try { Destroy(FindObjectOfType<MissionRecoveryDialog>().gameObject); } catch (NullReferenceException) { }
                string introText = "... " + Localizer.Format("#autoLOC_8006460") + "\n\n\n... " + Localizer.Format("#autoLOC_8014035") + " ...\n" + Localizer.Format("#autoLOC_6002250") + " ... " + Localizer.Format("#autoLOC_900165") + " ...\n\n\n... " +
                    Localizer.Format("#autoLOC_6001862") + " ... " + Localizer.Format("#autoLOC_6001648") + " ...\n" + Localizer.Format("#autoLOC_901061") + " ... " + Localizer.Format("#autoLOC_900174") + " ... " + Localizer.Format("#autoLOC_445551") + " ...\n\n\n" +
                    Localizer.Format("#autoLOC_6001660") + " ...\n" + Localizer.Format("#autoLOC_6002249") + " ... " + Localizer.Format("#autoLOC_901039") + " ...\n\n\n" + Localizer.Format("#autoLOC_8005131") + " ... ";
                string instructor;
                if (settings.trait == KerbalRoster.scientistTrait) { introText += Localizer.Format("#autoLOC_8005007") + "?\n... " + Localizer.Format("#autoLOC_468312"); instructor = "#autoLOC_501659";
                } else if (settings.trait == KerbalRoster.pilotTrait) { introText += Localizer.Format("#autoLOC_8005006") + "?\n... " + Localizer.Format("#autoLOC_296780"); instructor = "#autoLOC_501660";
                } else { introText += Localizer.Format("#autoLOC_8005008") + "?\n... " + Localizer.Format("#autoLOC_6001739"); instructor = "#autoLOC_501661"; }
                introText += "!\n\n... " + Localizer.Format("#autoLOC_316086") + " ...";
                dialog.Add(new DialogGUIHorizontalLayout(new DialogGUISpace(65), new DialogGUIImage(new Vector2(150, 150), new Vector2(0, 0), Color.white, GameDatabase.Instance.GetTexture("KerbalHero/signal_lost", false))));
                dialog.Add(new DialogGUIHorizontalLayout(new DialogGUISpace(65), new DialogGUIBox(Localizer.Format(instructor), 150, 40)));
                dialog.Add(new DialogGUIHorizontalLayout(new DialogGUIBox(introText, 290, 300)));
                dialog.Add(new DialogGUIHorizontalLayout(new DialogGUISpace(65), new DialogGUIButton(Localizer.Format("#autoLOC_317538"), delegate { }, 150, 40, true)));
            } else {
                dialog.Add(new DialogGUIHorizontalLayout(new DialogGUIBox(Localizer.Format("#autoLOC_463116") + " " + data[mission].vesselName + "!", 290, 40)));
                dialog.Add(new DialogGUIHorizontalLayout(new DialogGUISpace(65), new DialogGUIButton(Localizer.Format("#autoLOC_900978"), delegate { SpawnKerbal(mission, true); }, 150, 40, true)));
            }
            PopupDialog.SpawnPopupDialog(new MultiOptionDialog("KerbalHeroPopup", "", Localizer.Format("#autoLOC_481636") + " " + (mission == Mission.Hero ? "... " + Localizer.Format("#autoLOC_900165") : Localizer.Format("#autoLOC_20803")) + "!\n\n",
                HighLogic.UISkin, dialog.ToArray()), false, HighLogic.UISkin);
        }
        private void SpawnKerbal(Mission mission, bool spawnVessel = true) {
            ProtoCrewMember kerbal = new ProtoCrewMember(ProtoCrewMember.KerbalType.Crew, data[mission].name) {
                gender = data[mission].gender, veteran = data[mission].veteran, courage = data[mission].courage, stupidity = data[mission].stupidity, isBadass = data[mission].badS };
            KerbalRoster.SetExperienceTrait(kerbal, data[mission].trait);
            if (mission == Mission.Hero) {
                kerbal.outDueToG = true;
                kerbal.hasToured = false;
                KerbalRoster.SetExperienceLevel(kerbal, 1);
            }
            kerbal.InventoryNode = new ConfigNode("INVENTORY");
            if (HighLogic.CurrentGame.CrewRoster.AddCrewMember(kerbal))
                if (!spawnVessel) HighLogic.CurrentGame.CrewRoster[data[mission].name].rosterStatus = ProtoCrewMember.RosterStatus.Available;
                else {
                    HighLogic.CurrentGame.CrewRoster[data[mission].name].rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
                    if (mission == Mission.Hero) status.hero = ProtoCrewMember.RosterStatus.Assigned;
                    else if (mission == Mission.Jeb) status.jeb = ProtoCrewMember.RosterStatus.Assigned;
                    else if (mission == Mission.Val) status.val = ProtoCrewMember.RosterStatus.Assigned;
                    else if (mission == Mission.Bill) status.bill = ProtoCrewMember.RosterStatus.Assigned;
                    else if (mission == Mission.Bob) status.bob = ProtoCrewMember.RosterStatus.Assigned;
                    VesselHandler(mission);
                }
            else foreach (ProtoCrewMember applicant in HighLogic.CurrentGame.CrewRoster.Applicants.ToList().Where(a => a.name.Contains(CrewGenerator.RemoveLastName(data[mission].name))))
                    HighLogic.CurrentGame.CrewRoster.Remove(applicant);
        }
        private void VesselHandler(Mission mission) {
            // GamePersistence.SaveGame("kerbalHero_backup_" + mission, HighLogic.SaveFolder, SaveMode.BACKUP);
            if (mission == Mission.Hero) for (int i = 0; i < 5; i++) SpawnVessel(mission, i);
            else SpawnVessel(mission);
            GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
            if ((int)mission > 1) AlarmClockScenario.AddAlarm(new AlarmTypeRaw {
                title = Localizer.Format("#autoLOC_463116") + " " + data[mission].vesselName + "!", ut = Planetarium.GetUniversalTime() + 1,
                actions = { warp = AlarmActions.WarpEnum.KillWarp, message = AlarmActions.MessageEnum.Yes }});
            else {
                int vesselIndex = -1;
                foreach (Vessel vsl in FlightGlobals.Vessels.Where(v => v.name.Contains(data[mission].vesselName + (mission == Mission.Hero ? " Debris 0" : ""))))
                    vesselIndex = FlightGlobals.Vessels.IndexOf(vsl);
                if (vesselIndex > -1) FlightDriver.StartAndFocusVessel("persistent", vesselIndex);
            }
        }
        private void SpawnVessel(Mission mission, int debrisIndex = -1) {
            string debrisModifier = debrisIndex > -1 ? "_Debris_" + debrisIndex : "";
            ConfigNode[] parts = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/KerbalHero/Data/" + mission + debrisModifier + ".cfg").GetNode("KerbalHeroData").GetNodes("PART");
            parts[0].AddValue("crew", data[mission].name);
            if (debrisIndex == 0)
                for (int i = 0; i < 3; i++) {
                    ProtoCrewMember crew = CrewGenerator.RandomCrewMemberPrototype(ProtoCrewMember.KerbalType.Tourist);
                    HighLogic.CurrentGame.CrewRoster.AddCrewMember(crew);
                    parts[0].AddValue("crew", crew.name);
                }
            Orbit orbit = data[mission].orbit;
            if (mission == Mission.Jeb) orbit.LAN = (105 + Planetarium.GetUniversalTime() / 21600 % 1 * 360) % 360;
            if (debrisIndex > 0) orbit.semiMajorAxis -= debrisIndex * 51;
            ConfigNode vessel = ProtoVessel.CreateVesselNode(data[mission].vesselName + debrisModifier.Replace("_"," "), data[mission].vesselType, orbit, 0, parts);
            vessel.SetValue("rot", mission == Mission.Jeb ? "0.0228343662,-0.0342921764,0.746270776,-0.664366305" : UnityEngine.Random.rotation.ToString().Trim('(', ')'));
            HighLogic.CurrentGame.AddVessel(vessel);
            // parts = new ConfigNode[1] { ProtoVessel.CreatePartNode(kerbal.gender == ProtoCrewMember.Gender.Female ? "kerbalEVAfemale" : "kerbalEVA", ShipConstruction.GetUniqueFlightID(HighLogic.CurrentGame.flightState), kerbal) };
            // [Mission.Hero] = new VesselData(settings.useSaveName ? HighLogic.SaveFolder : settings.name, VesselType.EVA, new Orbit(0.097, 0.995, 301000, 105.5, 90, Math.PI, Planetarium.GetUniversalTime(), Planetarium.fetch.Home)), // 3.14, 5.6
        }
        internal class KerbalHeroData {
            //internal Mission mission;
            internal string name;
            internal ProtoCrewMember.Gender gender;
            internal string trait;
            internal bool veteran;
            internal float courage;
            internal float stupidity;
            internal bool badS;
            internal string vesselName;
            internal VesselType vesselType;
            internal Orbit orbit;
            internal KerbalHeroData(string name, ProtoCrewMember.Gender gender, string trait, bool veteran, float courage, float stupidity, bool badS, string vesselName, VesselType vesselType, Orbit orbit) {
                //this.mission = mission;
                this.name = name;
                this.gender = gender;
                this.trait = trait;
                this.veteran = veteran;
                this.courage = courage;
                this.stupidity = stupidity;
                this.badS = badS;
                this.vesselName = vesselName;
                this.vesselType = vesselType;
                this.orbit = orbit;
            }
        }
    }
}
