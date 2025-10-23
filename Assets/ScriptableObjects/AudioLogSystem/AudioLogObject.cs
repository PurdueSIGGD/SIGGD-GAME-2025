using System.IO;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioLogObject", menuName = "Scriptable Objects/AudioLogObject")]
public class AudioLogObject : ScriptableObject
{
    public AudioClip voiceline;
    public string text;
    public double length;
}
