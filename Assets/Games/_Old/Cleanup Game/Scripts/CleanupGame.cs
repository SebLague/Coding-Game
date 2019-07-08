using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanupGame : Task {

    string[] functionNames = {
        "Left",
        "Right",
        "Up",
        "Down"
    };

    public CleanupBot bot;
    public GameObject junkPrefab;
    Vector3[, ] grid;

    Vector2Int targetCoord;

    Queue<string> commandStringQueue;

    protected override void Start () {
        base.Start();
        grid = FindObjectOfType<CleanupGrid> ().grid;

        Vector2Int coord = new Vector2Int (grid.GetLength (0) / 2, grid.GetLength (1) / 2);
        bot.Init (grid, coord);

        //BeginGame ();
    }

    string GetOutputs () {
        float[] inputValues = { bot.coord.x, bot.coord.y, targetCoord.x, targetCoord.y };
        //var compiler = new VirtualCompiler (scriptEditor.code);
       // compiler.SetInputs (inputNames, inputValues);
        //compiler.SetFunctions (functionNames);

        //string result = compiler.Run ();
        return "";
    }

    protected override void Run (string code) {
        base.Run (code);
        commandStringQueue = new Queue<string> ();

        AddJunk ();
        RunGameStep ();
    }

    void RunGameStep () {
        if (commandStringQueue.Count == 0) {
            string newCommandString = GetOutputs ();
            print (newCommandString);
            if (newCommandString.Contains (",")) {
                string[] newCommandStrings = newCommandString.Split (',');
                for (int i = 0; i < newCommandStrings.Length; i++) {
                    commandStringQueue.Enqueue (newCommandStrings[i]);
                }
            } else {
                commandStringQueue.Enqueue (newCommandString);
            }
        }

        if (commandStringQueue.Count > 0) {
            string commandString = commandStringQueue.Dequeue ();

        } else {
            print ("No command");
        }

        var command = CleanupBot.Command.Left;
        bot.ExecuteCommand (command, BotCompletedCommand);
    }

    void BotCompletedCommand () {
        RunGameStep ();
    }

    void AddJunk () {
        Vector2Int coord = Vector2Int.zero;
        while (true) {
            Vector2Int testCoord = new Vector2Int (Random.Range (0, grid.GetLength (0)), Random.Range (0, grid.GetLength (1)));
            if ((testCoord - bot.coord).magnitude >= 2) {
                coord = testCoord;
                break;
            }
        }

        Instantiate (junkPrefab, grid[coord.x, coord.y], Quaternion.identity);
        targetCoord = coord;
    }
}