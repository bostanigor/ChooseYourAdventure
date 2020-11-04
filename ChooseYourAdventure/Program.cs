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
            // prodSystem.BackwardSearch(
            //     new string[] { "Оружие", "Щит", "Карта", "Удача", "Вода" },
            //     new string[] { "Темный лес"});
            
            prodSystem.BackwardSearch(
                new string[] { "Оружие", "Щит", "Удача", "Вода" },
                new string[] { "Темный лес"});
            
            // prodSystem.ForwardSearchSandbox(
            //     new string[] { "Оружие", "Щит", "Карта" });
        }
    }
}