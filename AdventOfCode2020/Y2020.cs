using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace AdventOfCode2020
{
    class Y2020
    {
        static void Main(string[] args)
        {
            d25();
        }

        static void d25()
        {
            long key1 = 6930903;
            long key2 = 19716708;

            //key1 = 5764801;
            //key2 = 17807724;

            long key1Loop = findLoop(key1);
            long key2Loop = findLoop(key2);

            Console.WriteLine("Loops: " + key1Loop + " " + key2Loop);

            long encKey1 = transform(key1, key2Loop);
            long encKey2 = transform(key2, key1Loop);
            
            Console.WriteLine("Loops: " + encKey1 + " " + encKey2);
        }

        static long findLoop(long subject)
        {
            long loops = 0;
            long val = 1;
            while(val != subject)
            {
                val = (val * 7) % 20201227;
                loops++;
            }
            return loops;
        }

        static long transform(long subject, long loop)
        {
            long res = 1;
            for(int i = 0; i < loop; i++)
            {
                res = (res * subject) % 20201227;
            }
            return res;
        }

        static void d24()
        {
            var lines = ReadInput(24);
            var black = new HashSet<(int x, int y)>();
            foreach(var l in lines)
            {
                int x = 0;
                int y = 0;
                var s = l;
                for(int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    switch (c)
                    {
                        case 'e':
                            x++;
                            break;
                        case 'w':
                            x--;
                            break;
                        case 'n':
                            i++;
                            if (s[i] == 'e' && Math.Abs(y % 2) == 1)
                            {
                                x++;
                            }
                            else if (s[i] == 'w' && y % 2 == 0)
                            { 
                                x--;
                            }
                            y--;
                            break;
                        case 's':
                            i++;
                            if (s[i] == 'e' && Math.Abs(y % 2) == 1)
                            {
                                x++;
                            }
                            else if(s[i] == 'w' && y % 2 == 0)
                            {
                                x--;
                            }
                            y++;
                            break;
                        default:
                            throw new Exception();
                    }
                }
                var pos = (x, y);
                if (black.Contains(pos))
                {
                    black.Remove(pos);
                }
                else
                {
                    black.Add(pos);
                }
            }

            for(int i = 0; i < 100; i++)
            {
                var newTiles = new HashSet<(int x, int y)>();
                foreach(var t in black)
                {
                    var adj = new[]
                    {
                        (t.x, t.y-1),
                        (t.x, t.y),
                        (t.x, t.y+1),
                        (t.x+1, t.y-1),
                        (t.x+1, t.y),
                        (t.x+1, t.y+1),
                        (t.x-1, t.y-1),
                        (t.x-1, t.y),
                        (t.x-1, t.y+1)
                    };
                    foreach(var a in adj)
                    {
                        int n = hexNeigh(a, black);
                        //White
                        if(!black.Contains(a) && n == 2)
                        {
                            newTiles.Add(a);
                        }
                        //Black
                        if(black.Contains(a) && (n == 1 || n == 2))
                        {
                            newTiles.Add(a);
                        }
                    }
                }
                black = newTiles;
            }

            Console.WriteLine(black.Count());
        }

        static int hexNeigh((int x, int y) pos, HashSet<(int x, int y)> tiles)
        {
            (int x, int y)[] neigh;
            if(pos.y % 2 == 0)
            {
                neigh = new[] { (0, -1), (1, 0), (0, 1), (-1, 1), (-1, 0), (-1, -1) };
            }
            else
            {
                neigh = new[] { (1, -1), (1, 0), (1, 1), (0, 1), (-1, 0), (0, -1) };
            }

            int res = 0;
            foreach(var n in neigh)
            {
                if (tiles.Contains((pos.x + n.x, pos.y + n.y))) res++;
            }
            return res;
        }

        class Cup
        {
            public int N { get; set; }
            public Cup Next { get; set; }
        }

        static void d23()
        {
            var sw = new Stopwatch();
            sw.Start();
            int moves = 10000000;
            
            string input = "135468729";
            int N = 1000000;

            var cupMap = new Dictionary<int, Cup>();

            Cup first = new Cup
            {
                N = input[0] - '0'
            };
            cupMap[first.N] = first;
            Cup tmp = first;
            for(int i = 1; i < N; i++)
            {
                var cup = new Cup
                {
                    N = (i < input.Length ? input[i] - '0' : i+1)
                };
                cupMap[cup.N] = cup;
                tmp.Next = cup;
                tmp = cup;
            }
            tmp.Next = first;
            Console.WriteLine($"Created ring in {sw.ElapsedMilliseconds} ms");
            Cup curr = first;
            for(int i = 0; i < moves; i++)
            {
                Cup cOut = curr.Next;
                curr.Next = curr.Next.Next.Next.Next;
                int dest = curr.N - 1;
                if (dest < 1) dest = N;
                while (cOut.N == dest || cOut.Next.N == dest || cOut.Next.Next.N == dest)
                {
                    dest--;
                    if (dest < 1) dest = N;
                }
                Cup cDest = cupMap[dest];
                Cup cDestNext = cDest.Next;
                cDest.Next = cOut;
                cOut.Next.Next.Next = cDestNext;

                curr = curr.Next;
            }
            curr = cupMap[1];
            Console.WriteLine(1L * curr.Next.N * curr.Next.Next.N);
            Console.WriteLine($"Solved part 2 in {sw.ElapsedMilliseconds} ms");
        }

        static string CreateHash(Queue<byte> p1, Queue<byte> p2)
        {
            return Encoding.ASCII.GetString(p1.ToArray()) + 'z' + Encoding.ASCII.GetString(p2.ToArray());
        }

        static (bool, long) RecursiveCombat(Queue<byte> p1, Queue<byte> p2, bool first)
        {
            //If player 1 has the highest card he can't lose!
            if (!first && p1.Max() > p2.Max()) return (true, 0);
            var played = new HashSet<string>();
            while (p1.Count() > 0 && p2.Count() > 0)
            {
                var hash = CreateHash(p1, p2);
                if (played.Contains(hash))
                {
                    //P1 wins if same state as before
                    p2.Clear();
                    continue;
                }
                played.Add(hash);

                var c1 = p1.Dequeue();
                var c2 = p2.Dequeue();
                bool p1Wins = true;
                //Console.WriteLine($"P1: {c1}, P2: {c2}");
                if(c1 <= p1.Count() && c2 <= p2.Count())
                {
                    var pn1 = new Queue<byte>();
                    foreach (var c in p1.ToArray().Take(c1)) pn1.Enqueue(c);
                    var pn2 = new Queue<byte>();
                    foreach (var c in p2.ToArray().Take(c2)) pn2.Enqueue(c);
                    //Console.WriteLine("Going into subgame...");
                    (p1Wins, _) = RecursiveCombat(pn1, pn2, false);
                }
                else
                {
                    p1Wins = c1 > c2;
                }
                
                if (p1Wins)
                {
                    p1.Enqueue(c1);
                    p1.Enqueue(c2);
                }
                else
                {
                    p2.Enqueue(c2);
                    p2.Enqueue(c1);
                }
                //Console.WriteLine($"Player {(p1Wins ? "1" : "2")} won the round!");

            }
            bool p1Won = p2.Count() == 0;
            //Console.WriteLine($"Player {(p1Won ? "1":"2")} won the game!");
            var winner = p1Won ? p1 : p2;
            long result = 0;
            while (winner.Count() > 0)
            {
                result += winner.Count() * winner.Dequeue();
            }
            return (p1Won, result);
        }

        static void d22_1()
        {
            var input = ReadInput(22);
            var p1 = new Queue<byte>();
            var p2 = new Queue<byte>();
            bool player2 = false;
            foreach (var s in input)
            {
                if (s.Contains("Player 2")) player2 = true;
                if (byte.TryParse(s, out byte card))
                {
                    if (!player2)
                    {
                        p1.Enqueue(card);
                    }
                    else
                    {
                        p2.Enqueue(card);
                    }
                }
            }
            var played = new HashSet<string>();
            (bool p1Won, long result) res = RecursiveCombat(p1, p2, true);
            
            Console.WriteLine(res.result);
        }

        static void d22()
        {
            var input = ReadInput(22);
            var p1 = new Queue<byte>();
            var p2 = new Queue<byte>();
            bool player2 = false;
            foreach(var s in input)
            {
                if (s.Contains("Player 2")) player2 = true;
                if(byte.TryParse(s, out byte card))
                {
                    if (!player2)
                    {
                        p1.Enqueue(card);
                    }
                    else
                    {
                        p2.Enqueue(card);
                    }
                }
            }

            while(p1.Count() > 0 && p2.Count() > 0)
            {
                var c1 = p1.Dequeue();
                var c2 = p2.Dequeue();
                if(c1 > c2)
                {
                    p1.Enqueue(c1);
                    p1.Enqueue(c2);
                }
                else
                {
                    p2.Enqueue(c2);
                    p2.Enqueue(c1);
                }
            }

            var winner = p1.Count() == 0 ? p2 : p1;
            long result = 0;
            while(winner.Count() > 0)
            {
                result += winner.Count() * winner.Dequeue();
            }
            Console.WriteLine(result);
        }

        static void d21()
        {
            var lines = ReadInput(21).Select(s => s.Replace(",", "").Replace("(", "").Replace(")", ""));
            var ingred = new HashSet<string>();
            var allerg = new HashSet<string>();
            var pairs = new List<(string ing, string all)>();

            var counts = new Dictionary<string, int>();

            //Gather all data
            foreach(var l in lines)
            {
                bool isAllergen = false;
                var sp = l.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var ing = new HashSet<string>();
                var all = new HashSet<string>();
                foreach(var s in sp)
                {
                    if(s == "contains")
                    {
                        isAllergen = true;
                    }
                    else if (isAllergen)
                    {
                        all.Add(s);
                        allerg.Add(s);
                    }
                    else
                    {
                        ing.Add(s);
                        ingred.Add(s);
                        if (!counts.ContainsKey(s)) counts[s] = 0;
                        counts[s]++;
                    }
                }
                foreach (var a in all)
                {
                    if (!pairs.Any(p => p.all == a))
                    {
                        //This is a new allergen
                        foreach (var i in ing) pairs.Add((i, a));
                    }
                    else
                    {
                        //This allergen is already known
                        pairs.RemoveAll(p => all.Contains(p.all) && !ing.Contains(p.ing));
                    }
                }
            }

            var safe = ingred.Where(i => !pairs.Any(p => p.ing == i)).ToList();
            var safeCount = safe.Sum(c => counts[c]);
            Console.WriteLine(safeCount);
            
            while(pairs.Count() > allerg.Count())
            {
                foreach(var p in pairs)
                {
                    int nIng = pairs.Count(pp => pp.ing == p.ing);
                    if(nIng == 1)
                    {
                        var unique = pairs.First(pp => pp.ing == p.ing);
                        int rm = pairs.RemoveAll(pp => pp.ing != unique.ing && pp.all == unique.all);
                        if(rm > 0) break;
                    }

                    int nAll = pairs.Count(pp => pp.all == p.all);
                    if (nAll == 1)
                    {
                        var unique = pairs.First(pp => pp.all == p.all);
                        int rm = pairs.RemoveAll(pp => pp.all != unique.all && pp.ing == unique.ing);
                        if(rm > 0) break;
                    }
                }
            }

            var riskList = pairs.Select(s => s.all).ToList();
            riskList.Sort();
            riskList = riskList.Select(s => pairs.First(pp => pp.all == s).ing).ToList();
            Console.WriteLine(string.Join(',', riskList));
        }

        class Image
        {
            public int ID { get; set; }
            public int N { get; set; } //LSB Right
            public int S { get; set; } //LSB Right
            public int W { get; set; } //LSB Down
            public int E { get; set; } //LSB Down

            public string Mod { get; set; } //R for rotate, F for Flip

            int Reverse(int border)
            {
                int result = 0;
                for (int i = 0; i < 10; i++)
                {
                    if ((border & (1 << i)) != 0)
                    {
                        result |= (1 << (9 - i));
                    }
                }
                return result;
            }

            //Flip around the N/S axis
            public Image Flip()
            {
                return new Image
                {
                    ID = ID,
                    E = W,
                    W = E,
                    S = Reverse(S),
                    N = Reverse(N),
                    Mod = Mod + "F"
                };
            }

            public Image Rotate()
            {
                return new Image
                {
                    ID = ID,
                    N = Reverse(W),
                    E = N,
                    S = Reverse(E),
                    W = S,
                    Mod = Mod + "R"
                };
            }

            public static int StringToBorder(string s)
            {
                if (s.Length != 10)
                {
                    throw new Exception();
                }
                return Convert.ToInt32(s.Select(c => c == '#' ? '1' : '0').Aggregate("", (m, n) => m + n), 2);
            }

            public List<Image> GetVariations()
            {
                var list = new List<Image>();

                var tmp = this;
                list.Add(tmp);
                tmp = tmp.Rotate();
                list.Add(tmp);
                tmp = tmp.Rotate();
                list.Add(tmp);
                tmp = tmp.Rotate();
                list.Add(tmp);

                tmp = Flip();
                list.Add(tmp);
                tmp = tmp.Rotate();
                list.Add(tmp);
                tmp = tmp.Rotate();
                list.Add(tmp);
                tmp = tmp.Rotate();
                list.Add(tmp);

                return list;
            }

            public override string ToString()
            {
                return $"{ID}: {Mod}";
            }
        }

        static void d20()
        {
            var input = ReadInput(20);
            var tiles = new Dictionary<int, Image>();
            var baseTiles = new Dictionary<int, string>(); //All as one string line by line, excluding border
            for (int i = 0; i < input.Count(); i++)
            {
                if (!input[i].Contains("Tile")) continue;
                int id = int.Parse(input[i].Replace(":", "").Split()[1]);
                string n = input[i + 1];
                string s = input[i + 10];
                string w = input.Skip(i + 1).Take(10).Select(k => k[0]).Aggregate("", (m, n) => m + n);
                string e = input.Skip(i + 1).Take(10).Select(k => k[9]).Aggregate("", (m, n) => m + n);

                string innerImage = input.Skip(i + 2).Take(8).Select(s => s.Substring(1, 8)).Aggregate("", (m, n) => m + n);
                baseTiles[id] = innerImage;
                var image = new Image
                {
                    ID = id,
                    N = Image.StringToBorder(n),
                    S = Image.StringToBorder(s),
                    W = Image.StringToBorder(w),
                    E = Image.StringToBorder(e),
                    Mod = ""
                };
                tiles.Add(id, image);
            }

            //This is where the fun begins
            var map = new Dictionary<(int x, int y), Image>();
            var used = new HashSet<int>();

            var firstTile = tiles.First().Value;
            //Control orientation of image here...
            map.Add((0, 0), firstTile.Flip().Rotate().Rotate().Rotate());
            used.Add(firstTile.ID);

            expand20((0, 0), tiles, map, used);

            int xMin = map.Keys.Min(m => m.x);
            int xMax = map.Keys.Max(m => m.x);
            int yMin = map.Keys.Min(m => m.y);
            int yMax = map.Keys.Max(m => m.y);

            long result = 1L
                * map[(xMin, yMin)].ID
                * map[(xMax, yMin)].ID
                * map[(xMin, yMax)].ID
                * map[(xMax, yMax)].ID;
            Console.WriteLine("Part 1: " + result);

            //Stitch together the image
            int tileSize = 10 - 2;
            int size = tileSize * (int)Math.Sqrt(tiles.Count());
            var sea = new bool[size, size];
            for (int tX = xMin; tX <= xMax; tX++)
            {
                for (int tY = yMin; tY <= yMax; tY++)
                {
                    var tile = map[(tX, tY)];
                    var tmpTile = new bool[tileSize, tileSize];
                    for (int x = 0; x < tileSize; x++)
                    {
                        for (int y = 0; y < tileSize; y++)
                        {
                            tmpTile[y, x] = baseTiles[tile.ID][y * tileSize + x] == '#';
                        }
                    }
                    tmpTile = modTile(tmpTile, tile.Mod);
                    for (int x = 0; x < tileSize; x++)
                    {
                        for (int y = 0; y < tileSize; y++)
                        {
                            sea[(tY - yMin) * tileSize + y, (tX - xMin) * tileSize + x] = tmpTile[y, x];
                        }
                    }
                }
            }

            var filteredSea = new bool[size, size];
            Array.Copy(sea, filteredSea, sea.Length);
            //Now we have the entire map in sea
            //Time to find some sea monsters
            //Rotate sea monster instead of map
            var aSeaMonster = "                  # \n#    ##    ##    ###\n #  #  #  #  #  #   ";
            var seaMonsterCoord = new List<(int x, int y)>();
            int sX = 0;
            int sY = 0;
            foreach (var c in aSeaMonster)
            {
                if (c == '#')
                {
                    seaMonsterCoord.Add((sX, sY));
                }
                else if (c == '\n')
                {
                    sY++;
                    sX = -1;
                }

                sX++;
            }
            int sW = seaMonsterCoord.Max(s => s.x);
            int sH = seaMonsterCoord.Max(s => s.y);

            for (int x = 0; x < size-sW; x++)
            {
                for (int y = 0; y < size-sY; y++)
                {
                    bool match = true;
                    foreach(var c in seaMonsterCoord)
                    {
                        if(!sea[y+c.y, x + c.x])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        Console.WriteLine("I found meself a sea monster!");
                        foreach (var c in seaMonsterCoord)
                        {
                            filteredSea[y + c.y, x + c.x] = false;
                        }
                    }
                }
            }
            int part2 = 0;
            foreach(bool b in filteredSea)
            {
                if (b) part2++;
            }
            Console.WriteLine("Part 2: " + part2);
        }

        static bool[,] modTile(bool[,] tile, string mod)
        {
            int h = tile.GetLength(0);
            int w = tile.GetLength(1);
            var newTile = new bool[h, w];
            for (int i = 0; i < mod.Length; i++)
            {
                if (mod[i] == 'F')
                {
                    //Flip over N/S axis
                    for (int x = 0; x < w; x++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            newTile[y, x] = tile[y, w - x - 1];
                        }
                    }
                }
                else
                {
                    //Rotate CW
                    for (int x = 0; x < w; x++)
                    {
                        for (int y = 0; y < h; y++)
                        {
                            newTile[y, x] = tile[w - x - 1, y];
                        }
                    }

                    //Assert that you're not retarded
                    for(int k = 0; k < 8; k++)
                    {
                        if (newTile[k, 0] != tile[7, k]) throw new Exception("You're retarded...");
                    }
                }
                Array.Copy(newTile, tile, tile.Length);
            }
            return tile;
        }

        static void expand20((int x, int y) pos, Dictionary<int, Image> tiles, Dictionary<(int x, int y), Image> map, HashSet<int> used)
        {
            //Check each border of x, y, if there is already something in the map, continue
            var tile = map[pos];
            (int x, int y) nPos = (pos.x, pos.y);

            foreach (var e in Enum.GetValues(typeof(Dir)))
            {
                int border = 0;
                Func<Image, int> getBorder = i => -1;
                switch (e)
                {
                    case Dir.N:
                        nPos = (pos.x, pos.y - 1);
                        border = tile.N;
                        getBorder = i => i.S;
                        break;
                    case Dir.E:
                        nPos = (pos.x + 1, pos.y);
                        border = tile.E;
                        getBorder = i => i.W;
                        break;
                    case Dir.S:
                        nPos = (pos.x, pos.y + 1);
                        border = tile.S;
                        getBorder = i => i.N;
                        break;
                    case Dir.W:
                        nPos = (pos.x - 1, pos.y);
                        border = tile.W;
                        getBorder = i => i.E;
                        break;
                    default:
                        throw new Exception();
                }

                foreach (var cand in tiles.Where(t => !used.Contains(t.Key)))
                {
                    var img = cand.Value;
                    Image match = null;
                    var variations = img.GetVariations();
                    foreach (var v in variations)
                    {
                        if (getBorder(v) == border)
                        {
                            match = v;
                            break;
                        }
                    }
                    if (match != null)
                    {
                        map.Add(nPos, match);
                        used.Add(match.ID);
                        expand20(nPos, tiles, map, used);
                        break;
                    }
                }
            }
        }

        enum Dir
        {
            N,
            E,
            S,
            W
        }

        static void d19()
        {
            var rules = new Dictionary<int, string>();
            var regex = new Dictionary<int, string>();
            var input = ReadInput(19);

            foreach (var s in input.Where(k => k.Contains(":")))
            {
                var sp = s.Replace(":", "").Split();
                var number = int.Parse(sp[0]);
                var rule = string.Join(' ', sp.Skip(1));
                rules[number] = rule;
            }

            foreach (var r in rules)
            {
                string res = expand19(r.Key, rules, regex, 0);
            }

            int valid = input.Where(k => k.Length > 0 && !k.Contains(":")).Count(s => Regex.IsMatch(s, "^" + regex[0] + "$"));
            Console.WriteLine("Part 1: " + valid);

            rules[8] = "42 | 42 8";
            rules[11] = "42 31 | 42 11 31";
            regex = new Dictionary<int, string>();
            foreach (var r in rules)
            {
                string res = expand19(r.Key, rules, regex, 0);
            }
            valid = input.Where(k => k.Length > 0 && !k.Contains(":")).Count(s => Regex.IsMatch(s, "^" + regex[0] + "$"));
            Console.WriteLine("Part 2: " + valid);
        }
        static int depthMax = 0;
        static string expand19(int number, Dictionary<int, string> rules, Dictionary<int, string> regex, int depth)
        {
            if (depth > 100)
            {
                return "";
            }
            if (regex.ContainsKey(number)) return regex[number];

            var rule = rules[number];

            if (rule.Contains("a") || rule.Contains("b"))
            {
                string reg = rule[1].ToString();
                regex[number] = reg;
                return reg;
            }

            //Build new regex
            var sp = rule.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var newRegex = new StringBuilder();
            foreach (var s in sp)
            {
                if (s == "|")
                {
                    newRegex.Append('|');
                }
                else
                {
                    newRegex.Append('(');
                    newRegex.Append(expand19(int.Parse(s), rules, regex, depth + 1));
                    newRegex.Append(')');
                }
            }
            var result = newRegex.ToString();
            regex[number] = result;
            return result;
        }

        static void d18()
        {
            var input = ReadInput(18);

            testEval("   1 + 2 * 3 + 4 * 5 + 6   ", 231);

            testEval("   2 * 3 + (4 * 5)   ", 46);
            testEval("   5 + (8 * 3 + 9 + 3 * 4 * 3)   ", 1445);
            testEval("   5 * 9 * (7 * 3 * 3 + 9 * 3 + (8 + 6 * 4))   ", 669060);
            testEval("   ((2 + 4 * 9) * (6 + 9 * 8 + 6) + 6) + 2 + 4 * 2   ", 23340);


            long sum = input.Sum(s => ToOps(s).Eval());
            Console.WriteLine(sum);
        }

        static void testEval(string e, long expected)
        {
            var exp = ToOps(e);
            long val = exp.Eval();
            bool pass = val == expected;
            Console.WriteLine((pass ? "PASS" : "FAIL") + ": " + e + " = " + val + " [" + expected + "]");

        }


        interface IOp
        {
            public IOp left { get; set; }
            public IOp right { get; set; }
            long Eval();
        }

        class Paran : IOp
        {
            public IOp left { get; set; }
            public IOp right { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public long Eval()
            {
                return left.Eval();
            }

            public override string ToString()
            {
                return "(" + left + ")";
            }
        }

        class Plus : IOp
        {
            public IOp left { get; set; }
            public IOp right { get; set; }
            public long Eval()
            {
                return left.Eval() + right.Eval();
            }

            public override string ToString()
            {
                return "(" + left + " + " + (right == null ? "?" : right.ToString()) + ")";
            }
        }

        class Mul : IOp
        {
            public IOp left { get; set; }
            public IOp right { get; set; }

            public long Eval()
            {
                return left.Eval() * right.Eval();
            }

            public override string ToString()
            {
                return left + " * " + (right == null ? "?" : right.ToString());
            }
        }

        class Val : IOp
        {
            public long val { get; set; }
            public IOp left { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public IOp right { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public long Eval()
            {
                return val;
            }

            public override string ToString()
            {
                return val.ToString();
            }
        }


        static long eval18(string e)
        {
            string s = e.Replace(" ", "");
            int ptr = 0;
            return innerVal(s, ref ptr);
        }

        static IOp ToOps(string e)
        {
            int ptr = 0;
            return ToOps(e.Replace(" ", ""), ref ptr);
        }

        static IOp ToOps(string e, ref int ptr)
        {
            IOp last = null;
            for (; ptr < e.Length; ptr++)
            {
                char c = e[ptr];
                switch (c)
                {
                    case '*':
                        last = new Mul { left = last };
                        break;
                    case '+':
                        if (last != null && last is Mul)
                        {
                            last.right = new Plus { left = last.right };
                        }
                        else
                        {
                            last = new Plus { left = last };
                        }
                        break;
                    case '(':
                        ptr++;
                        var inParan = new Paran { left = ToOps(e, ref ptr) };
                        if (last == null)
                        {
                            last = inParan;
                        }
                        else
                        {
                            var tmp = last;
                            while (tmp.right != null) tmp = tmp.right;
                            tmp.right = inParan;
                        }
                        break;
                    case ')':
                        return last;
                    default: //Numeric
                        if (last == null)
                        {
                            last = new Val { val = long.Parse(c.ToString()) };
                        }
                        else
                        {
                            var tmp = last;
                            while (tmp.right != null) tmp = tmp.right;
                            tmp.right = new Val { val = long.Parse(c.ToString()) };
                        }
                        break;
                }
            }
            return last;
        }

        static long innerVal(string e, ref int ptr)
        {
            long result = 0;
            bool mul = false;
            for (; ptr < e.Length; ptr++)
            {
                char c = e[ptr];
                switch (c)
                {
                    case '*':
                        mul = true;
                        ptr++;
                        result *= innerVal(e, ref ptr);
                        ptr--;
                        break;
                    case '+':
                        mul = false;
                        break;
                    case '(':
                        ptr++;
                        if (mul)
                        {
                            result *= innerVal(e, ref ptr);
                        }
                        else
                        {
                            result += innerVal(e, ref ptr);
                        }
                        ptr--;
                        break;
                    case ')':
                        return result;
                    default: //Numeric
                        if (mul)
                        {
                            result *= long.Parse(c.ToString());
                        }
                        else
                        {
                            result += long.Parse(c.ToString());
                        }
                        break;
                }
            }
            return result;
        }

        static void d17()
        {
            var input = ReadInput(17);
            var cells = new HashSet<(int x, int y, int z, int w)>();

            int y = 0;
            foreach (var s in input)
            {
                for (int x = 0; x < s.Count(); x++)
                {
                    if (s[x] == '#')
                    {
                        cells.Add((x, y, 0, 0));
                    }
                }
                y++;
            }

            //Simulate
            for (int i = 0; i < 6; i++)
            {
                cells = expand17(cells);
            }
            Console.WriteLine(cells.Count());
        }

        static HashSet<(int, int, int, int)> expand17(HashSet<(int x, int y, int z, int w)> cells)
        {
            var newCells = new HashSet<(int x, int y, int z, int w)>();
            if (cells.Count() == 0) return newCells;

            int xMin = cells.Min(c => c.x);
            int xMax = cells.Max(c => c.x);
            int yMin = cells.Min(c => c.y);
            int yMax = cells.Max(c => c.y);
            int zMin = cells.Min(c => c.z);
            int zMax = cells.Max(c => c.z);
            int wMin = cells.Min(c => c.w);
            int wMax = cells.Max(c => c.w);

            for (int x = xMin - 1; x <= xMax + 1; x++)
                for (int y = yMin - 1; y <= yMax + 1; y++)
                    for (int z = zMin - 1; z <= zMax + 1; z++)
                        for (int w = wMin - 1; w <= wMax + 1; w++)
                        {
                            var pos = (x, y, z, w);
                            int n = active17(cells, pos);
                            bool active = cells.Contains(pos);
                            if ((active && (n == 2 || n == 3)) || (!active && n == 3))
                            {
                                newCells.Add(pos);
                            }
                        }

            return newCells;
        }

        static int active17(HashSet<(int x, int y, int z, int w)> cells, (int x, int y, int z, int w) pos)
        {
            int active = 0;
            for (int x = pos.x - 1; x <= pos.x + 1; x++)
                for (int y = pos.y - 1; y <= pos.y + 1; y++)
                    for (int z = pos.z - 1; z <= pos.z + 1; z++)
                        for (int w = pos.w - 1; w <= pos.w + 1; w++)
                            if ((x, y, z, w) != pos && cells.Contains((x, y, z, w))) active++;

            return active;
        }

        static void d16()
        {
            var lines = ReadInput(16);
            var ranges = new List<(int type, int from, int to)>();
            var typeToField = new Dictionary<int, string>();
            var yourTicket = new List<int>();
            var tickets = new List<List<int>>();

            int t = 0;
            foreach (var l in lines.Where(s => s.Contains("or")))
            {
                var field = l.Split(':')[0];
                var sp = l.Replace(field + ": ", "").Split();
                var first = sp[0].Split('-').Select(int.Parse).ToArray();
                var second = sp[2].Split('-').Select(int.Parse).ToArray();
                ranges.Add((t, first[0], first[1]));
                ranges.Add((t, second[0], second[1]));
                typeToField.Add(t, field);
                t++;
            }

            bool isYourTicket = true;
            foreach (var l in lines.Where(s => s.Contains(",")))
            {
                var ticket = l.Split(',').Select(int.Parse).ToList();
                if (isYourTicket)
                {
                    yourTicket = ticket;
                    isYourTicket = false;
                }
                else
                {
                    tickets.Add(ticket);
                }
            }

            int rate = 0;
            for (int i = tickets.Count() - 1; i >= 0; i--)
            {
                int sum = tickets[i].Where(t => !ranges.Any(r => r.from <= t && t <= r.to)).Sum();
                rate += sum;
                if (sum > 0)
                {
                    tickets.RemoveAt(i);
                }
            }

            Console.WriteLine("Part 1: " + rate);

            //Find all candidates
            var indices = Enumerable.Range(0, yourTicket.Count());
            var candidates = new List<(int index, int type)>();
            foreach (var index in indices)
            {
                foreach (var type in typeToField.Keys)
                {
                    var range = ranges.Where(r => r.type == type).ToArray();
                    if (tickets.All(t => range.Any(r => r.from <= t[index] && t[index] <= r.to)))
                    {
                        candidates.Add((index, type));
                    }
                }
            }

            //Remove candidates by exclusion
            while (candidates.Count() > typeToField.Count())
            {
                for (int index = 0; index < yourTicket.Count(); index++)
                {
                    if (candidates.Count(t => t.index == index) == 1)
                    {
                        int type = candidates.First(t => t.index == index).type;
                        candidates.RemoveAll(t => t.type == type && t.index != index);
                    }

                }
                for (int type = 0; type < typeToField.Count(); type++)
                {
                    if (candidates.Count(t => t.type == type) == 1)
                    {
                        int index = candidates.First(t => t.type == type).index;
                        candidates.RemoveAll(t => t.index == index && t.type != type);
                    }

                }
            }

            var depart = typeToField.Where(t => t.Value.Contains("departure")).Select(s => s.Key).ToList();
            var part2 = candidates
                .Where(c => depart.Contains(c.type))
                .Select(c => c.index)
                .Select(i => (long)yourTicket[i])
                .Aggregate((a, b) => a * b);

            //One type is missing...
            foreach (var type in typeToField)
            {
                Console.WriteLine($"Type: {type.Key}, Name: {type.Value}, Index: {candidates.FirstOrDefault(s => s.type == type.Key).index}");
            }
            Console.WriteLine("Part 2: " + part2);
        }

        static void d15()
        {
            var nums = new Dictionary<int, (int, int)>();
            var start = "6,13,1,15,2,0".Split(',').Select(int.Parse).ToArray();
            int last = 0;
            for (int turn = 0; turn < 30000000; turn++)
            {
                int speak = 0;
                if (turn < start.Count())
                {
                    speak = start[turn];
                }
                else
                {
                    if (nums.ContainsKey(last))
                    {
                        var n = nums[last];
                        if (n.Item2 >= 0)
                        {
                            speak = nums[last].Item1 - nums[last].Item2;
                        }
                    }
                }
                last = speak;
                int lastlast = nums.ContainsKey(speak) ? nums[speak].Item1 : -1;
                nums[speak] = (turn, lastlast);
            }
            Console.WriteLine(last);
        }

        static void d14()
        {
            var lines = ReadInput(14);
            Console.WriteLine("Part 1: " + memSum(lines, true));
            Console.WriteLine("Part 2: " + memSum(lines, false));
        }

        static long memSum(string[] input, bool part1)
        {
            long onMask = 0;
            long offMask = 0;
            var mem = new Dictionary<long, long>();
            var offsets = new HashSet<long>();
            foreach (var l in input)
            {
                if (l.Contains("mask"))
                {
                    var maskS = l.Replace("mask = ", "");
                    onMask = 0;
                    offMask = 0;
                    offsets.Clear();
                    offsets.Add(0);
                    for (int i = maskS.Length - 1; i >= 0; i--)
                    {
                        if (maskS[i] == '1') onMask |= (1L << (maskS.Length - i - 1));
                        if (part1)
                        {
                            if (maskS[i] == '0') offMask |= (1L << (maskS.Length - i - 1));
                        }
                        else
                        {
                            if (maskS[i] == 'X')
                            {
                                offMask |= (1L << (maskS.Length - i - 1));
                                long offset = 1L << (maskS.Length - i - 1);

                                var newOffset = offsets.Select(o => o + offset).ToHashSet();
                                offsets.UnionWith(newOffset);
                            }
                        }
                    }
                }
                else
                {
                    var sp = l.Split("] = ");
                    long addr = long.Parse(sp[0].Replace("mem[", ""));
                    long val = long.Parse(sp[1]);
                    if (part1)
                    {
                        val |= onMask;
                        val &= ~offMask;
                        mem[addr] = val;
                    }
                    else
                    {
                        addr |= onMask;
                        addr &= ~offMask;
                        foreach (var o in offsets)
                        {
                            mem[addr + o] = val;
                        }
                    }
                }
            }
            return mem.Values.Sum();
        }

        static void d13()
        {
            var lines = ReadInput(13);
            int start = int.Parse(lines[0]);
            var buses = lines[1].Split(',', StringSplitOptions.RemoveEmptyEntries).Where(s => !s.Contains("x")).Select(s => int.Parse(s)).ToArray();

            var deps = new Dictionary<int, int>(); //bus ID to departure time

            foreach (var b in buses)
            {
                int dep = (start % b == 0) ? start : b * (start / b + 1);
                deps.Add(b, dep);
            }

            var best = deps.First(k => k.Value == deps.Min(t => t.Value));
            Console.WriteLine("Part 1: " + best.Key * (best.Value - start));
            Console.WriteLine();

            var sp = lines[1].Split(',');
            char[] variable = new[] { 'a', 'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k' };
            int varI = 0;
            string query = "";
            for (int i = 0; i < sp.Length; i++)
            {
                if (sp[i] == "x") continue;
                query += $"{int.Parse(sp[i])}{variable[varI++]}-{i}=T,";
            }
            string url = "https://www.wolframalpha.com/input/?i=" + query.TrimEnd(',');
            Console.WriteLine("Opening " + url);
            OpenBrowser(url);
            Console.WriteLine("Find the part that says T = an + b");
            Console.WriteLine("The answer is b");
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private static ulong GCD(ulong a, ulong b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }

            return a | b;
        }

        static void d12_2()
        {
            var lines = ReadInput(12);
            double x = 0;
            double y = 0;
            int a = 0; //East
            double wpX = 10;
            double wpY = 1;

            foreach (var l in lines)
            {
                char c = l[0];
                int val = int.Parse(l[1..]);
                double oX = wpX;
                double oY = wpY;
                switch (c)
                {
                    case 'N': wpY += val; break;
                    case 'S': wpY -= val; break;
                    case 'E': wpX += val; break;
                    case 'W': wpX -= val; break;
                    case 'R':

                        wpX = oX * Math.Cos(-val * Math.PI / 180) - oY * Math.Sin(-val * Math.PI / 180);
                        wpY = oX * Math.Sin(-val * Math.PI / 180) + oY * Math.Cos(-val * Math.PI / 180);
                        break;
                    case 'L':
                        wpX = oX * Math.Cos(val * Math.PI / 180) - oY * Math.Sin(val * Math.PI / 180);
                        wpY = oX * Math.Sin(val * Math.PI / 180) + oY * Math.Cos(val * Math.PI / 180);
                        break;
                    case 'F':
                        x += wpX * val;
                        y += wpY * val;
                        break;
                    default:
                        throw new Exception();
                }
            }
            Console.WriteLine($"X: {x}, Y: {y}, Manhattan {Math.Round(Math.Abs(x) + Math.Abs(y))}");
        }

        static void d12()
        {
            var lines = ReadInput(12);
            double x = 0;
            double y = 0;
            int a = 0; //East

            foreach (var l in lines)
            {
                char c = l[0];
                int val = int.Parse(l[1..]);
                Console.WriteLine($"X: {x}, Y: {y}, Manhattan {Math.Round(Math.Abs(x) + Math.Abs(y))}");
                switch (c)
                {
                    case 'N': y += val; break;
                    case 'S': y -= val; break;
                    case 'E': x += val; break;
                    case 'W': x -= val; break;
                    case 'R': a -= val; break;
                    case 'L': a += val; break;
                    case 'F':
                        x += Math.Cos(a * Math.PI / 180) * val;
                        y += Math.Sin(a * Math.PI / 180) * val;
                        break;
                    default:
                        throw new Exception();
                }
            }
            Console.WriteLine($"X: {x}, Y: {y}, Manhattan {Math.Round(Math.Abs(x) + Math.Abs(y))}");
        }

        static void d11()
        {
            var lines = ReadInput(11);

            var grid = new char[lines.Length, lines[0].Length];
            for (int y = 0; y < lines.Length; y++)
                for (int x = 0; x < lines[0].Length; x++)
                {
                    grid[y, x] = lines[y][x];
                }

            var newG = new char[lines.Length, lines[0].Length];
            int its = 0;
            while (true)
            {
                for (int y = 0; y < lines.Length; y++)
                    for (int x = 0; x < lines[0].Length; x++)
                    {
                        char c = grid[y, x];
                        if (c == '.') continue;
                        int o = 0;
                        for (int X = Math.Max(0, x - 1); X <= Math.Min(x + 1, grid.GetLength(1) - 1); X++)
                            for (int Y = Math.Max(0, y - 1); Y <= Math.Min(y + 1, grid.GetLength(0) - 1); Y++)
                            {
                                if (X == x && y == Y) continue;
                                int dy = Y - y;
                                int dx = X - x;
                                int XX = X;
                                int YY = Y;
                                for (; ; )
                                {
                                    if (XX < 0 || XX > grid.GetLength(1) - 1 || YY < 0 || YY > grid.GetLength(0) - 1) break;
                                    char C = grid[YY, XX];
                                    if (C == 'L')
                                    {
                                        break;
                                    }
                                    if (C == '#')
                                    {
                                        o++;
                                        break;
                                    }
                                    XX += dx;
                                    YY += dy;
                                }

                            }
                        newG[y, x] = (c == 'L' && o == 0) ? '#' : newG[y, x];
                        newG[y, x] = (c == '#' && o >= 5) ? 'L' : newG[y, x];
                    }

                /*for (int y = 0; y < lines.Length; y++)
                {
                    for (int x = 0; x < lines[0].Length; x++)
                    {
                        Console.Write(newG[y, x]);
                    }
                    Console.WriteLine();
                }*/



                if (comp(grid, newG)) break;
                for (int y = 0; y < lines.Length; y++)
                    for (int x = 0; x < lines[0].Length; x++)
                    {
                        grid[y, x] = newG[y, x];
                    }
                its++;

            }

            int part1 = 0;
            for (int y = 0; y < lines.Length; y++)
                for (int x = 0; x < lines[0].Length; x++)
                {
                    if (grid[y, x] == '#') part1++;
                }

            print(part1);
        }

        static bool comp(char[,] a, char[,] b)
        {
            for (int y = 0; y < a.GetLength(0); y++)
                for (int x = 0; x < a.GetLength(1); x++)
                {
                    if (a[y, x] != b[y, x]) return false;
                }

            return true;
        }

        static void d10()
        {
            var adapters = ReadInput(10).Select(s => int.Parse(s)).ToList();
            adapters.Add(0);
            adapters.Add(adapters.Max() + 3);
            adapters.Sort();
            int one = 0;
            int three = 0;
            for (int i = 1; i < adapters.Count(); i++)
            {
                int diff = adapters[i] - adapters[i - 1];
                if (diff == 1) one++;
                if (diff == 3) three++;
            }

            Console.WriteLine("Part 1: " + one * three);
            var memo = new Dictionary<int, long>();
            memo.Add(0, 1); //There is "one way" to 0
            long waysToEnd = waysTo(adapters.Count() - 1, adapters, memo);
            Console.WriteLine("Part 2: " + waysToEnd);
        }

        static long waysTo(int i, List<int> adp, Dictionary<int, long> memo)
        {
            if (memo.ContainsKey(i)) return memo[i];
            long ways = 0;
            int pre = i - 1;
            while (pre >= 0 && adp[i] - adp[pre] <= 3)
            {
                ways += waysTo(pre, adp, memo);
                pre--;
            }
            memo.Add(i, ways);
            return ways;
        }

        static void d9()
        {
            var xmas = ReadInput(9).Select(s => long.Parse(s)).ToList();
            long target = 0;
            for (int i = 25; i < xmas.Count(); i++)
            {
                var sums = allSums(xmas.Skip(i - 25).Take(25).ToList());
                if (!sums.Contains(xmas[i]))
                {
                    Console.WriteLine(xmas[i]);
                    target = xmas[i];
                    break;
                }
            }

            for (int i = 0; i < xmas.Count(); i++)
            {
                if (xmas[i] == target) continue;
                long sum = 0;
                var set = new HashSet<long>();
                for (int j = i; j < xmas.Count(); j++)
                {
                    sum += xmas[j];
                    set.Add(xmas[j]);
                    if (sum > target)
                    {
                        break;
                    }
                    if (sum == target)
                    {
                        Console.WriteLine(set.Min() + "+" + set.Max() + "=" + (set.Min() + set.Max()));
                        return;
                    }
                }
            }
        }

        static List<long> allSums(List<long> xmas)
        {
            var sums = new List<long>();
            for (int i = 0; i < xmas.Count(); i++)
                for (int j = i + 1; j < xmas.Count(); j++)
                    sums.Add(xmas[i] + xmas[j]);
            return sums;
        }

        static void d8()
        {
            var lines = ReadInput(8);
            runCode(lines);
            for (int i = 0; i < lines.Length; i++)
            {
                var s = lines[i].Split();
                var op = s[0];
                var val = int.Parse(s[1]);
                if (!lines[i].Contains("acc") && i + val > 628)
                {
                    Console.WriteLine(i + " " + (i + val) + " " + lines[i]);
                }


            }
            for (int i = 0; i < lines.Length; i++)
            {
                Console.WriteLine(i);
                if (lines[i].Contains("nop"))
                {
                    lines[i] = lines[i].Replace("nop", "jmp");
                    if (runCode(lines))
                    {
                        Console.WriteLine("Done!");
                        return;
                    }
                    lines[i] = lines[i].Replace("jmp", "nop");
                }
                if (lines[i].Contains("jmp"))
                {
                    lines[i] = lines[i].Replace("jmp", "nop");
                    if (runCode(lines))
                    {
                        Console.WriteLine("Done!");
                        return;
                    }
                    lines[i] = lines[i].Replace("nop", "jmp");
                }
            }
        }

        static bool runCode(string[] lines)
        {
            int i = 0;
            int acc = 0;
            var visited = new HashSet<int>();
            int executes = 0;
            while (true)
            {
                //Console.WriteLine("At " + i);
                if (i >= lines.Count())
                {
                    Console.WriteLine($"Ins {i}: acc {acc}");
                    return true;
                }
                executes++;

                var line = lines[i];
                var s = line.Split();
                var op = s[0];
                var val = int.Parse(s[1]);
                if (visited.Contains(i))
                {
                    //Console.WriteLine($"Ins {i}: acc {acc}");
                    return false;
                }

                visited.Add(i);


                switch (op)
                {
                    case "nop":
                        break;
                    case "acc":
                        acc += val;
                        break;
                    case "jmp":
                        i += val - 1;
                        break;
                }
                i++;
            }
        }

        static void d7()
        {
            var lines = ReadInput(7);
            var map = new List<Tuple<string, string, int>>();
            var mybag = "shiny gold";
            foreach (var s in lines)
            {
                if (s.Contains("no other"))
                {
                    continue;
                }
                var sp = s.Replace(",", "").Split();
                var color = sp[0] + " " + sp[1];
                var outColors = new List<string>();
                for (int i = 4; i < sp.Length; i += 4)
                {
                    var newColor = sp[i + 1] + " " + sp[i + 2];
                    var n = int.Parse(sp[i]);
                    map.Add(new Tuple<string, string, int>(color, newColor, n));
                }
            }

            //Part 1
            var visited = new HashSet<string>();
            var toVisit = new HashSet<string>();
            toVisit.Add(mybag);
            while (toVisit.Count() > 0)
            {
                var bag = toVisit.First();
                toVisit.Remove(bag);
                visited.Add(bag);

                var toBag = map.Where(m => m.Item2 == bag).ToList();
                foreach (var t in toBag)
                {
                    if (!visited.Contains(t.Item1))
                    {
                        toVisit.Add(t.Item1);
                    }
                }
            }
            Console.WriteLine($"Part 1: {visited.Count() - 1}");

            var expand = new List<Tuple<int, string>>();
            var containedBags = new List<Tuple<int, string>>();
            expand.Add(new Tuple<int, string>(1, mybag));
            while (expand.Count() > 0)
            {
                var current = expand.First();
                expand.RemoveAt(0);
                foreach (var t in map.Where(s => s.Item1 == current.Item2))
                {
                    var newB = new Tuple<int, string>(t.Item3 * current.Item1, t.Item2);
                    expand.Add(newB);
                    containedBags.Add(newB);
                }
            }
            int part2 = containedBags.Sum(s => s.Item1);
            Console.WriteLine($"Part 2: {part2}");
        }

        static void d6()
        {
            var groups = ReadInputAsString(6).Split("\n\n").ToList();

            //Part 1
            var groupsAny = groups.Select(s => s.Replace("\n", "").Distinct().Count());

            //Part 2
            int all = 0;
            foreach (var group in groups)
            {
                var split = group.Trim().Split('\n');
                IEnumerable<char> set = split[0].ToHashSet();
                for (int i = 1; i < split.Length; i++)
                {
                    set = set.Intersect(split[i].ToHashSet());
                }
                all += set.Count();
            }

            //Part 2 revisit
            var groupsAll = groups.Select(s => s.Trim().Split('\n').Select(s => (IEnumerable<char>)s).Aggregate((c, n) => c.Intersect(n)).Count()).Sum();

            Console.WriteLine($"Part 1: {groupsAny.Sum()}");
            Console.WriteLine($"Part 2.0: {all}");
            Console.WriteLine($"Part 2.1: {groupsAll}");
        }

        static void d5()
        {
            var ids = ReadInput(5).Select(s => seatId(s)).ToList();
            ids.Sort();
            Console.WriteLine($"Max ID: {ids.Max()}");
            for (int i = 1; i < ids.Count(); i++)
            {
                if (ids[i] - ids[i - 1] != 1)
                {
                    Console.WriteLine($"Your ID: {ids[i] - 1}");
                    break;
                }
            }
        }

        static int seatId(string code)
        {
            int rMin = 0;
            int rMax = 128;
            int cMin = 0;
            int cMax = 8;
            foreach (var c in code)
            {
                switch (c)
                {
                    case 'B': rMin = (rMax + rMin) / 2; break;
                    case 'F': rMax = (rMax + rMin) / 2; break;
                    case 'R': cMin = (cMax + cMin) / 2; break;
                    case 'L': cMax = (cMax + cMin) / 2; break;
                }
            }
            return rMin * 8 + cMin;
        }

        static int seatIdBin(string code)
        {
            //return Convert.ToInt32(code.Replace('B', '1').Replace('R', '1').Replace('F', '0').Replace('L', '0'), 2);
            return Convert.ToInt32(tr(code, "BRFL", "1100"), 2);
        }

        static string tr(string s, string from, string to)
        {
            int len = Math.Min(from.Length, to.Length);
            for (int i = 0; i < len; i++)
            {
                s = s.Replace(from[i], to[i]);
            }
            return s;
        }

        static void d4_linq()
        {
            var byr = @"byr:(19[2-9]\d|200[0-2])\b";
            var iyr = @"iyr:(201\d|2020)\b";
            var eyr = @"eyr:(202\d|2030)\b";
            var hgt = @"hgt:((1[5-8]\d|19[0-3])cm|(59|6\d|7[0-6])in)\b";
            var hcl = @"hcl:#[0-9a-f]{6}\b";
            var ecl = @"ecl:(amb|blu|brn|gry|grn|hzl|oth)";
            var pid = @"pid:\d{9}\b";
            var valid = ReadInputAsString(4)
                .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Replace('\n', ' '))
                .Where(s => Regex.IsMatch(s, byr))
                .Where(s => Regex.IsMatch(s, eyr))
                .Where(s => Regex.IsMatch(s, iyr))
                .Where(s => Regex.IsMatch(s, hgt))
                .Where(s => Regex.IsMatch(s, hcl))
                .Where(s => Regex.IsMatch(s, ecl))
                .Where(s => Regex.IsMatch(s, pid));
            Console.WriteLine(valid.Count());
        }

        static void d4()
        {
            var lines = ReadInput(4);
            int valid = 0;
            var props = new List<string> { "byr", "iyr", "eyr", "hgt", "hcl", "ecl", "pid" };
            var set = new HashSet<string>();
            var eyeColors = new HashSet<string> { "amb", "blu", "brn", "gry", "grn", "hzl", "oth" };
            foreach (var l in lines)
            {
                if (l.Trim().Length == 0)
                {
                    if (set.Count() == props.Count()) valid++;
                    set.Clear();
                    continue;
                }

                var split = l.Split();
                foreach (var s in split)
                {
                    var property = s.Split(':')[0];
                    var value = s.Split(':')[1];
                    if (props.Contains(property))
                    {
                        bool pValid = false;
                        switch (property)
                        {
                            case "byr":
                                int byrVal;
                                if (value.Length == 4 && int.TryParse(value, out byrVal))
                                {
                                    pValid = byrVal >= 1920 && byrVal <= 2002;
                                }
                                break;
                            case "iyr":
                                int iyrVal;
                                if (value.Length == 4 && int.TryParse(value, out iyrVal))
                                {
                                    pValid = iyrVal >= 2010 && iyrVal <= 2020;
                                }
                                break;
                            case "eyr":
                                int eyrVal;
                                if (value.Length == 4 && int.TryParse(value, out eyrVal))
                                {
                                    pValid = eyrVal >= 2020 && eyrVal <= 2030;
                                }
                                break;
                            case "hgt":
                                if (Regex.IsMatch(value, @"^\d+(in|cm)$"))
                                {
                                    if (value.Contains("cm"))
                                    {
                                        int h = int.Parse(value.Replace("cm", ""));
                                        pValid = h >= 150 && h <= 193;
                                    }
                                    if (value.Contains("in"))
                                    {
                                        int h = int.Parse(value.Replace("in", ""));
                                        pValid = h >= 59 && h <= 76;
                                    }
                                }
                                break;
                            case "hcl":
                                pValid = Regex.IsMatch(value, @"^#[0-9a-f]{6}$");
                                break;
                            case "ecl":
                                pValid = eyeColors.Contains(value);
                                break;
                            case "pid":
                                pValid = Regex.IsMatch(value, @"^\d{9}$");
                                break;
                        }
                        if (pValid) set.Add(property);
                    }
                }
            }

            Console.WriteLine(valid);
        }

        static void d3()
        {
            var lines = ReadInput(3);
            UInt64 trees =
              Tob(1, 1, lines)
            * Tob(3, 1, lines)
            * Tob(5, 1, lines)
            * Tob(7, 1, lines)
            * Tob(1, 2, lines);
            Console.WriteLine(trees);
        }

        static UInt64 Tob(int xSpeed, int ySpeed, string[] lines)
        {
            int x = 0;
            int trees = 0;
            for (int y = 0; y < lines.Length; y += ySpeed)
            {
                var l = lines[y];
                if (l[x] == '#') trees++;
                x = (x + xSpeed) % lines[0].Length;
            }
            return (UInt64)trees;
        }

        static void d2()
        {
            var lines = ReadInput(2);
            int correct = 0;
            foreach (var s in lines)
            {
                var split = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var times = split[0].Split('-', StringSplitOptions.RemoveEmptyEntries);
                int min = int.Parse(times[0]);
                int max = int.Parse(times[1]);
                char letter = split[1][0];
                var password = split[2];

                //var count = password.Count(c => c == letter);
                //if (count >= min && count <= max) correct++;
                if ((password[min - 1] == letter) != (password[max - 1] == letter)) correct++;
            }
            Console.WriteLine(correct);
        }

        static void d1()
        {
            var lines = ReadInput(1).Select(s => int.Parse(s)).ToList();
            for (int i = 0; i < lines.Count - 2; i++)
                for (int j = i + 1; j < lines.Count - 1; j++)
                    for (int k = j + 1; k < lines.Count; k++)
                        if (lines[i] + lines[j] + lines[k] == 2020)
                        {
                            Console.WriteLine(lines[i] * lines[j] * lines[k]);
                            return;
                        }
        }

        static void print(string s)
        {
            Console.WriteLine(s);
        }

        static void print(int i)
        {
            print(i.ToString());
        }

        static void print(long i)
        {
            print(i.ToString());
        }

        static string[] ReadInput(int day)
        {
            string path = @"C:\git\AdventOfCode\AdventOfCode2020\Input\d" + day + ".txt";
            return File.ReadAllLines(path);
        }

        static string ReadInputAsString(int day)
        {
            string path = @"C:\git\AdventOfCode\AdventOfCode2020\Input\d" + day + ".txt";
            return File.ReadAllText(path);
        }
    }
}
