namespace Anestrelsoft.StateTransition.Core;

/// <summary>
/// 遷移条件を格納するためのデリゲート
/// </summary>
/// <param name="arg">引数</param>
/// <param name="input">入力情報</param>
/// <returns></returns>
public delegate bool DStateTransitionJudge(IStateEventArgs arg, Queue<IStateEventArgs> input);