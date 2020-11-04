using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ChooseYourAdventure
{
    public class ProductionSystem
    {
        private Dictionary<int, Fact> _idToFacts;
        private Dictionary<string, Fact> _descToFacts;
        private Dictionary<Fact, List<Rule>> _factToRules;
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

            //_rules.ForEach(rule => Console.WriteLine(rule));
        }

        public void ForwardSearch(string[] startFacts, string[] endFacts)
        {
            var endFactsIds = endFacts.Select(desc => _descToFacts[desc].Id).ToArray(); 
            var startBoolState = new bool[_facts.Count];
            Array.ForEach(startFacts, desc => startBoolState[_descToFacts[desc].Id] = true);
            var startRuleSet = new HashSet<Rule>(_rules);
            var startState = new FactState(startBoolState, null, null, startRuleSet);
            
            var stateSet = new HashSet<FactState>(new FactStateComparer());
            stateSet.Add(startState);
            
            var stateQueue = new Queue<FactState>();
            stateQueue.Enqueue(startState);

            FactState result = null;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stateQueue.Count != 0 && result == null)
            {
                var state = stateQueue.Dequeue();
                foreach (var rule in state.AvailableRules)
                {
                    var newState = state.ApplyRule(rule);
                    if (newState != null && !stateSet.Contains(newState))
                    {
                        if (newState.ContainsFactsIds(endFactsIds)) {
                            result = newState;
                            break;
                        }

                        stateSet.Add(newState);
                        stateQueue.Enqueue(newState);
                    }
                }
            }

            stopwatch.Stop();
            if (result == null)
                Console.WriteLine("NOT REACHABLE");
            else
            {
                var node = result;
                while (node != null)
                {
                    Console.WriteLine(StateToString(node.GetState()));
                    Console.WriteLine($"  {node.GetRule()}");
                    node = node.GetParent();
                }
            }
            Console.WriteLine($"Elapsed time is {stopwatch.ElapsedMilliseconds / 1000.0} seconds");
        }

        public void BackwardSearch(string[] startFacts, string[] endFacts)
        {
            var factNodes = endFacts.Select(desc =>
                new BackwardNode(_descToFacts[desc], null, null)).ToList();
            var queue = new Queue<BackwardNode>();
            factNodes.ForEach(fact => queue.Enqueue(fact));
            while (queue.Count != 0)
            {
                var node = queue.Dequeue();
                if (node.Fact != null) // This is 'OR' node
                {
                    
                }
                else // This is 'And' node
                {
                    
                }
            }
        }

        public void ForwardSearchSandbox(string[] startFacts)
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

        public string StateToString(bool[] state)
        {
            var sB = new StringBuilder();
            for (var i = 0; i < state.Length; i++)
                if (state[i])
                    sB.Append($"{_facts[i].Desc}, ");
            sB.Remove(sB.Length - 2, 2);
            return sB.ToString();
        }
    }
}