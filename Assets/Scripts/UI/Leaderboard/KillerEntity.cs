using System;
using Unity.Collections;
using Unity.Netcode;


public struct KillerEntity : INetworkSerializable, IEquatable<KillerEntity>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;
    public int TotalKills;


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref TotalKills);     
    }

    public bool Equals(KillerEntity other)
    {
        return ClientId == other.ClientId && PlayerName.Equals(other.PlayerName) && TotalKills == other.TotalKills;
    }
}
