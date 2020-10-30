using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace ChooseYourAdventure
{
    public class ProductionSystem
    {
        private Dictionary<int, Fact> _idToFacts;
        private Dictionary<string, Fact> _descToFacts;
        private List<Fact> _facts;
        private List<Rule> _rules;
        public ProductionSystem(string factsPath, string rulesPath)
        {
            _idToFacts = new Dictionary<int, Fact>();
            _descToFacts = new Dictionary<string, Fact>();
            _rules = new List<Rule>();
            _facts = new List<Fact>();

            using (StreamReader sr = new StreamReader(factsPath))
            {
                int i = 0;
                while (sr.Peek() >= 0)
                {
                    var fact = new Fact(i++, sr.ReadLine());
                    _idToFacts[fact.Id] = fact;
                    _descToFacts[fact.Desc] = fact;
                    _facts.Add(fact);
                }
            }
            using (StreamReader sr = new StreamReader(rulesPath)) {
                while (sr.Peek() >= 0)
                {
                    var line = sr.ReadLine();
                    var temp = line.Split(new[] {"->"}, StringSplitOptions.None);
                    var left = temp[0].Trim();
                    var right = temp[1].Trim();
                    var antecdents = left.Split(',').Select(str => _descToFacts[str.Trim()]);
                    var consequent = _descToFacts[right];
                    _rules.Add(new Rule(antecdents.ToArray(), consequent));
                }
            }

            _rules.ForEach(rule => Console.WriteLine(rule));
        }

        public void ForwardSearch(string[] startFacts, string[] endFacts)
        {
            var state = new bool[_facts.Count];
            var rulesSet = new HashSet<Rule>(_rules);
            Array.ForEach(startFacts, desc => state[_descToFacts[desc].Id] = true);

            var flag = true;
            while (flag)
            {
                var toRemove = new List<Rule>();
                flag = false;
                foreach (var rule in rulesSet)
                {
                    var newFact = rule.Apply(state);
                    if (newFact != null)
                    {
                        state[newFact.Id] = true;
                        toRemove.Add(rule);
                        flag = true;
                        break;
                    }
                }
                toRemove.ForEach(rule => rulesSet.Remove(rule));
            }

            var result = new List<Fact>();
            for (int i = 0; i < state.Length; i++)
                if (state[i] == true)
                    result.Add(_idToFacts[i]);
            result.ForEach(fact => Console.WriteLine(fact.Desc));
        }
    }

    class FactState
    {
        private FactState _parent;
        private bool[] _state;
        private Rule _rule;

        public FactState(bool[] state, FactState parent, Rule rule)
        {
            _parent = parent;
            _state = state;
            _rule = rule;
        }
    }

    class Rule
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
            sB.Append($"-> {_consequent.Desc}");
            return sB.ToString();
        }

        public Fact? Apply(bool[] state)
        {
            foreach (var a in _antecedents)
                if (state[a.Id] == false)
                    return null;
            return _consequent;
        }
    }

    class Fact
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