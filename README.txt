Changes made:
Players dies when reaching 0, not -1
Players max hp does not increase if they go over it with a potion
Death screen doesn't repeat "y/n" when given a false input
Inventory and other similar screens close when inputting the string ""
Removed "List<Monster> enemies, List<Item> items" from drawinfo, since it wasn't being used
Added an input buffer to movement, so if you hold a key, you don't move 30 times in 5 seconds and have to sit there watching it
Gave monsters different damages, so that armor can actually do something
Increased player health

Additions:
A working shop with multiple choices for what the shop has for sale
Buying the scroll in the shop lets you win the game


