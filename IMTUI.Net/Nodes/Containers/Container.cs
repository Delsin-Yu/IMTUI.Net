namespace IMTUI.Nodes.Containers;

internal abstract class Container : Control
{
    protected Size GetOverlayMinimumSize()
    {
        var maxWidth = 0;
        var maxHeight = 0;

        foreach (var child in Children)
        {
            var childMinimum = child.GetCombinedMinimumSize();
            if (childMinimum.Width > maxWidth) maxWidth = childMinimum.Width;
            if (childMinimum.Height > maxHeight) maxHeight = childMinimum.Height;
        }

        return (maxWidth, maxHeight);
    }

    protected Size GetLinearMinimumSize(int separation, LayoutAxis axis)
    {
        var childCount = Children.Count;
        if (childCount == 0) return Size.Zero;

        var normalizedSeparation = Math.Max(0, separation);
        var totalPrimary = normalizedSeparation * Math.Max(0, childCount - 1);
        var maxCross = 0;

        foreach (var child in Children)
        {
            var childMinimum = child.GetCombinedMinimumSize();
            var childPrimary = axis == LayoutAxis.Horizontal ? childMinimum.Width : childMinimum.Height;
            var childCross = axis == LayoutAxis.Horizontal ? childMinimum.Height : childMinimum.Width;

            totalPrimary += childPrimary;
            if (childCross > maxCross) maxCross = childCross;
        }

        return axis == LayoutAxis.Horizontal
            ? (totalPrimary, maxCross)
            : (maxCross, totalPrimary);
    }

    protected Size LayoutOverlayChildren(Size availableSize)
    {
        var normalizedSize = NormalizeAvailableSize(availableSize);
        var availableWidth = normalizedSize.Width;
        var availableHeight = normalizedSize.Height;

        if (Children.Count == 0)
        {
            SetLayoutSize(Size.Zero);
            return LayoutSize;
        }

        var minX = 0;
        var minY = 0;
        var maxRight = 0;
        var maxBottom = 0;

        foreach (var child in Children)
        {
            child.Position = (0, 0);
            child.Layout((availableWidth, availableHeight));

            if (child.Position.X < minX) minX = child.Position.X;
            if (child.Position.Y < minY) minY = child.Position.Y;
            if (child.Position.X + child.LayoutSize.Width > maxRight) maxRight = child.Position.X + child.LayoutSize.Width;
            if (child.Position.Y + child.LayoutSize.Height > maxBottom) maxBottom = child.Position.Y + child.LayoutSize.Height;
        }

        ClampSizeToAvailableWithGrow((maxRight - minX, maxBottom - minY), (availableWidth, availableHeight));

        return LayoutSize;
    }

    protected Size LayoutLinearChildren(Size availableSize, int separation, LayoutAxis axis)
    {
        var normalizedSize = NormalizeAvailableSize(availableSize);
        var availableWidth = normalizedSize.Width;
        var availableHeight = normalizedSize.Height;

        var childCount = Children.Count;
        if (childCount == 0)
        {
            SetLayoutSize(Size.Zero);
            return LayoutSize;
        }

        var normalizedSeparation = Math.Max(0, separation);
        var availablePrimary = axis == LayoutAxis.Horizontal ? availableWidth : availableHeight;
        var availableCross = axis == LayoutAxis.Horizontal ? availableHeight : availableWidth;
        var totalSeparation = normalizedSeparation * Math.Max(0, childCount - 1);
        var axisBudget = Math.Max(0, availablePrimary - totalSeparation);

        // Phase 1: combined minimum baseline.
        var slotPrimarySizes = new int[childCount];
        var childMinimumPrimary = new int[childCount];
        var totalMinimumPrimary = 0;

        for (var index = 0; index < childCount; index++)
        {
            var child = Children[index];
            var minimumAvailableSize = axis == LayoutAxis.Horizontal
                ? (Width: availablePrimary, Height: availableCross)
                : (Width: availableCross, Height: availablePrimary);
            var minimumSize = child.GetCombinedMinimumSizeForAvailable(minimumAvailableSize);
            var minimumPrimary = axis == LayoutAxis.Horizontal ? minimumSize.Width : minimumSize.Height;

            childMinimumPrimary[index] = minimumPrimary;
            slotPrimarySizes[index] = minimumPrimary;
            totalMinimumPrimary += minimumPrimary;
        }

        var freePrimary = Math.Max(0, axisBudget - totalMinimumPrimary);
        DistributeFreePrimaryToExpandChildren(slotPrimarySizes, axis, freePrimary);

        // Phase 2: final layout and positioning in slots.
        var cursorPrimary = 0;
        var minX = 0;
        var minY = 0;
        var maxRight = 0;
        var maxBottom = 0;

        for (var index = 0; index < childCount; index++)
        {
            var child = Children[index];
            var slotPrimary = Math.Max(0, slotPrimarySizes[index]);
            var minimumPrimary = Math.Max(0, childMinimumPrimary[index]);
            var sizeFlags = axis == LayoutAxis.Horizontal ? child.SizeFlagsHorizontal : child.SizeFlagsVertical;
            var crossSizeFlags = axis == LayoutAxis.Horizontal ? child.SizeFlagsVertical : child.SizeFlagsHorizontal;
            var fill = sizeFlags.HasFlag(ControlSizeFlags.Fill);
            var crossFill = crossSizeFlags.HasFlag(ControlSizeFlags.Fill);
            var childAvailablePrimary = fill ? slotPrimary : minimumPrimary;

            child.Position = (0, 0);

            var childAvailableSize = axis == LayoutAxis.Horizontal
                ? (Width: childAvailablePrimary, Height: availableCross)
                : (Width: availableCross, Height: childAvailablePrimary);

            child.Layout(childAvailableSize);
            var growthAdjustedPosition = child.Position;

            var childPrimary = axis == LayoutAxis.Horizontal ? child.LayoutSize.Width : child.LayoutSize.Height;
            var childCross = axis == LayoutAxis.Horizontal ? child.LayoutSize.Height : child.LayoutSize.Width;
            var childMinimumSize = child.GetCombinedMinimumSizeForAvailable(childAvailableSize);
            var minimumCross = axis == LayoutAxis.Horizontal ? childMinimumSize.Height : childMinimumSize.Width;
            var finalPrimary = fill
                ? Math.Max(slotPrimary, childPrimary)
                : Math.Min(childPrimary, slotPrimary);
            var finalCross = crossFill ? Math.Max(minimumCross, availableCross) : childCross;
            var primaryOffset = ComputeShrinkOffset(sizeFlags, slotPrimary, finalPrimary, fill);

            child.SetLayoutSize(axis == LayoutAxis.Horizontal
                ? (finalPrimary, finalCross)
                : (finalCross, finalPrimary));

            child.Position = axis == LayoutAxis.Horizontal
                ? (X: cursorPrimary + primaryOffset + growthAdjustedPosition.X, Y: growthAdjustedPosition.Y)
                : (X: growthAdjustedPosition.X, Y: cursorPrimary + primaryOffset + growthAdjustedPosition.Y);

            if (child.Position.X < minX) minX = child.Position.X;
            if (child.Position.Y < minY) minY = child.Position.Y;
            if (child.Position.X + child.LayoutSize.Width > maxRight) maxRight = child.Position.X + child.LayoutSize.Width;
            if (child.Position.Y + child.LayoutSize.Height > maxBottom) maxBottom = child.Position.Y + child.LayoutSize.Height;

            cursorPrimary += finalPrimary;
            if (index < childCount - 1) cursorPrimary += normalizedSeparation;
        }

        var desiredSize = (Width: maxRight - minX, Height: maxBottom - minY);

        ClampSizeToAvailableWithGrow(desiredSize, (availableWidth, availableHeight));

        return LayoutSize;
    }

    private void DistributeFreePrimaryToExpandChildren(int[] slotPrimarySizes, LayoutAxis axis, int freePrimary)
    {
        if (freePrimary <= 0) return;

        var expanders = Children
            .Select((child, index) => (child, index))
            .Where(item =>
            {
                var flags = axis == LayoutAxis.Horizontal ? item.child.SizeFlagsHorizontal : item.child.SizeFlagsVertical;
                return flags.HasFlag(ControlSizeFlags.Expand);
            })
            .ToArray();

        if (expanders.Length == 0) return;

        var hasPositiveRatio = expanders.Any(item => item.child.SizeFlagsStretchRatio > 0f);
        var totalRatio = hasPositiveRatio
            ? expanders.Sum(item => Math.Max(0d, item.child.SizeFlagsStretchRatio))
            : expanders.Length;

        if (totalRatio <= 0d) return;

        double accumulatedRatio = 0d;
        var distributed = 0;

        foreach (var (child, index) in expanders)
        {
            var ratio = hasPositiveRatio ? Math.Max(0d, child.SizeFlagsStretchRatio) : 1d;
            accumulatedRatio += ratio;

            var distributedUpToCurrent = (int)Math.Floor(freePrimary * (accumulatedRatio / totalRatio));
            var extraForCurrent = distributedUpToCurrent - distributed;
            if (extraForCurrent > 0) slotPrimarySizes[index] += extraForCurrent;
            distributed = distributedUpToCurrent;
        }

        var remainder = freePrimary - distributed;
        if (remainder > 0)
        {
            var lastIndex = expanders[^1].index;
            slotPrimarySizes[lastIndex] += remainder;
        }
    }

    private static int ComputeShrinkOffset(
        ControlSizeFlags sizeFlags,
        int slotPrimary,
        int finalPrimary,
        bool fill)
    {
        if (fill) return 0;

        var slack = Math.Max(0, slotPrimary - finalPrimary);
        if (slack == 0) return 0;

        if (sizeFlags.HasFlag(ControlSizeFlags.ShrinkEnd)) return slack;
        if (sizeFlags.HasFlag(ControlSizeFlags.ShrinkCenter)) return slack / 2;

        return 0;
    }

    private static Size NormalizeAvailableSize(Size availableSize) =>
        (
            Math.Max(0, availableSize.Width),
            Math.Max(0, availableSize.Height)
        );
}

internal enum LayoutAxis
{
    Horizontal,
    Vertical,
}
