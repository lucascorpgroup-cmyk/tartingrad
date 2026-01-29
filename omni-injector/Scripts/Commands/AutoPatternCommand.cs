using System;
using System.Threading.Tasks;
using UnityEngine;

[Command("autopattern")]
sealed class AutoPatternCommand : ICommand
{
    private static PatternRunner runner;

    public async Task Execute(Arguments args, System.Threading.CancellationToken cancellationToken)
    {
        if (Helper.LocalPlayer == null) return;

        string cmd = args.Length > 0 ? args[0].ToLowerInvariant() : "";
        string pattern = args.Length > 1 ? args[1].ToLowerInvariant() : "circle";
        float speed = args.Length > 2 && float.TryParse(args[2], out float sp) ? sp : 10f;

        switch (cmd)
        {
            case "on":
                if (runner == null)
                {
                    GameObject go = new GameObject("AutoPatternRunner");
                    runner = go.AddComponent<PatternRunner>();
                    runner.Init(pattern, speed);
                }
                Chat.Print($"AutoPattern activé ({pattern}) !");
                break;

            case "off":
                if (runner != null)
                {
                    runner.Stop();
                    runner = null;
                }
                Chat.Print("AutoPattern désactivé !");
                break;

            default:
                Chat.Print("Usage: /autopattern on/off [pattern=circle/square] [speed]");
                break;
        }

        await Task.Yield();
    }

    private sealed class PatternRunner : MonoBehaviour
    {
        private string pattern = "circle";
        private float speed = 10f;
        private Vector3 center;
        private float angle = 0f;
        private int squareStep = 0;
        private float squareSide = 5f;

        public void Init(string patternType, float moveSpeed)
        {
            pattern = patternType;
            speed = moveSpeed;
            center = Helper.LocalPlayer.transform.position;
        }

        public void Stop()
        {
            Destroy(this.gameObject);
        }

        private void Update()
        {
            if (Helper.LocalPlayer == null) return;

            switch (pattern)
            {
                case "circle":
                    MoveCircle();
                    break;
                case "square":
                    MoveSquare();
                    break;
            }
        }

        private void MoveCircle()
        {
            angle += speed * Time.deltaTime;
            float rad = angle * Mathf.Deg2Rad;
            float radius = 5f; // rayon du cercle
            Vector3 offset = new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;
            Helper.LocalPlayer.transform.position = center + offset + Vector3.up * 2f; // légèrement au dessus du sol
            Helper.LocalPlayer.transform.Rotate(Vector3.up * speed * Time.deltaTime); // rotation rapide
        }

        private void MoveSquare()
        {
            float stepSize = speed * Time.deltaTime;
            Vector3 target = center;

            switch (squareStep)
            {
                case 0: target = center + new Vector3(squareSide, 0, 0); break;
                case 1: target = center + new Vector3(squareSide, 0, squareSide); break;
                case 2: target = center + new Vector3(0, 0, squareSide); break;
                case 3: target = center; break;
            }

            Vector3 dir = (target - Helper.LocalPlayer.transform.position);
            if (dir.magnitude < 0.1f)
            {
                squareStep = (squareStep + 1) % 4;
            }
            else
            {
                Helper.LocalPlayer.transform.position += dir.normalized * stepSize;
                Helper.LocalPlayer.transform.Rotate(Vector3.up * speed * Time.deltaTime);
            }
        }
    }
}
