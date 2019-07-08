using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteInEditMode]
public class TaskSelection : VirtualProgram {

    public TMP_Text levelsUI;
    public Image caret;
    public float blinkRate = 1;
    public Color highlightCol;
    public int levelIndex = 0;

    public string[] levelNames;
    int activeLevelBuildIndex = -1;

    float blinkTimer;
    float lastInputTime;

    string levelsText;
    string[] levelLines;

    void Start () {
        if (Application.isPlaying) {
            CustomInput.instance.RegisterKey (KeyCode.UpArrow);
            CustomInput.instance.RegisterKey (KeyCode.DownArrow);

            UpdateText ();

            levelLines = levelsText.Split ('\n');
        }
    }

    void UpdateText () {
        levelsText = "";
        for (int i = 0; i < levelNames.Length; i++) {
            levelsText += i.ToString ("D3") + ": " + levelNames[i];
            if (i != levelNames.Length - 1) {
                levelsText += "\n";
            }
        }
        levelsUI.text = levelsText;
    }
    void Update () {
        if (!Application.isPlaying) {
            UpdateText ();
            return;
        }

        if (!active) {
            return;
        }

        HandleInput ();
        SetCaret ();
        levelsUI.text = GetHighlightedLevelString ();
    }

    void HandleInput () {
        if (Input.GetKeyDown (KeyCode.Return)) {
            if (activeLevelBuildIndex != -1) {
                SceneManager.UnloadSceneAsync (activeLevelBuildIndex);
                //SceneManager.UnloadScene(activeLevelBuildIndex);
            }

            if (levelIndex == 0) {
                FindObjectOfType<VirtualOS> ().SetProgram (VirtualOS.Program.Instructions);
            } else {
                activeLevelBuildIndex = levelIndex;
                SceneManager.LoadScene (activeLevelBuildIndex, LoadSceneMode.Additive);
            }
        }

        if (CustomInput.instance.GetKeyPress (KeyCode.UpArrow)) {
            levelIndex = Mathf.Max (0, levelIndex - 1);
            lastInputTime = Time.time;
        }
        if (CustomInput.instance.GetKeyPress (KeyCode.DownArrow)) {
            lastInputTime = Time.time;
            levelIndex = Mathf.Min (levelLines.Length - 1, levelIndex + 1);
        }
    }

    string GetHighlightedLevelString () {
        string highlightedText = "";
        for (int i = 0; i < levelLines.Length; i++) {
            if (i == levelIndex) {
                string colString = ColorUtility.ToHtmlStringRGB (highlightCol);
                highlightedText += $"<color=#{colString}>";
            }
            highlightedText += levelLines[i];

            if (i == levelIndex) {
                highlightedText += "</color>";
            }
            if (i != levelLines.Length - 1) {
                highlightedText += "\n";
            }
        }
        return highlightedText;
    }

    void SetCaret () {
        // Blink
        blinkTimer += Time.deltaTime;
        if (Time.time - lastInputTime < blinkRate / 2) {
            caret.enabled = true;
            blinkTimer = 0;
        } else {
            caret.enabled = (blinkTimer % blinkRate < blinkRate / 2);
        }

        // Place
        string levelsUpToCaret = "";
        for (int i = 0; i <= levelIndex; i++) {
            levelsUpToCaret += levelLines[i];
            if (i != levelIndex) {
                levelsUpToCaret += "\n";
            }
        }

        levelsUI.text = ".";
        float singleLineHeight = levelsUI.preferredHeight;
        levelsUI.text = levelsUpToCaret;
        float height = levelsUI.preferredHeight - singleLineHeight;

        levelsUI.text = "0";
        float width = levelsUI.preferredWidth;

        caret.rectTransform.position = levelsUI.rectTransform.position;
        caret.rectTransform.localPosition += Vector3.left * (width + caret.rectTransform.rect.width / 2f);
        caret.rectTransform.localPosition += Vector3.down * (caret.rectTransform.rect.height / 2 + height);
    }

}