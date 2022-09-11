namespace Anestrelsoft.StateTransition.Core;

/// <summary>
/// StateMachineのエラー通知処理
/// </summary>
/// <param name="error"></param>
public delegate void DErrorStateNotification(IStateEventArgs error);