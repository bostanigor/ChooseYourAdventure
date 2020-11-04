using System.Collections.Generic;
using System.Text;

namespace ChooseYourAdventure
{
    public class FactState
    {
        private FactState _parent;
        private bool[] _state;
        private Rule _rule;
        
        public HashSet<Rule> AvailableRules { get; set; }

        public FactState(bool[] state, FactState parent, Rule rule, HashSet<Rule> availableRules)
        {
            _parent = parent;
            _state = state;
            _rule = rule;
            AvailableRules = availableRules;
        }

        public FactState ApplyRule(Rule rule)
        {
            var newFact = rule.Apply(_state);
            if (newFact != null && _state[newFact.Id] == false)
            {
                var newRules = new HashSet<Rule>(AvailableRules);
                newRules.Remove(rule);
                var newState = new bool[_state.Length];
                _state.CopyTo(newState, 0);
                newState[newFact.Id] = true;
                return new FactState(newState, this, rule, newRules);
            }
            return null;
        }

        public bool ContainsFactsIds(int[] ids)
        {
            foreach (var id in ids)
                if (_state[id] == false)
                    return false;
            return true;
        }

        public bool Equals(FactState other)
        {
            for (int i = 0; i < _state.Length; i++)
                if (this._state[i] != other._state[i])
                    return false;
            return true;
        }

        public bool[] GetState() => _state;
        public FactState GetParent() => _parent;
        public Rule GetRule() => _rule;
    }

    public class BackwardNode
    {
        public List<BackwardNode> Parents { get; private set; }
        public Fact Fact { get; private set; }
        public Rule Rule { get; private set; }

        public int ChildrenCount { get; private set; }
        public int GoodChildrenCount { get; private set; }

        public BackwardNode(Fact fact, Rule rule, List<BackwardNode> parents)
        {
            Fact = fact;
            Rule = rule;
            Parents = parents;
            GoodChildrenCount = 0;
        }
    }
    public class FactStateComparer : IEqualityComparer<FactState>
    {
        public bool Equals(FactState x, FactState y) =>
            x.Equals(y);

        public int GetHashCode(FactState item) =>
            item.GetState().GetHashCode();
    }

    public class Rule
    {
        private Fact[] _antecedents;
        private Fact _consequent;

        public Rule(Fact[] antecedents, Fact consequent)
        {
            _antecedents = antecedents;
            _consequent = consequent;
        }

        public override string ToString()
        {
            var sB = new StringBuilder();
            foreach (var a in _antecedents)
                sB.Append($"{a.Desc}, ");
            sB.Remove(sB.Length - 2, 2);
            sB.Append($" -> {_consequent.Desc}");
            return sB.ToString();
        }

        public Fact Apply(bool[] state)
        {
            foreach (var a in _antecedents)
                if (state[a.Id] == false)
                    return null;
            return _consequent;
        }
    }

    public class Fact
    {
        public int Id { get; private set; }
        public string Desc { get; private set; }

        public Fact(int id, string desc)
        {
            Id = id;
            Desc = desc;
        }
    }
}