using System.Numerics;

namespace DungeonCrawl
{

    enum GameState
    {
        CharacterCreation,
        GameLoop,
        Inventory,
        Shop,
        DeathScreen,
        Quit
    }
    enum PlayerTurnResult
    {
        TurnOver,
        NewTurn,
        OpenInventory,
        EnterShop,
        NextLevel,
        BackToGame
    }

    internal enum ItemType
    {
        Weapon,
        Armor,
        Potion,
        Treasure
    }


    internal class Program
    {
        const int INFO_HEIGHT = 6;
        const int COMMANDS_WIDTH = 12;
        const int ENEMY_CHANCE = 3;
        const int ITEM_CHANCE = 4;

        // Room generation 
        const int ROOM_AMOUNT = 12;
        const int ROOM_MIN_W = 4;
        const int ROOM_MAX_W = 12;
        const int ROOM_MIN_H = 4;
        const int ROOM_MAX_H = 8;
        const int SHOP_CHANCE = 2; // should be a 1/20 (2/40) chance for a room to be a shop

        // Shop per floor
        int shopType = 0;

        bool scrollBought = false;

        // shop prices
        int shop;
        int swordQuality;
        int sword1Quality;
        int sword2Quality;

        int armorQuality;
        int armor1Quality;
        int armor2Quality;

        int potion1Quality;
        int potion2Quality;
        int potion3Quality;
        int potion4Quality;

        int scrollPrice;

        static void Main(string[] args)
        {
            List<Monster> monsters = null;
            List<Item> items = null;
            PlayerCharacter player = new PlayerCharacter();
            Map currentLevel = new Map();
            Random random = new Random();
            Program program = new Program();
            Item itemClass = new Item();
            Monster monsterClass = new Monster();

            List<int> dirtyTiles = new List<int>();
            List<string> messages = new List<string>();

            // Main loop
            GameState state = GameState.CharacterCreation;
            while (state != GameState.Quit)
            {
                switch (state)
                {
                    case GameState.CharacterCreation:
                        // Character creation screen
                        player = player.CreateCharacter(program);
                        Console.CursorVisible = false;
                        Console.Clear();

                        // Map Creation 
                        currentLevel = CreateMap(random);

                        // Enemy init
                        monsters = monsterClass.CreateEnemies(currentLevel, random, monsterClass);
                        // Item init
                        items = itemClass.CreateItems(currentLevel, random);
                        // Player init
                        currentLevel.PlacePlayerToMap(player, currentLevel);
                        currentLevel.PlaceStairsToMap(currentLevel);

                        //shop
                        program.shop = random.Next(1, 3);
                        program.swordQuality = 2;
                        program.sword1Quality = random.Next(4, 6);
                        program.sword2Quality = random.Next(2, 5);

                        program.armorQuality = 2;
                        program.armor1Quality = random.Next(4, 6);
                        program.armor2Quality = random.Next(2, 5);

                        program.potion1Quality = random.Next(1, 3);
                        program.potion2Quality = random.Next(1, 6);
                        program.potion3Quality = random.Next(2, 5);
                        program.potion4Quality = random.Next(4, 8);


                        program.scrollPrice = random.Next(30, 50);
                        state = GameState.GameLoop;
                        break;
                    case GameState.GameLoop:
                        currentLevel.DrawMap(currentLevel, dirtyTiles, program);
                        dirtyTiles.Clear();
                        monsterClass.DrawEnemies(monsters, program);
                        itemClass.DrawItems(items, program);

                        player.DrawPlayer(player, program);
                        program.DrawCommands();
                        program.DrawInfo(player, messages);
                        // Draw map
                        // Draw information
                        // Wait for player command
                        // Process player command
                        while (true)
                        {
                            messages.Clear();
                            PlayerTurnResult result = DoPlayerTurn(currentLevel, player, itemClass, monsters, items, dirtyTiles, messages);
                            program.DrawInfo(player, messages);
                            if (result == PlayerTurnResult.TurnOver)
                            {
                                break;
                            }
                            else if (result == PlayerTurnResult.OpenInventory)
                            {
                                Console.Clear();
                                state = GameState.Inventory;
                                break;
                            }
                            else if (result == PlayerTurnResult.EnterShop)
                            {
                                Console.Clear();
                                state = GameState.Shop;
                                break;
                            }
                            else if (result == PlayerTurnResult.NextLevel)
                            {
                                currentLevel = CreateMap(random);
                                monsters = monsterClass.CreateEnemies(currentLevel, random, monsterClass);
                                items = itemClass.CreateItems(currentLevel, random);
                                currentLevel.PlacePlayerToMap(player, currentLevel);
                                currentLevel.PlaceStairsToMap(currentLevel);
                                program.shopType = random.Next(3);

                                //shop
                                program.shop = random.Next(1, 3);
                                program.swordQuality = 2;
                                program.sword1Quality = random.Next(4, 6);
                                program.sword2Quality = random.Next(2, 5);

                                program.armorQuality = 2;
                                program.armor1Quality = random.Next(4, 6);
                                program.armor2Quality = random.Next(2, 5);

                                program.potion1Quality = random.Next(1, 3);
                                program.potion2Quality = random.Next(1, 6);
                                program.potion3Quality = random.Next(2, 5);
                                program.potion4Quality = random.Next(4, 8);


                                program.scrollPrice = random.Next(30, 50);
                                Console.Clear();
                                break;
                            }
                        }
                        // Either do computer turn or wait command again
                        // Do computer turn
                        // Process enemies
                        monsterClass.ProcessEnemies(monsters, currentLevel, player, dirtyTiles, messages);

                        program.DrawInfo(player, messages);

                        // Is player dead?
                        if (player.hitpoints <= 0)
                        {
                            state = GameState.DeathScreen;
                        }

                        break;
                    case GameState.Inventory:
                        // Draw inventory 
                        PlayerTurnResult inventoryResult = program.DrawInventory(player, itemClass, messages);
                        if (inventoryResult == PlayerTurnResult.BackToGame)
                        {
                            state = GameState.GameLoop;
                            currentLevel.DrawMapAll(currentLevel, program);
                            program.DrawInfo(player, messages);
                        }
                        // Read player command
                        // Change back to game loop
                        break;
                    case GameState.Shop:
                        PlayerTurnResult shopResult = program.DrawShop(player, itemClass, messages, program.shopType, random);
                        if (shopResult == PlayerTurnResult.BackToGame)
                        {
                            state = GameState.GameLoop;
                            currentLevel.DrawMapAll(currentLevel, program);
                            program.DrawInfo(player, messages);
                        }
                        break;
                    case GameState.DeathScreen:
                        DrawEndScreen(random);
                        // Animation is over
                        Console.SetCursorPosition(Console.WindowWidth / 2 - 4, Console.WindowHeight / 2);
                        program.Print("YOU DIED", ConsoleColor.Yellow);
                        Console.SetCursorPosition(Console.WindowWidth / 2 - 4, Console.WindowHeight / 2 + 1);
                        int noRepeat = 0;
                        while (true)
                        {
                            if (noRepeat == 0)
                            {
                                program.Print("Play again (y/n)", ConsoleColor.Gray);
                                noRepeat += 1;
                            }
                            ConsoleKeyInfo answer = Console.ReadKey();
                            if (answer.Key == ConsoleKey.Y)
                            {
                                state = GameState.CharacterCreation;
                                break;
                            }
                            else if (answer.Key == ConsoleKey.N)
                            {
                                state = GameState.Quit;
                                break;
                            }
                        }
                        break;
                };
            }
            Console.ResetColor();
            Console.Clear();
            Console.CursorVisible = true;
        }

        static void PrintLine(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }
        public void Print(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
        }
        static void PrintLine(string text)
        {
            Console.WriteLine(text);
        }
        public void Print(string text)
        {
            Console.Write(text);
        }
        public void Print(char symbol, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(symbol);
        }



        public void DrawBrickBg()
        {
            // Draw tiles
            Console.BackgroundColor = ConsoleColor.DarkGray;
            for (int y = 0; y < Console.WindowHeight; y++)
            {
                Console.SetCursorPosition(0, y);
                for (int x = 0; x < Console.WindowWidth; x++)
                {
                    if ((x + y) % 3 == 0)
                    {
                        Print("|", ConsoleColor.Black);
                    }
                    else
                    {
                        Print(" ", ConsoleColor.DarkGray);
                    }
                }
            }
        }

        public void DrawRectangle(int x, int y, int width, int height, ConsoleColor color)
        {
            Console.BackgroundColor = color;
            for (int dy = y; dy < y + height; dy++)
            {
                Console.SetCursorPosition(x, dy);
                for (int dx = x; dx < x + width; dx++)
                {
                    Print(" ");
                }
            }
        }

        public void DrawRectangleBorders(int x, int y, int width, int height, ConsoleColor color, string symbol)
        {
            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = color;
            for (int dx = x; dx < x + width; dx++)
            {
                Print(symbol);
            }

            for (int dy = y; dy < y + height; dy++)
            {
                Console.SetCursorPosition(x, dy);

                Print(symbol);
                Console.SetCursorPosition(x + width - 1, dy);
                Print(symbol);
            }
        }
        static void DrawEndScreen(Random random)
        {
            // Run death animation: blood flowing down the screen in columns
            // Wait until keypress
            byte[] speeds = new byte[Console.WindowWidth];
            byte[] ends = new byte[Console.WindowWidth];
            for (int i = 0; i < speeds.Length; i++)
            {
                speeds[i] = (byte)random.Next(1, 4);
                ends[i] = 0;
            }
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;


            for (int row = 0; row < Console.WindowHeight - 2; row++)
            {
                Console.SetCursorPosition(0, row);
                for (int i = 0; i < Console.WindowWidth; i++)
                {
                    Console.Write(" ");
                }
                Thread.Sleep(100);
            }

        }


        // the turns
        static bool DoPlayerTurnVsEnemies(PlayerCharacter character, List<Monster> enemies, Vector2 destinationPlace, List<string> messages)
        {
            // Check enemies
            bool hitEnemy = false;
            Monster toRemoveMonster = null;
            foreach (Monster enemy in enemies)
            {
                if (enemy.position == destinationPlace)
                {
                    int damage = character.GetCharacterDamage();
                    messages.Add($"You hit {enemy.name} for {damage}!");
                    enemy.hitpoints -= damage;
                    hitEnemy = true;
                    if (enemy.hitpoints <= 0)
                    {
                        toRemoveMonster = enemy;
                    }
                }
            }
            if (toRemoveMonster != null)
            {
                enemies.Remove(toRemoveMonster);
            }
            return hitEnemy;
        }

        static bool DoPlayerTurnVsItems(PlayerCharacter character, Item itemClass, List<Item> items, Vector2 destinationPlace, List<string> messages)
        {
            // Check items
            Item toRemoveItem = null;
            foreach (Item item in items)
            {
                if (item.position == destinationPlace)
                {
                    string itemMessage = $"You find a ";
                    switch (item.type)
                    {
                        case ItemType.Armor:
                            itemMessage += $"{item.name}, it fits you well";
                            break;
                        case ItemType.Weapon:
                            itemMessage += $"{item.name} to use in battle";
                            break;
                        case ItemType.Potion:
                            itemMessage += $"potion of {item.name}";
                            break;
                        case ItemType.Treasure:
                            itemMessage += $"valuable {item.name} and get {item.quality} gold!";
                            break;
                    };
                    messages.Add(itemMessage);
                    toRemoveItem = item;
                    itemClass.GiveItem(character, item);
                    break;
                }
            }
            if (toRemoveItem != null)
            {
                items.Remove(toRemoveItem);
            }
            return false;
        }

        static PlayerTurnResult DoPlayerTurn(Map level, PlayerCharacter character, Item itemClass, List<Monster> enemies, List<Item> items, List<int> dirtyTiles, List<string> messages)
        {
            Vector2 playerMove = new Vector2(0, 0);
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.W || key.Key == ConsoleKey.UpArrow)
                {
                    playerMove.Y = -1;
                    break;
                }
                else if (key.Key == ConsoleKey.S || key.Key == ConsoleKey.DownArrow)
                {
                    playerMove.Y = 1;
                    break;
                }
                else if (key.Key == ConsoleKey.A || key.Key == ConsoleKey.LeftArrow)
                {
                    playerMove.X = -1;
                    break;
                }
                else if (key.Key == ConsoleKey.D || key.Key == ConsoleKey.RightArrow)
                {
                    playerMove.X = 1;
                    break;
                }
                // Other commands
                else if (key.Key == ConsoleKey.I)
                {
                    return PlayerTurnResult.OpenInventory;
                }
            }

            int startTile = level.PositionToTileIndex(character.position, level);
            Vector2 destinationPlace = character.position + playerMove;

            if (DoPlayerTurnVsEnemies(character, enemies, destinationPlace, messages))
            {
                return PlayerTurnResult.TurnOver;
            }

            if (DoPlayerTurnVsItems(character, itemClass, items, destinationPlace, messages))
            {
                return PlayerTurnResult.TurnOver;
            }

            // Check movement
            Map.Tile destination = level.GetTileAtMap(level, destinationPlace);
            if (destination == Map.Tile.Floor)
            {
                character.position = destinationPlace;
                dirtyTiles.Add(startTile);
            }
            else if (destination == Map.Tile.Door)
            {
                messages.Add("You open a door");
                character.position = destinationPlace;
                dirtyTiles.Add(startTile);
            }
            else if (destination == Map.Tile.ShopDoor)
            {
                messages.Add("You open a door and find yourself in a Shop");
                dirtyTiles.Add(startTile);
                return PlayerTurnResult.EnterShop;
            }
            else if (destination == Map.Tile.Wall || destination == Map.Tile.Shop)
            {
                messages.Add("You hit a wall");
            }
            else if (destination == Map.Tile.Stairs)
            {
                messages.Add("You find stairs leading down");
                return PlayerTurnResult.NextLevel;
            }

            return PlayerTurnResult.TurnOver;
        }

        public PlayerTurnResult DrawInventory(PlayerCharacter character, Item itemClass, List<string> messages)
        {
            // INVALID INPUT DOES NOT CLOSE INVENTORY IF IT'S EMPTY
            Console.SetCursorPosition(1, 1);
            PrintLine("Inventory. Select item by inputting the number next to it. Null input closes inventory");
            ItemType currentType = ItemType.Weapon;
            PrintLine("Weapons", ConsoleColor.DarkCyan);
            for (int i = 0; i < character.inventory.Count; i++)
            {
                Item it = character.inventory[i];
                if (currentType == ItemType.Weapon && it.type == ItemType.Armor)
                {
                    currentType = ItemType.Armor;
                    PrintLine("Armors", ConsoleColor.DarkRed);
                }
                else if (currentType == ItemType.Armor && it.type == ItemType.Potion)
                {
                    currentType = ItemType.Potion;
                    PrintLine("Potions", ConsoleColor.DarkMagenta);
                }
                Print($"{i} ", ConsoleColor.Cyan);
                PrintLine($"{it.name} ({it.quality})", ConsoleColor.White);
            }
            while (true)
            {
                Print("Choose item: ", ConsoleColor.Yellow);
                string choiceStr = Console.ReadLine();
                int selectionindex = 0;
                if (choiceStr == "")
                {
                    break;
                }
                if (int.TryParse(choiceStr, out selectionindex))
                {
                    if (selectionindex >= 0 && selectionindex < character.inventory.Count)
                    {
                        itemClass.UseItem(character, character.inventory[selectionindex], messages);
                        break;
                    }
                }
                else
                {
                    messages.Add("No such item");
                }
            };
            return PlayerTurnResult.BackToGame;
        }

        public PlayerTurnResult DrawShop(PlayerCharacter character, Item item, List<String> messages, int shopType, Random random)
        {
            int cursorPosition = 1;
            Console.SetCursorPosition(1, cursorPosition);
            PrintLine("Shop. You can buy any item on sale, if you can afford it. Null input closes shop");
            PrintLine("Shop", ConsoleColor.DarkCyan);
            cursorPosition += 2;
            // The shops on the floor are decided at the start of the floor and it always starts with shop 0 on floor 1, each shop should have a return scroll to win, cus otherwise the game can last for too long to be fun            
            // the shop qualities


            // draws the starting shop
            if (shopType == 0)
            {

                // get the item prices
                int swordPrice = 5;
                int armorPrice = 5;
                int potion1Price = potion1Quality * 2;
                int potion2Price = potion2Quality * 2;


                if (shop == 1)
                {
                    // print the sword with quality 2, 2 potions with random qualities and the scroll
                    PrintLine($"Sword (2), price {swordPrice}; ", ConsoleColor.Cyan);
                    PrintLine($"Potion ({potion1Quality}), price {potion1Price}; ", ConsoleColor.Cyan);
                    PrintLine($"Potion ({potion2Quality}), price {potion2Price}; ", ConsoleColor.Cyan);
                    PrintLine($"Return scroll, price {scrollPrice}", ConsoleColor.Yellow);
                }
                else
                {
                    // print the armor with quality 2, 2 potions with random qualities and the scroll
                    PrintLine($"Armor (2), price {armorPrice}; ", ConsoleColor.Cyan);
                    PrintLine($"Potion ({potion1Quality}), price {potion1Price}; ", ConsoleColor.Cyan);
                    PrintLine($"Potion ({potion2Quality}), price {potion2Price}; ", ConsoleColor.Cyan);
                    PrintLine($"Return scroll, price {scrollPrice}", ConsoleColor.Yellow);
                }
                cursorPosition += 4;
            }
            else if (shopType == 1)
            {
                // shop with mostly consumables and a mediocre equipment item

                // give prices
                int potion1Price = potion1Quality * 2;
                int potion2Price = potion2Quality * 2;
                int potion3Price = potion3Quality * 2;
                int potion4Price = potion4Quality * 2;
                int swordPrice = sword2Quality * 3;
                int armorPrice = armor2Quality * 3;

                if (shop == 1)
                {
                    // 4 consumables and a sword
                    Print($"Sword ({sword2Quality}), price {swordPrice}; ", ConsoleColor.Cyan);
                    Print($"Potion ({potion1Quality}), price {potion1Price}; ", ConsoleColor.Cyan);
                    Print($"Potion ({potion2Quality}), price {potion2Price}; ", ConsoleColor.Cyan);
                    Print($"Potion ({potion3Quality}), price {potion3Price}; ", ConsoleColor.Cyan);
                    Print($"Potion ({potion4Quality}), price {potion4Price}; ", ConsoleColor.Cyan);
                    Print($"Return scroll, price {scrollPrice}", ConsoleColor.Yellow);
                    cursorPosition += 6;
                }
                else
                {
                    // 3 consumables and armor
                    Print($"Armor ({armor2Quality}), price {armorPrice}; ", ConsoleColor.Cyan);
                    Print($"Potion ({potion1Quality}), price {potion1Price}; ", ConsoleColor.Cyan);
                    Print($"Potion ({potion2Quality}), price {potion2Price}; ", ConsoleColor.Cyan);
                    Print($"Potion ({potion3Quality}), price {potion3Price}; ", ConsoleColor.Cyan);
                    Print($"Return scroll, price {scrollPrice}", ConsoleColor.Yellow);
                    cursorPosition += 5;
                }
            }
            else if (shopType == 2)
            {
                //get prices
                int sword1Price = sword1Quality * 3;
                int sword2Price = sword2Quality * 3;
                int armor1Price = armor1Quality * 3;
                int potion1Price = potion1Quality * 3;
                int potion2Price = potion2Quality * 3;

                if (shop == 1)
                {
                    Print($"Sword ({sword1Quality}), price {sword1Price}; ", ConsoleColor.Cyan);
                    Print($"Sword ({sword2Quality}), price {sword2Price}; ", ConsoleColor.Cyan);
                    Print($"Potion ({potion1Quality}), price {potion1Price}; ", ConsoleColor.Cyan);
                    Print($"Potion ({potion2Quality}), price {potion2Price}; ", ConsoleColor.Cyan);
                    Print($"Return scroll, price {scrollPrice}", ConsoleColor.Yellow);
                }
                else
                {
                    Print($"Sword ({sword1Quality}), price {sword1Price}; ", ConsoleColor.Cyan);
                    Print($"Armor ({armor1Quality}), price {armor1Price}; ", ConsoleColor.Cyan);
                    Print($"Potion ({potion1Quality}), price {potion1Price}; ", ConsoleColor.Cyan);
                    Print($"Potion ({potion2Quality}), price {potion2Price}; ", ConsoleColor.Cyan);
                    Print($"Return scroll, price {scrollPrice}", ConsoleColor.Yellow);
                }
                cursorPosition += 5;
            }

            while (true)
            {
                string choiceStr = Console.ReadLine();
                cursorPosition += 1;
                if (choiceStr == "")
                {
                    break;
                }
                else
                {
                    if (shopType == 0)
                    {
                        // shop has 1 item, 2 potions and a scroll

                        // get the item prices
                        int swordPrice = 5;
                        int armorPrice = 5;
                        int potion1Price = potion1Quality * 2;
                        int potion2Price = potion2Quality * 2;

                        if (shop == 1)
                        {
                            if ((choiceStr.ToLower() == "sword2" || choiceStr.ToLower() == "sword 2" || choiceStr.ToLower() == "sword(2)" || choiceStr.ToLower() == "sword (2)".ToLower() || choiceStr.ToLower() == "sword") && character.gold >= swordPrice)
                            {
                                Item i = new Item();
                                i.type = ItemType.Weapon;
                                i.quality = 2;
                                i.name = "Greedy Sword";
                                item.GiveItem(character, i);
                                character.gold -= swordPrice;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion1Quality}" || choiceStr.ToLower() == $"potion {potion1Quality}" || choiceStr.ToLower() == $"potion({potion1Quality})" || choiceStr.ToLower() == $"potion ({potion1Quality})".ToLower()) && character.gold >= potion1Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion1Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion1Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion2Quality}" || choiceStr.ToLower() == $"potion {potion2Quality}" || choiceStr.ToLower() == $"potion({potion2Quality})" || choiceStr.ToLower() == $"potion ({potion2Quality})".ToLower()) && character.gold >= potion2Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion2Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion2Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"return scroll" || choiceStr.ToLower() == $"returnscroll" || choiceStr.ToLower() == $"scroll" || choiceStr.ToLower() == $"return".ToLower()) && character.gold >= scrollPrice)
                            {
                                character.gold -= scrollPrice;
                                scrollBought = true;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                        }
                        else
                        {
                            if ((choiceStr.ToLower() == "armor2" || choiceStr.ToLower() == "armor 2" || choiceStr.ToLower() == "armor(2)" || choiceStr.ToLower() == "armor (2)".ToLower() || choiceStr.ToLower() == "armor") && character.gold >= armorPrice)
                            {
                                Item i = new Item();
                                i.type = ItemType.Armor;
                                i.quality = 2;
                                i.name = "Greedy Armor";
                                item.GiveItem(character, i);
                                character.gold -= armorPrice;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion1Quality}" || choiceStr.ToLower() == $"potion {potion1Quality}" || choiceStr.ToLower() == $"potion({potion1Quality})" || choiceStr.ToLower() == $"potion ({potion1Quality})".ToLower()) && character.gold >= potion1Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion1Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion1Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion2Quality}" || choiceStr.ToLower() == $"potion {potion2Quality}" || choiceStr.ToLower() == $"potion({potion2Quality})" || choiceStr.ToLower() == $"potion ({potion2Quality})".ToLower()) && character.gold >= potion2Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion2Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion2Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"return scroll" || choiceStr.ToLower() == $"returnscroll" || choiceStr.ToLower() == $"scroll" || choiceStr.ToLower() == $"return".ToLower()) && character.gold >= scrollPrice)
                            {
                                character.gold -= scrollPrice;
                                scrollBought = true;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                        }
                        DrawInfo(character, messages);
                        Console.SetCursorPosition(1, cursorPosition);
                    }
                    else if (shopType == 1)
                    {
                        // shop with mostly consumables and a mediocre equipment item

                        // give prices
                        int potion1Price = potion1Quality * 2;
                        int potion2Price = potion2Quality * 2;
                        int potion3Price = potion3Quality * 2;
                        int potion4Price = potion4Quality * 2;
                        int swordPrice = swordQuality * 3;
                        int armorPrice = armorQuality * 3;

                        if (shop == 1)
                        {
                            if ((choiceStr.ToLower() == $"sword{swordQuality}" || choiceStr.ToLower() == $"sword {swordQuality}" || choiceStr.ToLower() == $"sword({swordQuality})" || choiceStr.ToLower() == $"sword ({swordQuality})".ToLower() || choiceStr.ToLower() == "sword") && character.gold >= swordPrice)
                            {
                                Item i = new Item();
                                i.type = ItemType.Weapon;
                                i.quality = sword2Quality;
                                i.name = "Greedy Sword";
                                item.GiveItem(character, i);
                                character.gold -= swordPrice;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion1Quality}" || choiceStr.ToLower() == $"potion {potion1Quality}" || choiceStr.ToLower() == $"potion({potion1Quality})" || choiceStr.ToLower() == $"potion ({potion1Quality})".ToLower()) && character.gold >= potion1Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion1Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion1Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion2Quality}" || choiceStr.ToLower() == $"potion {potion2Quality}" || choiceStr.ToLower() == $"potion({potion2Quality})" || choiceStr.ToLower() == $"potion ({potion2Quality})".ToLower()) && character.gold >= potion2Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion2Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion2Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion3Quality}" || choiceStr.ToLower() == $"potion {potion3Quality}" || choiceStr.ToLower() == $"potion({potion3Quality})" || choiceStr.ToLower() == $"potion ({potion3Quality})".ToLower()) && character.gold >= potion3Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion3Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion3Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion4Quality}" || choiceStr.ToLower() == $"potion {potion4Quality}" || choiceStr.ToLower() == $"potion({potion4Quality})" || choiceStr.ToLower() == $"potion ({potion4Quality})".ToLower()) && character.gold >= potion4Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion4Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion4Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"return scroll" || choiceStr.ToLower() == $"returnscroll" || choiceStr.ToLower() == $"scroll" || choiceStr.ToLower() == $"return".ToLower()) && character.gold >= scrollPrice)
                            {
                                character.gold -= scrollPrice;
                                scrollBought = true;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                        }
                        else
                        {
                            if ((choiceStr.ToLower() == $"armor{armor2Quality}" || choiceStr.ToLower() == $"armor {armor2Quality}" || choiceStr.ToLower() == $"armor({armor2Quality})") || choiceStr.ToLower() == $"armor ({armor2Quality})")
                            {
                                Item i = new Item();
                                i.type = ItemType.Armor;
                                i.quality = armor2Quality;
                                i.name = "Greedy Armor";
                                item.GiveItem(character, i);
                                character.gold -= armorPrice;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion1Quality}" || choiceStr.ToLower() == $"potion {potion1Quality}" || choiceStr.ToLower() == $"potion({potion1Quality})" || choiceStr.ToLower() == $"potion ({potion1Quality})".ToLower()) && character.gold >= potion1Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion1Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion1Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion2Quality}" || choiceStr.ToLower() == $"potion {potion2Quality}" || choiceStr.ToLower() == $"potion({potion2Quality})" || choiceStr.ToLower() == $"potion ({potion2Quality})".ToLower()) && character.gold >= potion2Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion2Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion2Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion3Quality}" || choiceStr.ToLower() == $"potion {potion3Quality}" || choiceStr.ToLower() == $"potion({potion3Quality})" || choiceStr.ToLower() == $"potion ({potion3Quality})".ToLower()) && character.gold >= potion3Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion3Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion3Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion4Quality}" || choiceStr.ToLower() == $"potion {potion4Quality}" || choiceStr.ToLower() == $"potion({potion4Quality})" || choiceStr.ToLower() == $"potion ({potion4Quality})".ToLower()) && character.gold >= potion4Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion4Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion4Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"return scroll" || choiceStr.ToLower() == $"returnscroll" || choiceStr.ToLower() == $"scroll" || choiceStr.ToLower() == $"return".ToLower()) && character.gold >= scrollPrice)
                            {
                                character.gold -= scrollPrice;
                                scrollBought = true;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                        }
                        DrawInfo(character, messages);
                        Console.SetCursorPosition(1, cursorPosition);
                    }
                    else if (shopType == 2)
                    {

                        //get prices
                        int sword1Price = sword1Quality * 3;
                        int sword2Price = sword2Quality * 3;
                        int armor1Price = armor1Quality * 3;
                        int potion1Price = potion1Quality * 3;
                        int potion2Price = potion2Quality * 3;

                        if (shop == 1)
                        {
                            Print($"Potion ({potion1Quality}), price {potion1Price}; ", ConsoleColor.Cyan);
                            Print($"Potion ({potion2Quality}), price {potion2Price}; ", ConsoleColor.Cyan);
                            Print($"Return scroll, price {scrollPrice}", ConsoleColor.Yellow);

                            if ((choiceStr.ToLower() == $"sword{sword1Quality}" || choiceStr.ToLower() == $"sword {sword1Quality}" || choiceStr.ToLower() == $"sword({sword1Quality})" || choiceStr.ToLower() == $"sword ({sword1Quality})".ToLower() || choiceStr.ToLower() == "sword") && character.gold >= sword1Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Weapon;
                                i.quality = sword1Quality;
                                i.name = "Greedy Sword";
                                item.GiveItem(character, i);
                                character.gold -= sword1Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"sword{sword2Quality}" || choiceStr.ToLower() == $"sword {sword2Quality}" || choiceStr.ToLower() == $"sword({sword2Quality})" || choiceStr.ToLower() == $"sword ({sword2Quality})".ToLower() || choiceStr.ToLower() == "sword") && character.gold >= sword2Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Weapon;
                                i.quality = sword2Quality;
                                i.name = "Greedy Sword";
                                item.GiveItem(character, i);
                                character.gold -= sword2Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion1Quality}" || choiceStr.ToLower() == $"potion {potion1Quality}" || choiceStr.ToLower() == $"potion({potion1Quality})" || choiceStr.ToLower() == $"potion ({potion1Quality})".ToLower()) && character.gold >= potion1Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion1Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion1Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion2Quality}" || choiceStr.ToLower() == $"potion {potion2Quality}" || choiceStr.ToLower() == $"potion({potion2Quality})" || choiceStr.ToLower() == $"potion ({potion2Quality})".ToLower()) && character.gold >= potion2Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion2Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion2Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"return scroll" || choiceStr.ToLower() == $"returnscroll" || choiceStr.ToLower() == $"scroll" || choiceStr.ToLower() == $"return".ToLower()) && character.gold >= scrollPrice)
                            {
                                character.gold -= scrollPrice;
                                scrollBought = true;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                        }
                        else
                        {
                            Print($"Armor ({armor1Quality}), price {armor1Price}; ", ConsoleColor.Cyan);
                            Print($"Potion ({potion1Quality}), price {potion1Price}; ", ConsoleColor.Cyan);
                            Print($"Potion ({potion2Quality}), price {potion2Price}; ", ConsoleColor.Cyan);
                            Print($"Return scroll, price {scrollPrice}", ConsoleColor.Yellow);

                            if ((choiceStr.ToLower() == $"sword{sword2Quality}" || choiceStr.ToLower() == $"sword {sword2Quality}" || choiceStr.ToLower() == $"sword({sword2Quality})" || choiceStr.ToLower() == $"sword ({sword2Quality})".ToLower() || choiceStr.ToLower() == "sword") && character.gold >= sword2Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Weapon;
                                i.quality = sword2Quality;
                                i.name = "Greedy Sword";
                                item.GiveItem(character, i);
                                character.gold -= sword2Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"armor{armor2Quality}" || choiceStr.ToLower() == $"armor {armor2Quality}" || choiceStr.ToLower() == $"armor({armor2Quality})") || choiceStr.ToLower() == $"armor ({armor2Quality})")
                            {
                                Item i = new Item();
                                i.type = ItemType.Armor;
                                i.quality = armor2Quality;
                                i.name = "Greedy Armor";
                                item.GiveItem(character, i);
                                character.gold -= armor1Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion1Quality}" || choiceStr.ToLower() == $"potion {potion1Quality}" || choiceStr.ToLower() == $"potion({potion1Quality})" || choiceStr.ToLower() == $"potion ({potion1Quality})".ToLower()) && character.gold >= potion1Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion1Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion1Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"potion{potion2Quality}" || choiceStr.ToLower() == $"potion {potion2Quality}" || choiceStr.ToLower() == $"potion({potion2Quality})" || choiceStr.ToLower() == $"potion ({potion2Quality})".ToLower()) && character.gold >= potion2Price)
                            {
                                Item i = new Item();
                                i.type = ItemType.Potion;
                                i.quality = potion2Quality;
                                i.name = "Potion of the Greedy";
                                item.GiveItem(character, i);
                                character.gold -= potion2Price;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                            else if ((choiceStr.ToLower() == $"return scroll" || choiceStr.ToLower() == $"returnscroll" || choiceStr.ToLower() == $"scroll" || choiceStr.ToLower() == $"return".ToLower()) && character.gold >= scrollPrice)
                            {
                                character.gold -= scrollPrice;
                                scrollBought = true;
                                Print("Successfully bought\n");
                                cursorPosition += 1;
                            }
                        }
                        DrawInfo(character, messages);
                        Console.SetCursorPosition(1, cursorPosition);
                    }
                }
            }
            return PlayerTurnResult.BackToGame;
        }

        public void DrawInfo(PlayerCharacter player, List<string> messages)
        {
            int infoLine1 = Console.WindowHeight - INFO_HEIGHT;
            Console.SetCursorPosition(0, infoLine1);
            Print($"{player.name}: hp ({player.hitpoints}/{player.maxHitpoints}) gold ({player.gold}) ", ConsoleColor.White);
            int damage = 1;
            if (player.weapon != null)
            {
                damage = player.weapon.quality;
            }
            Print($"Weapon damage: {damage} ");
            int armor = 0;
            if (player.armor != null)
            {
                armor = player.armor.quality;
            }
            Print($"Armor: {armor} ");



            // Print last INFO_HEIGHT -1 messages
            DrawRectangle(0, infoLine1 + 1, Console.WindowWidth, INFO_HEIGHT - 2, ConsoleColor.Black);
            Console.SetCursorPosition(0, infoLine1 + 1);
            int firstMessage = 0;
            if (messages.Count > (INFO_HEIGHT - 1))
            {
                firstMessage = messages.Count - (INFO_HEIGHT - 1);
            }
            for (int i = firstMessage; i < messages.Count; i++)
            {
                Print(messages[i], ConsoleColor.Yellow);
            }
        }

        public void DrawCommands()
        {
            int cx = Console.WindowWidth - COMMANDS_WIDTH + 1;
            int ln = 1;
            Console.SetCursorPosition(cx, ln); ln++;
            Print(":Commands:", ConsoleColor.Yellow);
            Console.SetCursorPosition(cx, ln); ln++;
            Print("I", ConsoleColor.Cyan); Print("nventory", ConsoleColor.White);
        }

        static void AddRoom(Map level, int boxX, int boxY, int boxWidth, int boxHeight, Random random, bool shop)
        {
            int width = random.Next(ROOM_MIN_W, boxWidth);
            int height = random.Next(ROOM_MIN_H, boxHeight);
            int sx = boxX + random.Next(0, boxWidth - width);
            int sy = boxY + random.Next(0, boxHeight - height);
            int doorX = random.Next(1, width - 1);
            int doorY = random.Next(1, height - 1);
            int shopChance = random.Next(2);

            // Create perimeter wall
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int ti = (sy + y) * level.width + sx + x;
                    if (y == 0 || x == 0 || y == height - 1 || x == width - 1)
                    {
                        if ((y == doorY || x == doorX) && shopChance <= SHOP_CHANCE)
                        {
                            level.Tiles[ti] = Map.Tile.ShopDoor;
                        }
                        else if (y == doorY || x == doorX)
                        {
                            level.Tiles[ti] = Map.Tile.Door;
                        }
                        else if (shopChance <= SHOP_CHANCE)
                        {
                            level.Tiles[ti] = Map.Tile.Shop;
                        }
                        else
                        {
                            level.Tiles[ti] = Map.Tile.Wall;
                        }
                    }
                }
            }
        }

        static Map CreateMap(Random random)
        {
            Map level = new Map();

            level.width = Console.WindowWidth - COMMANDS_WIDTH;
            level.height = Console.WindowHeight - INFO_HEIGHT;
            level.Tiles = new Map.Tile[level.width * level.height];

            // Create perimeter wall
            for (int y = 0; y < level.height; y++)
            {
                for (int x = 0; x < level.width; x++)
                {
                    int ti = y * level.width + x;
                    if (y == 0 || x == 0 || y == level.height - 1 || x == level.width - 1)
                    {
                        level.Tiles[ti] = Map.Tile.Wall;
                    }
                    else
                    {
                        level.Tiles[ti] = Map.Tile.Floor;
                    }
                }
            }

            int roomRows = 3;
            int roomsPerRow = 6;
            int boxWidth = (Console.WindowWidth - COMMANDS_WIDTH - 2) / roomsPerRow;
            int boxHeight = (Console.WindowHeight - INFO_HEIGHT - 2) / roomRows;
            for (int roomRow = 0; roomRow < roomRows; roomRow++)
            {
                for (int roomColumn = 0; roomColumn < roomsPerRow; roomColumn++)
                {
                    AddRoom(level, roomColumn * boxWidth + 1, roomRow * boxHeight + 1, boxWidth, boxHeight, random, false);
                }
            }

            // Add enemies and items
            for (int y = 0; y < level.height; y++)
            {
                for (int x = 0; x < level.width; x++)
                {
                    int ti = y * level.width + x;
                    if (level.Tiles[ti] == Map.Tile.Floor)
                    {
                        int chance = random.Next(100);
                        if (chance < ENEMY_CHANCE)
                        {
                            level.Tiles[ti] = Map.Tile.Monster;
                            continue;
                        }

                        chance = random.Next(100);
                        if (chance < ITEM_CHANCE)
                        {
                            level.Tiles[ti] = Map.Tile.Item;
                        }
                    }
                }
            }

            // Find starting place for player
            for (int i = 0; i < level.Tiles.Length; i++)
            {
                if (level.Tiles[i] == Map.Tile.Floor)
                {
                    level.Tiles[i] = Map.Tile.Player;
                    break;
                }
            }

            return level;
        }
    }
}
