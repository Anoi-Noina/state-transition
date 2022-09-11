// See https://aka.ms/new-console-template for more information
using Anestrelsoft.StateTransition.Core;

/*
 * StateMachineの動作確認
 * 
 */
Console.WriteLine("Start Do State Machine.");

var machine = new StateMachineCore01();

IStateEventArgs? nodearg = new DoNextEventArg();

Task.Run(async () =>
{
    while (!machine.IsFinished)
    {
        if (nodearg != null)
            nodearg = await machine.Update(nodearg);
        else
        {
            throw new ArgumentException("argmunt is null.");
        }
    }

    Console.WriteLine("Machine End");
    if(nodearg != null)
        Console.WriteLine("Result Message : " + ((Result)nodearg).Message);
});

while (!machine.IsFinished)
{
    var line = Console.ReadLine();
    Console.WriteLine("key pusshed " + line);
    switch (line)
    {
        case "next":
            Console.WriteLine("input next");
            machine.Input(new InputEventArg { Kind = InputEventArg.KindList.Next });
            break;
        case "prev":
            Console.WriteLine("input prev");
            machine.Input(new InputEventArg { Kind = InputEventArg.KindList.Previus });
            break;
        case "redo":
            Console.WriteLine("input redo");
            machine.Input(new InputEventArg { Kind = InputEventArg.KindList.None });
            break;
        default:
            Console.WriteLine("input other");
            machine.Input(new InputEventArg { Kind = InputEventArg.KindList.None });
            break;
    }
}
Console.WriteLine($"All End");
