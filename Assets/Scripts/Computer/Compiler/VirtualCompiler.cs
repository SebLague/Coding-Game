using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCompiler {

    public const char outputFunctionArgumentSeperator = ':';
    public const char outputFunctionSeperator = '|';

    public const string commentMarker = "//";

    public static readonly string[] builtinFunctions = {
        "sqrt",
        "abs",
        "sign",
        "sin",
        "cos",
        "tan",
        "asin",
        "acos",
        "atan"
    };

    public static readonly string[] reservedNames = {
        "if",
        "loop",
        "and",
        "or"
    };

    public static readonly string[] comparisonOperators = {
        "and",
        "or",
        "<",
        "<=",
        "==",
        ">=",
        ">"
    };

    public static readonly string[] mathOperators = {
        "+",
        "-",
        "*",
        "/",
    };

    public static readonly string[] compoundOperators = {
        "+=",
        "-=",
        "*=",
        "/=",
        "++",
        "--",
        "<=",
        ">=",
        "==",
        "elseif",
        commentMarker
    };

    const string mathOperatorsString = "+-*/()";
    const string comparisonOperatorsString = "=<>";
    const string assignmentOperator = "=";
    public const string arrayIndexSeperator = "$"; // myVar[i] is stored as myVar$i

    public const string allOperators = mathOperatorsString + comparisonOperatorsString + assignmentOperator;

    public const string valueMarker = "#";

    string code;
    string[] lines;
    string processedCode;

    Dictionary<string, float> variables;
    Dictionary<int, ConditionBlockInfo> conditionByLineIndex;

    List<string> outputFunctionNames;
    List<VirtualFunction> outputs;

    public VirtualCompiler (string code) {
        if (code == null) {
            code = "";
        }
        variables = new Dictionary<string, float> ();
        conditionByLineIndex = new Dictionary<int, ConditionBlockInfo> ();
        outputFunctionNames = new List<string> ();
        outputs = new List<VirtualFunction> ();

        PreprocessCode (code);
    }

    // Set input arguments to the code
    public void AddInput (string varName, float varValue) {
        variables.Add (varName.ToLower (), varValue);
    }

    // Set input arguments to the code
    public void AddInputArray (string varName, float[] varValues) {
        for (int i = 0; i < varValues.Length; i++) {
            variables.Add (varName.ToLower () + arrayIndexSeperator + i, varValues[i]);
        }
    }

    // Set the names of external functions.
    // These are how the user outputs commands from their code.
    // When the user calls a function e.g. MoveLeft(),
    // 'MoveLeft' will be added to the code's output string.
    public void AddOutputFunction (string name) {
        outputFunctionNames.Add (name);
    }

    // Run the code and return the output string, which contains the names of all external functions called by the user.
    public List<VirtualFunction> Run () {

        RunLines (0, lines.Length);
        return outputs;
    }

    void PreprocessCode (string rawCode) {
        code = rawCode.ToLower ();

        // Enforce curly braces to be on their own line:
        code = code.Replace ("{", "\n{\n");
        code = code.Replace ("}", "\n}\n");

        // Format lines
        var unformattedLines = code.Split ('\n');
        var formattedLines = new List<string> ();
        foreach (string unformattedLine in unformattedLines) {
            if (!string.IsNullOrWhiteSpace (unformattedLine)) {
                string formattedLine = CompilerFormatter.Format (unformattedLine);
                string[] sections = formattedLine.Split (' ');

                // Strip comments
                foreach (string s in sections) {
                    if (s == commentMarker) {
                        formattedLine = formattedLine.Substring (0, formattedLine.IndexOf (commentMarker));
                    }
                }
                if (!string.IsNullOrWhiteSpace (formattedLine)) {
                    // Syntactic sugar
                    if (sections.Length > 1) {
                        sections = formattedLine.Split (' ');
                        if (sections[1] == "+=") {
                            formattedLine = string.Format ("{0} = {0} + ( {1} )", sections[0], ArrayToString (sections, 2));
                        } else if (sections[1] == "-=") {
                            formattedLine = string.Format ("{0} = {0} - ( {1} )", sections[0], ArrayToString (sections, 2));
                        } else if (sections[1] == "*=") {
                            formattedLine = string.Format ("{0} = {0} * ( {1} )", sections[0], ArrayToString (sections, 2));
                        } else if (sections[1] == "/=") {
                            formattedLine = string.Format ("{0} = {0} / ( {1} )", sections[0], ArrayToString (sections, 2));
                        } else if (sections[1] == "++") {
                            formattedLine = string.Format ("{0} = {0} + 1", sections[0]);
                        } else if (sections[1] == "--") {
                            formattedLine = string.Format ("{0} = {0} - 1", sections[0]);
                        }
                    }
                    formattedLines.Add (formattedLine);
                    processedCode += formattedLine + '\n';
                }
            }
        }

        lines = formattedLines.ToArray ();

        // Create condition block info
        var conditionBlocks = new List<ConditionBlockInfo> ();
        int blockDepth = 0;
        for (int i = 0; i < lines.Length; i++) {
            string section = lines[i].Split (' ') [0];
            var condition = new ConditionBlockInfo () { lineIndex = i, depth = blockDepth };

            switch (section) {
                case "if":
                    condition.type = ConditionBlockInfo.Type.If;
                    conditionBlocks.Add (condition);
                    break;
                case "elseif":
                    condition.type = ConditionBlockInfo.Type.ElseIf;
                    conditionBlocks.Add (condition);
                    break;
                case "else":
                    condition.type = ConditionBlockInfo.Type.Else;
                    conditionBlocks.Add (condition);
                    break;
                case "{":
                    blockDepth++;
                    break;
                case "}":
                    blockDepth--;
                    break;
            }
        }

        // Create links between chained conditions (if-elseif-else)
        for (int i = 0; i < conditionBlocks.Count; i++) {
            var condition = conditionBlocks[i];
            conditionByLineIndex.Add (condition.lineIndex, condition);
            // Try find an elseif/else block attached to this block
            if (condition.type == ConditionBlockInfo.Type.If || condition.type == ConditionBlockInfo.Type.ElseIf) {
                for (int j = i + 1; j < conditionBlocks.Count; j++) {
                    var nextCondition = conditionBlocks[j];

                    // At same depth, so might be continuation of chain
                    if (nextCondition.depth == condition.depth) {
                        // If-statement is start of a new chain, so must be end of current chain
                        if (nextCondition.type == ConditionBlockInfo.Type.If) {
                            break;
                        }
                        condition.nextInChain = nextCondition;
                        nextCondition.previousInChain = condition;
                        break;
                    }
                    // Out of scope, so is end of chain
                    else if (nextCondition.depth < condition.depth) {
                        break;
                    }

                }
            }
        }

    }

    // Runs lines of code from a start to end index
    void RunLines (int lineIndex, int stopIndex) {

        if (lineIndex >= lines.Length || lineIndex == stopIndex) {
            return;
        }

        string line = lines[lineIndex];
        string[] sections = line.Split (' ');

        //Debug.Log ($"Line: {lineIndex}: " + line + " stop: " + stopIndex);

        // Test if line is an external function call
        if (outputFunctionNames != null) {
            for (int i = 0; i < outputFunctionNames.Count; i++) {
                if (sections[0] == outputFunctionNames[i].ToLower ()) {
                    string argumentString = Substring (line, line.IndexOf ('(') + 1, line.LastIndexOf (')'));
                    string[] argumentSections = argumentString.Split (',');
                    var outputFunction = new VirtualFunction ();
                    outputFunction.name = outputFunctionNames[i];

                    for (int j = 0; j < argumentSections.Length; j++) {
                        var valueString = new ValueString (argumentSections[j], variables);
                        float value = new NumericalExpression (valueString).Evaluate ();
                        outputFunction.values.Add (value);
                    }

                    outputs.Add (outputFunction);

                    // Run the next line
                    RunLines (lineIndex + 1, stopIndex);
                    return;
                }
            }
        }

        // Test if line is a conditional statement
        if (conditionByLineIndex.ContainsKey (lineIndex)) {
            var condition = conditionByLineIndex[lineIndex];
            bool runCondition = GetConditionResult (line);
            // Else statement has no condition to test, so set run to true by default
            if (condition.type == ConditionBlockInfo.Type.Else) {
                runCondition = true;
            }

            // elseif/else conditions only run if all previous conditions in the chain were false
            if (condition.type != ConditionBlockInfo.Type.If) {
                var previousInChain = condition.previousInChain;
                while (previousInChain != null) {
                    if (previousInChain.lastEvaluation == true) {
                        runCondition = false;
                        break;
                    }
                    previousInChain = previousInChain.previousInChain;
                }
            }

            condition.lastEvaluation = runCondition;
            HandleCondition (runCondition, lineIndex, stopIndex);
            return;
        }

        if (sections[0] == "loop") {
            string argumentString = Substring (line, line.IndexOf ('(') + 1, line.LastIndexOf (')'));
            var e = new NumericalExpression (new ValueString (argumentString, variables));
            int numRepeats = (int) e.Evaluate ();

            // Pass off control to the handle condition function. It will resume the running of lines.
            HandleLoop (numRepeats, lineIndex, stopIndex);
            return;
        }

        // Assignment
        if (sections.Length > 1) {
            if (sections[1] == "=") {
                ProcessAssignment (line);
            }
        }

        if (sections[0] == "print") {
            ProcessPrint (line);
        }

        // Run next line
        RunLines (lineIndex + 1, stopIndex);
    }

    void HandleLoop (int numRepeats, int lineIndex, int codeStopIndex) {
        int loopEndIndex = BlockEndLineIndex (lineIndex);

        // Run loop block n times
        for (int i = 0; i < numRepeats; i++) {
            RunLines (lineIndex + 1, loopEndIndex);
        }

        // Resume running at end of loop block
        //Debug.Log ("Resume loop at " + (loopEndIndex + 1));
        RunLines (loopEndIndex + 1, codeStopIndex);

    }

    void HandleCondition (bool condition, int lineIndex, int codeStopIndex) {

        if (condition) {
            // Condition is true, run the block
            RunLines (lineIndex + 1, codeStopIndex);
        } else {
            // Condition is false, skip over the block
            int conditionEndIndex = BlockEndLineIndex (lineIndex);
            RunLines (conditionEndIndex + 1, codeStopIndex);
        }

    }

    int BlockEndLineIndex (int blockStartIndex) {
        bool hasFoundStartBrace = false;
        int indentLevel = 0;
        int blockEndIndex = blockStartIndex;

        // Find line index of the end of this condition
        for (int i = blockStartIndex; i < lines.Length; i++) {
            string line = lines[i];
            if (line.Contains ("{")) {
                hasFoundStartBrace = true;
                indentLevel++;
            }
            if (line.Contains ("}")) {
                indentLevel--;
            }

            if (indentLevel == 0 && hasFoundStartBrace) {
                blockEndIndex = i;
                break;
            }
        }
        return blockEndIndex;
    }

    bool GetConditionResult (string line) {

        int startIndex = line.IndexOf ('(') + 1;
        int endIndex = line.LastIndexOf (')');
        string conditionString = Substring (line, startIndex, endIndex);

        string[] sections = conditionString.Split (' ');

        var numericalValues = new List<float> ();
        var operators = new List<string> ();
        string currentString = "";

        // First simplify the condition by finding and evaluating any/all numerical expressions within it.
        // Store as list of values and operators.
        // For example, "3+2 < 7-1 or 2 > 3-2" will be simplified to [5,6,2,1], ["<", "or", ">"]

        // TODO: Resolve brackets that don't belong to the numerical expression
        // For example, in "if ((3+2>5 or 2<5+2) and 3 < 10)", the inner brackets belong to the 'or'
        for (int i = 0; i < sections.Length; i++) {
            string section = sections[i];
            bool isConditionOperator = ArrayContainsString (comparisonOperators, section);

            if (isConditionOperator || i == sections.Length - 1) {
                if (isConditionOperator) {
                    operators.Add (section);
                }

                if (!string.IsNullOrEmpty (currentString)) {
                    var expression = new NumericalExpression (new ValueString (currentString, variables));
                    numericalValues.Add (expression.Evaluate ());
                    currentString = "";
                }
            } else {
                currentString += section + " ";
            }
        }

        // Evaluate comparisons to bool values
        // e.g. (3 < 2 or 8 > 6) should evalulate to (false or true)
        var boolValues = new List<bool> ();
        var boolOperators = new List<BooleanExpression.Element> ();

        for (int i = 0; i < operators.Count; i++) {
            float a = numericalValues[i];
            float b = numericalValues[i + 1];
            string op = operators[i];
            switch (op) {
                case "<":
                    boolValues.Add (a < b);
                    boolOperators.Add (BooleanExpression.Element.Value);
                    break;
                case "<=":
                    boolValues.Add (a <= b);
                    boolOperators.Add (BooleanExpression.Element.Value);
                    break;
                case "==":
                    boolValues.Add (a == b);
                    boolOperators.Add (BooleanExpression.Element.Value);
                    break;
                case ">=":
                    boolValues.Add (a >= b);
                    boolOperators.Add (BooleanExpression.Element.Value);
                    break;
                case ">":
                    boolValues.Add (a > b);
                    boolOperators.Add (BooleanExpression.Element.Value);
                    break;
                case "and":
                    boolOperators.Add (BooleanExpression.Element.And);
                    break;
                case "or":
                    boolOperators.Add (BooleanExpression.Element.Or);
                    break;
            }
        }

        var booleanExpression = new BooleanExpression (boolValues, boolOperators);
        bool result = booleanExpression.Evaluate ();
        return result;

    }

    void ProcessPrint (string line) {
        int indexOfFirstBracket = line.IndexOf ('(');
        int indexOfLastBracket = line.LastIndexOf (')');
        line = Substring (line, indexOfFirstBracket + 1, indexOfLastBracket);
        //line = RemoveSpaces (line);
        string[] sections = line.Split (' ');
        string p = "";
        foreach (string s in sections) {
            if (variables.ContainsKey (s)) {
                float value = variables[s];
                p += value + " ";
            } else {
                p += s + " ";
            }
        }
        Debug.Log (p);
    }

    void ProcessAssignment (string line) {
        string varName = line.Split ('=') [0].Trim ();
        string expression = line.Split ('=') [1].Trim ();
        float value = new NumericalExpression (expression, variables).Evaluate ();

        if (!variables.ContainsKey (varName)) {
            variables.Add (varName, 0);
        }

        variables[varName] = value;
    }

    static string ArrayToString (string[] array, int startIndex) {
        string s = "";
        for (int i = startIndex; i < array.Length; i++) {
            s += array[i];
            if (i != array.Length - 1) {
                s += " ";
            }
        }
        return s;
    }

    public static bool ArrayContainsString (string[] array, string s) {
        for (int i = 0; i < array.Length; i++) {
            if (array[i] == s) {
                return true;
            }
        }
        return false;
    }

    static string Format (string s) {
        string formattedString = s.Trim ();
        return formattedString;
    }

    static string RemoveSpaces (string s) {
        return s.Replace (" ", "");
    }

    public static string Substring (string s, int startIndex, int endIndex) {
        int len = endIndex - startIndex;
        if (len <= 0) {
            return "";
        }
        return s.Substring (startIndex, len);
    }

    public class ConditionBlockInfo {
        public enum Type { If, ElseIf, Else }
        public Type type;
        public bool lastEvaluation;
        public int lineIndex;
        public ConditionBlockInfo previousInChain;
        public ConditionBlockInfo nextInChain;
        public int depth;

    }

}

public class VirtualFunction {
    public string name = "";
    public List<float> values = new List<float> ();
}