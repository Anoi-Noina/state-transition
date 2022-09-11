namespace Anestrelsoft.StateTransition.Core;

/// <summary>
/// ステートマシンの通知処理型定義
/// </summary>
/// <param name="notify"></param>
public delegate void DOutStateNotification(IStateEventArgs notify);