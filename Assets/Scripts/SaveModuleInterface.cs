using System;
using UnityEngine;

//Creates a save module interface, ISaveModule, that can be used as a basis for Json serialization and deserialization
public interface ISaveModule
{

    //Serializes an object and returns the Json script
    string serialize(System.Object item);

    //Deserializes an object from the Json script and returns the object
    System.Object deserialize(string item);
}
