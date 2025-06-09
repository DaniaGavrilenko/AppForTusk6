using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;

namespace App1
{
    public sealed partial class MainPage : Page
    {
        private int[] tiles = new int[16];
        private Stack<int[]> history = new Stack<int[]>();
        private int bestScore = int.MaxValue;

        public MainPage()
        {
            this.InitializeComponent();

            // Завантаження рекорду
            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey("BestScore"))
            {
                bestScore = (int)settings.Values["BestScore"];
            }

            StartNewGame();
        }

        private void StartNewGame()
        {
            do
            {
                for (int i = 0; i < 15; i++) tiles[i] = i + 1;
                tiles[15] = 0;
                Shuffle();
            }
            while (!IsSolvable());

            history.Clear();
            SaveState();
            DrawTiles();
        }

        private void Shuffle()
        {
            Random rand = new Random();
            for (int i = 0; i < 100; i++)
            {
                int a = rand.Next(16);
                int b = rand.Next(16);
                int tmp = tiles[a];
                tiles[a] = tiles[b];
                tiles[b] = tmp;
            }
        }

        private bool IsSolvable()
        {
            int inversions = 0;
            for (int i = 0; i < 16; i++)
            {
                for (int j = i + 1; j < 16; j++)
                {
                    if (tiles[i] > 0 && tiles[j] > 0 && tiles[i] > tiles[j])
                        inversions++;
                }
            }

            int emptyRow = Array.IndexOf(tiles, 0) / 4;
            return (inversions + emptyRow) % 2 == 0;
        }

        private void DrawTiles()
        {
            GameGrid.Children.Clear();
            for (int i = 0; i < 16; i++)
            {
                int value = tiles[i];
                if (value == 0) continue;

                Button btn = new Button
                {
                    Content = value.ToString(),
                    FontSize = 24
                };

                btn.Click += Tile_Click;

                Grid.SetRow(btn, i / 4);
                Grid.SetColumn(btn, i % 4);

                GameGrid.Children.Add(btn);
            }

            StatusText.Text = "Ходів: " + (history.Count - 1);
            BestScoreText.Text = bestScore < int.MaxValue ? $"Рекорд: {bestScore}" : "Рекорд: -";
        }

        private void Tile_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int value = int.Parse(btn.Content.ToString());
            int index = Array.IndexOf(tiles, value);
            int empty = Array.IndexOf(tiles, 0);

            if (IsAdjacent(index, empty))
            {
                tiles[empty] = value;
                tiles[index] = 0;
                SaveState();
                DrawTiles();

                if (IsFinished())
                {
                    int moves = history.Count - 1;
                    StatusText.Text = $"Виграш 🎉 Ходів: {moves}";

                    if (moves < bestScore)
                    {
                        bestScore = moves;
                        ApplicationData.Current.LocalSettings.Values["BestScore"] = bestScore;
                    }
                }
            }
        }

        private bool IsAdjacent(int a, int b)
        {
            int rowA = a / 4, colA = a % 4;
            int rowB = b / 4, colB = b % 4;
            return (Math.Abs(rowA - rowB) + Math.Abs(colA - colB)) == 1;
        }

        private bool IsFinished()
        {
            for (int i = 0; i < 15; i++)
            {
                if (tiles[i] != i + 1) return false;
            }
            return tiles[15] == 0;
        }

        private void SaveState()
        {
            int[] snapshot = new int[16];
            Array.Copy(tiles, snapshot, 16);
            history.Push(snapshot);
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (history.Count > 1)
            {
                history.Pop();
                tiles = history.Peek();
                DrawTiles();
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }
    }
}
