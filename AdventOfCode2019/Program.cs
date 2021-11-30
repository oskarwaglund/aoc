using System;
using System.Media;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Numerics;
using System.Text;

/// <summary>
/// www.adventofcode.com :)
/// </summary>

namespace AdventOfCode
{
    class Program
    {
        static void Main(string[] args)
        {
            d22_2020();
        }

        static void dx()
        {
            var lines = ReadInput("dx");
        }

        static Dictionary<string, string> d25Dict = new Dictionary<string, string>()
        {
            {"w", "north"},
            {"a", "west"},
            {"s", "south"},
            {"d", "east"},
            {"i", "inv"},
        };

        static string filterInput(string input)
        {
            if (d25Dict.ContainsKey(input))
            {
                return d25Dict[input];
            }
            return input;
        }

        static void drawHullMap(char[,] map, int X, int Y)
        {
            for (int y = 0; y < map.GetLength(0); y++)
            {
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    if(x == X && y == Y)
                    {
                        Console.Write('O');
                    } else
                    {
                        Console.Write(map[y, x] == 0 ? ' ' : map[y, x]);
                    }
                }
                Console.WriteLine();
            }
        }

        static void runD25()
        {
            string initialMoves = "wwsadsssdawassdaadwad";
            int currentI = 0;
            char[,] map = new char[40, 40];
            for (int yy = 1; yy < map.GetLength(0); yy += 2)
            {
                for (int xx = 1; xx < map.GetLength(1); xx += 2)
                {
                    map[yy, xx] = '+';
                }

            }
            int x = map.GetLength(1) / 2;
            int y = map.GetLength(0) / 2;

            string currentItem = "";

            HashSet<string> knownItems = new HashSet<string>();

            var lines = ReadInput("d25")[0];
            var c = new ICScript(lines);
            //Console.WriteLine(c.printCode());

            int dx = 0;
            int dy = 0;
            while (true)
            {
                Console.Clear();
                c.run();
                var output = c.output.Select(s => (char)s).Aggregate("", (curr, next) => curr + next);
                c.output.Clear();
                if ((dy != 0 || dx != 0) && !output.Contains("You can't go that way."))
                {
                    x += dx;
                    y += dy;
                    map[y - 1, x] = output.Contains("north") ? ' ' : '-';
                    map[y + 1, x] = output.Contains("south") ? ' ' : '-';
                    map[y, x - 1] = output.Contains("west") ? ' ' : '|';
                    map[y, x + 1] = output.Contains("east") ? ' ' : '|';
                }

                if(output.Contains("Security Checkpoint"))
                {
                    //c.doLog = true;   
                    x = map.GetLength(1) / 2 - 6;
                    y = map.GetLength(1) / 2 + 2;
                }

                if (output.Contains("Pressure-Sensitive"))
                {
                    ;
                }

                if (output.Contains("Items here:"))
                {
                    var split = output.Split('\n');
                    for (int i = 0; i < split.Length; i++)
                    {
                        if (split[i].Contains("Items here:"))
                        {
                            currentItem = split[i + 1].Substring(2).Replace("\n", "");
                            knownItems.Add(currentItem);
                            break;
                        }
                    }
                }

                Console.WriteLine(output);
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Current hull map:");
                drawHullMap(map, x, y);

                Console.WriteLine("Known items:");
                foreach (var item in knownItems) Console.WriteLine(item);
                string input;
                if (currentI < initialMoves.Length)
                {
                    input = filterInput(initialMoves.ElementAt(currentI) + "");
                    currentI++;
                }
                else
                {
                    input = filterInput(Console.ReadLine());
                    if (input == "t")
                    {
                        input = "take " + currentItem;
                        c.doLog = true;
                    }
                    if (input == "r")
                    {
                        return;
                    }
                }
                dx = 0;
                dy = 0;
                switch (input)
                {
                    case "north": dy = -2; break;
                    case "south": dy = 2; break;
                    case "west": dx = -2; break;
                    case "east": dx = 2; break;
                }
                c.AddInput(input + '\n');
            }
        }

        static void d25()
        {
            while (true) runD25();
        }

        static void d23()
        {
            var lines = ReadInput("d23")[0];
            var NICs = new List<ICScript>();
            for (int i = 0; i < 50; i++)
            {
                NICs.Add(new ICScript(lines));
                NICs[i].AddInput(i);
                NICs[i].run();
            }

            long natX = 0;
            long natY = 0;
            int idleCounter = 0;

            var natYTransmissions = new HashSet<long>();

            while (true)
            {
                for (int i = 0; i < 50; i++)
                {
                    var nic = NICs[i];

                    //Send
                    while (nic.output.Count >= 3)
                    {
                        long dst = nic.output[0];
                        long x = nic.output[1];
                        long y = nic.output[2];

                        Console.WriteLine($"nic {i} sent {x},{y} to nic {dst}");

                        if (dst < 50)
                        {
                            var dstNic = NICs[(int)dst];
                            dstNic.AddInput(x);
                            dstNic.AddInput(y);
                        }
                        else if (dst == 255)
                        {
                            //End of part 1
                            //return;

                            natX = x;
                            natY = y;
                        }
                        else
                        {
                            throw new Exception("Illegal dst");
                        }

                        nic.output = nic.output.Skip(3).ToList();

                    }

                    //Receive
                    if (nic.unprocessedInput() == 0)
                    {
                        nic.AddInput(-1);
                        idleCounter++;
                    }
                    else
                    {
                        idleCounter = 0;
                    }
                    nic.run();

                    if (idleCounter == 100)
                    {
                        var dstNic = NICs[0];
                        dstNic.AddInput(natX);
                        dstNic.AddInput(natY);
                        Console.WriteLine($"NAT sent {natX},{natY} to nic 0");
                        if (natYTransmissions.Contains(natY))
                        {
                            Console.WriteLine($"{natY} was sent twice by NAT");
                            return;
                        }
                        natYTransmissions.Add(natY);

                    }
                }
            }
        }




        static void printGol(bool[,] grid)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    Console.Write(grid[y, x] ? '#' : '.');
                }
                Console.WriteLine("");
            }
            Console.WriteLine("");
        }

        static void multiGolTest(int index)
        {
            var cGrid = new bool[5, 5];
            var uGrid = new bool[5, 5];
            var lGrid = new bool[5, 5];

            var Y = index / 5;
            var X = index % 5;

            var map = golMap2[index];
            for (int i = 0; i < map.GetLength(0); i++)
            {
                var dD = map[i, 0];
                var dX = map[i, 1];
                var dY = map[i, 2];
                var y = Y + dY;
                var x = X + dX;

                switch (dD)
                {
                    case -1:
                        lGrid[y, x] = true; break;
                    case 0:
                        cGrid[y, x] = true; break;
                    case 1:
                        uGrid[y, x] = true; break;
                }
            }

            cGrid[Y, X] = true;

            Console.WriteLine("Upper grid");
            printGol(uGrid);
            Console.WriteLine("Center grid");
            printGol(cGrid);
            Console.WriteLine("Lower grid");
            printGol(lGrid);

        }

        static void d24()
        {
            var lines = ReadInput("d24");

            //Test adjacency
            /*for(int kk = 0; kk < 25; kk++)
            {
                multiGolTest(kk);
                Console.ReadKey();
                Console.Clear();
            }*/

            var startGrid = new bool[5, 5];
            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                    startGrid[y, x] = lines[y].ElementAt(x) == '#';

            //Part 1
            var grids = new Dictionary<int, bool[,]>();
            grids.Add(0, startGrid);
            var visited = new HashSet<int>();
            visited.Add(golBio(startGrid));
            while (true)
            {
                grids = stepGOL(grids, golMap);
                int bio = golBio(grids[0]);
                if (visited.Contains(bio))
                {
                    Console.WriteLine("Part 1: " + bio);
                    break;
                }
                visited.Add(bio);
            }

            grids = new Dictionary<int, bool[,]>();
            grids.Add(0, startGrid);
            for (int t = 0; t < 200; t++)
            {
                grids = stepGOL(grids, golMap2);
            }
            Console.WriteLine("Part 2: " + grids.Values.Sum(s => golSum(s)));
        }

        static int golSum(bool[,] grid)
        {
            int sum = 0;
            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                    sum += grid[y, x] ? 1 : 0;
            return sum;
        }

        //Map from square index = 5*y+x (0-24) to neighbors of form {dimension, x, y} relation
        static Dictionary<int, int[,]> golMap2 = new Dictionary<int, int[,]>
        {
            {0, new int[,]{
                {0, 1, 0}, //B
                {0, 0, 1}, //F
                {1, 2, 1}, //8
                {1, 1, 2}, //12
            }},
            {1, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {1, 1, 1}, //8

            }},
            {2, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {1, 0, 1}, //8
            }},
            {3, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {1, -1, 1}, //8
            }},
            {4, new int[,]{
                {0, -1, 0},
                {0, 0, 1},
                {1, -2, 1}, //8
                {1, -1, 2}, //14
            }},
            {5, new int[,]{
                {0, 1, 0},
                {0, 0, 1},
                {0, 0, -1},
                {1, 1, 1}, //12
            }},
            {6, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {7, new int[,]{ //H
                {0, 1, 0}, //I
                {0, -1, 0}, //G
                {0, 0, -1}, //C
                {-1, -2, -1}, //1
                {-1, -1, -1}, //2
                {-1, 0, -1}, //3
                {-1, 1, -1}, //4
                {-1, 2, -1}, //5
            }},
            {8, new int[,]{ //I
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {9, new int[,]{ //J
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
                {1, -1, 1}, //14
            }},
            {10, new int[,]{ //K
                {0, 1, 0},
                {0, 0, 1},
                {0, 0, -1},
                {1, 1, 0}, //12
            }},
            {11, new int[,]{ //L
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
                {-1, -1, -2}, //1
                {-1, -1, -1}, //6
                {-1, -1, 0}, //11
                {-1, -1, 1}, //16
                {-1, -1, 2}, //21
            }},
            {12, new int[,]{ //M does not exist
            }},
            {13, new int[,]{ //N
                {0, 1, 0},
                {0, 0, 1},
                {0, 0, -1},
                {-1, 1, -2}, //5
                {-1, 1, -1}, //10
                {-1, 1, 0}, //15
                {-1, 1, 1}, //20
                {-1, 1, 2}, //25
            }},
            {14, new int[,]{ //O
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
                {1, -1, 0}, //14
            }},
            {15, new int[,]{ //P
                {0, 1, 0},
                {0, 0, 1},
                {0, 0, -1},
                {1, 1, -1}, //12
            }},
            {16, new int[,]{ //Q
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {17, new int[,]{ //R
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {-1, -2, 1}, //21
                {-1, -1, 1}, //22
                {-1, 0, 1}, //23
                {-1, 1, 1}, //24
                {-1, 2, 1}, //25
            }},
            {18, new int[,]{ //S
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {19, new int[,]{ //T
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
                {1, -1, -1}, // 14
            }},
            {20, new int[,]{ //U
                {0, 1, 0},
                {0, 0, -1},
                {1, 1, -2}, //12
                {1, 2, -1}, //18
            }},
            {21, new int[,]{ //V
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, -1},
                {1, 1, -1}, //18
            }},
            {22, new int[,]{ //W
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, -1},
                {1, 0, -1}, //18
            }},
            {23, new int[,]{ //X
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, -1},
                {1, -1, -1}, //18
            }},
            {24, new int[,]{ //Y
                {0, -1, 0},
                {0, 0, -1},
                {1, -1, -2}, //14
                {1, -2, -1}, //18
            }},

        };

        //Map from square index = 5*y+x (0-24) to neighbors of form {dimension, x, y} relation
        static Dictionary<int, int[,]> golMap = new Dictionary<int, int[,]>
        {
            {0, new int[,]{
                {0, 1, 0},
                {0, 0, 1},
            }},
            {1, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
            }},
            {2, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
            }},
            {3, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
            }},
            {4, new int[,]{
                {0, -1, 0},
                {0, 0, 1},
            }},
            {5, new int[,]{
                {0, 1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {6, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {7, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {8, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {9, new int[,]{
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {10, new int[,]{
                {0, 1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {11, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {12, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {13, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {14, new int[,]{
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {15, new int[,]{
                {0, 1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {16, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {17, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {18, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {19, new int[,]{
                {0, -1, 0},
                {0, 0, 1},
                {0, 0, -1},
            }},
            {20, new int[,]{
                {0, 1, 0},
                {0, 0, -1},
            }},
            {21, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, -1},
            }},
            {22, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, -1},
            }},
            {23, new int[,]{
                {0, 1, 0},
                {0, -1, 0},
                {0, 0, -1},
            }},
            {24, new int[,]{
                {0, -1, 0},
                {0, 0, -1},
            }},

        };

        static Dictionary<int, bool[,]> stepGOL(Dictionary<int, bool[,]> grids, Dictionary<int, int[,]> map)
        {
            var newG = new Dictionary<int, bool[,]>();
            foreach (var d in grids.Keys)
            {
                //On level d
                var newGrid = generateGolGrid(grids, map, d);
                newG.Add(d, newGrid);
            }

            //Check if new levels are needed
            int lowerDim = grids.Keys.Min() - 1;
            var lowerGrid = generateGolGrid(grids, map, lowerDim);
            if (golSum(lowerGrid) > 0)
            {
                newG.Add(lowerDim, lowerGrid);
            }
            int upperDim = grids.Keys.Max() + 1;
            var upperGrid = generateGolGrid(grids, map, upperDim);
            if (golSum(upperGrid) > 0)
            {
                newG.Add(upperDim, upperGrid);
            }

            return newG;
        }

        static bool[,] generateGolGrid(Dictionary<int, bool[,]> grids, Dictionary<int, int[,]> map, int d)
        {
            var currGrid = grids.ContainsKey(d) ? grids[d] : new bool[5, 5];
            var newGrid = new bool[5, 5];
            for (int y = 0; y < 5; y++)
                for (int x = 0; x < 5; x++)
                {
                    //Find neighbors to square d,x,y
                    int neighbors = 0;
                    int index = y * 5 + x;
                    var neighMap = map[index];
                    for (int i = 0; i < neighMap.GetLength(0); i++)
                    {
                        var dD = neighMap[i, 0];
                        var dX = neighMap[i, 1];
                        var dY = neighMap[i, 2];
                        if (grids.ContainsKey(d + dD) && grids[d + dD][y + dY, x + dX])
                        {
                            neighbors++;
                        }
                    }
                    newGrid[y, x] = currGrid[y, x] ? (neighbors == 1) : (neighbors == 1 || neighbors == 2);
                }
            return newGrid;
        }

        static int golBio(bool[,] grid)
        {
            int bio = 0;
            int mul = 1;
            for (int y = 0; y < 5; y++)
                for (int x = 0; x < 5; x++)
                {
                    if (grid[y, x]) bio += mul;
                    mul *= 2;
                }
            return bio;
        }

        static bool[,] stepGOL(bool[,] grid)
        {
            var newG = new bool[5, 5];
            for (int x = 0; x < 5; x++)
                for (int y = 0; y < 5; y++)
                {
                    int neighbors = 0;
                    if (x > 0 && grid[y, x - 1]) neighbors++;
                    if (x < 4 && grid[y, x + 1]) neighbors++;
                    if (y > 0 && grid[y - 1, x]) neighbors++;
                    if (y < 4 && grid[y + 1, x]) neighbors++;

                    newG[y, x] = grid[y, x] ? (neighbors == 1) : (neighbors == 1 || neighbors == 2);
                }
            return newG;
        }

        static void d22_2020()
        {
            long cards = 10007;
            var lines = ReadInput("d22");
            long pos = 2019;
            foreach (var l in lines)
            {
                var split = l.Split();
                if (split[0] == "cut")
                {
                    pos = cut(long.Parse(split[1]), pos, cards);
                }
                else if (split[1] == "into")
                {
                    pos = deal(pos, cards);
                }
                else if (split[1] == "with")
                {
                    pos = inc(long.Parse(split[3]), pos, cards);
                }
            }
            Console.WriteLine("Part 1: " + pos);


        }


        //int y = Ax+b

        //A = -1, b = cards - 1
        static long deal(long pos, long cards)
        {
            return cards - pos - 1;
            //(-pos - 1)%cards
        }

        static long cut(long n, long pos, long cards)
        {
            return (pos - n + cards) % cards;
            //(pos - n)%cards
        }

        static long inc(long n, long pos, long cards)
        {
            return (pos * n) % cards;
            //(pos*n)%cards
        }

        static void d22()
        {
            long times = 101741582076661;
            long card = 2020;
            var seen = new Dictionary<long, long>();
            findParam();
            for (long t = 0; t < 1; t++)
            {
                if (seen.ContainsKey(card)) Console.WriteLine("Found loop at {0} -> {1} ({2})", t, seen[card], card);
                if (t % 100000 == 0)
                {
                    seen.Add(card, t);
                    Console.WriteLine(t);
                }
                card = rcShuffle(card);
            }
            Console.WriteLine(card);
        }

        //Find linear operation x' = Ax + B mod N

        static long[] findParam()
        {
            const long N = 119315717514047;

            var param = new long[] { 1, 0 };
            pCut(param, 921, N);
            Console.WriteLine("Hello");
            return param;
        }

        static long norm(long v, long n)
        {
            while (v < 0) v += n;
            while (v >= n) v -= n;
            return v;
        }

        static void pCut(long[] param, long c, long N)
        {
            param[1] = norm(param[1] + c, N);
        }

        static void pDeal(long[] param, long c, long N)
        {
            //param[1] = norm(param[])
        }

        static long rcShuffle(long card)
        {
            const long N = 119315717514047;
            card = cCut(N - 3094, card, N);
            card = cInc(8, card, N);
            card = cCut(921, card, N);
            card = cInc(65, card, N);
            card = cCut(3874, card, N);
            card = cInc(7, card, N);
            card = cCut(N - 5173, card, N);
            card = cInc(70, card, N);
            card = cCut(N - 5465, card, N);
            card = cDeal(card, N);
            card = cCut(N - 1720, card, N);
            card = cInc(7, card, N);
            card = cDeal(card, N);
            card = cCut(6216, card, N);
            card = cDeal(card, N);
            card = cCut(N - 7831, card, N);
            card = cInc(72, card, N);
            card = cCut(4432, card, N);
            card = cDeal(card, N);
            card = cCut(N - 5295, card, N);
            card = cDeal(card, N);
            card = cInc(10, card, N);
            card = cCut(1412, card, N);
            card = cInc(29, card, N);
            card = cCut(798, card, N);
            card = cInc(75, card, N);
            card = cCut(848, card, N);
            card = cInc(10, card, N);
            card = cCut(N - 2906, card, N);
            card = cInc(14, card, N);
            card = cDeal(card, N);
            card = cInc(63, card, N);
            card = cCut(N - 9181, card, N);
            card = cInc(12, card, N);
            card = cDeal(card, N);
            card = cInc(60, card, N);
            card = cDeal(card, N);
            card = cInc(9, card, N);
            card = cCut(522, card, N);
            card = cInc(45, card, N);
            card = cDeal(card, N);
            card = cInc(36, card, N);
            card = cCut(3881, card, N);
            card = cInc(57, card, N);
            card = cCut(9621, card, N);
            card = cInc(56, card, N);
            card = cCut(N - 7962, card, N);
            card = cInc(57, card, N);
            card = cCut(4541, card, N);
            card = cInc(41, card, N);
            card = cCut(3062, card, N);
            card = cInc(52, card, N);
            card = cCut(N - 6788, card, N);
            card = cInc(28, card, N);
            card = cCut(N - 1341, card, N);
            card = cInc(44, card, N);
            card = cCut(N - 3391, card, N);
            card = cInc(50, card, N);
            card = cCut(N - 6755, card, N);
            card = cInc(45, card, N);
            card = cCut(228, card, N);
            card = cInc(11, card, N);
            card = cCut(N - 8473, card, N);
            card = cDeal(card, N);
            card = cInc(63, card, N);
            card = cCut(N - 5214, card, N);
            card = cInc(16, card, N);
            card = cCut(1120, card, N);
            card = cInc(54, card, N);
            card = cDeal(card, N);
            card = cCut(8379, card, N);
            card = cDeal(card, N);
            card = cInc(13, card, N);
            card = cCut(N - 6256, card, N);
            card = cDeal(card, N);
            card = cCut(433, card, N);
            card = cInc(60, card, N);
            card = cDeal(card, N);
            card = cCut(2202, card, N);
            card = cInc(28, card, N);
            card = cCut(3152, card, N);
            card = cInc(61, card, N);
            card = cCut(N - 6230, card, N);
            card = cInc(51, card, N);
            card = cCut(N - 6860, card, N);
            card = cInc(12, card, N);
            card = cCut(2609, card, N);
            card = cInc(14, card, N);
            card = cCut(N - 2842, card, N);
            card = cInc(6, card, N);
            card = cCut(6093, card, N);
            card = cInc(24, card, N);
            card = cCut(N - 1725, card, N);
            card = cInc(32, card, N);
            card = cDeal(card, N);
            card = cCut(5974, card, N);
            card = cInc(57, card, N);
            card = cDeal(card, N);
            card = cCut(N - 2732, card, N);
            card = cDeal(card, N);
            return card;
        }
        /*
            static UInt64 cShuffle(UInt64 card)
        {
            const UInt64 N = 10007; // 119315717514047;
            card = cDeal(card, N);
            card = cCut(N - 2732, card, N);
            card = cDeal(card, N);
            card = cInc(57, card, N);
            card = cCut(5974, card, N);
            card = cDeal(card, N);
            card = cInc(32, card, N);
            card = cCut(N - 1725, card, N);
            card = cInc(24, card, N);
            card = cCut(6093, card, N);
            card = cInc(6, card, N);
            card = cCut(N - 2842, card, N);
            card = cInc(14, card, N);
            card = cCut(2609, card, N);
            card = cInc(12, card, N);
            card = cCut(N - 6860, card, N);
            card = cInc(51, card, N);
            card = cCut(N - 6230, card, N);
            card = cInc(61, card, N);
            card = cCut(3152, card, N);
            card = cInc(28, card, N);
            card = cCut(2202, card, N);
            card = cDeal(card, N);
            card = cInc(60, card, N);
            card = cCut(433, card, N);
            card = cDeal(card, N);
            card = cCut(N - 6256, card, N);
            card = cInc(13, card, N);
            card = cDeal(card, N);
            card = cCut(8379, card, N);
            card = cDeal(card, N);
            card = cInc(54, card, N);
            card = cCut(1120, card, N);
            card = cInc(16, card, N);
            card = cCut(N - 5214, card, N);
            card = cInc(63, card, N);
            card = cDeal(card, N);
            card = cCut(N - 8473, card, N);
            card = cInc(11, card, N);
            card = cCut(228, card, N);
            card = cInc(45, card, N);
            card = cCut(N - 6755, card, N);
            card = cInc(50, card, N);
            card = cCut(N - 3391, card, N);
            card = cInc(44, card, N);
            card = cCut(N - 1341, card, N);
            card = cInc(28, card, N);
            card = cCut(N - 6788, card, N);
            card = cInc(52, card, N);
            card = cCut(3062, card, N);
            card = cInc(41, card, N);
            card = cCut(4541, card, N);
            card = cInc(57, card, N);
            card = cCut(N - 7962, card, N);
            card = cInc(56, card, N);
            card = cCut(9621, card, N);
            card = cInc(57, card, N);
            card = cCut(3881, card, N);
            card = cInc(36, card, N);
            card = cDeal(card, N);
            card = cInc(45, card, N);
            card = cCut(522, card, N);
            card = cInc(9, card, N);
            card = cDeal(card, N);
            card = cInc(60, card, N);
            card = cDeal(card, N);
            card = cInc(12, card, N);
            card = cCut(N - 9181, card, N);
            card = cInc(63, card, N);
            card = cDeal(card, N);
            card = cInc(14, card, N);
            card = cCut(N - 2906, card, N);
            card = cInc(10, card, N);
            card = cCut(848, card, N);
            card = cInc(75, card, N);
            card = cCut(798, card, N);
            card = cInc(29, card, N);
            card = cCut(1412, card, N);
            card = cInc(10, card, N);
            card = cDeal(card, N);
            card = cCut(N - 5295, card, N);
            card = cDeal(card, N);
            card = cCut(4432, card, N);
            card = cInc(72, card, N);
            card = cCut(N - 7831, card, N);
            card = cDeal(card, N);
            card = cCut(6216, card, N);
            card = cDeal(card, N);
            card = cInc(7, card, N);
            card = cCut(N - 1720, card, N);
            card = cDeal(card, N);
            card = cCut(N - 5465, card, N);
            card = cInc(70, card, N);
            card = cCut(N - 5173, card, N);
            card = cInc(7, card, N);
            card = cCut(3874, card, N);
            card = cInc(65, card, N);
            card = cCut(921, card, N);
            card = cInc(8, card, N);
            card = cCut(N - 3094, card, N);
            return card;
        }*/

        static long cCut(long val, long pos, long N)
        {
            //pos = pos + val mod N
            return (pos + val) % N;
        }

        static long cDeal(long pos, long N)
        {
            //pos = N - pos - 1 mod n
            return N - pos - 1;
        }

        static long cInc(long inc, long pos, long N)
        {
            //pos = pos*mod_inv(inc, N) mod n
            var Pos = new BigInteger(pos);
            long Inv = inv(inc, N);
            return (long)((Pos * Inv) % N);
        }

        static Dictionary<long, long> coeff = new Dictionary<long, long>
        {
            {1, 1 },
        };

        static long inv(long inc, long N)
        {
            if (coeff.ContainsKey(inc)) return coeff[inc];
            long res = ((N - N / inc) * inv(N % inc, N) % N) % N;
            if (res < 0) res += N;
            coeff.Add(inc, res);
            return res;
        }

        /*static long cInc(long inc, long pos, long N)
        {
            var s = BigInteger.Zero;
            var sp = BigInteger.One;
            var r = new BigInteger(N);
            var rp = new BigInteger(inc);

            while (r != 0)
            {
                var st = s;
                var rt = r;

                var q = rp / r;
                s = sp - q * s;
                sp = st;
                r = rp - q * r;
                rp = rt;
            }

            while (sp < 0)
                sp += N;
            var res = (sp * pos) % N;
            //if ((inc * res) % N != pos) throw new Exception();
            return (long)res;
        }*/
        /*
        static UInt64 cCut(UInt64 val, UInt64 pos, UInt64 N)
        {
            return (pos + N - val) % N;
        }

        static UInt64 cDeal(UInt64 pos, UInt64 N)
        {
            return N - pos - 1;
        }

        static UInt64 cInc(UInt64 inc, UInt64 pos, UInt64 N)
        {
            return (inc * pos) % N;
        }
        */
        static void d21()
        {
            var lines = ReadInput("d21")[0];
            var script = new ICScript(lines);
            script.run();
            var walkCommand = "" + //(!C && D) || !A
                "NOT C J\n" +
                "AND D J\n" +
                "NOT A T\n" + 
                "OR T J\n" + 
                "WALK\n";
            var commands = "" +
                "NOT C J\n" +
                "AND D J\n" +
                "AND H J\n" +
                "NOT B T\n" +
                "AND D T\n" +
                "OR T J\n" +
                "NOT A T\n" +
                "OR T J\n" +
                "RUN\n";
            script.AddInput(walkCommand);
            script.run();
            var output = String.Join('/', script.output.Select(c => (char)c)).Replace("/", "");
            Console.WriteLine(output);
            if (script.output.Last() > 255) Console.WriteLine("Damage: " + script.output.Last());
        }

        static void d20()
        {
            var lines = ReadInput("d20");
            var map = new PortalMap();
            int startX = 0, startY = 0;
            int stopX = 0, stopY = 0;
            for (int y = 0; y < lines.Length; y++)
            {
                for (int x = 0; x < lines[0].Length; x++)
                {
                    char c = lines[y][x];
                    if (c == 'A' && lines[y][x + 1] == 'A')
                    {
                        startX = x + 2;
                        startY = y;
                    }
                    if (c == 'Z' && lines[y - 1][x] == 'Z')
                    {
                        stopX = x;
                        stopY = y - 2;
                    }
                    if (c != ' ') map.set(x, y, c);
                }
            }
            var path = map.findShortestPath(startX, startY, stopX, stopY);
            if (path == null)
            {
                Console.WriteLine("No path!");
            }
            else
            {
                Console.WriteLine("Shortest path: " + (path.Count() - 1));
            }

        }

        class PortalMap
        {
            public Dictionary<Point, Piece> points;
            Dictionary<string, Portal> portals;
            int xMin, xMax, yMin, yMax;

            public PortalMap()
            {
                points = new Dictionary<Point, Piece>();
                portals = new Dictionary<string, Portal>();
                xMin = 0;
                xMax = 0;
                yMin = 0;
                yMax = 0;
            }

            class Portal
            {
                public Point x1, x2;
            }

            public class Piece
            {
                public char val;
                public Piece(char _v)
                {
                    val = _v;
                }
            }

            public Piece get(int x, int y)
            {
                var p = new Point(x, y);
                return points.ContainsKey(p) ? points[p] : null;
            }

            public void set(int x, int y, char val)
            {
                var p = new Point(x, y);
                if (points.ContainsKey(p))
                {
                    points[p].val = val;
                }
                else
                {
                    points.Add(p, new Piece(val));
                }

                xMax = Math.Max(x, xMax);
                xMin = Math.Min(x, xMin);
                yMax = Math.Max(y, yMax);
                yMin = Math.Min(y, yMin);
            }

            public void show()
            {
                //Console.Clear();
                Console.WriteLine();
                for (int y = yMin; y <= yMax; y++)
                {
                    for (int x = xMin; x <= xMax; x++)
                    {
                        var n = get(x, y);
                        char c = (n == null ? '?' : n.val);
                        Console.Write(c);
                    }
                    Console.WriteLine();
                }
            }

            public class ANode : IComparable
            {
                public int x, y, level;
                public int g;
                public int f;
                public ANode back;

                public static int START_VAL = 1000000;

                public ANode(int _x, int _y, int _l)
                {
                    x = _x;
                    y = _y;
                    level = _l;

                    g = START_VAL;
                    f = START_VAL;
                }

                public int CompareTo(object obj)
                {
                    if (obj == null) return 1;

                    ANode n = obj as ANode;
                    if (n == null) return 1;

                    if (n.x == x && n.y == y) return 0;
                    else if (n.x != x) return n.x > x ? 1 : -1;
                    else return n.y > y ? 1 : -1;
                }
            }

            private int heur(ANode n)
            {
                return n.level * 100;
            }

            private void expand(List<ANode> open, List<ANode> visited, ANode c, int dir)
            {
                int nX, nY;
                switch (dir)
                {
                    case 0: nX = c.x + 1; nY = c.y; break;
                    case 1: nX = c.x; nY = c.y + 1; break;
                    case 2: nX = c.x - 1; nY = c.y; break;
                    case 3: nX = c.x; nY = c.y - 1; break;
                    default: throw new Exception();
                }
                var ch = get(nX, nY).val;
                if (ch == '.')
                {
                    var tGScore = c.g + 1;
                    bool add = false;
                    if (visited.Any(p => p.x == nX && p.y == nY && p.level == c.level)) return;
                    var neighbor = open.FirstOrDefault(p => p.x == nX && p.y == nY && p.y == c.level);

                    if (neighbor == null)
                    {
                        neighbor = new ANode(nX, nY, c.level);
                        add = true;
                    }
                    if (tGScore < neighbor.g)
                    {
                        neighbor.g = tGScore;
                        neighbor.f = neighbor.g + heur(neighbor);
                        neighbor.back = c;
                        if (add) open.Add(neighbor);
                    }
                }
                else if (Char.IsUpper(ch))
                {
                    bool inner = c.x > 10 && c.x < 100 && c.y > 10 && c.y < 100;
                    if (c.level == 0 && !inner) return;
                    string port = "";
                    switch (dir)
                    {
                        case 0: port = ch + "" + get(nX + 1, nY).val; break;
                        case 1: port = ch + "" + get(nX, nY + 1).val; break;
                        case 2: port = get(nX - 1, nY).val + "" + ch; break;
                        case 3: port = get(nX, nY - 1).val + "" + ch; break;
                        default: throw new Exception();
                    }
                    if (port != "AA" && port != "ZZ")
                    {
                        int nLevel = c.level + (inner ? 1 : -1);
                        var portal = portals[port];
                        Point nPoint;
                        if (portal.x1.X == c.x && portal.x1.Y == c.y)
                        {
                            nPoint = portal.x2;
                        }
                        else
                        {
                            nPoint = portal.x1;
                        }
                        var tGScore = c.g + 1;
                        bool add = false;
                        if (visited.Any(p => p.x == nPoint.X && p.y == nPoint.Y && p.level == nLevel)) return;
                        var neighbor = open.FirstOrDefault(p => p.x == nPoint.X && p.y == nPoint.Y && p.y == nLevel);
                        if (neighbor == null)
                        {
                            neighbor = new ANode(nPoint.X, nPoint.Y, nLevel);
                            add = true;
                        }
                        if (tGScore < neighbor.g)
                        {
                            neighbor.g = tGScore;
                            neighbor.f = neighbor.g + heur(neighbor);
                            neighbor.back = c;
                            if (add) open.Add(neighbor);
                        }
                    }
                }
            }

            private void addNodes(List<ANode> open, List<ANode> visited, ANode curr)
            {
                for (int d = 0; d < 4; d++)
                    expand(open, visited, curr, d);
            }

            private void initPortals()
            {
                foreach (var k in points.Where(p => p.Value.val == '.'))
                {
                    char tel1, tel2;
                    string port;

                    var c = new ANode(k.Key.X, k.Key.Y, 0);

                    tel1 = get(c.x + 1, c.y).val;
                    if (tel1 >= 'A' && tel1 <= 'Z')
                    {
                        tel2 = get(c.x + 2, c.y).val;
                        port = tel1 + "" + tel2;
                        if (portals.ContainsKey(port))
                        {
                            portals[port].x2 = new Point(c.x, c.y);
                        }
                        else
                        {
                            portals.Add(port, new Portal());
                            portals[port].x1 = new Point(c.x, c.y);
                        }
                    }

                    tel1 = get(c.x - 1, c.y).val;
                    if (tel1 >= 'A' && tel1 <= 'Z')
                    {
                        tel2 = get(c.x - 2, c.y).val;
                        port = tel2 + "" + tel1;
                        if (portals.ContainsKey(port))
                        {
                            portals[port].x2 = new Point(c.x, c.y);
                        }
                        else
                        {
                            portals.Add(port, new Portal());
                            portals[port].x1 = new Point(c.x, c.y);
                        }
                    }

                    tel1 = get(c.x, c.y + 1).val;
                    if (tel1 >= 'A' && tel1 <= 'Z')
                    {
                        tel2 = get(c.x, c.y + 2).val;
                        port = tel1 + "" + tel2;
                        if (portals.ContainsKey(port))
                        {
                            portals[port].x2 = new Point(c.x, c.y);
                        }
                        else
                        {
                            portals.Add(port, new Portal());
                            portals[port].x1 = new Point(c.x, c.y);
                        }
                    }

                    tel1 = get(c.x, c.y - 1).val;
                    if (tel1 >= 'A' && tel1 <= 'Z')
                    {
                        tel2 = get(c.x, c.y - 2).val;
                        port = tel2 + "" + tel1;
                        if (portals.ContainsKey(port))
                        {
                            portals[port].x2 = new Point(c.x, c.y);
                        }
                        else
                        {
                            portals.Add(port, new Portal());
                            portals[port].x1 = new Point(c.x, c.y);
                        }
                    }
                }
                portals.Remove("AA");
                portals.Remove("ZZ");
            }

            public List<ANode> findShortestPath(int fromX, int fromY, int toX, int toY)
            {
                var path = new List<ANode>();
                var open = new List<ANode>();
                var visited = new List<ANode>();
                open.Add(new ANode(fromX, fromY, 0));
                initPortals();
                ANode c = null;

                var startNode = open.First(p => p.x == fromX && p.y == fromY);
                startNode.g = 0;
                startNode.f = heur(startNode);

                while (true)
                {
                    if (open.Count == 0) return null;
                    long min = open.Min(s => s.f);
                    c = open.First(p => p.f == min);

                    if (c.x == toX && c.y == toY && c.level == 0)
                        break;

                    open.Remove(c);
                    visited.Add(c);
                    if (visited.Count() % 1000 == 0) Console.WriteLine(visited.Count());
                    addNodes(open, visited, c);
                }

                while (c.x != fromX || c.y != fromY || c.level != 0)
                {
                    path.Insert(0, c);
                    c = c.back;
                }
                path.Insert(0, c);

                return path;
            }
        }

        static void d19()
        {
            var c = ReadInput("d19")[0];
            int x = 38;
            int y = 49;
            while (true)
            {
                Console.WriteLine(x + " " + y);
                int dx = 0;
                while (isBeam(c, x + 99 + dx, y))
                {
                    if (isBeam(c, x + dx, y + 99))
                    {
                        Console.WriteLine((x + dx) + " " + y);
                        return;
                    }
                    else
                    {
                        dx++;
                    }
                }

                y++;
                while (!isBeam(c, x, y)) x++;
            }
        }

        static bool isBeam(string code, int x, int y)
        {
            var script = new ICScript(code);
            script.AddInput(x);
            script.AddInput(y);
            script.run();
            return script.output[^1] == 1;
        }

        static int d18_v2(string file)
        {
            var lines = ReadInput(file);
            var nodes = new HashSet<char>();
            foreach (var line in lines)
            {
                var split = line.Split();
                nodes.Add(split[0][0]);
                nodes.Add(split[1][0]);
            }
            Console.WriteLine(nodes.Count());
            var nodeList = nodes.ToList();
            nodeList.Sort();
            Console.WriteLine();
            foreach (var c in nodeList) Console.Write(c + " ");
            Console.WriteLine();
            var dist = new int[nodes.Count(), nodes.Count()];
            for (int i = 0; i < nodes.Count(); i++)
                for (int j = 0; j < nodes.Count(); j++)
                    if (i != j) dist[i, j] = 999999;
            var dep = new List<char[]>();
            foreach (var line in lines)
            {
                var split = line.Split();
                char c1 = split[0][0];
                char c2 = split[1][0];
                int i1 = nodeList.IndexOf(c1);
                int i2 = nodeList.IndexOf(c2);
                int steps = int.Parse(split[2]);
                dist[i1, i2] = steps;
                dist[i2, i1] = steps;
                if (split.Count() > 3)
                {
                    if (split[3].Contains(Char.ToUpper(c1)))
                    {
                        Console.WriteLine(c1 + "<" + c2);
                        dep.Add(new char[] { c1, c2 });
                    }
                    if (split[3].Contains(Char.ToUpper(c2)))
                    {
                        Console.WriteLine(c2 + "<" + c1);
                        dep.Add(new char[] { c2, c1 });
                    }
                }
            }
            int best = int.MaxValue;
            findAllPath(dist, nodeList, dep, "" + nodeList[0], 0, ref best);
            Console.WriteLine("This was the best for " + nodeList[0]);
            return best;
        }

        static void findAllPath(int[,] dist, List<char> nodeList, List<char[]> dep, string current, int steps, ref int best)
        {
            if (steps >= best) return;
            if (current.Count() == nodeList.Count())
            {
                Console.WriteLine(steps + " " + current);
                best = steps;
                return;
            }
            int from = nodeList.IndexOf(current[^1]);
            foreach (char c in nodeList.Where(c => !current.Contains(c)).OrderBy(c => dist[nodeList.IndexOf(c), from]))
            {
                /*if (dep.Any(d => d[1] == c && !current.Contains(d[0])))
                {
                    continue;
                }*/
                findAllPath(dist, nodeList, dep, current + c, steps + dist[nodeList.IndexOf(c), from], ref best);
            }
        }

        static void d18(string file)
        {
            var lines = ReadInput(file);
            var map = new AdventMap();
            var keys = new List<DoorKey>();
            var doors = new List<DoorKey>();
            int startX = 0, startY = 0;
            char startC = (char)0;
            for (int y = 0; y < lines.Length; y++)
            {
                for (int x = 0; x < lines[0].Length; x++)
                {
                    char c = lines[y][x];
                    if (c >= 'a' && c <= 'z')
                    {
                        keys.Add(new DoorKey(x, y, c));
                    }
                    else if (c >= 'A' && c <= 'Z')
                    {
                        doors.Add(new DoorKey(x, y, c));
                    }
                    else
                    {
                        if ("0123".Contains(c))
                        {
                            startC = c;
                            startX = x;
                            startY = y;
                        }
                    }
                    map.set(x, y, c);
                }
            }
            //map.show();

            var keyPaths = new List<KeyPath>();

            int maxPath = 10000;

            for (int i = 0; i < keys.Count() - 1; i++)
            {
                for (int j = i + 1; j < keys.Count(); j++)
                {
                    var k1 = keys[i];
                    var k2 = keys[j];
                    var path = map.findShortestPath(k1.x, k1.y, k2.x, k2.y, new List<char>() { '#' });
                    var kp = new KeyPath();
                    kp.c1 = k1.type;
                    kp.c2 = k2.type;
                    kp.steps = path.Count() - 1;
                    foreach (var n in path)
                    {
                        char type = map.get(n.x, n.y).val;
                        if (type >= 'A' && type <= 'Z')
                        {
                            kp.doors += type;
                        }
                    }
                    if (kp.steps <= maxPath)
                    {
                        keyPaths.Add(kp);
                        Console.WriteLine("{0} {1} {2} {3}", kp.c1, kp.c2, kp.steps, kp.doors);
                    }
                }
            }

            var firstSteps = new List<KeyPath>();

            foreach (var key in keys)
            {
                var path = map.findShortestPath(startX, startY, key.x, key.y, new List<char>() { '#' });
                var kp = new KeyPath();
                kp.c1 = startC;
                kp.c2 = key.type;
                kp.steps = path.Count() - 1;
                foreach (var n in path)
                {
                    char type = map.get(n.x, n.y).val;
                    if (type >= 'A' && type <= 'Z')
                    {
                        kp.doors += type;
                    }
                }
                if (/*kp.doors.Length == 0 && */kp.steps <= 10000)
                {
                    firstSteps.Add(kp);
                    Console.WriteLine("{0} {1} {2} {3}", kp.c1, kp.c2, kp.steps, kp.doors);
                }

            }

        }

        static int findBestKeyPath(int keys, List<KeyPath> firstSteps, List<KeyPath> keyPaths, out string path)
        {
            path = "";
            int best = int.MaxValue;
            foreach (var kp in firstSteps)
            {
                string newPath;
                int steps = kp.steps + findBestKeyPath(keys - 1, "" + kp.c2, keyPaths, out newPath);
                if (steps < best)
                {
                    best = steps;
                    path = kp.c2 + newPath;
                }
                Console.WriteLine("Found " + kp.c2 + newPath);
            }
            return best;
        }
        static int findBestKeyPath(int depth, string currentPath, List<KeyPath> keyPaths, out string bestPath)
        {
            bestPath = "";
            if (depth == 0)
            {
                return 0;
            }

            int bestSteps = int.MaxValue;
            char current = currentPath[^1];
            string previous = currentPath[0..^1];
            foreach (var kp in keyPaths.Where(p => (p.c1 == current || p.c2 == current) && !previous.Contains(p.c1) && !previous.Contains(p.c2)))
            {
                if (kp.doors.ToLower().Except(currentPath).Count() == 0)
                {
                    char next = kp.c1 == current ? kp.c2 : kp.c1;
                    string path = "";
                    int steps = kp.steps + findBestKeyPath(depth - 1, currentPath + next, keyPaths, out path);
                    if (steps < bestSteps)
                    {
                        bestSteps = steps;
                        bestPath = next + path;
                    }
                }
            }
            return bestSteps;
        }

        class KeyPath
        {
            public char c1;
            public char c2;
            public int steps;
            public string doors;

            public KeyPath()
            {
                c1 = (char)0;
                c2 = (char)0;
                steps = 0;
                doors = "";
            }
        }
        class DoorKey
        {
            public int x;
            public int y;
            public char type;

            public DoorKey(int _x, int _y, int _t)
            {
                x = _x;
                y = _y;
                type = (char)_t;
            }
        }

        static void d17()
        {
            AdventMap map = new AdventMap();
            int x = 0;
            int y = 0;
            int roboY = 0;
            int roboX = 0;
            int xMax = 0, yMax = 0;
            var code = ReadInput("d17")[0].Split(',').Select(long.Parse).ToArray();
            var script = new ICScript(code);
            script.run();
            foreach (long l in script.output)
            {
                if (l == 10)
                {
                    Console.WriteLine();
                    x = 0;
                    y++;
                }
                else
                {
                    if (l == '^')
                    {
                        roboX = x;
                        roboY = y;
                    }
                    Console.Write((char)l);
                    map.set(x, y, (char)l);
                    x++;
                    xMax = Math.Max(xMax, x);
                    yMax = Math.Max(yMax, y);
                }
            }
            int sum = 0;
            for (x = 1; x < xMax - 1; x++)
            {
                for (y = 1; y < yMax - 1; y++)
                {
                    if (map.get(x, y).val == 35 &&
                        map.get(x - 1, y).val == 35 &&
                        map.get(x + 1, y).val == 35 &&
                        map.get(x, y - 1).val == 35 &&
                        map.get(x, y + 1).val == 35)
                    {
                        sum += x * y;
                    }
                }
            }
            Console.WriteLine(sum);
            /*
            int dir = 0; //0 up, //1 right etc
            x = roboX;
            y = roboY;
            int steps = 0;
            var res = new List<string>();
            while (true)
            {
                if (nextInDir(map, x, y, dir) == '#')
                {
                    steps++;
                    switch (dir)
                    {
                        case 0: y--; break;
                        case 1: x++; break;
                        case 2: y++; break;
                        case 3: x--; break;
                    }
                } 
                else
                {
                    if (steps > 0) res.Add("" + steps);
                    steps = 0;
                    if (nextInDir(map, x, y, (dir + 1) % 4) == '#')
                    {
                        res.Add("R");
                        dir = (dir + 1) % 4;
                    }
                    else if (nextInDir(map, x, y, (dir + 3) % 4) == '#')
                    {
                        res.Add("L");
                        dir = (dir + 3) % 4;
                    }
                    else
                    {
                        break;
                    }
                }

            }
            string command = string.Join(',', res);
            Console.WriteLine(command);
            */
            code[0] = 2;
            script = new ICScript(code);

            string move = "A,B,B,A,C,A,C,A,C,B\n";
            string A = "R,6,R,6,R,8,L,10,L,4\n";
            string B = "R,6,L,10,R,8\n";
            string C = "L,4,L,12,R,6,L,10\n";

            foreach (char c in (move + A + B + C + "n\n"))
            {
                script.AddInput(c);
            }
            script.run();
            Console.WriteLine(script.output.Last());
        }

        static long nextInDir(AdventMap map, int x, int y, int dir)
        {
            int nX = x, nY = y;
            switch (dir)
            {
                case 0: nY--; break;
                case 1: nX++; break;
                case 2: nY++; break;
                case 3: nX--; break;
            }
            var next = map.get(nX, nY);
            return next == null ? 0 : next.val;
        }

        static void repeat()
        {
            int[] bins = new int[650];
            int[] pat = { 0, 1, 0, -1 };
            for (int i = 0; i < bins.Length; i++)
            {
                int patIndex = 0;
                int subPatIndex = 1;
                bins[i] = 0;
                for (int j = 0; j < bins.Length * 10000; j++)
                {
                    if (subPatIndex == i + 1)
                    {
                        subPatIndex = 0;
                        patIndex = (patIndex + 1) % 4;
                    }
                    bins[i] += pat[patIndex];
                    subPatIndex++;
                }
                //next[i] = Math.Abs(next[i]) % 10;
            }
            Console.WriteLine("phew");
        }

        static void d16()
        {
            string input = ReadInput("d16")[0];
            var code = input.ToCharArray().Select(c => (c - '0')).ToArray();

            var len = code.Length;
            var realLen = len * 10000;

            int offset = int.Parse(input.Substring(0, 7));

            int[] fake1 = new int[realLen - offset];
            int[] fake2 = new int[fake1.Length];
            for (int i = 0; i < fake1.Length; i++)
            {
                fake1[i] = code[(i + offset) % code.Length];
            }
            for (int i = 0; i < 100; i++)
            {
                int total = fake1.Sum();
                for (int j = 0; j < fake1.Length; j++)
                {
                    fake2[j] = total % 10;
                    total -= fake1[j];
                }
                fake2.CopyTo(fake1, 0);
            }
            for (int i = 0; i < 8; i++)
            {
                Console.Write(fake1[i]);
            }
        }

        /*class Point : IComparable
        {
            public long x, y;
            public long val;

            public Point(long X, long Y, long V)
            {
                x = X;
                y = Y;
                val = V;
            }

            public int CompareTo(object obj)
            {
                if (obj == null) return 1;

                Point p = obj as Point;
                if (p == null) return 1;

                if (p.x == x && p.y == y) return 0;
                else if (p.x != x) return p.x > x ? 1 : -1;
                else return p.y > y ? 1 : -1;
            }
        }*/

        class AdventMap
        {
            public Dictionary<Point, Piece> points;

            int xMin, xMax, yMin, yMax;

            public AdventMap()
            {
                points = new Dictionary<Point, Piece>();

                xMin = 0;
                xMax = 0;
                yMin = 0;
                yMax = 0;
            }

            public class Piece
            {
                public char val;
                public Piece(char _v)
                {
                    val = _v;
                }
            }

            public Piece get(int x, int y)
            {
                var p = new Point(x, y);
                return points.ContainsKey(p) ? points[p] : null;
            }

            public void set(int x, int y, char val)
            {
                var p = new Point(x, y);
                if (points.ContainsKey(p))
                {
                    points[p].val = val;
                }
                else
                {
                    points.Add(p, new Piece(val));
                }

                xMax = Math.Max(x, xMax);
                xMin = Math.Min(x, xMin);
                yMax = Math.Max(y, yMax);
                yMin = Math.Min(y, yMin);
            }

            public void show()
            {
                //Console.Clear();
                Console.WriteLine();
                for (int y = yMin; y <= yMax; y++)
                {
                    for (int x = xMin; x <= xMax; x++)
                    {
                        var n = get(x, y);
                        char c = (n == null ? '?' : n.val);
                        Console.Write(c);
                    }
                    Console.WriteLine();
                }
            }

            public class ANode : IComparable
            {
                public int x, y;
                public int g;
                public int f;
                public ANode back;

                public ANode(int _x, int _y)
                {
                    x = _x;
                    y = _y;

                    g = int.MaxValue;
                    f = int.MaxValue;
                }

                public int CompareTo(object obj)
                {
                    if (obj == null) return 1;

                    ANode n = obj as ANode;
                    if (n == null) return 1;

                    if (n.x == x && n.y == y) return 0;
                    else if (n.x != x) return n.x > x ? 1 : -1;
                    else return n.y > y ? 1 : -1;
                }
            }

            private int heur(int n1x, int n1y, int n2x, int n2y)
            {
                return (Math.Abs(n1x - n2x) + Math.Abs(n1y - n2y));
            }

            public List<ANode> findShortestPath(int fromX, int fromY, int toX, int toY, List<char> walls)
            {
                var path = new List<ANode>();
                var open = new List<ANode>();
                var visited = new List<ANode>();
                foreach (var p in points.Where(pp => !walls.Contains(pp.Value.val)))
                {
                    open.Add(new ANode(p.Key.X, p.Key.Y));
                }
                open.First(p => p.x == fromX && p.y == fromY).g = 0;
                open.First(p => p.x == fromX && p.y == fromY).f = heur(fromX, fromY, toX, toY);
                while (true)
                {
                    if (open.Count == 0) return null;
                    long min = open.Min(s => s.f);
                    var c = open.First(p => p.f == min);

                    open.Remove(c);
                    visited.Add(c);
                    if (c.x == toX && c.y == toY) break;

                    ANode n = null;
                    var neighbors = new List<ANode>()
                    {
                        open.FirstOrDefault(p => p.x == c.x + 1 && p.y == c.y),
                        open.FirstOrDefault(p => p.x == c.x - 1 && p.y == c.y),
                        open.FirstOrDefault(p => p.x == c.x && p.y == c.y + 1),
                        open.FirstOrDefault(p => p.x == c.x && p.y == c.y - 1)
                    };
                    foreach (var neighbor in neighbors.Where(n => n != null))
                    {
                        if (c.g + 1 < neighbor.g)
                        {
                            neighbor.g = c.g + 1;
                            neighbor.f = neighbor.g + heur(neighbor.x, neighbor.y, toX, toY);
                            neighbor.back = c;
                        }
                    }
                }
                var nn = visited.First(p => p.x == toX && p.y == toY);

                while (nn.x != fromX || nn.y != fromY)
                {
                    path.Insert(0, nn);
                    nn = nn.back;
                }
                path.Insert(0, nn);

                return path;
            }
        }

        class Node : IComparable
        {
            public long x, y;
            public long val;
            public Node back;

            public Node(long _x, long _y, long _val)
            {
                x = _x;
                y = _y;
                val = _val;
            }

            public int CompareTo(object obj)
            {
                if (obj == null) return 1;

                Node n = obj as Node;
                if (n == null) return 1;

                if (n.x == x && n.y == y) return 0;
                else if (n.x != x) return n.x > x ? 1 : -1;
                else return n.y > y ? 1 : -1;
            }
        }
        /*
        static void d15()
        {
            var code = ReadInput("d15")[0].Split(',').Select(long.Parse).ToArray();
            var script = new ICScript(code);

            var map = new AdventMap();
            explore(map, script, 0, 0, 0);
            map.set(0, 0, 1);
            var p = map.points.First(p => p.val == 2);
            //map.show();

            //long res = findDist(map, p.x, p.y);
            int time = 0;
            while (map.points.Count(s => s.val == 1) > 0)
            {
                spreadGas(map);
                //map.show(0, 0);
                time++;
            }
            Console.WriteLine(time);
        }

        static void spreadGas(AdventMap map)
        {
            foreach (var p in map.points.Where(t => t.val == 2))
            {
                Point n = null;
                n = map.points.FirstOrDefault(s => s.x == p.x + 1 && s.y == p.y);
                if (n != null && n.val == 1) n.val = 10;

                n = map.points.FirstOrDefault(s => s.x == p.x - 1 && s.y == p.y);
                if (n != null && n.val == 1) n.val = 10;

                n = map.points.FirstOrDefault(s => s.x == p.x && s.y == p.y + 1);
                if (n != null && n.val == 1) n.val = 10;

                n = map.points.FirstOrDefault(s => s.x == p.x && s.y == p.y - 1);
                if (n != null && n.val == 1) n.val = 10;
            }

            foreach (var p in map.points.Where(t => t.val == 10))
            {
                p.val = 2;
            }
        }

        static long findDist(AdventMap map, long x, long y)
        {
            List<Node> open = new List<Node>();
            List<Node> visited = new List<Node>();
            foreach (var p in map.points.Where(pp => pp.val != 0))
            {
                open.Add(new Node(p.x, p.y, 10000));
            }
            open.First(p => p.x == 0 && p.y == 0).val = 0;
            while (true)
            {
                long min = open.Min(s => s.val);
                var c = open.First(p => p.val == min);
                open.Remove(c);
                visited.Add(c);
                if (c.x == x && c.y == y) break;
                Node n = null;
                if ((n = open.FirstOrDefault(p => p.x == c.x + 1 && p.y == c.y)) != null)
                {
                    if (c.val + 1 < n.val)
                    {
                        n.val = c.val + 1;
                        n.back = c;
                    }
                }
                if ((n = open.FirstOrDefault(p => p.x == c.x - 1 && p.y == c.y)) != null)
                {
                    if (c.val + 1 < n.val)
                    {
                        n.val = c.val + 1;
                        n.back = c;
                    }
                }
                if ((n = open.FirstOrDefault(p => p.x == c.x && p.y == c.y + 1)) != null)
                {
                    if (c.val + 1 < n.val)
                    {
                        n.val = c.val + 1;
                        n.back = c;
                    }
                }
                if ((n = open.FirstOrDefault(p => p.x == c.x && p.y == c.y - 1)) != null)
                {
                    if (c.val + 1 < n.val)
                    {
                        n.val = c.val + 1;
                        n.back = c;
                    }
                }
            }
            var nn = visited.First(p => p.x == x && p.y == y);

            while (nn.x != 0 || nn.y != 0)
            {
                nn = nn.back;
                map.set(nn.x, nn.y, 3);
            }
            return visited.First(p => p.x == x && p.y == y).val;
        }

        static void explore(AdventMap map, ICScript script, long x, long y, int dir)
        {
            if (map.get(x, y) == null)
            {
                if (dir != 0)
                {
                    script.AddInput(dir);
                    script.run();
                    long output = script.output.Last();
                    map.set(x, y, output);
                    //map.show(x, y);
                    if (output != 1)
                    {
                        if (output == 2)
                        {
                            script.AddInput((((dir - 1) ^ 1) + 1));
                            script.run();
                        }
                        return;
                    }
                }

                explore(map, script, x + 1, y, 4);
                explore(map, script, x - 1, y, 3);
                explore(map, script, x, y + 1, 2);
                explore(map, script, x, y - 1, 1);

                if (dir != 0)
                {
                    script.AddInput((((dir - 1) ^ 1) + 1));
                    script.run();
                }
            }
        }
        */
        /*static List<Node> findUnknown(AdventMap map, long x, long y)
        {
            var open = new List<Node>();
            var visited = new List<Node>();
            Node start = new Node(x, y, 0);
            open.Add(start);
            long minDist = 0;
            Node current = null;
            while (true)
            {
                current = null;
                for(long i = minDist; current == null; i++)
                {
                    current = open.FirstOrDefault(n => n.val == i);
                }
                minDist = current.val;
                open.Remove(current);
                visited.Add(current);

                //If open contains neighbor
                //If closed contains neighbor
                //If map contains neighbor
                    //If neighbor is wall
                    //If neighbor is open
                    //If neighbor is unknown

                if (map.get(current.x + 1, current.y) == null) break;
                if (map.get(current.x + 1, current.y).val == 0 && 


            }

            return map;
        }

        static void expand(AdventMap map, long x, long y)
        {

        }*/

        class Reaction
        {
            public List<Chem> chems;
            public Chem result;

            public Reaction()
            {
                chems = new List<Chem>();
            }

            public class Chem
            {
                public int n;
                public string name;

                public Chem(string text)
                {
                    var split = text.Trim().Split(' ');
                    n = int.Parse(split[0]);
                    name = split[1];
                }
            }
        }

        static void d14()
        {
            var lines = ReadInput("d14");
            var reactions = new List<Reaction>();
            foreach (var s in lines)
            {
                var r = new Reaction();
                var k = s.Split(" => ");
                var t = k[0].Split(',');
                r.result = new Reaction.Chem(k[1]);
                foreach (var m in t)
                {
                    r.chems.Add(new Reaction.Chem(m));
                }
                reactions.Add(r);
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            long usedOre = 0;
            long oreAvailable = 1000000000000;
            long producedFuel = 0;

            long oneFuel = produce(1, reactions);
            long lower = oreAvailable / oneFuel;
            long upper = lower * 2;
            while (upper - lower > 1)
            {
                long guess = (upper + lower) / 2;
                long f = produce(guess, reactions);
                if (f > oreAvailable)
                {
                    upper = guess;
                }
                else
                {
                    lower = guess;
                }
            }
            Console.WriteLine(lower + " " + produce(lower, reactions) + " " + upper + " " + produce(upper, reactions));
            Console.WriteLine(sw.ElapsedMilliseconds + " ms");
        }

        static long produce(long n, List<Reaction> reactions)
        {
            var inv = new Dictionary<string, long>();
            var wanted = new Dictionary<string, long>();
            foreach (var r in reactions)
            {
                inv.Add(r.result.name, 0);
                wanted.Add(r.result.name, 0);
            }
            inv.Add("ORE", 0);
            wanted.Add("ORE", 0);

            wanted["FUEL"] = n;
            while (wanted.Any(i => i.Key != "ORE" && i.Value != 0))
            {
                var chem = wanted.First(i => i.Key != "ORE" && i.Value != 0);
                var reaction = reactions.First(r => r.result.name == chem.Key);
                long neededReactions = chem.Value / reaction.result.n + (chem.Value % reaction.result.n == 0 ? 0 : 1);
                inv[chem.Key] += reaction.result.n * neededReactions - wanted[chem.Key];
                wanted[chem.Key] = 0;
                foreach (var c in reaction.chems)
                {
                    wanted[c.name] += c.n * neededReactions;
                    if (inv[c.name] <= wanted[c.name])
                    {
                        wanted[c.name] -= inv[c.name];
                        inv[c.name] = 0;
                    }
                    else
                    {
                        inv[c.name] -= wanted[c.name];
                        wanted[c.name] = 0;
                    }

                }

            }
            return wanted["ORE"];

        }

        static void d13()
        {
            var code = ReadInput("d13")[0].Split(',').Select(long.Parse).ToArray();
            code[0] = 2;
            int latestOutput = 0;
            var script = new ICScript(code);
            int score = 0;
            while (!script.halted)
            {
                var o = script.output;
                script.run();
                int ballX = 0, ballY = 0, paddleX = 0;
                int paddleCount = 0;
                for (int i = latestOutput; i < o.Count(); i += 3)
                {
                    int x = (int)o[i];
                    int y = (int)o[i + 1];
                    int val = (int)o[i + 2];

                    if (x == -1 && y == 0)
                    {
                        score = val;
                        Console.WriteLine("Score: " + score);
                    }
                    else if (val == 3)
                    {
                        paddleX = x;
                        paddleCount++;
                    }
                    else if (val == 4)
                    {
                        ballX = x;
                        ballY = y;
                    }
                }
                if (ballX < paddleX) script.AddInput(-1);
                if (ballX == paddleX) script.AddInput(0);
                if (ballX > paddleX) script.AddInput(1);

                latestOutput = o.Count;
            }

        }

        class Moon
        {
            public int x, y, z;
            public int dx, dy, dz;

            public Moon()
            {
                x = 0;
                y = 0;
                z = 0;
                dx = 0;
                dy = 0;
                dz = 0;
            }
        }

        static void d12()
        {
            var lines = ReadInput("d12");
            var m = new List<Moon>();
            string pat = "<x=(.*), y=(.*), z=(.*)>";

            foreach (var s in lines)
            {
                var r = new Regex(pat);
                var match = r.Match(s);
                if (match.Success)
                {
                    var moon = new Moon();

                    moon.x = int.Parse(match.Groups[1].Captures[0].ToString());
                    moon.y = int.Parse(match.Groups[2].Captures[0].ToString());
                    moon.z = int.Parse(match.Groups[3].Captures[0].ToString());

                    m.Add(moon);
                }
            }

            var xState = new[] { m[0].x, m[1].x, m[2].x, m[0].dx, m[1].dx, m[2].dx };
            var yState = new[] { m[0].y, m[1].y, m[2].y, m[0].dy, m[1].dy, m[2].dy };
            var zState = new[] { m[0].z, m[1].z, m[2].z, m[0].dz, m[1].dz, m[2].dz };

            int xP = 0;
            int yP = 0;
            int zP = 0;
            for (int t = 1; xP == 0 || yP == 0 || zP == 0; t++)
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = i + 1; j < 4; j++)
                    {
                        int ddx = Math.Sign(m[j].x - m[i].x);
                        m[i].dx += ddx;
                        m[j].dx -= ddx;

                        int ddy = Math.Sign(m[j].y - m[i].y);
                        m[i].dy += ddy;
                        m[j].dy -= ddy;

                        int ddz = Math.Sign(m[j].z - m[i].z);
                        m[i].dz += ddz;
                        m[j].dz -= ddz;
                    }
                }

                foreach (var moon in m)
                {
                    moon.x += moon.dx;
                    moon.y += moon.dy;
                    moon.z += moon.dz;
                }

                var newXState = new int[] { m[0].x, m[1].x, m[2].x, m[0].dx, m[1].dx, m[2].dx };
                var newYState = new int[] { m[0].y, m[1].y, m[2].y, m[0].dy, m[1].dy, m[2].dy };
                var newZState = new int[] { m[0].z, m[1].z, m[2].z, m[0].dz, m[1].dz, m[2].dz };

                if (xP == 0 && newXState.SequenceEqual(xState)) xP = t;
                if (yP == 0 && newYState.SequenceEqual(yState)) yP = t;
                if (zP == 0 && newZState.SequenceEqual(zState)) zP = t;
            }
            Console.WriteLine(LCM(new long[] { xP, yP, zP }));
        }

        static long LCM(long[] numbers)
        {
            return numbers.Aggregate(lcm);
        }
        static long lcm(long a, long b)
        {
            return Math.Abs(a * b) / GCD(a, b);
        }
        static long GCD(long a, long b)
        {
            return b == 0 ? a : GCD(b, a % b);
        }

        static int kE(List<Moon> moons)
        {
            int kE = 0;

            foreach (var m in moons)
            {
                kE += (Math.Abs(m.x) + Math.Abs(m.y) + Math.Abs(m.z)) * (Math.Abs(m.dx) + Math.Abs(m.dy) + Math.Abs(m.dz));
            }

            return kE;
        }

        static void d11()
        {
            var code = ReadInput("d11")[0].Split(',').Select(long.Parse).ToArray();
            var map = new Dictionary<KeyValuePair<int, int>, int>();
            int direction = 0; //0 up, 1 right, 2 down, 3 left
            int x = 0;
            int y = 0;

            var testMap = new Dictionary<KeyValuePair<int, int>, int>();
            testMap.Add(new KeyValuePair<int, int>(23, 46), 99);
            if (testMap[new KeyValuePair<int, int>(23, 46)] != 99) throw new Exception();
            var script = new ICScript(code);
            bool first = true;
            while (!script.halted)
            {
                var pos = new KeyValuePair<int, int>(x, y);

                int currentColor = map.ContainsKey(pos) ? map[pos] : 0;
                script.AddInput(first ? 1 : currentColor);
                script.run();
                int newColor = (int)script.output[script.output.Count() - 2];
                if (newColor > 1 || newColor < 0) throw new Exception();
                int dir = (int)script.output[script.output.Count() - 1];
                if (dir > 1 || dir < 0) throw new Exception();

                map[pos] = newColor;
                direction = (direction + 4 + (dir == 1 ? 1 : -1)) % 4;
                switch (direction)
                {
                    case 0: y--; break;
                    case 1: x++; break;
                    case 2: y++; break;
                    case 3: x--; break;
                }
            }

            int xMax = int.MinValue;
            int xMin = int.MaxValue;
            int yMax = int.MinValue;
            int yMin = int.MaxValue;

            foreach (var k in map)
            {
                xMax = Math.Max(xMax, k.Key.Key);
                xMin = Math.Min(xMin, k.Key.Key);
                yMax = Math.Max(yMax, k.Key.Value);
                yMin = Math.Min(yMin, k.Key.Value);
            }
            for (y = yMin; y <= yMax; y++)
            {
                for (x = xMin; x < xMax; x++)
                {
                    if (!map.ContainsKey(new KeyValuePair<int, int>(x, y))) Console.Write(" ");
                    else Console.Write(map[new KeyValuePair<int, int>(x, y)] == 1 ? 'X' : ' ');
                }
                Console.WriteLine();
            }
        }

        private static int GCD(int a, int b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }
            return a == 0 ? b : a;
        }

        static bool sees(bool[,] grid, int x1, int y1, int x2, int y2)
        {
            if (x1 == x2 && y1 == y2) return false;
            int dx = x2 - x1;
            int dy = y2 - y1;
            int gcd = GCD(dx, dy);

            if (gcd == 1) return true;
            dx /= gcd;
            dy /= gcd;

            int x = x1 + dx;
            int y = y1 + dy;
            for (; x != x2 || y != y2; x += dx, y += dy)
            {
                if (grid[x, y]) return false;
            }

            return true;
        }

        static void d10()
        {
            var lines = ReadInput("d10").Select(s => s.ToCharArray()).ToArray();
            int height = lines.Length;
            int width = lines[0].Length;
            var grid = new bool[width, height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    grid[x, y] = lines[y][x] == '#';


            int best = 0;
            int bestX = 0;
            int bestY = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!grid[x, y]) continue;
                    int count = 0;
                    for (int xx = 0; xx < width; xx++)
                    {
                        for (int yy = 0; yy < height; yy++)
                        {
                            if (!grid[xx, yy]) continue;
                            count += sees(grid, x, y, xx, yy) ? 1 : 0;
                        }
                    }
                    if (count > best)
                    {
                        best = count;
                        bestX = x;
                        bestY = y;
                    }
                }
            }

            int zapped = 0;
            double currentAngle = 3 * Math.PI / 2 - 0.0000001;
            while (zapped < 200)
            {
                var cand = new List<int[]>();
                for (int xx = 0; xx < width; xx++)
                {
                    for (int yy = 0; yy < height; yy++)
                    {
                        if (!grid[xx, yy]) continue;
                        if (sees(grid, bestX, bestY, xx, yy)) cand.Add(new[] { xx, yy });
                    }
                }

                double nextAngle = float.MaxValue;
                int nextInd = -1;

                for (int i = 0; i < cand.Count; i++)
                {
                    var ast = cand[i];
                    var dx = ast[0] - bestX;
                    var dy = ast[1] - bestY;
                    double angle = Math.Atan2(dy, dx);
                    while (angle < currentAngle) angle += 2 * Math.PI;

                    if (angle < nextAngle && angle - currentAngle > 0)
                    {
                        nextAngle = angle;
                        nextInd = i;
                    }
                }
                currentAngle = nextAngle;
                int nextX = cand[nextInd][0];
                int nextY = cand[nextInd][1];
                grid[nextX, nextY] = false;
                zapped++;
                Console.WriteLine("Zapped {0} at angle {1}", "{" + nextX + ", " + nextY + "}", nextAngle);
            }


            Console.WriteLine(best);
        }

        static void d9()
        {
            var program = ReadInput("d9")[0].Split(',').Select(long.Parse).ToArray();
            var script = new ICScript(program, new List<long> { 2 });
            //script = new ICScript(new long[] { 109, 1, 204, -1, 1001, 100, 1, 100, 1008, 100, 16, 101, 1006, 101, 0, 99 }, null);
            //script = new ICScript(new long[] { 1102, 34915192, 34915192, 7, 4, 7, 99, 0 }, null);
            script.run();
            foreach (var o in script.output)
                Console.WriteLine(o);
        }

        //static void d8()
        //{
        //    var line = ReadInput("d8")[0];
        //    string result = "";
        //    var msg = line.ToCharArray();
        //    for(int i = 0; i < 150; i++)
        //    {
        //        int j = i;
        //        while(j < line.Length && msg[j] == '2')
        //        {
        //            j += 150;
        //        }
        //        result += msg[j];
        //    }

        //    for (int i = 0; i < 150; i++)
        //    {
        //        if (i % 25 == 0) Console.WriteLine("");
        //        Console.Write(result[i] != '0' ? 'X' : ' ');
        //    }
        //}

        static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        class ICScript
        {
            static Dictionary<int, int> paramPerOp = new Dictionary<int, int>()
            {
                { 1, 3 },
                { 2, 3 },
                { 3, 1 },
                { 4, 1 },
                { 5, 2 },
                { 6, 2 },
                { 7, 3 },
                { 8, 3 },
                { 9, 1 },
                { 99, 0 }
            };

            long ptr;
            long relPtr;
            int inputPtr;
            public List<long> input;
            public List<long> output;

            Dictionary<long, long> code;

            public bool doLog = false;
            StringBuilder logS = new StringBuilder();

            public bool halted;

            public ICScript(long[] program) :
                this(program, new List<long>())
            {

            }

            public ICScript(string program) :
                this(program.Split(',').Select(long.Parse).ToArray())
            {

            }

            public ICScript(long[] program, List<long> inp)
            {
                ptr = 0;
                relPtr = 0;
                inputPtr = 0;

                input = inp;
                output = new List<long>();

                code = new Dictionary<long, long>();
                for (int i = 0; i < program.Length; i++) code[i] = program[i];

                halted = false;
            }

            long getAddr(long val, int mode)
            {
                switch (mode)
                {
                    case 0: return readReg(val);
                    case 1: return val;
                    case 2: return readReg(val) + relPtr;
                    default: throw new Exception();
                }
            }

            public string printCode()
            {
                string res = "";
                foreach(var i in code.Keys)
                {
                    string value = code[i] < 256 ? ""+(char)code[i] : code[i] + "";
                    res+= $"{i}: {value}\n";
                }
                return res;
            }

            long readReg(long add)
            {
                if (!code.ContainsKey(add)) code.Add(add, 0);
                return code[add];
            }

            public int unprocessedInput()
            {
                return input.Count() - inputPtr;
            }

            void log(string s)
            {
                if (doLog)
                {
                    logS.Append(ptr + ": " + s+"\n");
                }
            }

            public string readLog()
            {
                return logS.ToString();
            }

            public void run()
            {
                var c = code;
                while (true)
                {
                    long ins = c[ptr];
                    bool stepInstruction = true;
                    int opCode = (int)(ins % 100);
                    var addr = new long[paramPerOp[opCode]];
                    int modifier = 100;
                    for (int i = 0; i < addr.Length; i++)
                    {
                        int mode = (int)((ins / modifier) % 10);
                        addr[i] = getAddr(ptr + i + 1, mode);
                        modifier *= 10;
                    }
                    switch (opCode)
                    {
                        case 1: //Add
                            log($"ADD value {readReg(addr[0])}, address ({addr[0]}) and value {readReg(addr[1])}, address ({addr[1]})");
                            c[addr[2]] = readReg(addr[0]) + readReg(addr[1]);
                            break;
                        case 2: //Mul
                            log($"MUL value {readReg(addr[0])}, address ({addr[0]}) and value {readReg(addr[1])}, address ({addr[1]})");
                            c[addr[2]] = readReg(addr[0]) * readReg(addr[1]);
                            break;
                        case 3: //Read input
                            if (inputPtr >= input.Count()) return; //Await input
                            log($"READ INPUT '{(char)readReg(addr[0])}'");
                            c[addr[0]] = input[inputPtr];
                            inputPtr++;
                            break;
                        case 4: //Write output
                            output.Add(readReg(addr[0]));
                            log($"WRITE OUTPUT '{(char)readReg(addr[0])}'");
                            break;
                        case 5: //Jump if true
                            log($"JUMP IF TRUE {readReg(addr[0])}");
                            if (readReg(addr[0]) != 0)
                            {
                                log($"JUMPING TO {readReg(addr[1])}");
                                ptr = readReg(addr[1]);
                                stepInstruction = false;
                            }
                            break;
                        case 6: //Jump if false
                            log($"JUMP IF FALSE {readReg(addr[0])}");
                            if (readReg(addr[0]) == 0)
                            {
                                log($"JUMPING TO {readReg(addr[1])}");
                                ptr = readReg(addr[1]);
                                stepInstruction = false;
                            }
                            break;
                        case 7: //less than
                            c[addr[2]] = (readReg(addr[0]) < readReg(addr[1])) ? 1 : 0;
                            log($"LESS THAN value {readReg(addr[0])}, address ({addr[0]}) and value {readReg(addr[1])}, address ({addr[1]})");
                            break;
                        case 8: // equal
                            c[addr[2]] = (readReg(addr[0]) == readReg(addr[1])) ? 1 : 0;
                            log($"EQUAL value {readReg(addr[0])}, address ({addr[0]}) and value {readReg(addr[1])}, address ({addr[1]})");
                            break;
                        case 9: //jmp
                            relPtr += readReg(addr[0]);
                            log($"ADJUST RELATIVE BASE to {relPtr}");
                            break;
                        case 99: //exit
                            halted = true;
                            return;
                        default:
                            throw new Exception("Runtime error at " + ptr);
                    }
                    if (stepInstruction)
                        ptr += paramPerOp[opCode] + 1;

                }
            }

            public void AddInput(string k)
            {
                foreach (var c in k) AddInput(c);
            }

            public void AddInput(int i)
            {
                input.Add(i);
            }

            public void AddInput(long i)
            {
                input.Add(i);
            }
        }

        //static void d7()
        //{
        //    var code = ReadInput("d7")[0].Split(',').Select(int.Parse).ToArray();
        //    var p = GetPermutations(Enumerable.Range(5, 5), 5);
        //    int maxOutput = 0;
        //    foreach(var sequence in p)
        //    {
        //        var scripts = new List<ICScript>();

        //        var seqList = sequence.ToList();
        //        for(int i = 0; i < 5; i++)
        //        {
        //            scripts.Add(new ICScript(code, new List<int> { seqList[i] }));
        //        }
        //        scripts[0].AddInput(0);

        //        int currentScript = 0;
        //        while (!scripts.Last().halted) {
        //            scripts[currentScript].run();
        //            int nextScript = (currentScript + 1) % 5;
        //            scripts[nextScript].AddInput(scripts[currentScript].output.Last());
        //            currentScript = nextScript;
        //        }
        //        maxOutput = Math.Max(scripts.Last().output.Last(), maxOutput);
        //        Console.WriteLine("This: " + scripts.Last().output.Last());
        //        Console.WriteLine("Best: " + maxOutput);
        //    }
        //}

        //class Orbit
        //{
        //    public string name;
        //    public Orbit parent = null;
        //}

        //static void d6()
        //{
        //    var orbits = new List<Orbit>();

        //    var orbitS = ReadInput("d6").Select(s => s.Split(')')).ToArray();
        //    foreach (var s in orbitS)
        //    {
        //        Orbit orbit1 = orbits.FirstOrDefault(o => o.name == s[0]);
        //        if (orbit1 == null)
        //        {
        //            orbit1 = new Orbit();
        //            orbit1.name = s[0];
        //            orbits.Add(orbit1);
        //        }

        //        Orbit orbit2 = orbits.FirstOrDefault(o => o.name == s[1]);
        //        if (orbit2 == null)
        //        {
        //            orbit2 = new Orbit();
        //            orbit2.name = s[1];
        //            orbits.Add(orbit2);
        //        }

        //        orbit2.parent = orbit1;
        //    }

        //    int res = 0;
        //    foreach (var o in orbits)
        //    {
        //        Orbit orb = o;
        //        while (orb.parent != null)
        //        {
        //            res++;
        //            orb = orb.parent;
        //        }
        //    }

        //    var sanOrbits = new List<Orbit>();
        //    Orbit sanOrb = orbits.First(s => s.name == "SAN");
        //    Orbit youOrb = orbits.First(s => s.name == "YOU");
        //    while (sanOrb.parent != null)
        //    {
        //        Console.Write(sanOrb.parent.name + "->");
        //        sanOrbits.Add(sanOrb.parent);
        //        sanOrb = sanOrb.parent;
        //    }
        //    int res2 = 0;
        //    Console.WriteLine();
        //    Console.WriteLine();
        //    while (youOrb.parent != null)
        //    {
        //        youOrb = youOrb.parent;
        //        Console.Write(youOrb.name + "->");
        //        if (sanOrbits.Contains(youOrb))
        //        {
        //            Console.WriteLine("Meeting at " + youOrb.name);
        //            res2 += sanOrbits.IndexOf(youOrb);
        //            break;
        //        }
        //        res2++;
        //    }

        //    Console.WriteLine(res);
        //    Console.WriteLine(res2);
        //}

        //static void d5()
        //{
        //    var codes = ReadInput("d5")[0].Split(',').Select(int.Parse).ToArray();
        //    runIntCode(codes, null);
        //}

        //static void d4()
        //{
        //    int start = 156218;
        //    int stop = 652527;

        //    int found = 0;

        //    for (int i = start; i <= stop; i++)
        //    {
        //        char[] digits = i.ToString().ToCharArray();
        //        bool increasing = true;
        //        bool hasDouble = false;

        //        for (int j = 1; j < 6; j++)
        //        {
        //            if (digits[j] < digits[j - 1]) increasing = false;
        //            if (digits[j] == digits[j - 1] && digits.Count(c => c == digits[j]) == 2) hasDouble = true;
        //        }

        //        if (increasing && hasDouble) found++;
        //    }
        //    Console.WriteLine(found);
        //}

        //class Line
        //{
        //    public int x1, y1, x2, y2;
        //    public int steps; //Steps to reach start x1, y1
        //    public Line(int X1, int Y1, int X2, int Y2)
        //    {
        //        x1 = X1;
        //        y1 = Y1;
        //        x2 = X2;
        //        y2 = Y2;
        //        if ((X1 != X2 && Y1 != Y2) || (X1 == X2 && Y1 == Y2))
        //        {
        //            throw new Exception();
        //        }
        //    }

        //    public int[] Intersect(Line l)
        //    {
        //        if (x1 == x2)
        //        {
        //            if (l.x1 != l.x2 && Math.Min(l.x1, l.x2) <= x1 && Math.Max(l.x1, l.x2) >= x1
        //                && l.y1 >= Math.Min(y1, y2) && l.y1 <= Math.Max(y1, y2))
        //            {
        //                int intSteps = steps + l.steps + Math.Abs(l.x1 - x1) + Math.Abs(y1 - l.y1);

        //                return new[] { x1, l.y1, intSteps };
        //            }
        //        }
        //        else if (y1 == y2)
        //        {
        //            if (l.y1 != l.y2 && Math.Min(l.y1, l.y2) <= y1 && Math.Max(l.y1, l.y2) >= y1
        //                && l.x1 >= Math.Min(x1, x2) && l.x1 <= Math.Max(x1, x2))
        //            {
        //                int intSteps = steps + l.steps + Math.Abs(l.x1 - x1) + Math.Abs(l.y1 - y1);

        //                return new[] { l.x1, y1, intSteps };
        //            }
        //        }
        //        else
        //        {
        //            throw new Exception();
        //        }

        //        return null;
        //    }
        //}

        //static void d3()
        //{
        //    var lines = ReadInput("d3");
        //    //int[,] grid = new int[10000, 10000];
        //    Line[][] grid = new Line[][] { null, null };

        //    for (int i = 0; i < 2; i++)
        //    {
        //        var split = lines[i].Split(',');
        //        grid[i] = new Line[split.Length + 1];
        //        int x = 0;
        //        int y = 0;
        //        int j = 0;
        //        int steps = 0;
        //        foreach (var s in split)
        //        {
        //            int amount = int.Parse(s.Substring(1));
        //            j++;
        //            Line line = null;
        //            if (s.StartsWith("R"))
        //            {
        //                line = new Line(x, y, x + amount, y);
        //                x += amount;
        //            }
        //            if (s.StartsWith("L"))
        //            {
        //                line = new Line(x, y, x - amount, y);
        //                x -= amount;
        //            }
        //            if (s.StartsWith("U"))
        //            {
        //                line = new Line(x, y, x, y - amount);
        //                y -= amount;
        //            }
        //            if (s.StartsWith("D"))
        //            {
        //                line = new Line(x, y, x, y + amount);
        //                y += amount;
        //            }
        //            line.steps = steps;
        //            steps += amount;
        //            grid[i][j] = line;
        //        }
        //    }
        //    int closestIntersection = int.MaxValue;
        //    for (int i = 1; i < grid[0].Length; i++)
        //    {
        //        Line l1 = grid[0][i];
        //        for (int j = 1; j < grid[1].Length; j++)
        //        {
        //            Line l2 = grid[1][j];
        //            var intersect = l1.Intersect(l2);
        //            if (intersect != null)
        //            {
        //                int dist = intersect[2];
        //                if (dist > 0)
        //                {
        //                    closestIntersection = Math.Min(closestIntersection, dist);
        //                }
        //            }

        //        }
        //    }
        //    Console.WriteLine(closestIntersection);
        //}
        //static void d2()
        //{
        //    const int target = 19690720;

        //    for (int n = 0; n < 100; n++)
        //    {
        //        for (int v = 0; v < 100; v++)
        //        {
        //            Console.WriteLine("Trying {0},{1}", n, v);
        //            int[] codes = ReadInput("d2")[0].Split(',').Select(int.Parse).ToArray();

        //            codes[1] = n;
        //            codes[2] = v;

        //            runIntCode(codes, null);
        //            //Console.WriteLine(res);
        //            if (target == 0)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //}

        //static void d1()
        //{
        //    var mass = ReadInput("d1").Select(s => int.Parse(s));
        //    int sum = 0;
        //    foreach (var m in mass)
        //    {
        //        int curr = m;
        //        while (curr > 8)
        //        {
        //            curr = curr / 3 - 2;
        //            sum += curr;
        //        }
        //    }
        //    Console.WriteLine(sum);
        //}

        static string[] ReadInput(string fileName)
        {
            string path = @"C:\git\AdventOfCode\AdventOfCode2019\Input\" + fileName + ".txt";
            return File.ReadAllLines(path);
        }
    }
}
