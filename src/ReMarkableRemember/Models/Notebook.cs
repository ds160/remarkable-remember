using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace ReMarkableRemember.Models;

internal sealed class Notebook
{
    public Notebook(IEnumerable<Byte[]> pageBuffers, Int32 height, Int32 width, Int32 resolution)
    {
        this.Pages = pageBuffers.Select((pageBuffer, pageIndex) => new Page(pageBuffer, pageIndex, height, width, resolution)).ToArray();
    }

    public IEnumerable<Page> Pages { get; }

    internal sealed class Page
    {
        public Page(Byte[] buffer, Int32 index, Int32 height, Int32 width, Int32 resolution)
        {
            this.Height = height;
            this.Index = index;
            this.Resolution = resolution;
            this.Width = width;

            PageBuffer pageBuffer = new PageBuffer(buffer);
            String header = pageBuffer.ReadString(43);
            switch (header)
            {
                case "reMarkable .lines file, version=5          ":
                    this.Lines = V5Parse(pageBuffer);
                    break;
                case "reMarkable .lines file, version=6          ":
                    this.Lines = V6Parse(pageBuffer);
                    break;
                default:
                    throw new NotebookException("Unknown reMarkable .lines file header.");
            }
        }

        public Int32 Height { get; }

        public Int32 Index { get; }

        public IEnumerable<Line> Lines { get; }

        public Int32 Resolution { get; }

        public Int32 Width { get; }

        private static List<Line> V5Parse(PageBuffer buffer)
        {
            List<Line> lines = new List<Line>();

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

        private static Line V5ReadLine(PageBuffer buffer)
        {
            Line.PenType type = (Line.PenType)buffer.ReadInt32();
            Line.PenColor color = (Line.PenColor)buffer.ReadInt32();
            buffer.ReadInt32(); // unknown
            buffer.ReadSingle(); // thickness_scale
            buffer.ReadSingle(); // unknown

            List<Line.Point> points = new List<Line.Point>();
            Int32 pointsCounter = buffer.ReadInt32();
            while (pointsCounter-- > 0)
            {
                points.Add(V5ReadPoint(buffer));
            }

            return new Line(color, type, points);
        }

        private static Line.Point V5ReadPoint(PageBuffer buffer)
        {
            Single x = buffer.ReadSingle();
            Single y = buffer.ReadSingle();
            buffer.ReadSingle(); // speed
            buffer.ReadSingle(); // direction
            buffer.ReadSingle(); // width
            buffer.ReadSingle(); // pressure

            return new Line.Point(x, y);
        }

        private static List<Line> V6Parse(PageBuffer buffer)
        {
            List<Line> lines = new List<Line>();

            while (buffer.Position < buffer.Length)
            {
                BlockHeader blockHeader = V6ReadBlockHeader(buffer);
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
                    buffer.ReadInt32(5); // deleted_length
                    if (buffer.Position < blockHeader.EndPosition)
                    {
                        Int32 subBlockLength = buffer.ReadSubBlockLength(6);
                        Int32 itemType = buffer.ReadByte();
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

        private static BlockHeader V6ReadBlockHeader(PageBuffer buffer)
        {
            Int32 length = buffer.ReadInt32();
            Byte unknown = buffer.ReadByte();
            Byte versionMinimum = buffer.ReadByte();
            Byte versionCurrent = buffer.ReadByte();
            Byte type = buffer.ReadByte();

            if (unknown != 0) { throw new NotebookException($"Invalid reMarkable .lines file block header: '{unknown}'."); }
            if (versionMinimum > versionCurrent) { throw new NotebookException("Invalid reMarkable .lines file block header version."); }
            if (versionCurrent > 2) { throw new NotebookException($"Unknown reMarkable .lines file block header version: '{versionCurrent}'."); }

            return new BlockHeader(buffer.Position + length, length, type, versionCurrent);
        }

        private static Line V6ReadLine(PageBuffer buffer, Int32 version)
        {
            Line.PenType type = (Line.PenType)buffer.ReadInt32(1);
            Line.PenColor color = (Line.PenColor)buffer.ReadInt32(2);
            buffer.ReadDouble(3); // thickness_scale
            buffer.ReadSingle(4); // starting_length

            List<Line.Point> points = new List<Line.Point>();
            Int32 pointsDataLength = buffer.ReadSubBlockLength(5);
            Int32 pointsEndPosition = buffer.Position + pointsDataLength;
            while (buffer.Position < pointsEndPosition)
            {
                points.Add(V6ReadPoint(buffer, version));
            }

            buffer.ReadId(6); // timestamp

            return new Line(color, type, points);
        }

        private static Line.Point V6ReadPoint(PageBuffer buffer, Int32 version)
        {
            Single x = buffer.ReadSingle();
            Single y = buffer.ReadSingle();

            if (version == 1)
            {
                buffer.ReadSingle(); // speed
                buffer.ReadSingle(); // direction
                buffer.ReadSingle(); // width
                buffer.ReadSingle(); // pressure
            }
            else
            {
                buffer.ReadInt16(); // speed
                buffer.ReadInt16(); // direction
                buffer.ReadByte(); // width
                buffer.ReadByte(); // pressure
            }

            return new Line.Point(x, y);
        }

        private sealed class BlockHeader
        {
            public BlockHeader(Int32 endPosition, Int32 length, Byte type, Byte version)
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

        internal sealed class Line
        {
            internal enum PenColor
            {
                Black = 0,
                Grey = 1,
                White = 2,
                Yellow1 = 3,
                Green1 = 4,
                Pink = 5,
                Blue = 6,
                Red = 7,
                GrayOverlap = 8,
                Highlight = 9,
                Green2 = 10,
                Cyan = 11,
                Magenta = 12,
                Yellow2 = 13
            }

            internal enum PenType
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
                Caligraphy = 21,
                Shader = 23
            }

            public Line(PenColor color, PenType type, List<Point> points)
            {
                this.Color = color;
                this.Type = type;

                this.Points = points;
            }
            public PenColor Color { get; }
            public IEnumerable<Point> Points { get; }
            public PenType Type { get; }

            internal sealed class Point
            {
                public Point(Single x, Single y)
                {
                    this.X = x;
                    this.Y = y;
                }

                public Single X { get; }
                public Single Y { get; }
            }
        }

        private sealed class PageBuffer
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
    }
}
