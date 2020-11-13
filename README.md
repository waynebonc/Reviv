# Reviv
Extract critical SysCfg data like Serial Number, Wi-Fi & Bluetooth address from a corrupted or unreadable NAND dump.

<kbd><img src="https://github.com/waynebonc/Reviv/blob/master/image.jpg" width="584"></kbd>

## Usage
1. Dump the NAND SysCfg partition into a .bin or .dump (or any other extension)
1. Feed it into the tool.
1. Save the carved data.
1. Re-program a new NAND with the newly saved details.

## Note
IP Box users can dump the boot data and feed it directly to the tool.

## Credits
- [mjs3339](https://github.com/mjs3339) - Boyer Moore implementation in C#
