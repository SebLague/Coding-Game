using System.Collections.Generic;
using UnityEngine;

// Constructs a list of float values and expression elements from string
public class ValueString {

    public List<float> values;
    public List<ExpressionElement> elements;

    public ValueString (string code, Dictionary<string, float> variables) {
        values = new List<float> ();
        elements = new List<ExpressionElement> ();

        string[] sections = code.Split (' ');

        for (int i = 0; i < sections.Length; i++) {
            string section = sections[i];

            // Check for built-in functions (sqrt, abs) etc
            if (VirtualCompiler.ArrayContainsString (VirtualCompiler.builtinFunctions, section)) {
                switch (section) {
                    case "sqrt":
                        elements.Add (ExpressionElement.SqrtFunc);
                        break;
                    case "abs":
                        elements.Add (ExpressionElement.AbsFunc);
                        break;
                    case "sign":
                        elements.Add (ExpressionElement.SignFunc);
                        break;
                    case "sin":
                        elements.Add (ExpressionElement.SinFunc);
                        break;
                    case "cos":
                        elements.Add (ExpressionElement.CosFunc);
                        break;
                    case "tan":
                        elements.Add (ExpressionElement.TanFunc);
                        break;
                    case "asin":
                        elements.Add (ExpressionElement.AsinFunc);
                        break;
                    case "acos":
                        elements.Add (ExpressionElement.AcosFunc);
                        break;
                    case "atan":
                        elements.Add (ExpressionElement.AtanFunc);
                        break;
                }
            }
            // Check for math operators (+, -) etc
            else if (VirtualCompiler.ArrayContainsString (VirtualCompiler.mathOperators, section) || section == "(" || section == ")") {
                switch (section) {
                    case "+":
                        elements.Add (ExpressionElement.Plus);
                        break;
                    case "-":
                        elements.Add (ExpressionElement.Minus);
                        break;
                    case "*":
                        elements.Add (ExpressionElement.Multiply);
                        break;
                    case "/":
                        elements.Add (ExpressionElement.Divide);
                        break;
                    case "(":
                        elements.Add (ExpressionElement.StartGroup);
                        break;
                    case ")":
                        elements.Add (ExpressionElement.EndGroup);
                        break;
                }
            }
            // Try get value from variable name
            else if (variables.ContainsKey (section)) {
                values.Add (variables[section]);
                elements.Add (ExpressionElement.Value);
            }
            // Try get value from variable array
            else if (section.Contains ("[") && section.Contains ("]")) {
                string indexString = VirtualCompiler.Substring (section, section.IndexOf ("[") + 1, section.IndexOf ("]"));
                var indexValueString = new ValueString (CompilerFormatter.Format(indexString), variables);
                int index = (int) new NumericalExpression (indexValueString).Evaluate ();
                string varName = section.Substring (0, section.IndexOf ("[")) + VirtualCompiler.arrayIndexSeperator + index;
                if (variables.ContainsKey (varName)) {
                    elements.Add (ExpressionElement.Value);
                    values.Add (variables[varName]);
                }
            }
            // Try parse value string to float
            else {
                float value;
                if (float.TryParse (section.Replace (".", ","), out value)) {
                    values.Add (value);
                    elements.Add (ExpressionElement.Value);
                }
            }
        }
    }

    public void Log () {
        for (int i = 0; i < elements.Count; i++) {
            Debug.Log (elements[i]);
        }
        for (int i = 0; i < values.Count; i++) {
            Debug.Log (values[i]);
        }
    }

}