namespace TestStateMachineBase;

public class TestStateMachineBase
{
    #region  UnitTestMachine
    //テスト用の状態遷移クラス01
    class TestMachine01 : StateMachineCore
    {
        //テストノード
        StateNode Node1 = new StateNode();
        StateNode Node2 = new StateNode();

        protected override StateNode SetupNodes()
        {
            Node1.SetupNode(
                stateAction: Node1_Action,
                isJudgeBeforeAction: false,
                ((args, input) => true, Node2)
            );
            Node2.SetupNode(
                stateAction: Node2_Action,
                isJudgeBeforeAction: false
            );
            return Node1;
        }

        private async Task<IStateEventArgs> Node1_Action(IStateEventArgs arg, Queue<IStateEventArgs> input)
        {
            return await Task.FromResult(new Node1Result { Message = "Node1Result" });
        }

        private async Task<IStateEventArgs> Node2_Action(IStateEventArgs arg, Queue<IStateEventArgs> input)
        {
            InputEnable();
            IStateEventArgs read;
            while (ReadInput(out read) == -1)
                await Task.Delay(1);
            InputDisable();
            OnOutStateNotifiction(new Node2Out { Message = "Node2Out:" + ((Node2In)read).Message });
            return await Task.FromResult(new Node2Result { Message = "Node2Result" });
        }
    }
    class Node1Result : IStateEventArgs
    {
        public string Message = "";
    }
    class Node2In : IStateEventArgs
    {
        public string Message = "";
    }
    class Node2Out : IStateEventArgs
    {
        public string Message = "";
    }
    class Node2Result : IStateEventArgs
    {
        public string Message = "";
    }

    [Fact]
    public async void UnitTestMachine()
    {
        var machine = new TestMachine01();
        //node1
        var node1result = (Node1Result)await machine.Update(new NullStateEventArgs());
        Assert.Equal("Node1Result", node1result.Message);
        //node2
        var task = machine.Update(node1result);
        //node2 input
        machine.Input(new Node2In { Message = "Input" });
        //node2 read
        IStateEventArgs node2notify;
        while (machine.ReadNotify(out node2notify) == -1)
            await Task.Delay(1);
        Assert.Equal("Node2Out:Input", ((Node2Out)node2notify).Message);
        //node2 end
        var node2result = (Node2Result)await task;
        Assert.Equal("Node2Result", node2result.Message);
    }
    #endregion

    #region TestTrans
    class TestTransMachine : StateMachineCore
    {
        StateNode node1 = new StateNode();
        StateNode node2 = new StateNode();

        protected override StateNode SetupNodes()
        {
            node1.SetupNode(
                stateAction: async (args, input) => await Task.FromResult(new TransStateArgs { Message = "Result1" }),
                false,
                ((iarg, input) => true, node2)
            );
            node2.SetupNode(
                stateAction: async (args, input) => await Task.FromResult(new TransStateArgs { Message = "Result2:" + ((TransStateArgs)args).Message }),
                false
            );
            return node1;
        }
    }

    class TransStateArgs : IStateEventArgs
    {
        public string Message = "";
    }

    [Fact]
    public async void TestTrans()
    {
        var machine = new TestTransMachine();
        //node1実行
        var data1 = (TransStateArgs)await machine.Update(new NullStateEventArgs());
        //node2実行
        var data2 = (TransStateArgs)await machine.Update(data1);

        Assert.Equal("Result1", data1.Message);
        Assert.Equal("Result2:Result1", data2.Message);
    }
    #endregion

    #region InputTest
    class InputTestMachine : StateMachineCore
    {

        StateNode node1 = new StateNode();

        public string InputMessage = "";
        protected override StateNode SetupNodes()
        {
            InputEnable();
            node1.SetupNode(
                async (arg, input) => await Task.Run(async () =>
                {
                    IStateEventArgs value;
                    while (ReadInput(out value) == -1)
                        await Task.Delay(1);
                    InputMessage = ((InputArgs)value).Message;
                    return new NullStateEventArgs();
                }),
                    false
            );
            return node1;
        }
    }
    class InputArgs : IStateEventArgs
    {
        public string Message = "";
    }

    [Fact]
    public async void InputTest()
    {
        var machine = new InputTestMachine();

        //nodeを動作させる。
        var machine_task = machine.Update(new NullStateEventArgs());
        //input
        //inputできれば成功
        Assert.True(machine.Input(new InputArgs { Message = "input" }));
        //タスク終了待ち
        await machine_task;
        //判定
        Assert.Equal("input", machine.InputMessage);
    }
    #endregion

    #region  OutputTest
    class OutputTestMachine : StateMachineCore
    {

        StateNode node1 = new StateNode();
        protected override StateNode SetupNodes()
        {
            node1.SetupNode(
                async (arg, input) =>
                {
                    OnOutStateNotifiction(new OutputArgs { Message = "output" });
                    return await Task.FromResult(new NullStateEventArgs());
                },
                false
            );
            return node1;
        }
    }
    class OutputArgs : IStateEventArgs
    {
        public string Message = "";
    }

    [Fact]
    public async void OutputTest()
    {
        //マシンの生成
        var machine = new OutputTestMachine();
        //node1を動作
        await machine.Update(new NullStateEventArgs());
        //Outputを取得
        IStateEventArgs readvalue;
        while (machine.ReadNotify(out readvalue) == -1)
            await Task.Delay(1);
        //中身を確認
        Assert.Equal("output", ((OutputArgs)readvalue).Message);
    }

    #endregion

}