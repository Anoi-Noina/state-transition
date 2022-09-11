namespace Anestrelsoft.StateTransition.Core;
//状態を表現するノード
public class StateNode
{
    //==============================================================================================
    // 定義
    //==============================================================================================
    /// <summary>
    /// 状態が実行する作業
    /// </summary>
    /// <value></value>
    public DStateAction StateAction { get; private set; }

    /// <summary>
    /// 遷移条件とノードの関連付け表
    /// </summary>
    /// <value></value>
    public List<(DStateTransitionJudge transJudge, StateNode stateNode)> TrasnJudgeList { get; private set; }

    /// <summary>
    /// Action前に判定するかどうか
    /// </summary>
    public bool IsJudgeBeforeAction { get; set; } = false;


    //==============================================================================================
    // Constructor
    //==============================================================================================
    public StateNode()
    {
        TrasnJudgeList = new List<(DStateTransitionJudge, StateNode)>();
        StateAction = async (arg, input) => await Task.FromResult<IStateEventArgs>(new NullStateEventArgs());
    }


    //==============================================================================================
    // Functions
    //==============================================================================================
    /// <summary>
    /// イベントと遷移先状態を関連付ける。
    /// </summary>
    /// <param name="eventtype">遷移イベント</param>
    /// <param name="state">遷移先状態ノード</param>
    /// <param name="isBeforeAction">Actionの前に判定するか</param>
    private void AssociationEnventNode(DStateTransitionJudge transitionJudge, StateNode state)
    {
        TrasnJudgeList.Add((transitionJudge, state));
    }


    /// <summary>
    /// 状態を設定する。
    /// </summary>
    /// <param name="stateAction">状態が行う動作</param>
    /// <param name="transitions">遷移イベントと遷移先の状態ノード</param>
    public void SetupNode(DStateAction stateAction, bool isJudgeBeforeAction, params (DStateTransitionJudge transitionJudge, StateNode statenode)[] transitions)
    {
        IsJudgeBeforeAction = isJudgeBeforeAction;

        if (stateAction == null)
            throw new ArgumentException("StateAction is null.");
        else if (transitions == null)
            throw new ArgumentException("Transtions is null.");
        else
        {
            StateAction = stateAction;
            foreach (var trans in transitions)
            {
                AssociationEnventNode(trans.transitionJudge, trans.statenode);
            }
        }
    }
}
