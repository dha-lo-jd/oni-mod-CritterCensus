using System.Collections.Generic;

namespace CritterCensus;

// The area a census is taken over, as a set of grid cells. Drag selections and stable rooms both reduce to this.
public sealed class CensusSelection
{
    public readonly HashSet<int> Cells = new HashSet<int>();

    public static CensusSelection FromRect(int cellA, int cellB)
    {
        var selection = new CensusSelection();
        Grid.CellToXY(cellA, out var ax, out var ay);
        Grid.CellToXY(cellB, out var bx, out var by);
        for (var y = System.Math.Min(ay, by); y <= System.Math.Max(ay, by); y++)
        {
            for (var x = System.Math.Min(ax, bx); x <= System.Math.Max(ax, bx); x++)
            {
                var cell = Grid.XYToCell(x, y);
                if (Grid.IsValidCell(cell))
                {
                    selection.Cells.Add(cell);
                }
            }
        }

        return selection;
    }

    public static CensusSelection FromCells(IEnumerable<int> cells)
    {
        var selection = new CensusSelection();
        foreach (var cell in cells)
        {
            selection.Cells.Add(cell);
        }

        return selection;
    }
}
