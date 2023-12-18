using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ReMarkableRemember.Models;

internal enum NotebookBufferTagType
{
    Byte4 = 0x4,
    Byte8 = 0x8,
    Length4 = 0xC,
    ID = 0xF
}

[Flags]
public enum NotebookLineColor
{
    Black = 0,
    Grey = 1,
    White = 2,
    Yellow = 3,
    Green = 4,
    Pink = 5,
    Blue = 6,
    Red = 7,
    GrayOverlap = 8
}

[Flags]
public enum NotebookLineType
{
    Brush1 = 0,
    TiltPencil1 = 1,
    BallPoint1 = 2,
    Marker1 = 3,
    Fineliner1 = 4,
    Highlighter1 = 5,
    Eraser = 6,
    SharpPencil1 = 7,
    EraseArea = 8,
    Brush2 = 12,
    SharpPencil2 = 13,
    TiltPencil2 = 14,
    BallPoint2 = 15,
    Marker2 = 16,
    Fineliner2 = 17,
    Highlighter2 = 18,
    Caligraphy = 21
}

internal sealed class NotebookBuffer
{
    private readonly Byte[] buffer;

    public NotebookBuffer(Byte[] buffer)
    {
        this.buffer = buffer;
        this.Position = 0;
    }

    public Int32 Length { get { return this.buffer.Length; } }

    public Int32 Position { get; private set; }

    public Double ReadDouble(Int32? index = null)
    {
        if (index.HasValue)
        {
            this.ReadTag(index.Value, NotebookBufferTagType.Byte8);
        }

        return this.Read(offset => BitConverter.ToDouble(this.buffer, offset), 8);
    }

    public Single ReadFloat(Int32? index = null)
    {
        if (index.HasValue)
        {
            this.ReadTag(index.Value, NotebookBufferTagType.Byte4);
        }

        return this.Read(offset => BitConverter.ToSingle(this.buffer, offset), 4);
    }

    public (Int32 part1, Int32 part2) ReadId(Int32 index)
    {
        this.ReadTag(index, NotebookBufferTagType.ID);

        Int32 part1 = this.ReadUInt8();
        Int32 part2 = this.ReadUIntVariable();

        return (part1, part2);
    }

    public Int32 ReadInt32()
    {
        return this.Read(offset => BitConverter.ToInt32(this.buffer, offset), 4);
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
        this.ReadTag(index, NotebookBufferTagType.Length4);

        return this.ReadUInt32();
    }

    public Byte ReadUInt8()
    {
        return this.Read(offset => this.buffer[offset], 1);
    }

    public Int32 ReadUInt16()
    {
        return this.Read(offset => BitConverter.ToUInt16(this.buffer, offset), 2);
    }

    public Int32 ReadUInt32(Int32? index = null)
    {
        if (index.HasValue)
        {
            this.ReadTag(index.Value, NotebookBufferTagType.Byte4);
        }

        UInt32 result = this.Read(offset => BitConverter.ToUInt32(this.buffer, offset), 4);
        return Convert.ToInt32(result);
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

    private void ReadTag(Int32 expectedIndex, NotebookBufferTagType expectedType)
    {
        Int32 tag = this.ReadUIntVariable();
        Int32 tagIndex = tag >> 4;
        Int32 tagType = tag & 0xF;

        if (tagIndex != expectedIndex) { throw new NotebookException("Invalid reMarkable .lines file block tag index."); }
        if (tagType != (Int32)expectedType) { throw new NotebookException("Invalid reMarkable .lines file block tag type."); }
    }

    private Int32 ReadUIntVariable()
    {
        Int32 result = 0;
        Int32 shift = 0;

        while (true)
        {
            Int32 byteValue = this.ReadUInt8();

            result |= (byteValue & 0x7F) << shift;
            shift += 7;

            if ((byteValue & 0x80) == 0) { break; }
        }

        return result;
    }
}

internal sealed class NotebookBlockHeader
{
    public NotebookBlockHeader(Int32 endPosition, Int32 length, Byte type, Byte version)
    {
        this.EndPosition = endPosition;
        this.Length = length;
        this.Type = type;
        this.Version = version;
    }

    public Int32 EndPosition { get; }
    public Int32 Length { get; }
    public Byte Type { get; }
    public Byte Version { get; }
}

public class NotebookException : Exception
{
    public NotebookException() { }

    public NotebookException(String message) : base(message) { }

    public NotebookException(String message, Exception innerException) : base(message, innerException) { }
}

public class NotebookLine
{
    public NotebookLine(NotebookLineColor color, NotebookLineType type)
    {
        this.Color = color;
        this.Type = type;

        this.Points = new Collection<NotebookLinePoint>();
    }
    public NotebookLineColor Color { get; }
    public Collection<NotebookLinePoint> Points { get; }
    public NotebookLineType Type { get; }
}

public class NotebookLinePoint
{
    public NotebookLinePoint(Single x, Single y)
    {
        this.X = x;
        this.Y = y;
    }

    public Single X { get; }
    public Single Y { get; }
}

public class Notebook
{
    public Notebook(Byte[] buffer)
    {
        NotebookBuffer notebookBuffer = new NotebookBuffer(buffer);

        String header = notebookBuffer.ReadString(43);
        switch (header)
        {
            case "reMarkable .lines file, version=5          ":
                this.Lines = V5Parse(notebookBuffer);
                break;
            case "reMarkable .lines file, version=6          ":
                this.Lines = V6Parse(notebookBuffer);
                break;
            default:
                throw new NotebookException("Unknown reMarkable .lines file header.");
        }
    }

    public IEnumerable<NotebookLine> Lines { get; }

    private static List<NotebookLine> V5Parse(NotebookBuffer buffer)
    {
        List<NotebookLine> lines = new List<NotebookLine>();

        Int32 layersCounter = buffer.ReadInt32();
        while (layersCounter-- > 0)
        {
            Int32 linesCounter = buffer.ReadInt32();
            while (linesCounter-- > 0)
            {
                lines.Add(V5ReadLine(buffer));
            }
        }

        return lines;
    }

    private static NotebookLine V5ReadLine(NotebookBuffer buffer)
    {
        NotebookLineType type = (NotebookLineType)buffer.ReadInt32();
        NotebookLineColor color = (NotebookLineColor)buffer.ReadInt32();
        buffer.ReadInt32(); // unknown
        buffer.ReadFloat(); // thickness_scale
        buffer.ReadFloat(); // unknown

        NotebookLine line = new NotebookLine(color, type);

        Int32 pointsCounter = buffer.ReadInt32();
        while (pointsCounter-- > 0)
        {
            line.Points.Add(V5ReadPoint(buffer));
        }

        return line;
    }

    private static NotebookLinePoint V5ReadPoint(NotebookBuffer buffer)
    {
        Single x = buffer.ReadFloat();
        Single y = buffer.ReadFloat();
        buffer.ReadFloat(); // speed
        buffer.ReadFloat(); // direction
        buffer.ReadFloat(); // width
        buffer.ReadFloat(); // pressure

        return new NotebookLinePoint(x, y);
    }

    private static List<NotebookLine> V6Parse(NotebookBuffer buffer)
    {
        List<NotebookLine> lines = new List<NotebookLine>();

        while (buffer.Position < buffer.Length)
        {
            NotebookBlockHeader blockHeader = V6ReadBlockHeader(buffer);
            if (blockHeader.Type != 5)
            {
                buffer.Skip(blockHeader.Length);
            }
            else
            {
                buffer.ReadId(1); // parent_id
                buffer.ReadId(2); // item_id
                buffer.ReadId(3); // left_id
                buffer.ReadId(4); // right_id
                buffer.ReadUInt32(5); // deleted_length
                if (buffer.Position < blockHeader.EndPosition)
                {
                    Int32 subBlockLength = buffer.ReadSubBlockLength(6);
                    Int32 itemType = buffer.ReadUInt8();
                    if (itemType != 3)
                    {
                        buffer.Skip(subBlockLength);
                    }
                    else
                    {
                        lines.Add(V6ReadLine(buffer, blockHeader.Version));
                    }

                    Int32 remaining = blockHeader.EndPosition - buffer.Position;
                    buffer.Skip(remaining);
                }
            }
        }

        return lines;
    }

    private static NotebookBlockHeader V6ReadBlockHeader(NotebookBuffer buffer)
    {
        Int32 length = buffer.ReadUInt32();
        Byte unknown = buffer.ReadUInt8();
        Byte versionMinimum = buffer.ReadUInt8();
        Byte versionCurrent = buffer.ReadUInt8();
        Byte type = buffer.ReadUInt8();

        if (unknown != 0) { throw new NotebookException("Invalid reMarkable .lines file block header."); }
        if (versionMinimum > versionCurrent) { throw new NotebookException("Invalid reMarkable .lines file block header version."); }
        if (versionCurrent > 2) { throw new NotebookException("Unknown reMarkable .lines file block header version."); }

        return new NotebookBlockHeader(buffer.Position + length, length, type, versionCurrent);
    }

    private static NotebookLine V6ReadLine(NotebookBuffer buffer, Int32 version)
    {
        NotebookLineType type = (NotebookLineType)buffer.ReadUInt32(1);
        NotebookLineColor color = (NotebookLineColor)buffer.ReadUInt32(2);
        buffer.ReadDouble(3); // thickness_scale
        buffer.ReadFloat(4); // starting_length

        NotebookLine line = new NotebookLine(color, type);

        Int32 pointsDataLength = buffer.ReadSubBlockLength(5);
        Int32 pointsEndPosition = buffer.Position + pointsDataLength;
        while (buffer.Position < pointsEndPosition)
        {
            line.Points.Add(V6ReadPoint(buffer, version));
        }

        buffer.ReadId(6); // timestamp

        return line;
    }

    private static NotebookLinePoint V6ReadPoint(NotebookBuffer buffer, Int32 version)
    {
        Single x = buffer.ReadFloat();
        Single y = buffer.ReadFloat();

        if (version == 1)
        {
            buffer.ReadFloat(); // speed
            buffer.ReadFloat(); // direction
            buffer.ReadFloat(); // width
            buffer.ReadFloat(); // pressure
        }
        else
        {
            buffer.ReadUInt16(); // speed
            buffer.ReadUInt16(); // direction
            buffer.ReadUInt8(); // width
            buffer.ReadUInt8(); // pressure
        }

        return new NotebookLinePoint(x, y);
    }
}
