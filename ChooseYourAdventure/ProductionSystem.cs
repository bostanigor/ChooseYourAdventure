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
        private Dictionary<string, Fact> _descToFacts;
        private List<Fact> _facts;
        private List<Rule> _rules;
        private List<List<Rule>> _factIdToRules;        
        public ProductionSystem(string factsPath, string rulesPath)
        {
            _descToFacts = new Dictionary<string, Fact>();
            _factIdToRules = new List<List<Rule>>();
            _rules = new List<Rule>();
            _facts = new List<Fact>();

            using (StreamReader sr = new StreamReader(factsPath))
            {
                int i = 0;
                while (sr.Peek() >= 0)
                {
                    var fact = new Fact(i++, sr.ReadLine());
                    _descToFacts[fact.Desc] = fact;
                    _facts.Add(fact);
                    _factIdToRules.Add(new List<Rule>());
                }
            }
            using (StreamReader sr = new StreamReader(rulesPath)) {
                int i = 0;
                while (sr.Peek() >= 0)
                {
                    var line = sr.ReadLine();
                    var temp = line.Split(new[] {"->"}, StringSplitOptions.None);
                    var left = temp[0].Trim();
                    var right = temp[1].Trim();
                    var antecdents = left.Split(',').Select(str => _descToFacts[str.Trim()]);
                    var consequent = _descToFacts[right];
                    var rule = new Rule(i++, antecdents.ToArray(), consequent);
                    _rules.Add(rule);
                    _factIdToRules[consequent.Id].Add(rule);
                }
            }

            //_rules.ForEach(rule => Console.WriteLine(rule));
        }
        
        public void BackwardSearch(string[] startFacts, string[] endFacts)
        {
            var endNodes = endFacts.Select(desc =>
                new BackwardNode(_descToFacts[desc], null, null)).ToList();
            var factNodes = new BackwardNode[_facts.Count]; // Fact nodes that we already processed
            var ruleNodes = new BackwardNode[_rules.Count]; // Rule nodes that we already processed
            var ruleTraceback = new List<BackwardNode>();
            var queue = new Queue<BackwardNode>();
            
            foreach (var desc in startFacts)
                factNodes[_descToFacts[desc].Id] = new BackwardNode(_descToFacts[desc],
                    null, new List<BackwardNode>(), 0);
            
            var tempRule = new Rule(-1,
                endFacts.Select(desc => _descToFacts[desc]).ToArray(), null);
            var firstNode = new BackwardNode(null, tempRule, new List<BackwardNode>(),
                tempRule.Antecedents.Length);
            queue.Enqueue(firstNode);
            
            while (queue.Count != 0 && firstNode.UnsuccessfulChildrenCount != 0)
            {
                var node = queue.Dequeue();
                if (node.Fact != null) // This is 'OR' Fact node
                {
                    // If fact is Successful
                    if (node.UnsuccessfulChildrenCount <= 0) {
                        RiseToRoot(node); // Temporary
                        continue;
                    }

                    var applicableRules = _factIdToRules[node.Fact.Id];
                    node.UnsuccessfulChildrenCount = 1; // I need only one children to succeed
                    foreach (var rule in applicableRules)
                    {
                        if (ruleNodes[rule.Id] != null)
                            continue;
                        var newNode = new BackwardNode(null, rule, 
                            new List<BackwardNode>(){ node },
                            rule.Antecedents.Length);
                        ruleNodes[rule.Id] = newNode;
                        queue.Enqueue(newNode);
                    }
                }
                else // This is 'AND' Rule node
                {
                    foreach (var fact in node.Rule.Antecedents)
                    {
                        // Check if fact was processed
                        var factNode = factNodes[fact.Id];
                        if (factNode != null)
                        {
                            factNode.Parents.Add(node);
                            if (factNode.UnsuccessfulChildrenCount == 0)
                                node.UnsuccessfulChildrenCount--;
                            continue;
                        }
                        var newNode = new BackwardNode(fact, null,
                            new List<BackwardNode>() { node }, 1);
                        factNodes[fact.Id] = newNode;
                        queue.Enqueue(newNode);
                    }
                    // If all children were already successful
                    if (node.UnsuccessfulChildrenCount <= 0)
                    {
                        RiseToRoot(node);
                        ruleTraceback.Add(node);
                    }
                }
            }

            if (firstNode.UnsuccessfulChildrenCount == 0)
            {
                Console.WriteLine("Success");
                // ruleTraceback.Reverse();
                // foreach (var ruleNode in ruleTraceback)
                // {
                //     if (rule)
                // }
            }
            else
                Console.WriteLine("Unsuccess");
        }

        private void RiseToRoot(BackwardNode node)
        {
            if (node == null)
                return;
            foreach (var parent in node.Parents)
            {
                if (parent == node)
                    return;
                if (parent.UnsuccessfulChildrenCount > 0)
                {
                    parent.UnsuccessfulChildrenCount--;
                    if (parent.UnsuccessfulChildrenCount == 0)
                        RiseToRoot(parent);                    
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
                    result.Add(_facts[i]);
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