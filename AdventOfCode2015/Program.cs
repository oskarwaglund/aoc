using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdventOfCode2015
{
    class Program
    {
        static void Main(string[] args)
        {
            d12();
        }

        class BackRef
        {
            public int line;
            public bool obj;
            public bool last;

            public BackRef(int l, bool o, bool la)
            {
                line = l;
                obj = o;
                last = la;
            }
        }

        static void d12()
        {
            var test = ReadInput(12)[0];
            //string test = "{\"a\": [1,{\"c\":\"red\",\"b\":2},3]}";
            //string test = "{\"a\": {\"d\":\"red\",\"e\":[1,2,3,4],\"f\":5}}";
            //string test = "{\"a\": [1,\"red\",5]}";

            //Console.WriteLine(JObject.Parse(test).ToString());
            prune(test);
            //var json = JObject.Parse(prune(test));

            var pretty = prune(test).Split("\n");
            int result = 0;
            foreach(var s in pretty)
            {
                var match = Regex.Match(s, @"(-?\d+)");
                if (match.Success)
                {
                    result += int.Parse(match.Captures[0].Value);
                }
            }
            Console.WriteLine(result);
        }

        static string prune(string jsonString)
        {
            var json = JObject.Parse(jsonString).ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
            var obj = new Stack<BackRef>();
            for (int i = json.Count() - 1; i >= 0; i--)
            {
                var s = json[i];
                if (s.Contains("}")) obj.Push(new BackRef(i, true, !s.Contains(","))); //Object
                if (s.Contains("]")) obj.Push(new BackRef(i, false, !s.Contains(","))); //array
                if (s.Contains("[") && obj.Pop().obj)
                    throw new Exception("Expected array start");
                if (s.Contains("{") && !obj.Pop().obj)
                    throw new Exception("Expected object start");
                //Console.WriteLine(obj.Count());

                if (s.Contains("red"))
                {
                    if (obj.Peek().obj)
                    {
                        //In Object

                        //Find opener
                        int level = 1;
                        while(level > 0)
                        {
                            i--;
                            if (json[i].Contains("}")) level++;
                            if (json[i].Contains("{")) level--;
                        }
                        var br = obj.Pop();
                        if (!br.obj) throw new Exception("Unexpected [");

                        Console.WriteLine($"*** Removing section @ line {i}!");
                        Console.WriteLine(json[i - 1]);
                        Console.WriteLine("--------");
                        for (int j = 0; j <= br.line - i; j++)
                        {
                            Console.WriteLine(json[i]);
                            json.RemoveAt(i);
                            foreach(var b in obj)
                            {
                                b.line--;
                            }
                        }
                        Console.WriteLine("--------");
                        Console.WriteLine(json[i]);
                        Console.WriteLine("*** Done!");
                        Console.WriteLine();
                        JObject.Parse(string.Join('\n', json));
                    }
                    else
                    {
                        //In Array
                        json.RemoveAt(i);
                        JObject.Parse(string.Join('\n', json));
                    }
                    //Validate
                }
            }
            return string.Join('\n', json);
        }


        static void d25()
        {
            long row = 2981-1;
            long col = 3075-1;
            long val = 20151125;

            int r = 0;
            int c = 0;
            int currRow = 0;
            for (long i = 0;; i++)
            {
                if(r == row && c == col)
                {
                    Console.WriteLine($"row {r} col {c} val {val}");
                    return;
                }
                val = (val * 252533L) % 33554393L;
                c++;
                r--;
                if(r < 0)
                {
                    currRow++;
                    r = currRow;
                    c = 0;
                }
            }


            /*for(long r = 1; ; r++)
            {
                for(long t = 0; t <= r; t++)
                {
                    long c = r - t;
                    dic[index(r, c)] = dic[index(r, c)]
                }
            }*/
        }

        static long index(long r, long c)
        {
            return r * 4294967296L + c;
        }

        static void d24()
        {
            var pkgs = ReadInput(24).Select(s => long.Parse(s)).ToList();
            pkgs.Sort();
            pkgs.Reverse();
            long total = pkgs.Sum();
            long count = long.MaxValue;
            long qe = long.MaxValue;
            balance("", pkgs, total / 4, ref count, ref qe);
            Console.WriteLine("Count: " + count);
            Console.WriteLine("QE: " + qe);
        }

        static long weight(string s)
        {
            return s.Split().Select(s => long.Parse(s)).Sum();
        }

        //best = weight*10000 + QE
        static void balance(string curr, List<long> pkgs, long goal, ref long bestCount, ref long bestQE)
        {
            var used = curr.Split(" ", StringSplitOptions.RemoveEmptyEntries).Select(s => long.Parse(s)).ToArray();
            long w = used.Sum();
            long c = used.Count();
            long qe = c > 0 ? used.Aggregate((a, b) => a * b) : long.MaxValue;
            
            if (c > bestCount || w > goal)
            {
                return;
            }
            if (w == goal && qe < bestQE)
            {
                Console.WriteLine(curr + " qe " + qe);
                bestCount = c;
                bestQE = qe;
            }
            foreach(var n in used.Count() > 0 ? pkgs.Where(s => s < used.Min()) : pkgs)
            {
                balance((curr + " " + n).Trim(), pkgs, goal, ref bestCount, ref bestQE);
            }
        }

        static void d23()
        {
            var lines = ReadInput(23).Select(s => s.Replace(",", "")).ToArray();
            int i = 0;
            var regs = new Dictionary<string, int>()
            {
                { "a", 1 },
                { "b", 0 }
            };
            while (true)
            {
                if (i >= lines.Count()) break;
                var l = lines[i].Split();
                switch (l[0])
                {
                    case "hlf":
                        regs[l[1]] /= 2;
                        break;
                    case "tpl":
                        regs[l[1]] *= 3;
                        break;
                    case "inc":
                        regs[l[1]] += 1;
                        break;
                    case "jmp":
                        i += int.Parse(l[1]) - 1;
                        break;
                    case "jie":
                        i += regs[l[1]]%2 == 0 ? int.Parse(l[2]) - 1 : 0;
                        break;
                    case "jio":
                        i += regs[l[1]]==1 ? int.Parse(l[2]) - 1 : 0;
                        break;
                }
                i++;
            }
            Console.WriteLine(regs["b"]);
        }

        class Spell
        {
            public int cost;
            public int dur;
            public int armor;
            public int dmgPerTurn;
            public int manaPerTurn;
            public int hpPerTurn;

            public Spell(int _cost,
            int _dur,
            int _armor,
            int _dmgPerTurn,
            int _manaPerTurn,
            int _hpPerTurn)
            {
                cost = _cost;
                dur = _dur;
                armor = _armor;
                dmgPerTurn = _dmgPerTurn;
                manaPerTurn = _manaPerTurn;
                hpPerTurn = _hpPerTurn;
            }

            public Spell(Spell s)
            {
                cost = s.cost;
                dur = s.dur;
                armor = s.armor;
                dmgPerTurn = s.dmgPerTurn;
                manaPerTurn = s.manaPerTurn;
                hpPerTurn = s.hpPerTurn;
            }

            public string ToString()
            {
                return $"{cost}: dur {dur}";
            }
        }

        static List<Spell> spells = new List<Spell>
        {
            new Spell(53, 0, 0, 4, 0, 0), //Missile,0
            new Spell(73, 0, 0, 2, 0, 2), //Drain,1
            new Spell(113, 6, 7, 0, 0, 0), //Shield,2
            new Spell(173, 6, 0, 3, 0, 0), //Poison,3
            new Spell(229, 5, 0, 0, 101, 0), //Recharge,4
        };

        static void d22()
        {
            //Console.WriteLine(fightMagic("42130"));
            Console.WriteLine(fightDfs("", int.MaxValue));
        }

        static int fightDfs(string spell, int best)
        {
            //Console.WriteLine(spell);
            if(manaCost(spell) >= best)
            {
                return best;
            }
            //Do the fight
            var outcome = fightMagic(spell);
            switch (outcome)
            {
                case MagicResult.Win:
                    //We won
                    return Math.Min(manaCost(spell), best);
                case MagicResult.OutOfSpells:
                    //Need to go deeper
                    break;
                default:
                    //Dead, OutOfMana or ActiveSpell
                    //Unrecoverable
                    return best;
            }
            for(int i = 4; i >= 0; i--)
            {
                int res = fightDfs(spell + i, best);
                if(res < best)
                {
                    best = res;
                }
            }
            return best;
        }

        static int manaCost(string spell)
        {
            return spell.Sum(c => spells[c - '0'].cost);
        }

        enum MagicResult {
            Win,
            Dead,
            OutOfMana,
            OutOfSpells,
            ActiveSpellCalled
        }

        static MagicResult fightMagic(string spellOrder)
        {
            int hp = 50;
            int mana = 500;
            int bossHp = 58;
            int bossDmg = 9;

            var activeSpells = new List<Spell>();
            int turn = 0;
            while (true)
            {
                int armor = 0;
                //Console.WriteLine($"{(turn % 2 == 0 ? "Player" : "Boss")} turn:");
                //Console.WriteLine($"- Player has {hp} hit points, {armor} armor, {mana} mana");
                //Console.WriteLine($"- Boss has {bossHp} hit points");
                //Console.WriteLine("Spells take effect!");
                for (int i = activeSpells.Count() - 1; i >= 0; i--)
                {
                    var s = activeSpells[i];
                    bossHp -= s.dmgPerTurn;
                    mana += s.manaPerTurn;
                    hp += s.hpPerTurn;
                    armor += s.armor;
                    s.dur--;
                    if (s.dur <= 0)
                    {
                        activeSpells.RemoveAt(i);
                    }
                    //Console.WriteLine(s.ToString());
                }
                //Console.WriteLine($"- Player has {hp} hit points, {armor} armor, {mana} mana");
                //Console.WriteLine($"- Boss has {bossHp} hit points");
                //Console.WriteLine("----------");
                if (bossHp <= 0) return MagicResult.Win;

                //Log some stuff

                if (turn % 2 == 0)
                {
                    //Player turn

                    //Part 2 condition
                    hp--;
                    if (hp <= 0) return MagicResult.Dead;

                    if (spellOrder.Length == 0) return MagicResult.OutOfSpells;
                    int spellIndex = spellOrder[0] - '0';
                    var spell = spells[spellIndex];
                    spellOrder = spellOrder.Substring(1);

                    mana -= spell.cost;
                    if (mana < 0) return MagicResult.OutOfMana;
                    if (spell.dur > 0)
                    {
                        if (activeSpells.Any(s => s.cost == spell.cost)) return MagicResult.ActiveSpellCalled;
                        activeSpells.Add(new Spell(spell));
                    }
                    else
                    {
                        bossHp -= spell.dur == 0 ? spell.dmgPerTurn : 0;
                        hp += spell.dur == 0 ? spell.hpPerTurn : 0;

                        if (bossHp <= 0) return MagicResult.Win;
                    }
                } 
                else
                {
                    //Boss turn
                    int absDmg = Math.Max(1, bossDmg - armor);
                    hp -= absDmg;
                    if (hp <= 0) return MagicResult.Dead;
                }
                turn++;
            }
        }

        class Gear
        {
            public int cost;
            public int dmg;
            public int armor;

            public Gear(int c, int d, int a)
            {
                cost = c;
                dmg = d;
                armor = a;
            }
        }

        static List<Gear> weapons = new List<Gear>()
            {
                new Gear(8, 4, 0),
                new Gear(10, 5, 0),
                new Gear(25, 6, 0),
                new Gear(40, 7, 0),
                new Gear(74, 8, 0),
            };
        static List<Gear> armors = new List<Gear>()
            {
                new Gear(13, 0, 1),
                new Gear(31, 0, 2),
                new Gear(53, 0, 3),
                new Gear(75, 0, 4),
                new Gear(102, 0, 5),
                new Gear(0, 0, 0) //Naked
            };
        static List<Gear> rings = new List<Gear>()
            {
                new Gear(25, 1, 0),
                new Gear(50, 2, 0),
                new Gear(100, 3, 0),
                new Gear(20, 0, 1),
                new Gear(40, 0, 2),
                new Gear(80, 0, 3),
                new Gear(0, 0, 0), //No ring

            };

        static void d21()
        {
            //Try it out
            //Console.WriteLine(fight(8, 5, 5, 12, 7, 2) ? "You win":"You lose");

            //Console.WriteLine(allWeapons());
            int best = int.MaxValue;
            int best2 = int.MinValue;
            for (int w = 0; w < weapons.Count(); w++)
            {
                for (int a = 0; a < armors.Count(); a++)
                {
                    for (int r1 = 0; r1 < rings.Count(); r1++)
                    {
                        for (int r2 = r1 + 1; r2 < rings.Count(); r2++)
                        {
                            var gear = new List<Gear>()
                            {
                                weapons[w],
                                armors[a],
                                rings[r1],
                                rings[r2]
                            };
                            if (fight(gear))
                            {
                                best = Math.Min(best, gearCost(gear));
                            }
                        }
                    }
                }
            }
            Console.WriteLine(best);


            for (int w = 0; w < weapons.Count(); w++)
            {
                for (int a = 0; a < armors.Count(); a++)
                {
                    for (int r1 = 0; r1 < rings.Count(); r1++)
                    {
                        for (int r2 = r1 + 1; r2 < rings.Count(); r2++)
                        {
                            var gear = new List<Gear>()
                            {
                                weapons[w],
                                armors[a],
                                rings[r1],
                                rings[r2]
                            };
                            if (!fight(gear))
                            {
                                best2 = Math.Max(best2, gearCost(gear));
                            }
                        }
                    }
                }
            }
            Console.WriteLine(best2);

        }

        static int gearCost(List<Gear> gear)
        {
            return gear.Sum(s => s.cost);
        }

        //Hit Points: 100
        //Damage: 8
        //Armor: 2

        static bool fight(List<Gear> gear)
        {
            int bossHp = 100;
            int bossDmg = 8;
            int bossArmor = 2;

            int hp = 100;
            int dmg = gear.Sum(s => s.dmg);
            int armor = gear.Sum(s => s.armor);

            return fight(hp, dmg, armor, bossHp, bossDmg, bossArmor);
        }

        //True if player win
        static bool fight(int hp, int dmg, int armor, int bossHp, int bossDmg, int bossArmor)
        {
            int dmgToPlayer = Math.Max(1, bossDmg - armor);
            int dmgToBoss = Math.Max(1, dmg - bossArmor);
            while (true)
            {
                bossHp -= dmgToBoss;
                if (bossHp <= 0) return true;
                hp -= dmgToPlayer;
                if (hp <= 0) return false;
            }
        }

        static void d20()
        {
            long input = 33100000;
            for (long h = 1; h < input; h++)
            {
                var gifts = giftPerHouse2(h);
                if (gifts >= input)
                {
                    Console.WriteLine(h + ": " + gifts);
                    return;
                }
            }
        }

        static long giftPerHouse(long house)
        {
            long gifts = 0;
            for (long elf = 1; elf * elf <= house; elf++)
            {
                if (house % elf == 0)
                {
                    gifts += elf;
                    if (elf * elf != house)
                    {
                        gifts += house / elf;
                    }
                }
            }

            return gifts * 10;
        }

        static long giftPerHouse2(long house)
        {
            long gifts = 0;
            for (long elf = 1; elf * elf <= house; elf++)
            {
                if (house % elf != 0) continue;

                long lower = elf;
                long upper = house / elf;
                if (house / lower <= 50)
                {
                    gifts += lower;
                }
                if (house / upper <= 50 && upper != lower)
                {
                    gifts += upper;
                }
            }

            return gifts * 11;
        }

        static string[] ReadInput(int day)
        {
            string path = @"C:\git\AdventOfCode\AdventOfCode2015\Input\d" + day + ".txt";
            return File.ReadAllLines(path);
        }

        static string ReadInputAsString(int day)
        {
            string path = @"C:\git\AdventOfCode\AdventOfCode2015\Input\d" + day + ".txt";
            return File.ReadAllText(path);
        }
    }
}
