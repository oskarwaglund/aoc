using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AdventOfCode2018
{
    class Y2018
    {
        static void Main(string[] args)
        {
            d20();
        }

        static void d20()
        {
            var regex = ReadInputAsString(20).Replace("^", "").Replace("$", "");
            //regex = "ENWWW(NEEE|SSE(EE|N))";
            var doors = new HashSet<long>(); //long coordinate refers to right/down from room with coord x, y
            var rooms = new HashSet<long>();
            //A state is a long (position) and int (index in regex)
            var q = new Queue<Tuple<long, int>>();
            q.Enqueue(new Tuple<long, int>(posToLong(10000, 10000), 0));
            var seen = new HashSet<Tuple<long, int>>();

            while(q.Count() > 0)
            {
                var state = q.Dequeue();
                if (seen.Contains(state))
                {
                    continue;
                }
                seen.Add(state);

                int index = state.Item2;
                longToPos(state.Item1, out int x, out int y);
                rooms.Add(state.Item1);
                for(int i = index; i < regex.Length; i++)
                {
                    char c = regex[i];
                    switch (c)
                    {
                        case 'N':
                            doors.Add(doorToLong(x, y - 1, true));
                            y--;
                            break;
                        case 'S':
                            doors.Add(doorToLong(x, y, true));
                            y++;
                            break;
                        case 'W':
                            doors.Add(doorToLong(x - 1, y, false));
                            x--;
                            break;
                        case 'E':
                            doors.Add(doorToLong(x, y, false));
                            x++;
                            break;
                        case '|':
                        case ')':
                            i = regex.Length;
                            break;
                        case '(':
                            //Find starts of next level
                            int level = 1;
                            long pos = posToLong(x, y);
                            q.Enqueue(new Tuple<long, int>(pos, i + 1));
                            for (int j = i+1; j < regex.Length && level > 0; j++)
                            {
                                c = regex[j];
                                
                                switch (c)
                                {
                                    case 'N':
                                    case 'S':
                                    case 'E':
                                    case 'W':
                                        break;
                                    case '(':
                                        level++;
                                        var newPos1 = new Tuple<long, int>(pos, j + 1);
                                        if (level == 1 && !seen.Contains(newPos1))
                                        {
                                            q.Enqueue(newPos1);
                                        }
                                        break;
                                    case '|':
                                        var newPos2 = new Tuple<long, int>(pos, j + 1);
                                        if (level == 1 && !seen.Contains(newPos2))
                                        {
                                            q.Enqueue(newPos2);
                                        }
                                        break;
                                    case ')':
                                        var newPos3 = new Tuple<long, int>(pos, j + 1);
                                        level--;
                                        if (level <= 1 && !seen.Contains(newPos3))
                                        {
                                            q.Enqueue(newPos3);
                                        }
                                        break;
                                }
                            }
                            i = regex.Length; break;
                    }
                    rooms.Add(posToLong(x, y));
                }
            }
            Console.WriteLine(rooms.Count());
            //print20(rooms, doors);
        }

        static void print20(HashSet<long> rooms, HashSet<long> doors)
        {
            int xMax = 0;
            int xMin = int.MaxValue;
            int yMax = 0;
            int yMin = int.MaxValue;

            foreach(var r in rooms)
            {
                longToPos(r, out int x, out int y);
                xMax = Math.Max(xMax, x);
                xMin = Math.Min(xMin, x);
                yMax = Math.Max(yMax, y);
                yMin = Math.Min(yMin, y);
            }

            Console.WriteLine(string.Join("", Enumerable.Repeat("#", (xMax-xMin+1)*2+1)));
            for (int y = yMin; y <= yMax; y++)
            {
                Console.Write("#");
                for (int x = xMin; x <= xMax; x++)
                {
                    Console.Write((x==10000 && y==10000) ? 'O' : rooms.Contains(posToLong(x, y)) ? '.' : '#');
                    Console.Write(doors.Contains(doorToLong(x, y, false)) ? '|' : '#');
                }
                Console.WriteLine();
                if (yMax == y)
                {
                    Console.WriteLine(string.Join("", Enumerable.Repeat("#", (xMax - xMin + 1) * 2 + 1)));
                }
                else
                {
                    Console.Write("#");
                    for (int x = xMin; x <= xMax; x++)
                    {
                        Console.Write(doors.Contains(doorToLong(x, y, true)) ? '-' : '#');
                        Console.Write('#');
                    }
                    Console.WriteLine();
                }
            }
        }

        static long posToLong(int x, int y)
        {
            return (((long)x) << 32) | (long)y;
        }

        static long doorToLong(int x, int y, bool down)
        {
            return  (down ? 1L<<60 : 0) | (((long)x) << 30) | (long)y;
        }

        static int longToX(long pos)
        {
            longToPos(pos, out int x, out _);
            return x;
        }

        static int longToY(long pos)
        {
            longToPos(pos, out _, out int y);
            return y;
        }

        static void longToPos(long pos, out int x, out int y)
        {
            y = (int)pos;
            x = (int)(pos >> 32);
        }

        static string[] ReadInput(int day)
        {
            string path = @"C:\git\AdventOfCode\AdventOfCode2018\Input\d" + day + ".txt";
            return File.ReadAllLines(path);
        }

        static string ReadInputAsString(int day)
        {
            string path = @"C:\git\AdventOfCode\AdventOfCode2018\Input\d" + day + ".txt";
            return File.ReadAllText(path);
        }
    }
}
