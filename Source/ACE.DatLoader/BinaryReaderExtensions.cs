using System;
using System.IO;

namespace ACE.DatLoader
{
    static class BinaryReaderExtensions
    {
        /// <summary>
        /// Aligns the stream to the next DWORD boundary.
        /// </summary>
        public static void AlignBoundary(this BinaryReader reader)
        {
            // Aligns the DatReader to the next DWORD boundary.
            long alignDelta = reader.BaseStream.Position % 4;

            if (alignDelta != 0)
                reader.BaseStream.Position += (int)(4 - alignDelta);
        }


        /// <summary>
        /// A Compressed UInt32 can be 1, 2, or 4 bytes.<para />
        /// If the first MSB (0x80) is 0, it is one byte.<para />
        /// If the first MSB (0x80) is set and the second MSB (0x40) is 0, it's 2 bytes.<para />
        /// If both (0x80) and (0x40) are set, it's 4 bytes.
        /// </summary>
        public static uint ReadCompressedUInt32(this BinaryReader reader)
        {
            var b0 = reader.ReadByte();
            if ((b0 & 0x80) == 0)
                return b0;

            var b1 = reader.ReadByte();
            if ((b0 & 0x40) == 0)
                return (uint)(((b0 & 0x7F) << 8) | b1);

            var s = reader.ReadUInt16();
            return (uint)(((((b0 & 0x3F) << 8) | b1) << 16) | s);
        }

        /// <summary>
        /// First reads a UInt16. If the MSB is set, it will be masked with 0x3FFF, shifted left 2 bytes, and then OR'd with the next UInt16. The sum is then added to knownType.
        /// </summary>
        public static uint ReadAsDataIDOfKnownType(this BinaryReader reader, uint knownType)
        {
            var value = reader.ReadUInt16();

            if ((value & 0x8000) != 0)
            {
                var lower = reader.ReadUInt16();
                var higher = (value & 0x3FFF) << 16;

                return (uint)(knownType + (higher | lower));
            }

            return (knownType + value);
        }

        /// <summary>
        /// Returns a string as defined by the first 2-byte's length
        /// </summary>
        public static string ReadPString(this BinaryReader reader)
        {
            int stringlength = reader.ReadUInt16();

            byte[] thestring = reader.ReadBytes(stringlength);

            return System.Text.Encoding.Default.GetString(thestring);
        }
    }
}
