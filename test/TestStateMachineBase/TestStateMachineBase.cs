using Anestrelsoft.StateTransition;
using Anestrelsoft.StateTransition.Core;

namespace TestStateMachineBase;

public class TestStateMachineBase
{

    #region RunTest
    //クラス
    private class TestRunMachine : StateMachineBase
    {
        /*
        *   node list
        */
        StateNode node1 = new();
        StateNode node2 = new();

        public TestRunMachine(IStateEventArgs args) : base(args)
        {
        }

        protected override StateNode SetupNodes()
        {
            node1.SetupNode(
                async (iarg, input_list) =>
                {
                    return await Task.FromResult(new TestRunMachineEventArgs() { Number = 102 });
                },
                false,
                ((iarg, input_list) => true, node2)
                );
            node2.SetupNode(
                async (iarg, input_list) =>
                {
                    return await Task.FromResult(iarg);
                },
                false
            );
            return node1;
        }
    }

    private class TestRunMachineEventArgs : IStateEventArgs
    {
        public int Number { get; set; }
    }

    [Fact]
    public async void TestRun()
    {
        var machine = new TestRunMachine(new TestRunMachineEventArgs());
        machine.InitNodes();

        var value = (TestRunMachineEventArgs)await machine.Run();
        Assert.Equal(102, value.Number);
    }
    #endregion

    #region TestCancel
    /*
    * キャンセル動作のテスト
    */
    private class TestCancelMachine : StateMachineBase
    {
        StateNode node1 = new();
        StateNode node2 = new();
        protected override StateNode SetupNodes()
        {
            node1.SetupNode(
                async (args, input) =>
                {
                    await Task.Delay(50);
                    return await Task.FromResult(new TestCancelMachineEventArgs { Message = "Node1" });
                },
                false,
                ((args, input) => true, node2)
            );
            node2.SetupNode(
                async (args, input) =>
                {
                    return await Task.FromResult(new TestCancelMachineEventArgs { Message = "Node2" });
                },
                false
            );
            return node1;
        }
    }
    private class TestCancelMachineEventArgs : IStateEventArgs
    {
        public string Message { get; set; } = "";
    }

    [Fact]
    public void TestCancel()
    {
        // machineの生成
        var machine = new TestCancelMachine();
        machine.InitNodes();

        // キャンセル処理
        var task = machine.Run();
        machine.Cancel();

        var data = (TestCancelMachineEventArgs)task.Result;
        // 判定
        // machineタスクが終了している
        // キャンセルされている
        // Node1で停止している。
        Assert.True(task.IsCompleted);
        Assert.True(machine.IsCanceled);
        Assert.Equal("Node1", data.Message);
    }
    #endregion

    #region TestInputAsync
    class TestInputAsyncMachine : StateMachineBase
    {
        StateNode node1 = new();
        protected override StateNode SetupNodes()
        {
            node1.SetupNode(
                async (iarg, input) =>
                {
                    var input_data = (TestInputAsyncMachineEventArgs)await ReadInputAsync(50);
                    return await Task.FromResult(new TestInputAsyncMachineEventArgs { Message = input_data.Message });
                },
                false
            );
            return node1;
        }
    }
    class TestInputAsyncMachineEventArgs : IStateEventArgs
    {
        public string Message { get; set; } = "";
    }

    [Fact]
    public void TestInputAsync()
    {
        //マシンの生成
        var machine = new TestInputAsyncMachine();
        machine.InitNodes();

        //マシンを動作
        var task = machine.Run();

        //入力処理
        machine.Input(new TestInputAsyncMachineEventArgs { Message = "this is input" });
        var result = (TestInputAsyncMachineEventArgs)task.Result;

        Assert.Equal("this is input", result.Message);
    }
    #endregion

    #region TesWaitInputCancel
    class TestWaitInputCancelMachine : StateMachineBase
    {
        StateNode node1 = new();

        protected override StateNode SetupNodes()
        {
            node1.SetupNode(
                async (arg, input) =>
                {
                    //待ち時間を測定する。
                    System.Diagnostics.Stopwatch sw = new();
                    sw.Start();
                    await ReadInputAsync(1000);
                    sw.Stop();

                    if (IsCanceled)
                        return await Task.FromResult(new TestWaitInputCancelMachineEventArgs { Message = "WaitCancel", Time = sw.Elapsed.TotalMilliseconds });
                    else
                        return await Task.FromResult(new TestWaitInputCancelMachineEventArgs() { Message = "NotCancel", Time = sw.Elapsed.TotalMilliseconds });
                },
                false
            );
            return node1;
        }
    }
    class TestWaitInputCancelMachineEventArgs : IStateEventArgs
    {
        public string Message { get; set; } = "";
        public double Time { get; set; } = 0.0;
    }
    [Fact]
    public void TestWaitInputCancel()
    {
        //マシンの生成
        var machine = new TestWaitInputCancelMachine();
        machine.InitNodes();

        //マシンの動作
        var task = machine.Run();

        //インプットキャンセル
        machine.Cancel();
        var result = (TestWaitInputCancelMachineEventArgs)task.Result;

        //判定
        Assert.Equal("WaitCancel", result.Message);
        Assert.True(result.Time < 900); //タイムアウトで終了していないか確認する。
    }
    #endregion

}