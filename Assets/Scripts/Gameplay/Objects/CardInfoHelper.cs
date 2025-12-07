using System.Collections.Generic;
using System.Linq;

public static class CardInfoHelper<T>
{
    public static List<T> OrderDataByID(ulong myId, T[] data, ulong[] dataIDIndices, ulong[] order)
    {
        List<ulong> idList = dataIDIndices.ToList();
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
        for (int i = 0; i < order.Length; i++)
        {
            ulong clientId = order[i];
            if (myId != clientId)
            {
                int index = idList.FindIndex(id => id == clientId);
                orderedData.Add(data[index]);
            }
        }

        return orderedData;
    }
}
