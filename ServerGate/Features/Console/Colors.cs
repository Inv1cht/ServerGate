namespace ServerGate.Features.Console
{
    public static class ColorsExtensions
    {
        public static ConsoleColor ToConsoleColor(this Colors colors)
        {
            return colors switch
            {
                Colors.Black => ConsoleColor.Black,
                Colors.White => ConsoleColor.White,
                Colors.Red => ConsoleColor.Red,
                Colors.Green => ConsoleColor.Green,
                Colors.Blue => ConsoleColor.Blue,
                Colors.Yellow => ConsoleColor.Yellow,
                Colors.Cyan => ConsoleColor.Cyan,
                Colors.Magenta => ConsoleColor.Magenta,
                Colors.Gray => ConsoleColor.Gray,
                Colors.Grey => ConsoleColor.DarkGray,
                _ => ConsoleColor.Black,
            };
        }
    }

    public enum Colors : byte
    {
        Black = 0x00,
        White = 0x01,
        Red = 0x02,
        Green = 0x03,
        Blue = 0x04,
        Yellow = 0x05,
        Cyan = 0x06,
        Magenta = 0x07,
        Gray = 0x0C,
        Grey = 0x0D,
        None = 0x0F
    }
}
