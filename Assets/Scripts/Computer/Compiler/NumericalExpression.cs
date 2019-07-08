using System.Collections.Generic;
using UnityEngine;

class NumericalExpression {

    const string elementChars = "+-*/()" + VirtualCompiler.valueMarker;

    List<float> values;
    List<ExpressionElement> elements;

    public NumericalExpression (ValueString valueString) {
        values = valueString.values;
        elements = valueString.elements;
    }

    public NumericalExpression (string expression, Dictionary<string, float> variables) {
        var valueString = new ValueString (expression, variables);
        values = valueString.values;
        elements = valueString.elements;
    }

    public float Evaluate () {
        ResolveBracketGroups ();
        return EvaluateGroup (elements, values);
    }

    // Evaluates value of group, e.g. (2+4*4) = 18
    // A group is an expression without any sub-groups (expressions in paretheses)
    float EvaluateGroup (List<ExpressionElement> groupElements, List<float> groupValues) {
        int valueIndex = 0;

        // resolve multiplication and division
        for (int i = 0; i < groupElements.Count; i++) {
            ExpressionElement element = groupElements[i];
            if (element == ExpressionElement.Value) {
                valueIndex++;
            } else if (element == ExpressionElement.Multiply || element == ExpressionElement.Divide) {
                float valueA = groupValues[valueIndex - 1];
                float valueB = groupValues[valueIndex];
                float result_dm = (element == ExpressionElement.Multiply) ? valueA * valueB : valueA / valueB;
                groupValues[valueIndex - 1] = result_dm;
                groupValues.RemoveAt (valueIndex);
                groupElements.RemoveRange (i - 1, 2);
                i--;
            }
        }

        // Sum up remaining terms
        valueIndex = 0;
        float result = 0;
        for (int i = 0; i < groupElements.Count; i++) {
            ExpressionElement element = groupElements[i];
            if (element == ExpressionElement.Value) {
                int sign = 1;
                // Backtrack through operators up to last value to figure out if should be addition or subtraction
                // This deals with multiple operators like x-(-y) = x+y
                for (int j = i - 1; j >= 0; j--) {
                    if (groupElements[j] == ExpressionElement.Value) {
                        break;
                    }
                    if (groupElements[j] == ExpressionElement.Minus) {
                        sign *= -1;
                    }
                }
                result += groupValues[valueIndex] * sign;
                valueIndex++;
            }
        }
        return result;
    }

    // Recursively replaces expressions inside brackets with the actual value
    // So 2*(3-4*(5-2)) becomes 2*(3-4*3) becomes 2*-9
    void ResolveBracketGroups () {
        if (elements.Contains (ExpressionElement.StartGroup)) {
            int groupStartIndex = -1;
            int groupEndIndex = -1;
            int deepestGroupIndex = -1;

            int currentDepth = 0;

            // Find start/end index of most deeply nested bracket pair
            for (int i = 0; i < elements.Count; i++) {
                if (elements[i] == ExpressionElement.StartGroup) {
                    currentDepth++;
                    if (currentDepth >= deepestGroupIndex) {
                        deepestGroupIndex = currentDepth;
                        groupStartIndex = i;
                    }
                } else if (elements[i] == ExpressionElement.EndGroup) {
                    if (currentDepth == deepestGroupIndex) {
                        groupEndIndex = i;
                    }
                    currentDepth--;
                }
            }

            if (currentDepth != 0) {
                Debug.Log ("Unmatched bracket");
                return;
            }

            // Find index of first value in the current group
            int valueStartIndex = 0;
            for (int i = 0; i < groupStartIndex; i++) {
                if (elements[i] == ExpressionElement.Value) {
                    valueStartIndex++;
                }
            }

            // Get list of elements and values inside the current group
            var groupElements = new List<ExpressionElement> ();
            var groupValues = new List<float> ();

            int valueIndex = valueStartIndex;
            for (int i = groupStartIndex + 1; i < groupEndIndex; i++) {
                groupElements.Add (elements[i]);
                if (elements[i] == ExpressionElement.Value) {
                    groupValues.Add (values[valueIndex]);
                    valueIndex++;
                }
            }

            // Get value of the group
            float value = EvaluateGroup (groupElements, groupValues);
            bool hasFunc = false;

            // Check for modifying function, e.g. abs() or sqrt()
            if (groupStartIndex > 0) {
                switch (elements[groupStartIndex - 1]) {
                    case ExpressionElement.AbsFunc:
                        value = Mathf.Abs (value);
                        hasFunc = true;
                        break;
                    case ExpressionElement.SqrtFunc:
                        value = Mathf.Sqrt (value);
                        hasFunc = true;
                        break;
                    case ExpressionElement.SignFunc:
                        if (value < 0) {
                            value = -1;
                        } else if (value > 0) {
                            value = 1;
                        }
                        hasFunc = true;
                        break;
                    case ExpressionElement.SinFunc:
                        value = Mathf.Sin (value);
                        hasFunc = true;
                        break;
                    case ExpressionElement.CosFunc:
                        value = Mathf.Cos (value);
                        hasFunc = true;
                        break;
                    case ExpressionElement.TanFunc:
                        value = Mathf.Tan (value);
                        hasFunc = true;
                        break;
                    case ExpressionElement.AsinFunc:
                        value = Mathf.Asin (value);
                        hasFunc = true;
                        break;
                    case ExpressionElement.AcosFunc:
                        value = Mathf.Acos (value);
                        hasFunc = true;
                        break;
                    case ExpressionElement.AtanFunc:
                        value = Mathf.Atan (value);
                        hasFunc = true;
                        break;
                }
            }
            if (hasFunc) {
                groupStartIndex--;
            }
            // Replace group with the value
            elements.RemoveRange (groupStartIndex, groupEndIndex - groupStartIndex + 1);
            elements.Insert (groupStartIndex, ExpressionElement.Value);
            values.RemoveRange (valueStartIndex, (valueIndex - 1) - valueStartIndex);

            values[valueStartIndex] = value;

            ResolveBracketGroups ();
        }
    }

    void Log (List<ExpressionElement> e, List<float> v) {
        string s = "";
        int valueIndex = 0;
        for (int i = 0; i < e.Count; i++) {
            s += e[i].ToString () + " ";
            if (e[i] == ExpressionElement.Value) {
                s += $"({v[valueIndex]}) ";
                valueIndex++;
            }
        }
        Debug.Log (s);
    }

    void LogMath (List<ExpressionElement> e, List<float> v) {
        string s = "";
        int valueIndex = 0;
        for (int i = 0; i < e.Count; i++) {

            if (e[i] == ExpressionElement.Value) {
                s += v[valueIndex] + " ";
                valueIndex++;
            } else {
                s += elementChars[(int) e[i]] + " ";
            }
        }
        Debug.Log (s);
    }
}