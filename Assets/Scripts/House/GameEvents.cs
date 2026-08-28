using System;

public enum InteractionKind
{
    Pickup,
    KeyItem,
    Door,
    LightSwitch,
    RoomExit,
    RoomEnter,
    Other
}

// dumb static bus. anyone can fire, anyone can listen, no references needed
public static class GameEvents
{
    public static Action<InteractionKind> Interacted;
    public static Action HouseShuffled;   // vfx/audio hook onto this
    public static Action<int> KeysChanged;    // new key total
    public static Action<int> ExitRefused;    // how many keys still missing
    public static Action<bool> RunEnded;      // true = escaped
    public static Action<ReadableInteractables> NoteOpened;
    public static Action NoteClosed;

    public static void Interact(InteractionKind kind)
    {
        if (Interacted != null) Interacted(kind);
    }
}