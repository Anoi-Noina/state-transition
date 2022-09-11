namespace Anestrelsoft.StateTransition.Core;

public abstract class StateMachineCore
{
    //==============================================================================================
    // Private Valiables
    //==============================================================================================
    /// <summary>
    /// StateMachineの終了フラグ
    /// </summary>
    private bool _isFinished = false;

    /// <summary>
    /// 現在の状態
    /// </summary>
    /// <value></value>
    private StateNode _current = new NullStateNode();

    /// <summary>
    /// 入力データキュー
    /// </summary>
    private Queue<IStateEventArgs> _inDataQue = new();

    /// <summary>
    /// 出力データキュー
    /// </summary>
    private Queue<IStateEventArgs> _NotifyDataQue = new();

    /// <summary>
    /// 入力許可
    /// </summary>
    private bool _isAllowInput = false;


    //==============================================================================================
    // Event
    //==============================================================================================
    /// <summary>
    /// 標準通知イベント
    /// </summary>
    public event DOutStateNotification? OutStateNotification;

    /// <summary>
    /// エラー通知イベント
    /// </summary>
    public event DErrorStateNotification? ErrorStateNotification;


    //==============================================================================================
    //  Protected Property
    //==============================================================================================


    //==============================================================================================
    // Public Property
    //==============================================================================================
    /// <summary>
    /// StateMachineの終了フラグs
    /// </summary>
    /// <value></value>
    public bool IsFinished { get { return _isFinished; } }

    /// <summary>
    /// StateMachineからの通知方法でイベントを使用するか。
    /// </summary>
    public bool IsUseNotifyEvent { get; set; } = false;


    //==============================================================================================
    // Constructor
    //==============================================================================================
    public StateMachineCore()
    {
        _current = SetupNodes();
    }

    //==============================================================================================
    // Private Function
    //==============================================================================================
    /// <summary>
    /// 条件から次のノードを取得する。
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private StateNode GetNextStateNode(IStateEventArgs arg, Queue<IStateEventArgs> inputData)
    {
        if (_current is null)
            return new NullStateNode();

        foreach (var transinfo in _current.TrasnJudgeList)
        {
            if (transinfo.transJudge(arg, inputData))
            {
                return transinfo.stateNode;
            }
        }
        return new NullStateNode();
    }


    //==============================================================================================
    // Abstractor Function
    //==============================================================================================
    /// <summary>
    /// ノードを初期化する。
    /// 初期状態を戻り値として設定する。
    /// </summary>
    /// <returns>初期状態ノード</returns>
    protected abstract StateNode SetupNodes();


    //==============================================================================================
    // Vitural Method
    //==============================================================================================


    //==============================================================================================
    // Protected Method
    //==============================================================================================
    /// <summary>
    /// 標準通知処理
    /// </summary>
    /// <param name="notify"></param>
    protected void OnOutStateNotifiction(IStateEventArgs notify)
    {
        if (IsUseNotifyEvent)
            OutStateNotification?.Invoke(notify);
        else
            _NotifyDataQue.Enqueue(notify);
    }

    /// <summary>
    /// エラー通知処理
    /// </summary>
    /// <param name="notify"></param>
    protected void OnErrorStateNotifiction(IStateEventArgs notify)
    {
        if (IsUseNotifyEvent)
            ErrorStateNotification?.Invoke(notify);
        else
            _NotifyDataQue.Enqueue(notify);
    }

    /// <summary>
    /// 入力受付を開始する。
    /// </summary>
    protected void InputEnable()
    {
        _isAllowInput = true;
    }

    /// <summary>
    /// 入力データをクリアする。
    /// /// /// </summary>
    protected void InputDataClear()
    {
        _inDataQue = new();
    }


    /// <summary>
    /// インプットデータを取得する。
    /// データがなければNullStateEventArgsを返却する。
    /// </summary>
    /// <param name="value">入力データ</param>
    /// <returns>残りデータ数、データがなければ-1を返却する</returns>
    protected int ReadInput(out IStateEventArgs value)
    {
        IStateEventArgs? data;
        if (_inDataQue.TryDequeue(out data))
        {
            value = data;
            return _inDataQue.Count;
        }
        else
        {
            value = new NullStateEventArgs();
            return -1;
        }
    }

    /// <summary>
    /// 現在持っている入力を全て取得する。
    /// データがなければ空のリストを返す。
    /// </summary>
    protected List<IStateEventArgs> ReadAllInput()
    {
        var values = new List<IStateEventArgs>();
        while (_inDataQue.Any())
            values.Add(_inDataQue.Dequeue());
        return values;
    }

    /// <summary>
    /// 入力受付を停止する。
    /// 強制的に終了したい場合に使用する。
    /// </summary>
    protected void InputDisable()
    {
        InputDataClear();
        _isAllowInput = false;
    }


    //==============================================================================================
    // Functions
    //==============================================================================================
    /// <summary>
    /// 状態を更新する。
    /// インプットがあれば、引き渡す。
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public async Task<IStateEventArgs> Update(IStateEventArgs arg)
    {
        IStateEventArgs result = arg;

        /* 終了していればNullを返す。 */
        if (_isFinished)
            return new NullStateEventArgs();

        /* Nodeがセットされているかを確認 */
        if (_current.GetType().Equals(typeof(NullStateNode)))
            throw new Exception("StateNode is not set.");

        /* Action前遷移を判定*/
        if (_current.IsJudgeBeforeAction)
        {
            _current = GetNextStateNode(arg, _inDataQue);
            if (_current.GetType().Equals(typeof(NullStateNode)))
            {
                //次のNodeが無い場合は処理を終了する。
                _isFinished = true;
                return result;
            }
        }

        /* Action処理 */
        if (_current.GetType().Equals(typeof(NullStateNode)))
            throw new Exception("StateNode does not have StateAction.");
        else
        {
            result = await _current.StateAction(arg, _inDataQue);
        }

        /* Action 処理後の判定  */
        if (!_current.IsJudgeBeforeAction)
        {
            _current = GetNextStateNode(result, _inDataQue);
            if (_current.GetType().Equals(typeof(NullStateNode)))
            {
                _isFinished = true;
                return result;
            }
        }

        return result;
    }

    /// <summary>
    /// 入力処理 喋る
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public bool Input(IStateEventArgs input)
    {
        if (_isAllowInput)
        {
            if (input != null)
            {
                _inDataQue.Enqueue(input);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// StateMachineからの通知を取得する。
    /// データがなければNullStateEventArgsを返却する。
    /// </summary>
    /// <param name="value">通知データ</param>
    /// <returns>残りデータ数、データがなければ-1を返却する</returns>
    public int ReadNotify(out IStateEventArgs value)
    {
        IStateEventArgs? data;
        if (_NotifyDataQue.TryDequeue(out data))
        {
            value = data;
            return _NotifyDataQue.Count;
        }
        else
        {
            value = new NullStateEventArgs();
            return -1;
        }
    }

    /// <summary>
    /// 現在保持している入力を全て取得する。
    /// </summary>
    /// <returns></returns>
    public List<IStateEventArgs> ReadAllNotify()
    {
        var values = new List<IStateEventArgs>();
        while (_NotifyDataQue.Any())
            values.Add(_NotifyDataQue.Dequeue());
        return values;
    }
}