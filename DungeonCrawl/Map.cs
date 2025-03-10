using System.Numerics;

namespace DungeonCrawl
{
    /// <summary>
    /// Make the map
    /// Checks where stuff on the map is
    /// Process map changes
    /// </summary>
    internal class Map
    {
        public enum Tile : sbyte
        {
            Floor,
            Wall,
            Door,
            Monster,
            Item,
            Shop,
            ShopDoor,
            Player,
            Stairs
        }
        public int width;
        public int height;
        public Tile[] Tiles;

        public void DrawTile(byte x, byte y, Tile tile, Program program)
        {
            Console.SetCursorPosition(x, y);
            switch (tile)
            {
                case Tile.Floor:
                    program.Print(".", ConsoleColor.Gray); break;

                case Tile.Wall:
                    program.Print("#", ConsoleColor.DarkGray); break;

                case Tile.Door:
                    program.Print("+", ConsoleColor.Yellow); break;

                case Tile.Stairs:
                    program.Print(">", ConsoleColor.Yellow); break;

                case Tile.Shop:
                    program.Print("ß", ConsoleColor.DarkCyan); break;

                case Tile.ShopDoor:
                    program.Print("+", ConsoleColor.DarkYellow); break;

                default: break;
            }
        }

        public void DrawMapAll(Map level, Program program)
        {
            for (byte y = 0; y < level.height; y++)
            {
                for (byte x = 0; x < level.width; x++)
                {
                    int ti = y * level.width + x;
                    Tile tile = level.Tiles[ti];
                    DrawTile(x, y, tile, program);
                }
            }
        }

        public void DrawMap(Map level, List<int> dirtyTiles, Program program)
        {
            if (dirtyTiles.Count == 0)
            {
                DrawMapAll(level, program);
            }
            else
            {
                foreach (int dt in dirtyTiles)
                {
                    byte x = (byte)(dt % level.width);
                    byte y = (byte)(dt / level.width);
                    Tile tile = level.Tiles[dt];
                    DrawTile(x, y, tile, program);
                }
            }
        }

        public void PlaceStairsToMap(Map level)
        {
            for (int i = level.Tiles.Length - 1; i >= 0; i--)
            {
                if (level.Tiles[i] == Tile.Floor)
                {
                    level.Tiles[i] = Tile.Stairs;
                    break;
                }
            }
        }

        public int PositionToTileIndex(Vector2 position, Map level)
        {
            return (int)position.X + (int)position.Y * level.width;
        }

        public Tile GetTileAtMap(Map level, Vector2 position)
        {
            if (position.X >= 0 && position.X < level.width)
            {
                if (position.Y >= 0 && position.Y < level.height)
                {
                    int ti = (int)position.Y * level.width + (int)position.X;
                    return level.Tiles[ti];
                }
            }
            return Tile.Wall;
        }



        public void PlacePlayerToMap(PlayerCharacter character, Map level)
        {
            for (int i = 0; i < level.Tiles.Length; i++)
            {
                if (level.Tiles[i] == Tile.Player)
                {
                    level.Tiles[i] = Tile.Floor;
                    int px = i % level.width;
                    int py = i / level.width;

                    character.position = new Vector2(px, py);
                    break;
                }
            }
        }
    }
}
