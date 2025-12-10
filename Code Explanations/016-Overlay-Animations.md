# 016 - Overlay Animations (Slide In/Out)

**Topic:** Implementing Slide-In and Slide-Out animations for Overlays using Pure XAML.

This document explains how we achieved the smooth "Drawer" animation that slides in when opened and slides out when closed (Back button is clicked), without using any C# code-behind or external libraries.

## The Challenge
Standard WPF `Visibility` binding (`Visible` <-> `Collapsed`) makes elements disappear **instantly** when set to `Collapsed`. This prevents any "Exit" animation (like sliding out or fading out) from being seen because the element is removed from the rendering tree immediately.

## The Solution: Tag Binding & Style Triggers

To solve this, we decoupled the **Animation State** from the immediate **Visibility State**.

### 1. The Styles (`Styles/Page.xaml`)

We created two coordinate styles that trigger based on a `Tag` property being `True` or `False`.

**`Overlay_Grid_Style` (The Background)**
*   **Tag=True (Enter)**: 
    *   Sets `Visibility=Visible` **immediately** via an ObjectAnimation (KeyTime="0").
    *   Fades Opacity 0 -> 1.
*   **Tag=False (Exit)**: 
    *   Fades Opacity 1 -> 0.
    *   Sets `Visibility=Collapsed` **delayed** via an ObjectAnimation (KeyTime="0:0:0.3").

**Crucial Logic**: We do **NOT** use a simple `<Setter Property="Visibility" Value="Visible"/>` inside the `DataTrigger`. If we did, the moment `Tag` becomes False, the setter would be removed, and the element would collapse instantly—killing our exit animation. Instead, we control Visibility entirely within the Storyboards.

```xml
<DataTrigger.EnterActions>
    <BeginStoryboard>
        <Storyboard>
            <!-- Show immediately -->
            <ObjectAnimationUsingKeyFrames Storyboard.TargetProperty="Visibility">
                <DiscreteObjectKeyFrame KeyTime="0" Value="{x:Static Visibility.Visible}"/>
            </ObjectAnimationUsingKeyFrames>
            <DoubleAnimation Storyboard.TargetProperty="Opacity" To="1" Duration="0:0:0.3"/>
        </Storyboard>
    </BeginStoryboard>
</DataTrigger.EnterActions>

<DataTrigger.ExitActions>
    <BeginStoryboard>
        <Storyboard>
            <DoubleAnimation Storyboard.TargetProperty="Opacity" To="0" Duration="0:0:0.3"/>
            <!-- Delay the Collapse until animation finishes -->
            <ObjectAnimationUsingKeyFrames Storyboard.TargetProperty="Visibility">
                <DiscreteObjectKeyFrame KeyTime="0:0:0.3" Value="{x:Static Visibility.Collapsed}"/>
            </ObjectAnimationUsingKeyFrames>
        </Storyboard>
    </BeginStoryboard>
</DataTrigger.ExitActions>
```

### 2. The Implementation (View XAML)

In `EyeAnalysis.xaml` and `Supplements.xaml`, instead of binding `Visibility` directly to our boolean (e.g., `IsManagerOpen`), we bind the `Tag` property to it.

```xml
<!-- Before (Instant Collapse) -->
<Grid Visibility="{Binding IsManagerOpen, Converter={StaticResource BoolToVis}}">

<!-- After (Animated) -->
<Grid Tag="{Binding IsManagerOpen}" Style="{StaticResource Overlay_Grid_Style}">
    <Border Tag="{Binding IsManagerOpen}" Style="{StaticResource Drawer_Style}">
        <!-- Content -->
    </Border>
</Grid>
```

### Why checking `Tag`?
The `Style` triggers need a property to watch. Since `Grid` and `Border` don't have an `IsManagerOpen` property, we use `Tag` (an arbitrary property available on all FrameworkElements) to hold our state. The Style then watches `Tag` to know when to play Enter or Exit animations.
