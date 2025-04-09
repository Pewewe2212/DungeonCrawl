using System.Numerics;

namespace DungeonCrawl
{
    /// <summary>
    /// Make the monsters
    /// Draw the monsters
    /// Monsters move toward player if they're too close
    /// Monster attacks player (may be changed to something else)
    /// </summary>
    internal class Monster
    {
        public string name;
        public Vector2 position;
        public int hitpoints;
        public char symbol;
        public ConsoleColor color;
        public int damage;

        static Monster CreateMonster(string name, int hitpoints, char symbol, ConsoleColor color, Vector2 position, int damage)
        {
            Monster monster = new Monster();
            monster.name = name;
            monster.hitpoints = hitpoints;
            monster.symbol = symbol;
            monster.color = color;
            monster.position = position;
            monster.damage = damage;
            return monster;
        }

        public Monster CreateRandomMonster(Random random, Vector2 position)
        {
            int type = random.Next(4);
            return type switch
            {
                0 => CreateMonster("Goblin", 6, 'g', ConsoleColor.Green, position, 3),
                1 => CreateMonster("Bat Man", 4, 'M', ConsoleColor.Magenta, position, 2),
                2 => CreateMonster("Orc", 20, 'o', ConsoleColor.Red, position, 5),
                3 => CreateMonster("Bunny", 3, 'B', ConsoleColor.Yellow, position, 1)
            };
        }

        public List<Monster> CreateEnemies(Map level, Random random, Monster monsterClass)
        {
            List<Monster> monsters = new List<Monster>();

            for (int y = 0; y < level.height; y++)
            {
                for (int x = 0; x < level.width; x++)
                {
                    int ti = y * level.width + x;
                    if (level.Tiles[ti] == Map.Tile.Monster)
                    {
                        Monster m = monsterClass.CreateRandomMonster(random, new Vector2(x, y));
                        monsters.Add(m);
                        level.Tiles[ti] = (sbyte)Map.Tile.Floor;
                    }
                }
            }
            return monsters;
        }

        public void DrawEnemies(List<Monster> enemies, Program program)
        {
            foreach (Monster m in enemies)
            {
                Console.SetCursorPosition((int)m.position.X, (int)m.position.Y);
                program.Print(m.symbol, m.color);
            }
        }

        static int GetDistanceBetween(Vector2 A, Vector2 B)
        {
            return (int)Vector2.Distance(A, B);
        }

        public void ProcessEnemies(List<Monster> enemies, Map level, PlayerCharacter character, List<int> dirtyTiles, List<string> messages)
        {
            foreach (Monster enemy in enemies)
            {

                if (GetDistanceBetween(enemy.position, character.position) < 5)
                {
                    Vector2 enemyMove = new Vector2(0, 0);

                    if (character.position.X < enemy.position.X)
                    {
                        enemyMove.X = -1;
                    }
                    else if (character.position.X > enemy.position.X)
                    {
                        enemyMove.X = 1;
                    }
                    else if (character.position.Y > enemy.position.Y)
                    {
                        enemyMove.Y = 1;
                    }
                    else if (character.position.Y < enemy.position.Y)
                    {
                        enemyMove.Y = -1;
                    }

                    int startTile = level.PositionToTileIndex(enemy.position, level);
                    Vector2 destinationPlace = enemy.position + enemyMove;
                    if (destinationPlace == character.position)
                    {
                        int damage = enemy.damage;
                        damage -= character.GetCharacterDefense();
                        if (damage <= 0)
                        {
                            damage = 1;
                        }
                        character.hitpoints -= damage;
                        messages.Add($"{enemy.name} hits you for {damage} damage!");
                    }
                    else
                    {
                        Map.Tile destination = level.GetTileAtMap(level, destinationPlace);
                        if (destination == Map.Tile.Floor)
                        {
                            enemy.position = destinationPlace;
                            dirtyTiles.Add(startTile);
                        }
                        else if (destination == Map.Tile.Door)
                        {
                            enemy.position = destinationPlace;
                            dirtyTiles.Add(startTile);
                        }
                        else if (destination == Map.Tile.ShopDoor)
                        {
                            enemy.position = destinationPlace;
                            dirtyTiles.Add(startTile);
                        }
                        else if (destination == Map.Tile.Wall)
                        {
                            // NOP
                        }
                    }
                }
            }
        }

    }
}
