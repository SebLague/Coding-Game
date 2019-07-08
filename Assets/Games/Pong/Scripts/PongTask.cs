using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongTask : Task {

    public float ballSpeed = 5;
    public Transform paddlePlayer;
    public Transform paddleOpponent;
    public Transform ball;
    public Transform arenaTop;
    public Transform arenaBottom;
    public float bounceAngle = 45;
    Vector2 ballDir;
    bool opponentHasPrediction;
    bool playerHasPrediction;

    float arenaHalfWidth;
    float arenaHalfHeight;
    float ballRadius;

    float opponentTargetY;
    float playerTargetY;
    float opponentInitialY;
    float playerInitialY;
    float opponentPercent;
    float playerPercent;
    float paddleSpeed;

    protected override void Start () {
        arenaHalfWidth = arenaTop.localScale.x / 2;
        arenaHalfHeight = arenaTop.position.y;
        ballRadius = ball.localScale.x / 2;

        arenaTop.position += Vector3.up * arenaTop.localScale.y / 2;
        arenaBottom.position += Vector3.down * arenaBottom.localScale.y / 2;
        paddlePlayer.position += Vector3.left * paddlePlayer.localScale.x / 2;
        paddleOpponent.position += Vector3.right * paddleOpponent.localScale.x / 2;

        taskInfo.constants[0].value = ballRadius;
        taskInfo.constants[1].value = arenaHalfWidth * 2;
        taskInfo.constants[2].value = arenaHalfHeight * 2;

        base.Start ();
    }

    protected override void Run (string code) {
        base.Run (code);
    }

    protected override void StartNextTestIteration () {
        base.StartNextTestIteration ();
        ballDir = Vector2.right;
        ballDir = new Vector2(-1,0).normalized;
        // Run test
    }

    void Update () {
        if (running) {
            HandleBall ();

            if (ballDir.x > 0 && !opponentHasPrediction) {
                playerHasPrediction = false;
                opponentHasPrediction = true;
                opponentTargetY = Predict () + (Random.value * 2 - 1) * paddleOpponent.localScale.y * .4f;
                float paddleMin = arenaBottom.position.y - arenaBottom.localScale.y / 2 + paddleOpponent.localScale.y / 2;
                float paddleMax = arenaTop.position.y + arenaTop.localScale.y / 2 - paddleOpponent.localScale.y / 2;
                opponentTargetY = Mathf.Clamp (opponentTargetY, paddleMin, paddleMax);
                opponentInitialY = paddleOpponent.position.y;
                opponentPercent = 0;
                paddleSpeed = ballSpeed / (Mathf.Abs (arenaHalfWidth - ball.position.x));
            }
            if (ballDir.x < 0) {
                opponentHasPrediction = false;
                if (!playerHasPrediction) {
                    playerHasPrediction = true;
                    //Predict();
                    float[] inputs = { ball.position.x, ball.position.y, ballDir.x, ballDir.y };
                    GenerateOutputs (inputs);
                    if (outputQueue.Count > 0) {
                        var func = outputQueue.Dequeue ();
                        float paddleMin = arenaBottom.position.y - arenaBottom.localScale.y / 2 + paddleOpponent.localScale.y / 2;
                        float paddleMax = arenaTop.position.y + arenaTop.localScale.y / 2 - paddleOpponent.localScale.y / 2;
                        playerTargetY = Mathf.Clamp (func.value, paddleMin, paddleMax);
                        playerInitialY = paddlePlayer.position.y;
                        paddleSpeed = ballSpeed / (Mathf.Abs (-arenaHalfWidth - ball.position.x));
                        playerPercent = 0;
                    }
                }
            }

            opponentPercent += Time.deltaTime * paddleSpeed;
            playerPercent += Time.deltaTime * paddleSpeed;

            paddlePlayer.position = new Vector3 (paddlePlayer.position.x, Mathf.Lerp (playerInitialY, playerTargetY, playerPercent));
            //paddlePlayer.position = new Vector3 (paddlePlayer.position.x, ball.position.y);
            paddleOpponent.position = new Vector3 (paddleOpponent.position.x, Mathf.Lerp (opponentInitialY, opponentTargetY, opponentPercent));

            float scoreBuffer = 1;
            if (ball.position.x < -(arenaHalfWidth + scoreBuffer)) {
                TestFailed ();
            }
        }
    }

    float Predict () {
        // Inputs
        float pX = ball.position.x;
        float pY = ball.position.y;
        float dirX = ballDir.x;
        float dirY = ballDir.y;

        // Code
        if (dirY == 0) {
            return pY;
        }

        for (int i = 0; i < 20; i++) {
            float deltaXPerY = dirX / Mathf.Abs (dirY);
            float deltaYPerX = dirY / Mathf.Abs (dirX);

            float dstVertical = arenaHalfHeight * Mathf.Sign (dirY) - pY;
            float dx = Mathf.Abs (dstVertical) * deltaXPerY;

            float pXAfterMove = pX + dx - Mathf.Abs (deltaXPerY * ballRadius) * Mathf.Sign (dirX);
            if (Mathf.Abs (pXAfterMove + ballRadius * Mathf.Sign (dirX)) >= arenaHalfWidth) {
                return pY + Mathf.Abs (arenaHalfWidth * Mathf.Sign (dirX) - pX - ballRadius * Mathf.Sign (dirX)) * deltaYPerX;
            }
            pX = pXAfterMove;
            pY = (arenaHalfHeight - ballRadius) * Mathf.Sign (dirY);
            dirY *= -1;
        }

        return -100;
    }

    protected override void Clear () {
        base.Clear ();
    }

    void HandleBall () {
        Vector2 ballPos = ball.position;
        Vector2 newBallPos = ballPos + this.ballDir * Time.deltaTime * ballSpeed;
        ball.position = newBallPos;

        Transform paddle = (this.ballDir.x < 0) ? paddlePlayer : paddleOpponent;
        Vector2 paddleTop = (Vector2) paddle.transform.position + Vector2.up * paddle.localScale.y / 2;
        Vector2 paddleBottom = (Vector2) paddle.transform.position - Vector2.up * paddle.localScale.y / 2;
        // Ball-paddle intersection
        if (this.ballDir.x > 0 && ballPos.x > 0 || this.ballDir.x < 0 && ballPos.x < 0) {
            float dstToEdgeX = arenaHalfWidth - Mathf.Abs (ballPos.x);
            float edgeY = ballPos.y + dstToEdgeX * ballDir.y / Mathf.Abs (ballDir.x);
            float backtrack = ballRadius / Mathf.Abs (ballDir.x);
            Vector2 intersection = new Vector2 (ballPos.x + dstToEdgeX * Mathf.Sign (ballDir.x), edgeY) - ballDir * backtrack;

            float dstToIntersection = (ballPos - intersection).magnitude;
            float moveDst = (ballPos - newBallPos).magnitude;
            if (moveDst >= dstToIntersection) {
                float collisionPointY = intersection.y + ballDir.y * ballRadius;
                if (collisionPointY >= paddleBottom.y && intersection.y <= paddleTop.y) {
                    ballPos = intersection;
                    float percent = Mathf.InverseLerp (paddleBottom.y, paddleTop.y, collisionPointY);
                    float angle = Mathf.Lerp (-bounceAngle, bounceAngle, percent);
                    angle = (ballDir.x < 0) ? angle : -angle + 180;
                    ballDir = new Vector2 (Mathf.Cos (angle * Mathf.Deg2Rad), Mathf.Sin (angle * Mathf.Deg2Rad));
                    //print (percent + "  " + angle + "  " + ballDir);
                    //this.ballDir.x *= -1;
                    float bounceDst = moveDst - dstToIntersection;
                    ballPos += ballDir * bounceDst;
                    ball.position = ballPos;
                }
            }
            //Debug.DrawLine (ballPos, new Vector2 (ballPos.x + dstToEdgeX * Mathf.Sign (dir.x), edgeY));
            // Debug.DrawLine (ballPos, intersection, Color.red);
        }
        // Floor/ceiling collision
        if (this.ballDir.y != 0) {
            if (this.ballDir.y > 0 && ballPos.y > 0 || this.ballDir.y < 0 && ballPos.y < 0) {
                float dstToEdgeY = arenaHalfHeight - Mathf.Abs (ballPos.y);
                float edgeX = ballPos.x + dstToEdgeY * ballDir.x / Mathf.Abs (ballDir.y);
                float backtrack = ballRadius / Mathf.Abs (ballDir.y);
                Vector2 intersection = new Vector2 (edgeX, ballPos.y + dstToEdgeY * Mathf.Sign (ballDir.y)) - ballDir * backtrack;
                float dstToIntersection = (ballPos - intersection).magnitude;
                float moveDst = (ballPos - newBallPos).magnitude;
                if (moveDst >= dstToIntersection) {
                    ballPos = intersection;
                    this.ballDir.y *= -1;
                    float bounceDst = moveDst - dstToIntersection;
                    ballPos += ballDir * bounceDst;
                    ball.position = ballPos;
                }
                //Debug.DrawLine (ballPos, new Vector2 (edgeX, ballPos.y + dstToEdge * Mathf.Sign (dir.y)));
                //Debug.DrawLine (ballPos, intersection, Color.red);
            }

        }
    }
}