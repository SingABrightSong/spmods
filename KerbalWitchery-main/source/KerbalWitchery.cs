using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using KSP.Localization;

namespace KerbalWitchery {
    public class ModuleEngineWitchery : PartModule {
        [KSPField(isPersistant = true, guiActive = true)]
        public int ignitions;
        private ModuleEngines engine;
        private float minThrottle;
        private bool ignited;
        private bool exploding;
        private readonly EngineType[] types = new EngineType[3] { EngineType.LiquidFuel, EngineType.Electric, EngineType.Nuclear };
        public void Start() {
            if (HighLogic.LoadedSceneIsFlight) {
                engine = part.FindModulesImplementing<ModuleEngines>().First(e => types.Contains(e.engineType));
                minThrottle = engine.throttleMin; }
        }
        public void FixedUpdate() {
            if (HighLogic.LoadedSceneIsFlight) {
                if (part.vessel.ctrlState.mainThrottle > 0f) engine.throttleMin = minThrottle;
                else engine.throttleMin = 0f;
                if (engine.GetCurrentThrust() > 0f) {
                    if (ignited == false && !exploding) {
                        if (ignitions > UnityEngine.Random.Range(5, 8)) {
                            exploding = true;
                            StartCoroutine(CallbackUtil.DelayedCallback(UnityEngine.Random.Range(1, 100), delegate { part.explode(); }));
                        } else if (ignitions > UnityEngine.Random.Range(0, 7)) {
                            ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_236416") + " " + engine.part.partInfo.title + " " +
                                Localizer.Format("#autoLOC_7001053") + " " + Localizer.Format("#autoLOC_8003101"));
                            engine.Shutdown();
                        } else ignited = true;
                        ignitions++; }
                } else ignited = false; }
        }
    }
}
