using System;
using System.Collections.Generic;
using ReMarkableRemember.Common.Localization;
using ReMarkableRemember.Common.Notebook.Enumerations;
using ReMarkableRemember.Common.Notebook.Exceptions;

namespace ReMarkableRemember.Common.Notebook;

public partial class Page
{
    private sealed class Version6(Buffer buffer, Int32 index, Int32 resolution) : Page(index, resolution, Parse(buffer))
    {
        private static List<Line> Parse(Buffer buffer)
        {
            List<Line> lines = new List<Line>();

            while (buffer.Position < buffer.Length)
            {
                Byte blockType = ReadBlockHeader(buffer, out Int32 blockLength, out Byte version);
                if (blockType != 5)
                {
                    buffer.Skip(blockLength);
                }
                else
                {
                    Int32 blockEndPosition = buffer.Position + blockLength;

                    buffer.ReadId(1); // parent_id
                    buffer.ReadId(2); // item_id
                    buffer.ReadId(3); // left_id
                    buffer.ReadId(4); // right_id
                    buffer.ReadInt32(5); // deleted_length
                    if (buffer.Position < blockEndPosition)
                    {
                        Int32 subBlockLength = buffer.ReadSubBlockLength(6);
                        Int32 itemType = buffer.ReadByte();
                        if (itemType != 3)
                        {
                            buffer.Skip(subBlockLength);
                        }
                        else
                        {
                            lines.Add(ReadLine(buffer, version));
                        }

                        Int32 remaining = blockEndPosition - buffer.Position;
                        buffer.Skip(remaining);
                    }
                }
            }

            return lines;
        }

        private static Byte ReadBlockHeader(Buffer buffer, out Int32 length, out Byte version)
        {
            length = buffer.ReadInt32();
            Byte unknown = buffer.ReadByte();
            Byte versionMinimum = buffer.ReadByte();
            version = buffer.ReadByte();
            Byte type = buffer.ReadByte();

            if (unknown != 0) { throw new NotebookException(Language.Current.NotebookBlockHeaderInvalid(unknown)); }
            if (versionMinimum > version) { throw new NotebookException(Language.Current.NotebookBlockHeaderVersionInvalid); }
            if (version > 2) { throw new NotebookException(Language.Current.NotebookBlockHeaderVersionUnknown(version)); }

            return type;
        }

        private static Line ReadLine(Buffer buffer, Int32 version)
        {
            PenType type = (PenType)buffer.ReadInt32(1);
            PenColor color = (PenColor)buffer.ReadInt32(2);
            buffer.ReadDouble(3); // thickness_scale
            buffer.ReadSingle(4); // starting_length

            List<Point> points = new List<Point>();
            Int32 pointsDataLength = buffer.ReadSubBlockLength(5);
            Int32 pointsEndPosition = buffer.Position + pointsDataLength;
            while (buffer.Position < pointsEndPosition)
            {
                points.Add(ReadPoint(buffer, version));
            }

            buffer.ReadId(6); // timestamp

            return new Line(color, type, points);
        }

        private static Point ReadPoint(Buffer buffer, Int32 version)
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

            return new Point(x, y);
        }
    }
}
