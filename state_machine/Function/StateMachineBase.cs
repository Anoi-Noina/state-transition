using System;
using System.Collections.Generic;
using Anestrelsoft.StateTransition.Core;

namespace Anestrelsoft.StateTransition;

public abstract class StateMachineBase : StateMachineCore
{
    private bool _isCanceled = false;

    private IStateEventArgs _currentData;


    public IStateEventArgs CurrentData { get { return _currentData; } }

    /// <summary>
    /// キャンセル
    /// </summary>
    public bool IsCanceled { get { return _isCanceled; } }


    public StateMachineBase()
    {
        _currentData = new NullStateEventArgs();
    }

    public StateMachineBase(IStateEventArgs initialData)
    {
        _currentData = initialData;
    }

    /// <summary>
    /// 入力を待機する。
    /// </summary>
    /// <params>タイムアウト時間[ms]</params>
    /// <returns></returns>
    protected async Task<IStateEventArgs> ReadInputAsync(int timeout = -1)
    {
        bool istimeout = false;
        System.Timers.Timer timer = new();
        if (timeout > 0)
        {
            timer.Interval = timeout;
            timer.Elapsed += (s, e) => istimeout = true;
            timer.Start();
        }

        InputEnable(); //入力を許可する。
        IStateEventArgs value;
        while (ReadInput(out value) == -1 && !istimeout && !IsCanceled)
            await Task.Delay(1);

        return await Task.FromResult(value);
    }

    /// <summary>
    /// StateMachineからの通知を取得する
    /// </summary>
    /// <returns></returns>
    public async Task<IStateEventArgs> ReadNotifyAsync(int timeout = -1)
    {
        bool istimeout = false;
        System.Timers.Timer timer = new();
        if (timeout > 0)
        {
            timer.Interval = timeout;
            timer.Elapsed += (s, e) => istimeout = true;
            timer.Start();
        }

        IStateEventArgs value;
        while (ReadNotify(out value) == -1 && !istimeout && !IsCanceled)
            await Task.Delay(1);

        return await Task.FromResult(value);
    }

    /// <summary>
    /// 次の動作を行う。
    /// ステップ実行用
    /// </summary>
    /// <returns>true: StateMachineが終了  false: StateMachineが未終了</returns>
    public async Task<bool> Next()
    {
        //初回動作をさせる。
        _currentData = await Update(_currentData);

        if (IsFinished)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 非同期で動作する
    /// 終了時に値を出力する。
    /// </summary>
    /// <returns>最終出力</returns>
    public async Task<IStateEventArgs> Run()
    {
        while (!await Next() && !_isCanceled) ;

        //終了
        //最終時に保持しているデータを返却する。
        return _currentData;
    }

    /// <summary>
    /// キャンセル
    /// </summary>
    public void Cancel() => _isCanceled = true;

}