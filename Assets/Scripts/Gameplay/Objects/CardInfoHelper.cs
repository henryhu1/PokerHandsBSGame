using System.Collections.Generic;

public static class CardInfoHelper<T>
{
    public static List<T> OrderDataByID(ulong myId, T[] data, ulong[] dataIDIndices, ulong[] order)
    {
        Dictionary<ulong, T> map = new();
        for (int i = 0; i < data.Length; i++)
        {
            ulong currentClientId = dataIDIndices[i];
            if (myId != currentClientId)
            {
                T opponentCardInfo = data[i];
                map.Add(currentClientId, opponentCardInfo);
            }
        }

        // Client must figure out turn order relative to itself,
        //    server can also do this for each client though
        List<T> orderedData = new();
        foreach (ulong clientId in order)
        {
            if (myId != clientId)
            {
                orderedData.Add(data[clientId]);
            }
        }

        return orderedData;
    }
}
