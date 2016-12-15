﻿using System;
using System.IO;
using MiscUtil.IO;
using MiscUtil.Conversion;

namespace TSO_E_Cityserver.Refpack
{
    public class RefpackStream
    {
        private EndianBinaryReader m_Reader;

        public byte BodyType = 0;
        public uint DecompressedSize = 0;
        public uint CompressedSize = 0;

        public RefpackStream(Stream Strm, bool LittleEndian)
        {
            if (LittleEndian)
                m_Reader = new EndianBinaryReader(new LittleEndianBitConverter(), Strm);
            else
                m_Reader = new EndianBinaryReader(new BigEndianBitConverter(), Strm);
        }

        /// <summary>
        /// Reads a Stream header and returns a decompressed Refpack body.
        /// </summary>
        public Stream Decompress()
        {
            byte BodyType = ReadByte();
            uint DecompressedSize = ReadUInt32();

            if (BodyType == 0)
                return new MemoryStream(ReadBytes((int)DecompressedSize));
            else
            {
                uint CompressedSize = ReadUInt32();
                //This is *always* in little endian, regardless of the rest
                //of the stream, probably to avoid switching, because its
                //value is the same as the above field.
                uint StreamSize = ReadUInt32();

                Decompresser Dec = new Decompresser();
                byte[] DecompressedData = Dec.Decompress(ReadBytes((int)CompressedSize));

                return new MemoryStream(DecompressedData);
            }
        }

        /// <summary>
        /// Reads an unsigned 32bit integer from the underlying RefPack stream.
        /// </summary>
        /// <returns>A uint.</returns>
        public uint ReadUInt32()
        {
            return m_Reader.ReadUInt32();
        }

        /// <summary>
        /// Reads a byte from the underlying RefPack stream.
        /// </summary>
        /// <returns>A byte.</returns>
        public byte ReadByte()
        {
            return m_Reader.ReadByte();
        }

        /// <summary>
        /// Reads a number of bytes from the underlying RefPack stream.
        /// </summary>
        /// <returns>An array of bytes.</returns>
        public byte[] ReadBytes(int Count)
        {
            return m_Reader.ReadBytes(Count);
        }

        private void SwitchEndianNess(EndianBitConverter Endianness)
        {
            long CurrentPos = m_Reader.BaseStream.Position;
            Stream CopyOfStream = m_Reader.BaseStream;
            m_Reader = new EndianBinaryReader(Endianness, CopyOfStream);
            m_Reader.Seek((int)CurrentPos, SeekOrigin.Begin);
        }
    }
}
