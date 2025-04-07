Changes made:
Players dies when reaching 0, not -1
Players max hp does not increase if they go over it with a potion
Death screen doesn't repeat "y/n" when given a false input
Inventory and other similar screens close when inputting the string ""
Removed "List<Monster> enemies, List<Item> items" from drawinfo, since it wasn't being used

Additions:
A working shop

Ideas:
Make the win condition be to buy a teleportation scroll from a merchant/shop (50g or smth)

Code Ideas:
Making the shop can be inputted into the AddRoom() function, to give a random chance for the room to be a shop instead of a normal room
(Done)
