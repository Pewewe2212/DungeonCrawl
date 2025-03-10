using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DungeonCrawl
{
    /// <summary>
    /// Make the player
    /// Process the changes to the player
    /// </summary>
    internal class PlayerCharacter
    {
        public string name;
        public int hitpoints;
        public int maxHitpoints;
        public Item weapon;
        public Item armor;
        public int gold;
        public Vector2 position;
        public List<Item> inventory;

        public PlayerCharacter CreateCharacter(Program program)
        {
            PlayerCharacter character = new PlayerCharacter();
            character.name = "";
            character.hitpoints = 20;
            character.maxHitpoints = character.hitpoints;
            character.gold = 0;
            character.weapon = null;
            character.armor = null;
            character.inventory = new List<Item>();

            Console.Clear();
            program.DrawBrickBg();

            // Draw entrance
            Console.BackgroundColor = ConsoleColor.Black;
            int doorHeight = (int)(Console.WindowHeight * (3.0f / 4.0f));
            int doorY = Console.WindowHeight - doorHeight;
            int doorWidth = (int)(Console.WindowWidth * (3.0f / 5.0f));
            int doorX = Console.WindowWidth / 2 - doorWidth / 2;

            program.DrawRectangle(doorX, doorY, doorWidth, doorHeight, ConsoleColor.Black);
            program.DrawRectangleBorders(doorX + 1, doorY + 1, doorWidth - 2, doorHeight - 2, ConsoleColor.Blue, "|");
            program.DrawRectangleBorders(doorX + 3, doorY + 3, doorWidth - 6, doorHeight - 6, ConsoleColor.DarkBlue, "|");

            Console.SetCursorPosition(Console.WindowWidth / 2 - 8, Console.WindowHeight / 2);
            program.Print("Welcome Brave Adventurer!");
            Console.SetCursorPosition(Console.WindowWidth / 2 - 8, Console.WindowHeight / 2 + 1);
            program.Print("What is your name?", ConsoleColor.Yellow);
            while (string.IsNullOrEmpty(character.name))
            {
                character.name = Console.ReadLine();
            }
            program.Print($"Welcome {character.name}!", ConsoleColor.Yellow);

            return character;
        }

        public int GetCharacterDamage()
        {
            if (weapon != null)
            {
                return weapon.quality;
            }
            else
            {
                return 1;
            }
        }
        public int GetCharacterDefense()
        {
            if (armor != null)
            {
                return armor.quality;
            }
            else
            {
                return 0;
            }
        }

        public void DrawPlayer(PlayerCharacter character, Program program)
        {
            Console.SetCursorPosition((int)character.position.X, (int)character.position.Y);
            program.Print("@", ConsoleColor.White);
        }
    }
}
