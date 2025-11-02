using System.IO;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioLogObject", menuName = "Scriptable Objects/AudioLogObject")]
public class AudioLogObject : ScriptableObject
{
    
    [System.Serializable]
    public struct lineInfo
    {
        public string line;
        public float seconds;
    }
    public lineInfo[] subtitles;
    public string audioName;
}