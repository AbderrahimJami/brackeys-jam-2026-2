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

    public static void Interact(InteractionKind kind)
    {
        if (Interacted != null) Interacted(kind);
    }
}