using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AdventOfCode2016
{
    class Y2016
    {
        static void Main(string[] args)
        {
            d25();
        }

        static void d25()
        {
            var code = ReadInput(25);
            for(int a = 0; a < 10000; a++)
            {
                var script = new AScript(code);
                script.Mem['a'] = a;
                script.Run();
                bool valid = true;
                for(int i = 1; i < script.output.Count(); i++)
                {
                    if (script.output[i] == script.output[i - 1])
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid)
                {
                    Console.WriteLine(a);
                    return;
                }
                else
                {
                    Console.WriteLine($"{a}: {(string.Join(" ", script.output.Take(20)))}");
                }
            }
        }

        static void d24()
        {
            var lines = ReadInput(24);
            var walls = new HashSet<(int x, int y)>();
            var goals = new Dictionary<int, (int x, int y)>();
            for (int y = 0; y < lines.Count(); y++)
            {
                for (int x = 0; x < lines[0].Length; x++)
                {
                    char c = lines[y][x];
                    if (c == '#') walls.Add((x, y));
                    else if (c != '.') goals.Add(c - '0', (x, y));
                }
            }

            Console.WriteLine("Found locations " + goals.Count());
            foreach(var k in goals.Keys)
            {
                Console.WriteLine(k);
            }

            //Find shortest dist between each pair
            var dist = new Dictionary<(int start, int stop), int>();
            for(int start = 0; start < goals.Count() - 1; start++)
            {
                for(int stop = start+1; stop < goals.Count(); stop++)
                {
                    dist.Add((start, stop), distance(goals[start], goals[stop], walls));
                }
            }

            //DP 
            var state = "0";
            Console.WriteLine(dp(state, dist));
        }

        static int dp(string state, Dictionary<(int start, int stop), int> dist)
        {
            int best = int.MaxValue;
            if(state.Length == 8)
            {
                int res = 0;
                for(int i = 1; i < state.Length; i++)
                {
                    int c0 = state[i - 1] - '0';
                    int c1 = state[i] - '0';
                    int start = Math.Min(c0, c1);
                    int stop = Math.Max(c0, c1);
                    res += dist[(start, stop)];
                }
                res += dist[(0, state.Last() - '0')];
                return res;
            }

            for(int i = 0; i < 8; i++)
            {
                string s = i.ToString();
                if (!state.Contains(s))
                {
                    best = Math.Min(best, dp(state + s, dist));
                }
            }
            return best;
        }

        static int distance((int x, int y) start, (int x, int y) stop, HashSet<(int x, int y)> walls)
        {
            int steps = 0;
            var found = new HashSet<(int x, int y)>();
            found.Add(start);
            for(;;steps++)
            {
                if (found.Contains(stop))
                {
                    return steps;
                }
                foreach(var (x, y) in found.ToArray())
                {
                    var neigh = new[]
                    {
                        (x+1, y),
                        (x-1, y),
                        (x, y+1),
                        (x, y-1)
                    }.Where(n => !walls.Contains(n) && !found.Contains(n));
                    foreach (var n in neigh) found.Add(n);
                }
            }
        }

        static void d23()
        {
            var input = ("cpy 2 a\n" +
                        "tgl a  \n" +
                        "tgl a  \n" +
                        "tgl a  \n" +
                        "cpy 1 a\n" +
                        "dec a  \n" +
                        "dec a").Split('\n');
            input = ReadInput(23);
            for (int i = 6; i < 13; i++)
            {
                Console.WriteLine(i + ":");
                var script = new AScript(input);
                script.Mem['a'] = i;
                script.Run();
            }
        }

        static void d12()
        {
            var input = ReadInput(12);
            var script = new AScript(input);
            script.Run();
            Console.WriteLine("Part 1: " + script.Mem['a']);
            script = new AScript(input);
            script.Mem['c'] = 1;
            script.Run();
            Console.WriteLine("Part 2: " + script.Mem['a']);
        }

        public class AScript
        {
            public Dictionary<char, int> Mem;
            int ptr;
            string[] lines;
            public List<int> output;
            public AScript(string[] code)
            {
                lines = code.Select(s => s.Trim()).ToArray();
                ptr = 0;
                output = new List<int>();
                Mem = new Dictionary<char, int>
                {
                    ['a'] = 0,
                    ['b'] = 0,
                    ['c'] = 0,
                    ['d'] = 0,
                };
            }

            static Dictionary<string, string> tglMap = new Dictionary<string, string>
            {
                //One arg
                ["inc"] = "dec",
                ["dec"] = "inc",
                ["tgl"] = "inc",

                //Two arg
                ["jnz"] = "cpy",
                ["cpy"] = "jnz",
            };

            public void Run()
            {
                while (ptr >= 0 && ptr < lines.Length)
                {
                    var s = lines[ptr];
                    var sp = s.Split();

                    int arg0, arg1 = 0;
                    if (!int.TryParse(sp[1], out arg0))
                        arg0 = Mem[sp[1][0]];
                    if (sp.Length > 2)
                    {
                        if (!int.TryParse(sp[2], out arg1))
                            arg1 = Mem[sp[2][0]];
                    }

                    switch (sp[0])
                    {
                        case "cpy":
                            Mem[sp[2][0]] = arg0;
                            break;
                        case "inc":
                            Mem[sp[1][0]]++;
                            break;
                        case "dec":
                            Mem[sp[1][0]]--;
                            break;
                        case "jnz":
                            ptr += arg0 == 0 ? 0 : arg1 - 1;
                            break;
                        case "tgl":
                            int i = ptr + arg0;
                            if (i >= 0 && i < lines.Length)
                            {
                                var tgl = lines[i];
                                lines[i] = tgl.Replace(tgl[..3], tglMap[tgl[..3]]);
                            }
                            break;
                        case "out":
                            output.Add(arg0);
                            if (output.Count() >= 200) return;
                            break;
                        default:
                            throw new NotImplementedException(sp[0] + " is not supported!");
                    }
                    ptr++;
                }
                foreach (var d in Mem)
                {
                    Console.WriteLine($"{d.Key}: {d.Value}");
                }
            }

            public override string ToString()
            {
                return $"ptr={ptr}: a={Mem['a']}, b={Mem['b']}, c={Mem['c']}, d={Mem['d']}";
            }
        }

        public class Disk
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int ID
            {
                get
                {
                    return toId(X, Y);
                }
            }
            public int Size { get; set; }
            public int Used { get; set; }
            public int Avail
            {
                get
                {
                    return Size - Used;
                }
            }
            public List<Disk> Adj;

            public Disk()
            {
                Adj = new List<Disk>();
            }

            public bool canMove(Disk d)
            {
                return Used != 0 && Used <= d.Avail;
            }

            public void moveTo(Disk d)
            {
                if (!canMove(d)) throw new Exception("Disk can not receive that much data!");
                d.Used += Used;
                Used = 0;
            }

            static int toId(int x, int y)
            {
                return y * 100 + x;
            }

            public Disk copy()
            {
                return new Disk
                {
                    X = X,
                    Y = Y,
                    Size = Size,
                    Used = Used
                };
            }

            public override string ToString()
            {
                return $"({Y}, {X}) Usage: {Used}/{Size}";
            }
        }

        static void d22()
        {
            string prefix = "/dev/grid/node-x";
            string sep = "-y";
            var diskList = ReadInput(22)
                .Skip(2)
                .Select(s => s.Replace(prefix, "").Replace(sep, " ").Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Select(sp => new Disk
                {
                    X = int.Parse(sp[0]),
                    Y = int.Parse(sp[1]),
                    Size = int.Parse(sp[2][..^1]),
                    Used = int.Parse(sp[3][..^1]),
                })
                .ToList();
            var diskMap = diskList.ToDictionary(k => k.ID);
            var diskConn = new List<Tuple<int, int>>();
            int valid = 0;
            int realValid = 0;
            int xMax = diskList.Max(c => c.X);
            int yMax = diskList.Max(c => c.Y);

            for (int i = 0; i < diskList.Count(); i++)
            {
                for (int j = 0; j < diskList.Count(); j++)
                {
                    if (i == j) continue;
                    var d1 = diskList[i];
                    var d2 = diskList[j];
                    var dx = Math.Abs(d1.X - d2.X);
                    var dy = Math.Abs(d1.Y - d2.Y);
                    if ((dx == 0 && dy == 1) || (dx == 1 && dy == 0))
                    {
                        d1.Adj.Add(d2);
                        if (d1.ID < d2.ID)
                        {
                            diskConn.Add(new Tuple<int, int>(d1.ID, d2.ID));
                        }
                        if (d1.canMove(d2))
                        {
                            realValid++;
                        }
                    }
                    if (d1.canMove(d2))
                    {
                        valid++;
                    }

                }
            }

            Console.WriteLine("Part 1: " + valid + " valid pairs");

            int maxU = diskList.Where(d => d.Size < 500).Max(d => d.Used);
            int minU = diskList.Where(d => d.Size < 500 && d.Used > 0).Min(d => d.Used);

            int maxS = diskList.Where(d => d.Size < 500).Max(d => d.Size);
            int minS = diskList.Where(d => d.Size < 500).Min(d => d.Size);

            var empty = diskList.First(d => d.Used == 0);

            Console.WriteLine($"Usage {minU}-{maxU}, size {minS}-{maxS}");

            int w = xMax + 1;
            int h = yMax + 1;
            (int dX, int dY, int eX, int eY) startState = (xMax, 0, empty.X, empty.Y);
            var stateMap = new Dictionary<(int, int, int, int), (int g, int f)>
            {
                [startState] = (0, hScore(startState, h))
            };
            var open = new HashSet<(int dX, int dY, int eX, int eY)>();
            open.Add(startState);
            while (open.Count() > 0)
            {
                var min = open.Min(s => stateMap[s].f);
                var state = open.First(s => stateMap[s].f == min);
                open.Remove(state);
                if (state.dX == 0 && state.dY == 0)
                {
                    Console.WriteLine("Found " + stateMap[state].g);
                    return;
                }

                var moves = FindAllMoves(state, w, h, diskList);
                foreach (var m in moves)
                {
                    var tg = stateMap[state].g + 1;
                    var ng = stateMap.ContainsKey(m) ? stateMap[m].g : int.MaxValue;
                    if (tg < ng)
                    {
                        stateMap[m] = (tg, tg + hScore(m, h));
                        open.Add(m);
                    }
                }
            }
            Console.WriteLine("Done!");
        }

        static int hScore((int dX, int dY, int eX, int eY) state, int h)
        {
            //return Math.Abs(state.dX - state.eX) + state.eX + Math.Abs(state.dY - state.eY) + state.eY; 
            return state.dX + state.dY;
        }

        static List<(int dX, int dY, int eX, int eY)> FindAllMoves((int dX, int dY, int eX, int eY) state, int w, int h, List<Disk> disks)
        {
            var result = new List<(int, int, int, int)>();
            int size = w * h;
            var neighbors = new List<(int x, int y)>
            {
                (state.eX-1, state.eY),
                (state.eX+1, state.eY),
                (state.eX, state.eY-1),
                (state.eX, state.eY+1),
            }
            .Where(s => s.x >= 0 && s.x < w && s.y >= 0 && s.y < h);
            foreach (var n in neighbors)
            {
                var disk = disks.First(d => d.X == n.x && d.Y == n.y);
                if (disk.Size > 100) continue;
                if (n.x == state.dX && n.y == state.dY)
                {
                    //We are moving the data!
                    result.Add((state.eX, state.eY, state.dX, state.dY));
                }
                else
                {
                    result.Add((state.dX, state.dY, n.x, n.y));
                }
            }
            return result;
        }

        static int PosToIndex(int x, int y, int h)
        {
            return x * h + y;
        }

        static (int x, int y) IndexToPos(int pos, int h)
        {
            return (pos / h, pos % h);
        }

        static void getInitState(List<Disk> disks, List<Tuple<int, int>> conn, out List<Disk> newDiskList, out Dictionary<int, Disk> newDiskMap)
        {
            newDiskList = disks.Select(d => d.copy()).ToList();
            newDiskMap = newDiskList.ToDictionary(k => k.ID);
            foreach (var c in conn)
            {
                newDiskMap[c.Item1].Adj.Add(newDiskMap[c.Item2]);
                newDiskMap[c.Item2].Adj.Add(newDiskMap[c.Item1]);
            }
        }

        static char[] rotateBy(char[] curr, int steps)
        {
            steps = (steps + curr.Count()) % curr.Count();
            return curr[^steps..].Concat(curr[..^steps]).ToArray();
        }

        static char[] rotateBy(char[] curr, char c)
        {
            int steps = Array.IndexOf(curr, c);
            steps += 1 + (steps >= 4 ? 1 : 0);
            steps %= curr.Count();
            return curr[^steps..].Concat(curr[..^steps]).ToArray();
        }

        static void d21_2()
        {
            var lines = ReadInput(21);
            var curr = "fbgdceah".ToArray();
            //var curr = "agcebfdh".ToArray();
            char[] next = null;
            for (int pp = lines.Count() - 1; pp >= 0; pp--)
            //foreach (var l in lines)
            {
                var l = lines[pp];
                var s = l.Split();
                if (l.Contains("swap letter"))
                {
                    char x = s[2][0];
                    char y = s[5][0];
                    next = curr.Select(c => c == x ? y : (c == y ? x : c)).ToArray();
                }
                else if (l.Contains("swap position"))
                {
                    int i1 = int.Parse(s[2]);
                    int i2 = int.Parse(s[5]);
                    next = curr.Select((c, i) => i == i1 ? curr[i2] : (i == i2 ? curr[i1] : c)).ToArray();
                }
                else if (Regex.IsMatch(l, "rotate (left|right)"))
                {
                    bool left = !l.Contains("left");
                    int steps = int.Parse(s[2]) * (left ? -1 : 1);
                    steps = (steps + curr.Count()) % curr.Count();
                    next = curr[^steps..].Concat(curr[..^steps]).ToArray();
                }
                else if (l.Contains("rotate"))
                {
                    char c = s[6][0];
                    for (int i = 0; i < curr.Length; i++)
                    {
                        var guess = rotateBy(curr, i);
                        if (Enumerable.SequenceEqual(rotateBy(guess, c), curr))
                        {
                            next = guess;
                            break;
                        }
                    }

                }
                else if (l.Contains("reverse"))
                {
                    int i1 = int.Parse(s[2]);
                    int i2 = int.Parse(s[4]);
                    next = Enumerable.Range(0, curr.Count()).Select(i => (i >= i1 && i <= i2) ? curr[i1 + i2 - i] : curr[i]).ToArray();
                }
                else
                {
                    //move
                    int i1 = int.Parse(s[2]);
                    int i2 = int.Parse(s[5]);
                    char c = curr[i2];
                    var mid = curr.Except(new[] { c }).ToArray();
                    next = mid[..i1].Concat(new[] { c }).Concat(mid[i1..]).ToArray();
                }
                curr = next;
                Console.WriteLine(string.Join("", curr));
            }
        }

        static void d21()
        {
            var lines = ReadInput(21);
            var curr = "abcdefgh".ToArray();
            //var curr = "abcde".ToArray();
            char[] next = null;
            foreach (var l in lines)
            {
                var s = l.Split();
                if (l.Contains("swap letter"))
                {
                    char x = s[2][0];
                    char y = s[5][0];
                    next = curr.Select(c => c == x ? y : (c == y ? x : c)).ToArray();
                }
                else if (l.Contains("swap position"))
                {
                    int i1 = int.Parse(s[2]);
                    int i2 = int.Parse(s[5]);
                    next = curr.Select((c, i) => i == i1 ? curr[i2] : (i == i2 ? curr[i1] : c)).ToArray();
                }
                else if (Regex.IsMatch(l, "rotate (left|right)"))
                {
                    bool left = l.Contains("left");
                    int steps = int.Parse(s[2]) * (left ? -1 : 1);
                    next = rotateBy(curr, steps);
                }
                else if (l.Contains("rotate"))
                {
                    next = rotateBy(curr, s[6][0]);
                }
                else if (l.Contains("reverse"))
                {
                    int i1 = int.Parse(s[2]);
                    int i2 = int.Parse(s[4]);
                    next = Enumerable.Range(0, curr.Count()).Select(i => (i >= i1 && i <= i2) ? curr[i1 + i2 - i] : curr[i]).ToArray();
                }
                else
                {
                    //move
                    int i1 = int.Parse(s[2]);
                    int i2 = int.Parse(s[5]);
                    char c = curr[i1];
                    var mid = curr.Except(new[] { c }).ToArray();
                    next = mid[..i2].Concat(new[] { c }).Concat(mid[i2..]).ToArray();
                }
                curr = next;
                Console.WriteLine(string.Join("", curr));
            }
        }

        static void d20()
        {
            var lines = ReadInput(20);
            var ranges = lines.Select(l => l.Split('-')).Select(s => new Tuple<long, long>(long.Parse(s[0]), long.Parse(s[1]))).ToList();
            ranges.Sort();
            long stop = 0;
            long start = 0;
            var ran = new List<Tuple<long, long>>();
            foreach (var r in ranges)
            {
                if (stop + 1 < r.Item1)
                {
                    ran.Add(new Tuple<long, long>(start, stop));
                    start = r.Item1;
                    stop = r.Item2;
                    //return;
                }
                stop = Math.Max(r.Item2, stop);
            }
            ran.Add(new Tuple<long, long>(start, stop));
            long allowed = 0;
            for (int i = 1; i < ran.Count(); i++)
            {
                allowed += ran[i].Item1 - ran[i - 1].Item2 - 1;
            }
            allowed += uint.MaxValue - ran[^1].Item2;
            Console.WriteLine(allowed);
        }

        public class Elf
        {
            public int pos { set; get; }
            public Elf next { set; get; }
        }

        static void d19()
        {
            int elves = 3001330;
            //elves = 5;
            int left = elves;

            Elf root = new Elf { pos = 1 };
            Elf curr = root;
            Elf opp = null;
            for (int i = 2; i <= elves; i++)
            {
                var e = new Elf { pos = i, next = null };

                curr.next = e;
                curr = e;
                if (i == elves / 2)
                {
                    opp = e;
                }
                if (i == elves)
                {
                    e.next = root;
                }
            }
            curr = opp;
            while (left > 1)
            {
                opp.next = opp.next.next;
                if (left % 2 == 1)
                    opp = opp.next;
                left--;
            }
            Console.WriteLine(opp.pos);
        }

        static void d18()
        {
            var lines = new List<string>();
            string inp = ".^^.^^^..^.^..^.^^.^^^^.^^.^^...^..^...^^^..^^...^..^^^^^^..^.^^^..^.^^^^.^^^.^...^^^.^^.^^^.^.^^.^.";
            //string inp = ".^^.^.^^^^";
            lines.Add(inp);
            for (int r = 1; r < 400000; r++)
            {
                var sb = new StringBuilder();
                for (int c = 0; c < inp.Length; c++)
                {
                    var a = lines[r - 1].Substring(c == 0 ? 0 : c - 1, (c == inp.Length - 1) || (c == 0) ? 2 : 3);
                    if (c == 0) a = '.' + a;
                    if (c == inp.Length - 1) a += '.';
                    bool trap = a == "^^." || a == ".^^" || a == "..^" || a == "^..";
                    sb.Append(trap ? '^' : '.');
                }
                lines.Add(sb.ToString());
            }

            int part1 = lines.Sum(s => s.Count(c => c == '.'));
            Console.WriteLine(part1);
        }

        static void d17()
        {

            var q = new Queue<long>();
            var steps = new Dictionary<long, int>();
            q.Enqueue(posToLong(1, 1));
            steps.Add(posToLong(1, 1), 0);

            //long goal = posToLong(7, 4);
            long goal = posToLong(31, 39);

            while (q.Count() > 0)
            {
                var pos = q.Dequeue();
                int stp = steps[pos];
                if (pos == goal)
                {
                    Console.WriteLine(stp);
                    break;
                }
                longToPos(pos, out int x, out int y);
                var u = posToLong(x, y - 1);
                var d = posToLong(x, y + 1);
                var l = posToLong(x - 1, y);
                var r = posToLong(x + 1, y);

                if (x > 0 && isOpen(x - 1, y) && !steps.ContainsKey(l)) { q.Enqueue(l); steps.Add(l, stp + 1); }
                if (y > 0 && isOpen(x, y - 1) && !steps.ContainsKey(u)) { q.Enqueue(u); steps.Add(u, stp + 1); }
                if (isOpen(x + 1, y) && !steps.ContainsKey(r)) { q.Enqueue(r); steps.Add(r, stp + 1); }
                if (isOpen(x, y + 1) && !steps.ContainsKey(d)) { q.Enqueue(d); steps.Add(d, stp + 1); }
            }
            Console.WriteLine(steps.Count(s => s.Value <= 50));
        }

        static Dictionary<long, bool> d18Map = new Dictionary<long, bool>();

        static bool isOpen(int x, int y)
        {
            long pos = posToLong(x, y);
            if (!d18Map.ContainsKey(pos))
                d18Map.Add(pos, !isWall(x, y));
            return d18Map[pos];
        }

        static bool isWall(int x, int y)
        {
            //int a = 10;
            int a = 1362;
            int b = a + x * x + 3 * x + 2 * x * y + y + y * y;
            return Convert.ToString(b, 2).Count(c => c == '1') % 2 == 1;
        }

        static long posToLong(int x, int y)
        {
            return (((long)x) << 32) | (long)y;
        }

        static void longToPos(long pos, out int x, out int y)
        {
            y = (int)pos;
            x = (int)(pos >> 32);
        }



        static void d13()
        {
            var key = "udskfozm";
            var test = "hijkl";
            var dirs = "UDLR";

            var que = new Queue<string>();
            que.Enqueue("");

            int max = 0;
            while (que.Count() > 0)
            {
                var path = que.Dequeue();
                var open = hash("udskfozm" + path).Select(c => c >= 'b').ToArray();
                (int, int) p = pos(path);
                int x = p.Item1;
                int y = p.Item2;

                if (x == 3 && y == 3)
                {
                    max = Math.Max(max, path.Length);
                    continue;
                }

                if (y > 0 && open[0])
                {
                    que.Enqueue(path + 'U');
                }
                if (y < 3 && open[1])
                {
                    que.Enqueue(path + 'D');
                }
                if (x > 0 && open[2])
                {
                    que.Enqueue(path + 'L');
                }
                if (x < 3 && open[3])
                {
                    que.Enqueue(path + 'R');
                }
            }
            Console.WriteLine("Max " + max);
        }

        static (int, int) pos(string path)
        {
            int x = 0;
            int y = 0;
            foreach (var c in path)
            {
                switch (c)
                {
                    case 'U': y--; break;
                    case 'D': y++; break;
                    case 'R': x++; break;
                    case 'L': x--; break;
                }
            }
            return (x, y);
        }

        static string hash(string s)
        {
            var md5 = MD5.Create();
            var p = Encoding.ASCII.GetBytes(s);
            var mid = md5.ComputeHash(p).Select(c => c.ToString("x2"));
            return string.Join("", mid);
        }

        static void d16()
        {
            var input = "11011110011011101";
            //int length = 272;
            int length = 35651584;
            var code = input;
            while (code.Length < length)
            {
                Console.WriteLine($"Dragoning... {100.0 * code.Length / length:0.#} %");
                code = dragon(code);
            }

            Console.WriteLine($"Dragoning... 100.0 %");
            Console.WriteLine();

            code = code.Substring(0, length);
            while (code.Length % 2 == 0)
            {
                code = dragonReduce(code);
                Console.WriteLine($"Reducing... Current length {code.Length}");
            }

            Console.WriteLine();
            Console.WriteLine("Checksum: " + code);

        }

        static string dragon(string s)
        {
            var otherHalf = new string(s.Reverse().Select(c => (c == '1') ? '0' : '1').ToArray());
            return s + '0' + otherHalf;
        }

        static string dragonReduce(string s)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i += 2)
            {
                sb.Append((s[i] == s[i + 1]) ? '1' : '0');
            }
            return sb.ToString();
        }


        //Bit 0-55
        const long itemMask = 0x00FFFFFFFFFFFFFFL;

        //Bit 56-57
        const long floorMask = 0x0300000000000000L;

        //Bit 58-62
        //const long movesMask = 0x7C00000000000000L;

        //Floor 3 and all items there
        const long endCondition = 0x3FFFC0000000000L;
        //const long endCondition = 0x30FFC0000000000L;

        enum Rtg
        {
            SG,
            SM,
            PG,
            PM,
            TG,
            TM,
            RG,
            RM,
            CG,
            CM,
            EG,
            EM,
            DG,
            DM
        }

        static bool hasRtg(long state, Rtg item, int floor)
        {
            return (state & (1L << ((int)item + 14 * floor))) != 0;
        }

        static long insertRtg(long state, Rtg item, int floor)
        {
            return state | (1L << ((int)item + 14 * floor));
        }

        static long removeRtg(long state, Rtg item, int floor)
        {
            return state & ~(1L << ((int)item + 14 * floor));
        }

        static long setFloor(long state, int floor)
        {
            long s = state &= ~floorMask;
            return s | ((long)floor << 56);
        }

        static int getFloor(long state)
        {
            return (int)((state & floorMask) >> 56);
        }

        /*static long setMoves(long state, int moves)
        {
            long s = state &= ~movesMask;
            return s | ((long)moves << 58);
        }*/

        /*static int getMoves(long state)
        {
            return (int)(state >> 58);
        }*/

        /*static long stateWithoutMoves(long state)
        {
            return state & (itemMask | floorMask);
        }*/

        static bool valid(long state)
        {
            for (int i = 0; i < 4; i++)
            {
                long floorBits = (state >> 14 * i) & 0x3FFF;
                int gen = 0;

                //Count generators
                for (int j = 0; j < 14; j += 2)
                {
                    if ((floorBits & (0x1 << j)) != 0) gen++;
                }

                //Check each type
                for (int j = 0; j < 14; j += 2)
                {
                    if ((floorBits & (0x2 << j)) != 0 && //Has chip
                       (floorBits & (0x1 << j)) == 0 && //Does not have generator
                       gen > 0) return false; //More than one generator = fried
                }
            }
            return true;
        }

        static string stateToString(long state)
        {
            string result = "";
            //result += "Moves: " + getMoves(state) + "\n";
            result += "Floor: " + getFloor(state) + "\n";
            for (int i = 0; i < 4; i++)
            {
                result += $"Floor {i}: ";
                foreach (var e in Enum.GetValues(typeof(Rtg)).OfType<Rtg>())
                {
                    if (hasRtg(state, e, i))
                    {
                        result += e.ToString() + " ";
                    }
                }
                result += "\n";
            }
            return result;
        }

        struct RtgS
        {
            public long state;
            public int moves;
        }

        static void d11()
        {
            //1st floor SG, SM, PG, PM
            //2nd floor TG, RG, RM, CG, CM
            //3rd floor TM


            long init = 0;
            init = insertRtg(init, Rtg.SG, 0);
            init = insertRtg(init, Rtg.SM, 0);
            init = insertRtg(init, Rtg.PG, 0);
            init = insertRtg(init, Rtg.PM, 0);

            init = insertRtg(init, Rtg.TG, 1);
            init = insertRtg(init, Rtg.RG, 1);
            init = insertRtg(init, Rtg.RM, 1);
            init = insertRtg(init, Rtg.CG, 1);
            init = insertRtg(init, Rtg.CM, 1);

            init = insertRtg(init, Rtg.TM, 2);

            init = insertRtg(init, Rtg.EG, 0);
            init = insertRtg(init, Rtg.EM, 0);
            init = insertRtg(init, Rtg.DG, 0);
            init = insertRtg(init, Rtg.DM, 0);

            init = setFloor(init, 0);

            long shortcut = 0;
            shortcut = insertRtg(shortcut, Rtg.SG, 0);
            shortcut = insertRtg(shortcut, Rtg.SM, 0);
            shortcut = insertRtg(shortcut, Rtg.PG, 0);
            shortcut = insertRtg(shortcut, Rtg.PM, 0);
            shortcut = insertRtg(shortcut, Rtg.EG, 0);
            shortcut = insertRtg(shortcut, Rtg.EM, 3);

            shortcut = setFloor(shortcut, 0);

            long shortcutGoal = 0;
            shortcutGoal = insertRtg(shortcutGoal, Rtg.SG, 3);
            shortcutGoal = insertRtg(shortcutGoal, Rtg.SM, 3);
            shortcutGoal = insertRtg(shortcutGoal, Rtg.PG, 3);
            shortcutGoal = insertRtg(shortcutGoal, Rtg.PM, 3);
            shortcutGoal = insertRtg(shortcutGoal, Rtg.EG, 3);
            shortcutGoal = insertRtg(shortcutGoal, Rtg.EM, 3);

            shortcutGoal = setFloor(shortcutGoal, 3);

            var initS = new RtgS
            {
                state = init,
                moves = 0
            };


            Console.WriteLine("Starting with " + stateToString(initS.state));

            long testCase = 0;
            testCase = insertRtg(testCase, Rtg.SG, 1);
            testCase = insertRtg(testCase, Rtg.SM, 0);
            testCase = insertRtg(testCase, Rtg.PG, 2);
            testCase = insertRtg(testCase, Rtg.PM, 0);

            //Console.WriteLine(stateToString(testCase));


            long testGoal = (0x3L << 40) | (0xFL << 30);
            Console.WriteLine("Aiming for " + stateToString(endCondition));


            //states.Push(init);

            //int best = 46299; //Mined
            //long best = 905;
            long best = 823;
            best = 100;
            var states = new Stack<RtgS>();
            states.Push(initS);
            var visited = new Dictionary<long, int>();
            while (states.Any())
            {
                var stateS = states.Pop();
                var state = stateS.state;
                if (!valid(state)) throw new Exception();

                var moves = stateS.moves;
                var floor = getFloor(state);
                //long withoutMoves = stateWithoutMoves(state);
                if (visited.ContainsKey(state))
                {
                    if (moves >= visited[state])
                    {
                        continue;
                    }
                    visited[state] = moves;
                }
                else
                {
                    visited.Add(state, moves);
                }

                if (moves >= best)
                    continue;

                //End condition
                if (state == endCondition)
                {
                    Console.WriteLine("New best: " + moves);
                    best = moves;
                    continue;
                }

                //Find all possible moves and enqueue
                for (int i = -1; i < 13; i++)
                {
                    for (int j = i + 1; j < 14; j++)
                    {
                        if (i >= 0 && !hasRtg(state, (Rtg)i, floor)) continue;
                        if (!hasRtg(state, (Rtg)j, floor)) continue;

                        //Remove from current state
                        long elevState = removeRtg(state, (Rtg)j, floor);
                        if (i >= 0) elevState = removeRtg(elevState, (Rtg)i, floor);
                        for (int dy = 1; dy > -2; dy -= 2)
                        {
                            int newFloor = floor + dy;
                            if (newFloor < 0 || newFloor > 3) continue;
                            long newState = setFloor(elevState, newFloor);
                            //newState = setMoves(newState, moves + 1);
                            newState = insertRtg(newState, (Rtg)j, newFloor);
                            if (i >= 0) newState = insertRtg(newState, (Rtg)i, newFloor);
                            //Console.WriteLine(stateToString(newState));

                            if (!valid(newState)) continue;
                            //long newWithoutMoves = stateWithoutMoves(newState);
                            if (visited.ContainsKey(newState) && visited[newState] < moves + 1) continue;
                            var newStateS = new RtgS
                            {
                                state = newState,
                                moves = moves + 1
                            };

                            states.Push(newStateS);

                            //Console.WriteLine("Added new state!");
                        }
                    }
                }
            }
            Console.WriteLine("No more moves!");
            Console.WriteLine("Best path was " + best);
        }



        static string[] ReadInput(int day)
        {
            string path = @"C:\git\AdventOfCode\AdventOfCode2016\Input\d" + day + ".txt";
            return File.ReadAllLines(path);
        }

        static string ReadInputAsString(int day)
        {
            string path = @"C:\git\AdventOfCode\AdventOfCode2016\Input\d" + day + ".txt";
            return File.ReadAllText(path);
        }
    }
}
