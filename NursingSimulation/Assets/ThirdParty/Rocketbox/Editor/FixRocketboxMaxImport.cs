using UnityEngine;
using UnityEditor;

public class FixRocketboxMaxImport : AssetPostprocessor
{
    bool usingMixamoAnimations = true; 
    void OnPostprocessMaterial(Material material)
    {
        // This fixes two problems with importing 3DSMax materials. The first is that the Max materials
        // assumed that diffuse material was set by the texture, whereas Unity multiplies the texture 
        // colour with the flat colour. 
        material.color = Color.white;
        // Second Unity's transparent  materials still show specular highlights and thus hair looks 
        // like glass sheets. The material mode "Fade" goes to full transparent. 
        if (material.GetFloat("_Mode") == 3f)
            material.SetFloat("_Mode", 2f);
    }

    void OnPostprocessMeshHierarchy(GameObject gameObject)
    {
        // This function selects only the highest resolution mesh as being activated by default.
        // You might choose another poly level (they are "hipoly", "midpoly", "lowpoly" and "ultralowpoly")
        // to be selected. Or you could choose not to import, by changing OnPreprocessMeshHierarchy
        if (gameObject.name.ToLower().Contains("poly") &&
            !gameObject.name.ToLower().Contains("hipoly"))
            gameObject.SetActive(false);
    }
    
    void OnPreprocessTexture()
    {
        // This function changes textures that are labelled with "normal" in their title to be loaded as 
        // NormalMaps. This just avoids a warning dialogue box that would otherwise fix it.
        if (assetPath.ToLower().Contains("normal"))
        {
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.textureType = TextureImporterType.NormalMap;
            textureImporter.convertToNormalmap = false;
        }
    }

    void OnPostprocessModel(GameObject g)
    {
        if (!assetPath.Replace('\\', '/').Contains("/Rocketbox/")) return;

        if (g.transform.Find("Bip02") != null) RenameBip(g);

        Transform bip01 = g.transform.Find("Bip01");
        if (bip01 == null) return;
        Transform pelvis = bip01.Find("Bip01 Pelvis");
        if (pelvis == null) return;
        Transform spine = pelvis.Find("Bip01 Spine");
        Transform spine1 = spine != null ? spine.Find("Bip01 Spine1") : null;
        Transform spine2 = spine1 != null ? spine1.Find("Bip01 Spine2") : null;
        if (spine2 == null) return;
        Transform neck = spine2.Find("Bip01 Neck");
        Transform RClavicle = neck != null ? neck.Find("Bip01 R Clavicle") : null;
        Transform LClavicle = neck != null ? neck.Find("Bip01 L Clavicle") : null;

        if (!usingMixamoAnimations && RClavicle != null && LClavicle != null)
        {
            Transform lThigh = spine.Find("Bip01 L Thigh");
            Transform rThigh = spine.Find("Bip01 R Thigh");
            if (lThigh != null) lThigh.parent = pelvis;
            if (rThigh != null) rThigh.parent = pelvis;
            LClavicle.parent = spine2;
            RClavicle.parent = spine2;

            LClavicle.rotation = new Quaternion(-0.7215106f, 0, 0, 0.6924035f);
            RClavicle.rotation = new Quaternion(0, -0.6925546f, 0.721365f, 0);
            Transform lUpper = LClavicle.Find("Bip01 L UpperArm");
            Transform rUpper = RClavicle.Find("Bip01 R UpperArm");
            if (lUpper != null) lUpper.rotation = new Quaternion(0, 0, 0, 0);
            if (rUpper != null) rUpper.rotation = new Quaternion(0, 0, 0, 0);
        }

        // animationType는 외부 툴(RocketboxVerificationTool)에서 설정. 여기서 강제 지정하지 않음.
    }
    private void RenameBip(GameObject currentBone)
    {
        currentBone.name = currentBone.name.Replace("Bip02", "Bip01");
        for (int i = 0; i < currentBone.transform.childCount; i++)
        {
            RenameBip(currentBone.transform.GetChild(i).gameObject);
        }

    }
}
