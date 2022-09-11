using Anestrelsoft.StateTransition;
using Anestrelsoft.StateTransition.Core;

/*
    動作確認用のステートマシンを定義する。
    簡単に動作確認できるようにする。
*/

public class StateMachineCore01 : StateMachineBase
{
    //======================================================================================
    // Stateのインスタンスを作成
    //======================================================================================

    StateNode N01Entry = new StateNode();
    StateNode N02_Do01 = new StateNode();
    StateNode N03_Do02 = new StateNode();
    StateNode N04_Do03 = new StateNode();
    StateNode N05_Finish = new StateNode();


    //======================================================================================
    // Constructor
    //======================================================================================
    public StateMachineCore01()
    {
    }

    /// <summary>
    /// 状態遷移を定義する
    /// </summary>
    /// <returns>初期状態</returns>
    protected override StateNode SetupNodes()
    {
        //状態遷移を定義
        N01Entry.SetupNode(
            N01_Entrys_Action, false,
            ((arg, input) => arg.GetType().Equals(typeof(DoNextEventArg)), N02_Do01)
            );
        N02_Do01.SetupNode(
            N02_Do01_Action, false,
            ((arg, input) => arg.GetType().Equals(typeof(DoNextEventArg)), N03_Do02),
            ((arg, input) => arg.GetType().Equals(typeof(DoPrevEventArg)), N01Entry),
            ((arg, input) => arg.GetType().Equals(typeof(RedoEventArg)), N02_Do01)
            );
        N03_Do02.SetupNode(
            N03_Do02_Action, false,
            ((arg, input) => arg.GetType().Equals(typeof(DoNextEventArg)), N04_Do03)
            );
        N04_Do03.SetupNode(
            N04_Do03_Action, false,
            ((arg, input) => { Console.WriteLine("judge method"); return input.Count == 0; }, N04_Do03),
            ((arg, input) => input.Count != 0 && ((InputEventArg)input.Peek()).Kind == InputEventArg.KindList.Next, N05_Finish),
            ((arg, input) => input.Count != 0 && ((InputEventArg)input.Dequeue()).Kind == InputEventArg.KindList.Previus, N01Entry),
            ((arg, input) => true, N03_Do02)
            );
        N05_Finish.SetupNode(
            N05_Finish_Action, false
            );

        return N01Entry;
    }


    //======================================================================================
    // StateNodeActions
    //======================================================================================
    /// <summary>
    /// エントリー処理
    /// </summary>
    public async Task<IStateEventArgs> N01_Entrys_Action(IStateEventArgs arg, Queue<IStateEventArgs> input)
    {
        Console.WriteLine("State Entry!!");
        Console.WriteLine("Wait Input ----------- !!");

        InputEnable();
        return await Task.FromResult(new DoNextEventArg());
    }

    /// <summary>
    /// 入力待ち処理
    /// 判定をアクション内で行う方法
    /// </summary>
    public async Task<IStateEventArgs> N02_Do01_Action(IStateEventArgs arg, Queue<IStateEventArgs> input)
    {
        //Console.WriteLine("State02 Entry!!");
        IStateEventArgs? inputdata = null;

        if (input.Count != 0)
            inputdata = input.Dequeue();

        if (inputdata != null)
        {
            if (inputdata is InputEventArg)
            {
                Console.WriteLine("State02 Input!!");

                switch (((InputEventArg)inputdata).Kind)
                {
                    case InputEventArg.KindList.Next:
                        return await Task.FromResult(new DoNextEventArg());

                    case InputEventArg.KindList.Previus:
                        return await Task.FromResult(new DoPrevEventArg());

                    case InputEventArg.KindList.None:
                    default:
                        InputEnable();
                        System.Threading.Thread.Sleep(1);
                        return await Task.FromResult(new RedoEventArg());
                }
            }
        }

        System.Threading.Thread.Sleep(500);
        return await Task.FromResult(new RedoEventArg());
    }

    /// <summary>
    /// 入力待ち前処理
    /// </summary>
    public async Task<IStateEventArgs> N03_Do02_Action(IStateEventArgs arg, Queue<IStateEventArgs> input)
    {
        Console.WriteLine("State03 Entry!!");
        Console.WriteLine("Wait Input 2 ----------- !!");

        InputEnable();
        return await Task.FromResult(new DoNextEventArg());
    }

    /// <summary>
    /// 入力待ち
    /// 判定を条件式内で行う方法
    /// </summary>
    public async Task<IStateEventArgs> N04_Do03_Action(IStateEventArgs arg, Queue<IStateEventArgs>? input)
    {
        Console.WriteLine("action method");
        return await Task.FromResult(arg);
    }

    /// <summary>
    /// 終了前処理
    /// 最後に戻り値を返して、終了できる。
    /// </summary>
    public async Task<IStateEventArgs> N05_Finish_Action(IStateEventArgs arg, Queue<IStateEventArgs>? input)
    {
        Console.WriteLine("State04 Entry!!");

        return await Task.FromResult(new Result() { Message = "end desuwa yo." });
    }
}


//======================================================================================
// StateEventを定義する。
//======================================================================================
public class DoNextEventArg : IStateEventArgs
{
}

public class RedoEventArg : IStateEventArgs
{
}

public class DoPrevEventArg : IStateEventArgs
{
}

public class DoFinishEventArg : IStateEventArgs
{
}

public class Result : IStateEventArgs
{
    public string Message = "";
}

public class InputEventArg : IStateEventArgs
{
    public enum KindList
    {
        Next,
        Previus,
        Finish,

        None,
    }

    public KindList Kind = KindList.None;
}