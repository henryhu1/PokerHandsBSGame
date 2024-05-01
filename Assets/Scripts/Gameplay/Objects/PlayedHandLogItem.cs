public class PlayedHandLogItem
{
    public readonly PokerHand m_playedHand;
    public readonly string m_playerId;
    public readonly string m_playerName;
    public readonly bool m_existsInRound;

    public PlayedHandLogItem(PokerHand playedHand, string playerId, string playerName)
    {
        m_playedHand = playedHand;
        m_playerId = playerId;
        m_playerName = playerName;
        m_existsInRound = CardManager.Instance.IsHandInPlay(playedHand);
    }

    public bool IsPokerHandBetter(PokerHand pokerHand)
    {
        return m_playedHand.CompareTo(pokerHand) < 0;
    }
}
