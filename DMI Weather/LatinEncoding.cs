using System;
using System.Text;

namespace DMI_Weather
{
    public class LatinEncoding : Encoding
    {
        public override string WebName
        {
            get
            {
                return "iso-8859-1";
            }
        }

        public LatinEncoding()
        {
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            for (int i = 0; i < charCount; i++)
            {
                bytes[byteIndex + i] = (byte)chars[i + charIndex];
            }

            return charCount;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            for (int i = 0; i < byteCount; i++)
            {
                chars[charIndex + i] = (char)bytes[i + byteIndex];
            }

            return byteCount;
        }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            return count;
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return count;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return byteCount;
        }
    }
}
