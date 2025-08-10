using UnityEngine;

[CreateAssetMenu(fileName = "ComputerTomographySO", menuName = "Scriptable Objects/ComputerTomography")]
public class ComputerTomographySO : TomographySO
{
/*#if UNITY_EDITOR
    private protected override void OnValidate()
    {
        tomography = Tomography.Computer;
        base.OnValidate();
    }
    
    private protected override void Reset()
    {
        tomography = Tomography.Computer;
        base.Reset();
    }
#endif*/
}