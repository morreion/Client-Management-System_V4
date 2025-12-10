# 018 - DataGrid Header Separators

**Topic:** Customizing DataGrid Column Header Separators (Visibility and Color).

## The Problem
You set `SeparatorVisibility="Visible"` and `SeparatorBrush="Red"`, but nothing appeared. 
This happens because the default WPF `DataGridColumnHeader` style in many themes (especially implicit dark themes like the one we're building) either:
1.  Doesn't include the separator element in its visual tree (`ControlTemplate`).
2.  Has the separator hidden or overlaid by other borders.

## The Solution: Custom ControlTemplate

To guarantee that the separators show up exactly how you want, we must replace the default `ControlTemplate` with one that **explicitly** defines the Separator.

### The Code (`View/Clients.xaml`)

We updated the `DataGrid.ColumnHeaderStyle` to include a `Setter.Value > ControlTemplate`.

```xml
<Style TargetType="DataGridColumnHeader">
    <!-- 1. Define Properties -->
    <Setter Property="SeparatorVisibility" Value="Visible"/>
    <Setter Property="SeparatorBrush" Value="Red"/> <!-- Example Color -->
    
    <!-- 2. Define Template -->
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="DataGridColumnHeader">
                <Grid>
                    <!-- Header Background & Content -->
                    <Border Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter ... />
                    </Border>
                    
                    <!-- THE SEPARATOR (Thumb) -->
                    <!-- Matches standard WPF part name 'PART_RightHeaderGripper' -->
                    <Thumb x:Name="PART_RightHeaderGripper"
                           HorizontalAlignment="Right"
                           Width="1"
                           Background="{TemplateBinding SeparatorBrush}"
                           Visibility="{TemplateBinding SeparatorVisibility}"
                           Cursor="SizeWE"/>
                </Grid>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

### Key Components

*   **`PART_RightHeaderGripper`**: This is the standard name for the separator (which is actually a resize handle, hence "Gripper").
*   **`Width="1"`**: Defines how thick the line is.
*   **`TemplateBinding SeparatorBrush`**: Links the visual color of this Thumb to the property you set in the Style.

Now, whenever you change `SeparatorBrush` or `SeparatorVisibility` in the Style, this explicit element will update accordingly.
