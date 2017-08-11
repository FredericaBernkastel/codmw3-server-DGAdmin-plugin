using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace LambAdmin
{
    public partial class DGAdmin
    {
        public class PlayersFilter
        {
            private struct Selector
            {
                public string selector;
                public int operation_type;
                public bool isComplement;
            }

            private DGAdmin script;
            private Entity sender;

            private const int DISJUNCTION = 0;
            private const int CONJUNCTION = 1;

            private List<Selector> selectors;

            private List<string> Filters = new List<string>() {
                "all",
                "allies",
                "enemies",
                "team1",
                "team2",
                "spectators",
                "alive",
                "me"
            };

            private class EntityComparer : IEqualityComparer<Entity>
            {
                public bool Equals(Entity x, Entity y)
                {
                    return x.EntRef == y.EntRef;
                }

                public int GetHashCode(Entity x)
                {
                    return x.GetHashCode();
                }
            }

            private List<Entity> INTERSECT(List<Entity> set1, List<Entity> set2)
            {
                return set1.Intersect(set2, new EntityComparer()).ToList();
            }

            private List<Entity> UNION(List<Entity> set1, List<Entity> set2)
            {
                return set1.Union(set2, new EntityComparer()).ToList();
            }

            private List<Entity> COMPLEMENT(List<Entity> U, List<Entity> set)
            {
                return U.Except(set, new EntityComparer()).ToList();
            }

            public PlayersFilter(DGAdmin script, Entity sender)
            {
                this.script = script;
                this.sender = sender;
                this.selectors = new List<Selector>();
            }
            public List<Entity> Filter(string filter)
            {
                List<Entity> result = new List<Entity>();
                if (!SyntaxCheck(filter))
                {
                    script.WriteChatToPlayer(sender, Command.GetMessage("Filters_error1"));
                    return result;
                }

                if ((filter.IndexOf("&") != -1) || (filter.IndexOf("|") != -1))
                    Parse(filter);
                else
                {
                    bool isComplement = ComplementCheck(ref filter);
                    selectors.Add(new Selector { selector = filter, operation_type = DISJUNCTION, isComplement = isComplement });
                }

                selectors.Reverse();

                foreach (var selector in selectors.Select((value, i) => new { i, value }))
                {
                    if (String.IsNullOrWhiteSpace(selector.value.selector))
                    {
                        script.WriteChatToPlayer(sender, Command.GetMessage("Filters_error1"));
                        return result;
                    }
                    WriteLog.Debug("selector: " + selector.value.selector + ", type: " + selector.value.operation_type.ToString() + ", copmlement: " + selector.value.isComplement.ToString());
                }

                //begin the processing
                foreach (Selector selector in selectors)
                {
                    if (!Filters.Contains(selector.selector.ToLowerInvariant()))
                    {
                        script.WriteChatToPlayer(sender, Command.GetMessage("Filters_error2").Format(new Dictionary<string, string>(){
                            { "<selector>", selector.selector }
                        }));
                        return new List<Entity>();
                    }
                    else
                    {
                        List<Entity> set = new List<Entity>();
                        switch (selector.selector.ToLowerInvariant())
                        {
                            case "all":
                                set = _sel_all(selector.isComplement);
                                break;
                            case "allies":
                                set = _sel_allies(selector.isComplement);
                                break;
                            case "enemies":
                                set = _sel_enemies(selector.isComplement);
                                break;
                            case "team1":
                                set = _sel_team1(selector.isComplement);
                                break;
                            case "team2":
                                set = _sel_team2(selector.isComplement);
                                break;
                            case "spectators":
                                set = _sel_spectators(selector.isComplement);
                                break;
                            case "alive":
                                set = _sel_alive(selector.isComplement);
                                break;
                            case "me":
                                set = _sel_me(selector.isComplement);
                                break;

                        }
                        switch (selector.operation_type)
                        {
                            case DISJUNCTION:
                                result = UNION(result, set);
                                break;
                            case CONJUNCTION:
                                result = INTERSECT(result, set);
                                break;
                        }
                    }
                }

                return result;
            }

            private bool SyntaxCheck(string filter)
            {
                if (String.IsNullOrWhiteSpace(filter))
                    return false;

                bool _checked = true;
                (new List<string>() { "&", "|" }).ForEach(s =>
                {
                    _checked = _checked && !filter.StartsWith(s);
                    _checked = _checked && !filter.EndsWith(s);
                    _checked = _checked && (filter.IndexOf(s + s) == -1);
                });
                _checked = _checked && !(filter == "-");

                return _checked;
            }
            private void Parse(string filter)
            {
                int lastpos = filter.Length - 1;
                for (int i = filter.Length - 1; i >= 0; i--)
                {
                    if ((filter[i] == '&') || (filter[i] == '|'))
                    {
                        string selector = filter.Substring(i + 1, lastpos - i);
                        int operation_type = DISJUNCTION;
                        switch (filter[i])
                        {
                            case '&': operation_type = CONJUNCTION; break;
                            case '|': operation_type = DISJUNCTION; break;
                        }
                        bool isComplement = ComplementCheck(ref selector);
                        selectors.Add(new Selector { selector = selector, operation_type = operation_type, isComplement = isComplement });
                        lastpos = i;
                    }
                }
                string _selector = filter.Substring(0, lastpos);
                bool _isComplement = ComplementCheck(ref _selector);
                selectors.Add(new Selector { selector = _selector, operation_type = DISJUNCTION, isComplement = _isComplement });

            }
            private bool ComplementCheck(ref string selector)
            {
                if (selector[0] == '-')
                {
                    selector = selector.Substring(1, selector.Length - 1);
                    return true;
                }
                return false;
            }

            private List<Entity> _sel_all(bool isComplement)
            {
                if (!isComplement)
                    return script.Players;
                else
                    return new List<Entity>();
            }
            private List<Entity> _sel_allies(bool isComplement)
            {
                List<Entity> result = (
                    from player in script.Players
                    where player.GetTeam() == sender.GetTeam()
                    select player).ToList();
                if (isComplement)
                    result = COMPLEMENT(script.Players, result);
                return result;
            }
            private List<Entity> _sel_enemies(bool isComplement)
            {
                List<Entity> result = new List<Entity>();
                if (sender.GetTeam() == "axis")
                    result = (
                    from player in script.Players
                    where player.GetTeam() == "allies"
                    select player).ToList();
                else
                if (sender.GetTeam() == "allies")
                    result = (
                    from player in script.Players
                    where player.GetTeam() == "axis"
                    select player).ToList();

                if (isComplement)
                    result = COMPLEMENT(script.Players, result);
                return result;
            }
            private List<Entity> _sel_team1(bool isComplement)
            {
                List<Entity> result = (
                    from player in script.Players
                    where player.GetTeam() == "axis"
                    select player).ToList();

                if (isComplement)
                    result = COMPLEMENT(script.Players, result);
                return result;
            }
            private List<Entity> _sel_team2(bool isComplement)
            {
                List<Entity> result = (
                    from player in script.Players
                    where player.GetTeam() == "allies"
                    select player).ToList();

                if (isComplement)
                    result = COMPLEMENT(script.Players, result);
                return result;
            }
            private List<Entity> _sel_spectators(bool isComplement)
            {
                List<Entity> result = (
                    from player in script.Players
                    where player.IsSpectating()
                    select player).ToList();

                if (isComplement)
                    result = COMPLEMENT(script.Players, result);
                return result;
            }
            private List<Entity> _sel_alive(bool isComplement)
            {
                List<Entity> result = (
                    from player in script.Players
                    where player.IsAlive
                    select player).ToList();

                if (isComplement)
                    result = COMPLEMENT(script.Players, result);
                return result;
            }
            private List<Entity> _sel_me(bool isComplement)
            {
                List<Entity> result = new List<Entity>() { sender };

                if (isComplement)
                    result = COMPLEMENT(script.Players, result);
                return result;
            }
        }
    }
}
