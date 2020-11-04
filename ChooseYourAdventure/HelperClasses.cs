using System.Collections.Generic;
using System.Text;

namespace ChooseYourAdventure
{
    public class BackwardNode
    {
        public List<BackwardNode> Parents { get; private set; }
        public List<BackwardNode> Children { get; private set; }
        public Fact Fact { get; private set; }
        public Rule Rule { get; private set; }

        public int Depth { get; set; }
        public int UnsuccessfulChildrenCount { get; set; }

        public BackwardNode(Fact fact, Rule rule, List<BackwardNode> parents,
            int depth,
            int unsuccessfulChildrenCount = 0)
        {
            Fact = fact;
            Rule = rule;
            Parents = parents;
            Depth = depth;
            UnsuccessfulChildrenCount = unsuccessfulChildrenCount;
            Children = new List<BackwardNode>();
        }

        public void AddChild(BackwardNode node) =>
            Children.Add(node);
    }
    public class Rule
    {
        public int Id { get; private set; }
        public Fact[] Antecedents { get; private set; }
        public Fact Consequent { get; private set; }

        public Rule(int id, Fact[] antecedents, Fact consequent)
        {
            Id = id;
            Antecedents = antecedents;
            Consequent = consequent;
        }

        public override string ToString()
        {
            var sB = new StringBuilder();
            foreach (var a in Antecedents)
                sB.Append($"{a.Desc}, ");
            sB.Remove(sB.Length - 2, 2);
            sB.Append($" -> {Consequent.Desc}");
            return sB.ToString();
        }

        public Fact Apply(bool[] state)
        {
            foreach (var a in Antecedents)
                if (state[a.Id] == false)
                    return null;
            return Consequent;
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

        public override string ToString() => Desc;
    }
}