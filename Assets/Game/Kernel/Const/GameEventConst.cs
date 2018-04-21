
public static class GameEventConst
{
	private const int kGameEventBeginIndex = 10000;

	public const int kOnApplicationQuit = kGameEventBeginIndex + 1;

	public const int kOnLoadScene = kGameEventBeginIndex + 2;

	public const int kNotifyNeedLoadBundle = kGameEventBeginIndex + 3;

	public const int kOnLanguageUpdate = kGameEventBeginIndex + 4;

	public static readonly string kOnApplicationPause = string.Format("{0}", kGameEventBeginIndex + 5);

	public static readonly string kOnApplicationFocus = string.Format("{0}", kGameEventBeginIndex + 6);

	public const int kOnFinishPlayingRecord = kGameEventBeginIndex + 7;

	public const int kOnFinishLoadScene = kGameEventBeginIndex + 8;

	#region battle > kGameEventBeginIndex + 1000

	private const int kBattleEventBeginIndex = kGameEventBeginIndex + 1000;

	public static readonly string kOnCardBeginDrag = string.Format("{0}", kBattleEventBeginIndex + 4);
	public const int kOnQuitRound = kBattleEventBeginIndex + 1;

	#endregion

	public const string EVENT_LOGIN="event_loginSys";
	public const string EVENT_ReStartClient="event_restartClient";

	public const string EVENT_Close_MatchSLTDeckpanel="event_close_matchsltdeckpanel";
}
