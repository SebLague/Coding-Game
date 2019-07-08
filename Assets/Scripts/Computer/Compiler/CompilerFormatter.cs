using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CompilerFormatter {

    const char spaceChar = ' ';

    /// Format string such that all values/variables/operators are separated by a single space
    /// e.g. "x+7 *(3+y) <= -10" should be formatted as "x + 7 * ( 3 + y ) <= - 10"
    /// Note that compound operators (<= and >= and ==) should be treated as a single symbol
    /// Note also that anything inside array index should not be formatted: x[3*2+7] for example should be left as is
    public static string Format (string line) {
        line = line.Trim ();

        var sections = new List<string> ();
        string substring = "";
        bool lastWasOperator = false;
        bool inArrayIndexer = false;

        for (int i = 0; i < line.Length; i++) {
            char currentChar = line[i];
            bool isOperator = VirtualCompiler.allOperators.Contains (currentChar.ToString ());
            if (currentChar == '[') {
                inArrayIndexer = true;
            } else if (currentChar == ']') {
                inArrayIndexer = false;
            }
            if ((currentChar == spaceChar || isOperator || lastWasOperator) && !inArrayIndexer) {
                if (!string.IsNullOrEmpty (substring)) {
                    sections.Add (substring);
                }
                substring = "";
            }
            if (isOperator || currentChar != spaceChar) {
                substring += currentChar;
            }

            if (i == line.Length - 1) {
                if (!string.IsNullOrEmpty (substring)) {
                    sections.Add (substring);
                }
            }

            lastWasOperator = isOperator;
        }

        string formattedLine = "";
        string lastAddedSection = "";
        for (int i = 0; i < sections.Count; i++) {

            // Remove previous space if compound operator
            string compound = lastAddedSection + sections[i];
            if (VirtualCompiler.ArrayContainsString (VirtualCompiler.compoundOperators, compound)) {
                formattedLine = formattedLine.Substring (0, formattedLine.Length - 1);
            }

            formattedLine += sections[i];

            if (i != sections.Count - 1) {
                formattedLine += spaceChar;
            }

            lastAddedSection = sections[i];
        }

        return formattedLine;
    }
}