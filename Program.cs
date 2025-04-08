using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace KyraSimulation
{
    public static class Constants
    {
        public const int GridSize = 30;
        public const char EmptyCell = '.';
        public const char EnergyCell = '*';
        public const char DeadProtoCell = '#';
        public const int StarterKyraCount = 5;
    }

    public static class Program
    {
        static Cell[,] grid = new Cell[Constants.GridSize, Constants.GridSize];
        static List<Kyra> protos = new();

        private static void Main()
        {
            Console.Clear();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var rand = new Random();

            StartLogWindow();
            InitializeGrid(rand);

            for(int i = 0; i < Constants.StarterKyraCount; i++)
                SpawnStarterKyra(rand);

            bool allDeadOnce = false;

            while(true)
            {
                Console.SetCursorPosition(0, 0);
                DisplayGrid();
                UpdateGrid(rand);

                if(AllDead())
                {
                    if(allDeadOnce) break;
                    allDeadOnce = true;
                }
                else allDeadOnce = false;

                Thread.Sleep(300);
            }

            Console.WriteLine("\n[PROTO TREE]");
            PrintTree("ROOT", 0);
        }

        static void StartLogWindow()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "log.txt");
            File.WriteAllText(path, "=== KYRA LOG ===\n");

            var logWatcher = new Process();

            if(OperatingSystem.IsWindows())
            {
                logWatcher.StartInfo.FileName = "cmd.exe";
                logWatcher.StartInfo.Arguments = "/K powershell -Command \"Get-Content log.txt -Wait\"";
            }
            else if(OperatingSystem.IsLinux() && Environment.GetEnvironmentVariable("WSL_DISTRO_NAME") == null)
            {
                logWatcher.StartInfo.FileName = "x-terminal-emulator";
                logWatcher.StartInfo.Arguments = "-e tail -f log.txt";
            }
            else return;

            logWatcher.StartInfo.UseShellExecute = true;
            try
            {
                logWatcher.Start();
            }
            catch
            {
                /* shrug emoji */
            }
        }

        public static void LogEvent(string message)
        {
            string log = $"[{DateTime.Now:HH:mm:ss}] {message}";
            File.AppendAllText("log.txt", log + Environment.NewLine);
        }

        static void InitializeGrid(Random rand)
        {
            for(int y = 0; y < Constants.GridSize; y++)
            for(int x = 0; x < Constants.GridSize; x++)
                grid[y, x] = new Cell {Type = Constants.EnergyCell};
        }

        static void SpawnStarterKyra(Random rand)
        {
            int x = rand.Next(Constants.GridSize);
            int y = rand.Next(Constants.GridSize);
            Kyra starter = new Kyra
            {
                Id = $"K{protos.Count}",
                ParentId = "ROOT",
                X = x,
                Y = y,
            };
            grid[y, x].Type = 'K';
            grid[y, x].Id = starter.Id;
            protos.Add(starter);
            LogEvent($"{starter.Id} was initialized at ({x},{y}).");
        }

        static void UpdateGrid(Random rand)
        {
            int count = protos.Count;
            for(int i = 0; i < count; i++)
                protos[i].Act(grid, protos, rand);
        }

        static void DisplayGrid()
        {
            List<string> protoInfoLines = new();
            foreach(var proto in protos)
            {
                var status = proto.Alive? "🟢" : "⚫";
                protoInfoLines.Add($"{status} {proto.Id,-4} E:{proto.Energy,-3}");
            }

            for(int y = 0; y < Constants.GridSize; y++)
            {
                for(int x = 0; x < Constants.GridSize; x++)
                {
                    var cell = grid[y, x];

                    if(cell.Type == Constants.DeadProtoCell && cell.energy > 0)
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    else if(cell.Type == Constants.DeadProtoCell)
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    else if(!string.IsNullOrEmpty(cell.Id))
                        Console.ForegroundColor = GetColorForId(cell.Id);
                    else if(cell.Type == Constants.EnergyCell)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else
                        Console.ForegroundColor = ConsoleColor.DarkBlue;

                    Console.Write($"{cell.Type} ");
                    Console.ResetColor();
                }

                if(y < protoInfoLines.Count)
                    Console.Write("   " + protoInfoLines[y]);

                Console.WriteLine();
            }

            Console.WriteLine($"\nGRID: {Constants.GridSize}x{Constants.GridSize}\n");
        }

        static ConsoleColor GetColorForId(string id)
        {
            int number = 0;
            if(id.Length > 1 && int.TryParse(id.Substring(1), out int result))
                number = result;

            ConsoleColor[] palette = new[]
            {
                ConsoleColor.Green,
                ConsoleColor.Cyan,
                ConsoleColor.Magenta,
                ConsoleColor.Red,
                ConsoleColor.Blue,
                ConsoleColor.Yellow,
                ConsoleColor.White,
                ConsoleColor.DarkGreen,
                ConsoleColor.DarkCyan,
                ConsoleColor.DarkMagenta
            };

            return palette[number % palette.Length];
        }

        static bool AllDead()
        {
            foreach(var proto in protos)
                if(proto.Alive)
                    return false;
            return true;
        }

        static void PrintTree(string parentId, int depth)
        {
            foreach(var proto in protos)
            {
                if(proto.ParentId == parentId)
                {
                    Console.WriteLine($"{new string(' ', depth * 2)}{proto.Id}");
                    PrintTree(proto.Id, depth + 1);
                }
            }
        }
    }

    public class Cell
    {
        public char Type = '.';
        public string Id = "";
        public string TombId = "";
        public int energy = 3;
    }

    public class Kyra
    {
        public string Id;
        public string ParentId;
        public int X, Y;
        public int Energy = 20;
        public int TotalCollectedEnergy = 0;
        public bool Alive = true;

        public void Act(Cell[,] grid, List<Kyra> protos, Random rand)
        {
            if(!Alive) return;
            if(Energy <= 0)
            {
                Alive = false;
                Program.LogEvent($"{Id} died. Left {TotalCollectedEnergy} energy in tomb.");
                grid[Y, X].Type = Constants.DeadProtoCell;
                grid[Y, X].TombId = Id;
                grid[Y, X].Id = Id;
                grid[Y, X].energy = TotalCollectedEnergy;
                return;
            }

            if(Energy >= 25)
                TryReplicate(grid, protos);

            TryConsumeEnergy(grid);
            TryMove(grid, rand);
        }

        private void TryReplicate(Cell[,] grid, List<Kyra> protos)
        {
            var dx = new[] {0, 0, -1, 1};
            var dy = new[] {-1, 1, 0, 0};

            for(int i = 0; i < 4; i++)
            {
                int nx = X + dx[i];
                int ny = Y + dy[i];
                if(nx >= 0 && nx < Constants.GridSize && ny >= 0 && ny < Constants.GridSize)
                {
                    if(grid[ny, nx].Type == '.')
                    {
                        var child = new Kyra
                        {
                            X = nx,
                            Y = ny,
                            ParentId = Id,
                            Id = "K" + protos.Count,
                            Energy = Energy / 3
                        };
                        Energy -= child.Energy;
                        grid[ny, nx].Type = child.Id[0];
                        grid[ny, nx].Id = child.Id;
                        protos.Add(child);
                        Program.LogEvent($"{Id} replicated => created {child.Id}.");
                        break;
                    }
                }
            }
        }

        private void TryConsumeEnergy(Cell[,] grid)
        {
            var target = grid[X, Y];

            if(target.Type != Constants.EmptyCell && target.energy > 0)
            {
                if(target.Type == Constants.DeadProtoCell)
                    Program.LogEvent($"{Id} looted {target.energy} energy from tomb of {target.TombId}.");
                
                Energy += target.energy;
                TotalCollectedEnergy += target.energy;
                target.energy = 0;
            }

            if(target.Type == Constants.EnergyCell)
                target.Type = Constants.EmptyCell;
        }

        private void TryMove(Cell[,] grid, Random rand)
        {
            var dx = new[] {0, 0, -1, 1};
            var dy = new[] {-1, 1, 0, 0};

            int dir = rand.Next(4);
            int nx = X + dx[dir];
            int ny = Y + dy[dir];

            if(nx >= 0 && nx < Constants.GridSize && ny >= 0 && ny < Constants.GridSize)
            {
                var target = grid[ny, nx];

                bool canMove =
                    target.Type == Constants.EmptyCell ||
                    target.Type == Constants.EnergyCell ||
                    (target.Type == Constants.DeadProtoCell && target.energy > 0);

                if(canMove)
                {
                    grid[Y, X].Type = Constants.EmptyCell;
                    grid[Y, X].Id = "";

                    X = nx;
                    Y = ny;
                    grid[Y, X].Type = Id[0];
                    grid[Y, X].Id = Id;
                    Energy--;
                }
            }
        }
    }
}