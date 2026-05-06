using System;
using System.Collections.Generic;
using UnityEngine;

namespace NursingSim.Data
{
    [Serializable]
    public class PatientProfile
    {
        public string displayName = "김철수";
        public int ageYears = 45;
        public string sexLabel = "남";
        public string registrationNumber = "20260001";
        [TextArea] public string diagnosis = "통증 호소";
        [TextArea] public string notes;
    }

    [Serializable]
    public class Prescription
    {
        public string drugName = "Diclofenac";
        public string dose = "4mg";
        public string route = "IM";
        public string frequency = "1회";
    }

    [CreateAssetMenu(menuName = "NursingSim/Scenario", fileName = "SO_Scenario_")]
    public class NursingScenario : ScriptableObject
    {
        public string scenarioId = "SCN_IM_INJECTION_001";
        [TextArea] public string title = "근육주사 투약";
        [TextArea] public string briefingText;
        public ScenarioLevel level = ScenarioLevel.Easy;
        public int maxScore = 100;
        public string sceneKey = "Simulation_IMInjection";
        public PatientProfile patient = new PatientProfile();
        public Prescription prescription = new Prescription();
        public List<ScenarioStep> steps = new List<ScenarioStep>();
    }
}
