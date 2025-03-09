namespace OTPService.Utilities
{
    public static class RandomGenerator
    {
        #region charcters
        private static List<char> chars = (new char[] { '0',
                                '1',
                                '2',
                                '3',
                                '4',
                                '5',
                                '6',
                                '7',
                                '8',
                                '9',
                                'a',
                                'b',
                                'c',
                                'd',
                                'e',
                                'f',
                                'g',
                                'h',
                                'i',
                                'j',
                                'k',
                                'l',
                                'm',
                                'n',
                                'o',
                                'p',
                                'q',
                                'r',
                                's',
                                't',
                                'u',
                                'v',
                                'w',
                                'x',
                                'y',
                                'z',
                                'A',
                                'B',
                                'C',
                                'D',
                                'E',
                                'F',
                                'G',
                                'H',
                                'I',
                                'J',
                                'K',
                                'L',
                                'M',
                                'N',
                                'O',
                                'P',
                                'Q',
                                'R',
                                'S',
                                'T',
                                'U',
                                'V',
                                'W',
                                'X',
                                'Y',
                                'Z' }).ToList();
        #endregion

        public static string GenerateRandomString(int PasswordLength)
        {
            return GenerateRandomString(PasswordLength, true);
        }

        public static string GenerateRandomString(int PasswordLength, bool NumbersOnly)
        {
            string Password = "";
            Random random = new Random();
            int MaxCharsIndex = NumbersOnly ? 9 : chars.Count - 1;
            for (int i = 0; i < PasswordLength; i++)
            {
                int AsciiCode = Convert.ToInt32(random.NextDouble() * MaxCharsIndex);
                Password += chars[AsciiCode];
            }
            return Password;
        }

        public static string GenerateRandomCharacters(int PasswordLength)
        {
            string Password = "";
            Random random = new Random();
            var tmpChars = chars.Skip(10).ToList();
            int MaxCharsIndex = tmpChars.Count - 1;
            for (int i = 0; i <= PasswordLength - 1; i++)
            {
                int AsciiCode = Convert.ToInt32(random.NextDouble() * MaxCharsIndex);
                Password += tmpChars[AsciiCode];
            }
            return Password;
        }
    }
}
