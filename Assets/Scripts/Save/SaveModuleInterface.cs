// Creates a save module interface, ISaveModule, that can be used as a basis for binary serialization and deserialization
public interface ISaveModule
{
    // Serializes this modules related data and saves it to a binary file and returns success.
    bool serialize();

    // Deserializes this modules related data from the saved binary file and returns success.
    bool deserialize();
}
