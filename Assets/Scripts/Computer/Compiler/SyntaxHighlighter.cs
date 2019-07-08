using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SyntaxHighlighter {

    public static string HighlightCode (string code, SyntaxTheme theme) {
        string highlightedCode = "";

        string[] lines = code.Split ('\n');
        for (int i = 0; i < lines.Length; i++) {;
            highlightedCode += HighlightLine (lines[i], theme);
            if (i != lines.Length - 1) {
                highlightedCode += '\n';
            }
        }

        return highlightedCode;
    }

    static string HighlightLine (string line, SyntaxTheme theme) {
        var cols = new List<Color> ();
        var startEndIndices = new List<Vector2Int> ();

        string[] sections = CompilerFormatter.Format (line).Split (' ');
        int nonSpaceIndex = 0;
        bool isComment = false;

        for (int i = 0; i < sections.Length; i++) {
            Color colour = Color.clear;
            string section = sections[i];

            if (section == VirtualCompiler.commentMarker || isComment) {
                colour = theme.comment;
                isComment = true;
            } else if (section == "if" || section == "elseif" || section == "else") {
                colour = theme.conditionKeyword;
            } else if (section == "loop") {
                colour = theme.loopKeyword;
            } else if (VirtualCompiler.ArrayContainsString (VirtualCompiler.mathOperators, section)) {
                colour = theme.mathOperator;
            } else if (VirtualCompiler.ArrayContainsString (VirtualCompiler.comparisonOperators, section)) {
                colour = theme.comparisonOperator;
            } else if (section == "(" || section == ")") {
                colour = theme.parentheses;
            } else if (section == "{" || section == "}") {
                colour = theme.curlyBrackets;
            } else if (float.TryParse (section.Replace ('.', ','), out _)) {
                colour = theme.value;
            } else if (section == "=") {
                colour = theme.assignment;
            } else {
                colour = theme.variable;
            }

            if (colour != Color.clear) {
                cols.Add (colour);
                int endIndex = nonSpaceIndex + sections[i].Length;
                startEndIndices.Add (new Vector2Int (nonSpaceIndex, endIndex));
            }
            nonSpaceIndex += sections[i].Length;
        }

        if (cols.Count > 0) {
            nonSpaceIndex = 0;
            int colIndex = 0;
            int actualStartIndex = -1;
            for (int i = 0; i <= line.Length; i++) {

                if (startEndIndices[colIndex].x == nonSpaceIndex) {
                    actualStartIndex = i;
                } else if (startEndIndices[colIndex].y == nonSpaceIndex) {
                    //print (colIndex + " replace: " + startEndIndices[colIndex] +" with:  " + new Vector2Int (actualStartIndex, i));
                    startEndIndices[colIndex] = new Vector2Int (actualStartIndex, i);

                    colIndex++;
                    if (colIndex >= cols.Count) {
                        break;
                    }
                    i--;
                    continue;
                }

                if (i < line.Length) {
                    char c = line[i];
                    if (c != ' ') {
                        nonSpaceIndex++;
                    }
                }

            }
        }

        for (int i = cols.Count - 1; i >= 0; i--) {
            var col = cols[i];
            var startEndIndex = startEndIndices[i];
            string colString = ColorUtility.ToHtmlStringRGB (col);

            line = line.Insert (startEndIndex.y, "</color>");
            line = line.Insert (startEndIndex.x, $"<color=#{colString}>");
            //print ("insert: " + startEndIndex.x + " " + startEndIndex.y);
        }

        return line;
    }

    public static string CreateColouredText (Color colour, string text) {
        string colString = ColorUtility.ToHtmlStringRGB (colour);
        return $"<color=#{colString}>{text}</color>";
    }
}