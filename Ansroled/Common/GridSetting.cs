using System;

namespace Ansroled.Common;

public record GridSetting()
{
    public int Column { get; init; }
    public int Row { get; init; }
    public int ColumnSpan { get; init; }  = 1;
    public int RowSpan { get; init; }  = 1;
    public bool IsVisible { get; set; } = true;

    public GridSetting(int column, int row) : this()
    {
        Column = column;
        Row = row;
    }
    
    public GridSetting(int column, int row, bool isVisible) : this(column, row)
    {
        IsVisible = isVisible;
    }
    
    public GridSetting(int column, int row, int columnSpan, int rowSpan) : this(column, row)
    {
        ColumnSpan = columnSpan;
        RowSpan = rowSpan;
    }
    
    public GridSetting(int column, int row, int columnSpan, int rowSpan, bool isVisible) : this(column, row, columnSpan, rowSpan)
    {
        IsVisible = isVisible;
    }

    public static GridSetting Box0 => new(0, 0, 1, 3);
    public static GridSetting Box1 => new(2, 0);
    public static GridSetting Box2 => new(2, 2);
    public static GridSetting Box3 => new(4, 0);
    public static GridSetting Box4 => new(4, 2);
    public static GridSetting Box5 => new(6, 0);
    public static GridSetting Box6 => new(6, 2);
    public static GridSetting Hidden => new(0, 0, false);

    public static GridSetting GetBoxFromIndex(int index)
    {
        return index switch
        {
            0 => Box0,
            1 => Box1,
            2 => Box2,
            3 => Box3,
            4 => Box4,
            5 => Box5,
            6 => Box6,
            _ => throw new IndexOutOfRangeException()
        };
    }
}