using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ReMarkableRemember.Common.Notebook.Enumerations;

namespace ReMarkableRemember.Common.Notebook.Tests.Helpers;

/// <summary>
/// Builds binary .lines buffers in version 5 format for parsing tests.
/// </summary>
internal sealed class Version5BufferBuilder
{
    private const String Header = "reMarkable .lines file, version=5          ";

    private readonly List<List<LineSpec>> layers = new List<List<LineSpec>>();

    public Version5BufferBuilder AddLayer(params LineSpec[] lines)
    {
        this.layers.Add(new List<LineSpec>(lines));
        return this;
    }

    public Byte[] Build()
    {
        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream, Encoding.Default);

        Byte[] headerBytes = Encoding.Default.GetBytes(Header);
        if (headerBytes.Length != 43) { throw new InvalidOperationException($"Header must be 43 bytes, got {headerBytes.Length}."); }
        writer.Write(headerBytes);

        writer.Write(this.layers.Count);
        foreach (List<LineSpec> layer in this.layers)
        {
            writer.Write(layer.Count);
            foreach (LineSpec line in layer)
            {
                writer.Write((Int32)line.Type);
                writer.Write((Int32)line.Color);
                writer.Write(0);          // unknown
                writer.Write(1.0f);       // thickness_scale
                writer.Write(0.0f);       // unknown

                writer.Write(line.Points.Count);
                foreach ((Single x, Single y) in line.Points)
                {
                    writer.Write(x);
                    writer.Write(y);
                    writer.Write(0.0f);   // speed
                    writer.Write(0.0f);   // direction
                    writer.Write(1.0f);   // width
                    writer.Write(1.0f);   // pressure
                }
            }
        }

        return stream.ToArray();
    }

    public static Byte[] WithUnknownHeader()
    {
        Byte[] bytes = new Byte[43];
        Encoding.Default.GetBytes("not-a-real-reMarkable-file".PadRight(43, ' '), 0, 26, bytes, 0);
        return bytes;
    }
}

internal sealed class LineSpec
{
    public PenType Type { get; init; } = PenType.Fineliner1;
    public PenColor Color { get; init; } = PenColor.Black;
    public List<(Single X, Single Y)> Points { get; init; } = new List<(Single X, Single Y)>();
}
