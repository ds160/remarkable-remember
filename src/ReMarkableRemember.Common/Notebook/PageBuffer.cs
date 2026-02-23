using System;
using System.Text;
using ReMarkableRemember.Common.Notebook.Exceptions;

namespace ReMarkableRemember.Common.Notebook;

internal sealed class PageBuffer
{
    private enum TagType
    {
        Byte4 = 0x4,
        Byte8 = 0x8,
        Length4 = 0xC,
        ID = 0xF
    }

    private readonly Byte[] buffer;

    public PageBuffer(Byte[] buffer)
    {
        this.buffer = buffer;
        this.Position = 0;
    }

    public Int32 Length { get { return this.buffer.Length; } }

    public Int32 Position { get; private set; }

    public Byte ReadByte()
    {
        return this.Read(offset => this.buffer[offset], 1);
    }

    public Double ReadDouble(Int32? index = null)
    {
        if (index.HasValue)
        {
            this.ReadTag(index.Value, TagType.Byte8);
        }

        return this.Read(offset => BitConverter.ToDouble(this.buffer, offset), 8);
    }

    public (Int32 part1, Int32 part2) ReadId(Int32 index)
    {
        this.ReadTag(index, TagType.ID);

        Int32 part1 = this.ReadByte();
        Int32 part2 = this.ReadUIntVariable();

        return (part1, part2);
    }

    public Int16 ReadInt16()
    {
        return this.Read(offset => BitConverter.ToInt16(this.buffer, offset), 2);
    }

    public Int32 ReadInt32(Int32? index = null)
    {
        if (index.HasValue)
        {
            this.ReadTag(index.Value, TagType.Byte4);
        }

        return this.Read(offset => BitConverter.ToInt32(this.buffer, offset), 4);
    }

    public Single ReadSingle(Int32? index = null)
    {
        if (index.HasValue)
        {
            this.ReadTag(index.Value, TagType.Byte4);
        }

        return this.Read(offset => BitConverter.ToSingle(this.buffer, offset), 4);
    }

    public String ReadString(Int32 length)
    {
        Byte[] bytes = new Byte[length];
        Buffer.BlockCopy(this.buffer, this.Position, bytes, 0, length);

        this.Position += length;

        return Encoding.Default.GetString(bytes);
    }

    public Int32 ReadSubBlockLength(Int32 index)
    {
        this.ReadTag(index, TagType.Length4);

        return this.ReadInt32();
    }

    public void Skip(Int32 length)
    {
        this.Position += length;
    }

    private T Read<T>(Func<Int32, T> readFromBuffer, Int32 size)
    {
        T result = readFromBuffer(this.Position);

        this.Position += size;

        return result;
    }

    private void ReadTag(Int32 expectedIndex, TagType expectedType)
    {
        Int32 tag = this.ReadUIntVariable();
        Int32 tagIndex = tag >> 4;
        Int32 tagType = tag & 0xF;

        if (tagIndex != expectedIndex) { throw new NotebookException("Invalid reMarkable .lines file block tag index."); }
        if (tagType != (Int32)expectedType) { throw new NotebookException("Invalid reMarkable .lines file block tag type."); }
    }

    private Int32 ReadUIntVariable()
    {
        Int32 byteValue;
        Int32 result = 0;
        Int32 shift = 0;

        do
        {
            byteValue = this.ReadByte();
            result |= (byteValue & 0x7F) << shift;
            shift += 7;
        }
        while ((byteValue & 0x80) != 0);

        return result;
    }
}
