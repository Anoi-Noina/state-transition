namespace Anestrelsoft.StateTransition.Core;

/// <summary>
/// 状態の実行定義用デリゲート
/// </summary>
/// <param name="arg"></param>
/// <returns></returns>
public delegate Task<IStateEventArgs> DStateAction(IStateEventArgs arg, Queue<IStateEventArgs> input);