open System.Threading.Tasks
open System.Collections.Generic

open Anestrelsoft.StateTransition
open Anestrelsoft.StateTransition.Core

printfn "Fsharp State Machine Example"

// 操作を簡単にするコンピュテーション式を追加する。
type StateBuilder() =
    member _.Bind (x: Task, f) =
        x.Wait()
        f ()

    member _.Bind (x: Task<'T>, f) =
        f x.Result

    member _.Return (x : IStateEventArgs) =
        task { return x }

// ステートマシン内で使用するデータ
type MachineArg () =
    interface IStateEventArgs

    member val Count = 0 with get, set

// ステートマシン内で使用するデータを操作する機能
module MachineArg =
    let castArg (arg: IStateEventArgs) =
        match arg with
        | :? MachineArg as v-> v
        | _ -> failwith "Value type is not MachineArg."

// ステートマシンの定義
type ExampleMachine () =
    inherit StateMachineBase()

    let state = StateBuilder()

    // -------------------------------------
    // Node リスト
    let N01_Entry = StateNode()
    let N02_Work = StateNode()
    let N03_Finish = StateNode()


    // -------------------------------------
    // 各状態のアクション

    let A01_Entry _ _ =
        printfn "Action : Entry"
        let arg = MachineArg()
        state {
            arg.Count <- 1
            return arg
        }

    let A02_Work arg _ =
        printfn "Action : Work"
        state {
            let arg = arg |> MachineArg.castArg
            arg.Count <- arg.Count + 3
            do! Task.Delay(1000)
            return arg
        }

    let A03_Finish arg _ =
        printfn "Action : Finish"
        state {
            let arg = arg |> MachineArg.castArg
            arg.Count <- arg.Count + 12
            do! Task.Delay(500)
            return arg
        }

    // -------------------------------------
    // ノードの設定
    override this.SetupNodes () =
        N01_Entry.SetupNode(
            stateAction = DStateAction(A01_Entry),
            isJudgeBeforeAction = false,
            transitions = [| DStateTransitionJudge(fun _ _ -> true), N02_Work |]
        )
        N02_Work.SetupNode(
            stateAction = DStateAction(A02_Work),
            isJudgeBeforeAction = false,
            transitions = [| DStateTransitionJudge(fun _ _ -> true), N03_Finish |]
        )
        N03_Finish.SetupNode(
            stateAction = DStateAction A03_Finish,
            isJudgeBeforeAction = false
        )

        N01_Entry

// -----------------------------------------------------------------
// ステートマシンの動作

let machine = ExampleMachine()
machine.InitNodes()

let t = machine.Run()
let r = t.Result |> MachineArg.castArg

printfn "result value is %A" r.Count

printfn "State End."
