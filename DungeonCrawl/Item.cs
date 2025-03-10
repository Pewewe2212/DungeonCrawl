using System.Numerics;

namespace DungeonCrawl
{
    /// <summary>
    /// Create items
    /// Draw items
    /// Change the players items
    /// </summary>
    internal class Item
    {
        public string name;
        public int quality; // means different things depending on the type
        public Vector2 position;
        public ItemType type;
        public Item CreateItem(string name, ItemType type, int quality, Vector2 position)
        {
            Item i = new Item();
            i.name = name;
            i.type = type;
            i.quality = quality;
            i.position = position;
            return i;
        }

        public Item CreateRandomItem(Random random, Vector2 position)
        {
            ItemType type = Enum.GetValues<ItemType>()[random.Next(4)];
            Item i = type switch
            {
                ItemType.Treasure => CreateItem("Book", type, 2, position),
                ItemType.Weapon => CreateItem("Sword", type, 3, position),
                ItemType.Armor => CreateItem("Helmet", type, 1, position),
                ItemType.Potion => CreateItem("Apple Juice", type, 1, position)
            };
            return i;
        }

        public List<Item> CreateItems(Map level, Random random)
        {
            List<Item> items = new List<Item>();

            for (int y = 0; y < level.height; y++)
            {
                for (int x = 0; x < level.width; x++)
                {
                    int ti = y * level.width + x;
                    if (level.Tiles[ti] == Map.Tile.Item)
                    {
                        Item m = CreateRandomItem(random, new Vector2(x, y));
                        items.Add(m);
                        level.Tiles[ti] = (sbyte)Map.Tile.Floor;
                    }
                }
            }
            return items;
        }

        public void DrawItems(List<Item> items, Program program)
        {
            foreach (Item m in items)
            {
                Console.SetCursorPosition((int)m.position.X, (int)m.position.Y);
                char symbol = '$';
                ConsoleColor color = ConsoleColor.Yellow;
                switch (m.type)
                {
                    case ItemType.Armor:
                        symbol = '[';
                        color = ConsoleColor.White;
                        break;
                    case ItemType.Weapon:
                        symbol = '}';
                        color = ConsoleColor.Cyan;
                        break;
                    case ItemType.Treasure:
                        symbol = '$';
                        color = ConsoleColor.Yellow;
                        break;
                    case ItemType.Potion:
                        symbol = '!';
                        color = ConsoleColor.Red;
                        break;
                }
                program.Print(symbol, color);
            }
        }

        public void GiveItem(PlayerCharacter character, Item item)
        {
            // Inventory order
            // Weapons
            // Armors
            // Potions
            switch (item.type)
            {
                case ItemType.Weapon:
                    if ((character.weapon != null && character.weapon.quality < item.quality)
                        || character.weapon == null)
                    {
                        character.weapon = item;
                    }
                    character.inventory.Insert(0, item);
                    break;
                case ItemType.Armor:
                    if ((character.armor != null && character.armor.quality < item.quality)
                        || character.armor == null)
                    {
                        character.armor = item;
                    }
                    int armorIndex = 0;
                    while (armorIndex < character.inventory.Count && character.inventory[armorIndex].type == ItemType.Weapon)
                    {
                        armorIndex++;
                    }
                    character.inventory.Insert(armorIndex, item);
                    break;
                case ItemType.Potion:
                    character.inventory.Add(item);
                    break;
                case ItemType.Treasure:
                    character.gold += item.quality;
                    break;
            }

        }

        public void UseItem(PlayerCharacter character, Item item, List<string> messages)
        {
            switch (item.type)
            {
                case ItemType.Weapon:
                    character.weapon = item;
                    messages.Add($"You are now wielding a {item.name}");
                    break;
                case ItemType.Armor:
                    character.armor = item;
                    messages.Add($"You equip {item.name} on yourself.");
                    break;
                case ItemType.Potion:
                    character.hitpoints += item.quality;
                    if (character.hitpoints > character.maxHitpoints)
                    {
                        //character.maxHitpoints = character.hitpoints;
                        character.hitpoints = character.maxHitpoints;
                    }
                    messages.Add($"You drink a potion and gain {item.quality} hitpoints");
                    character.inventory.Remove(item);
                    break;
            }
        }


    }

}
