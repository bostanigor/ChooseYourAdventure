using System;
using System.Collections.Generic;
using System.IO;

namespace ChooseYourAdventure
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var factsPath = "../../facts.txt";
            var rulesPath = "../../rulesBase.txt";
            var prodSystem = new ProductionSystem(factsPath, rulesPath);
            prodSystem.ForwardSearch("Деньги", "Щит", "Оружие", "Латы", "Дети", "Отвага");
        }
    }
}