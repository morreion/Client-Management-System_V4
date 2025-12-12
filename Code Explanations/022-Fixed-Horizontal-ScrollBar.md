# 022-Fixed-Horizontal-ScrollBar

## Overview
Fixed a bug where the horizontal scrollbar required vertical mouse movement to operate. This was caused by an incorrect `RotateTransform` in the ScrollBar style.

## The Issue
The existing style attempted to create a horizontal scrollbar by rotating a vertical one by -90 degrees.
```xml
<!-- OLD (Incorrect) -->
<RotateTransform Angle="-90"/>
```
While this visually reoriented the bar, it caused the logic for the "Track" (the draggable area) to be mismatched with the mouse coordinates. Moving the mouse left/right was interpreted as moving "across" the track, while moving up/down (the original axis) was interpreted as moving "along" the track.

## The Solution
removed the rotation and instead configured the `GridRoot` and `Track` to layout naturally in a horizontal space. The WPF `Track` primitive automatically attempts to orient itself based on the parent ScrollBar's orientation if it has room.

```xml
<!-- NEW (Correct) -->
<Trigger Property="Orientation" Value="Horizontal">
    <!-- 1. Swap Dimensions: Make it short and wide -->
    <Setter Property="Height" Value="20"/>
    <Setter Property="Width" Value="Auto"/>
    
    <!-- 2. Ensure Container allows Horizontal layout -->
    <Setter TargetName="GridRoot" Property="Width" Value="Auto"/>
    <Setter TargetName="GridRoot" Property="Height" Value="20"/>
    
    <!-- 3. Removed RotateTransform (Track handles orientation natively) -->
</Trigger>
```
Now, when the ScrollBar is horizontal, the internal Track lays out horizontally, and Left/Right mouse movements correctly correspond to the scroll position.
