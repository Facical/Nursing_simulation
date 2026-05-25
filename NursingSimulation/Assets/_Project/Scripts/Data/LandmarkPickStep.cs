using System;
using System.Collections.Generic;
using UnityEngine;

namespace NursingSim.Data
{
    [Serializable]
    public class LandmarkPoint
    {
        public string id;
        public string label;
    }

    [CreateAssetMenu(menuName = "NursingSim/Step/LandmarkPick", fileName = "SO_Step_Landmark_")]
    public class LandmarkPickStep : ScenarioStep
    {
        public List<LandmarkPoint> points = new List<LandmarkPoint>();
        public bool requireOrder = true;
        public override StepCategory Category => StepCategory.LandmarkPick;
    }
}
