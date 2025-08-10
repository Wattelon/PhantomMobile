using System.Collections.Generic;
using UnityEngine;

public class Atlas : MonoBehaviour
{
    [SerializeField] private List<MeshRenderer> meshRenderers;

    public void SetSkinVisibility(bool visible)
    {
        SetAtlasVisibility(0, visible);
    }
    
    public void SetBonesVisibility(bool visible)
    {
        SetAtlasVisibility(1, visible);
    }
    
    public void SetArteriesVisibility(bool visible)
    {
        SetAtlasVisibility(2, visible);
    }
    
    public void SetVeinsVisibility(bool visible)
    {
        SetAtlasVisibility(3, visible);
    }
    
    public void SetTracheaVisibility(bool visible)
    {
        SetAtlasVisibility(4, visible);
    }
    
    public void SetLymphNodesVisibility(bool visible)
    {
        SetAtlasVisibility(5, visible);
    }
    
    public void SetThyroidGlandVisibility(bool visible)
    {
        SetAtlasVisibility(6, visible);
    }

    public void SetAtlasVisibility(int index, bool visible)
    {
        meshRenderers[index].enabled = visible;
    }
}