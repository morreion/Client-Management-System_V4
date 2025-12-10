# Body Systems Overview View Implementation - Code Explanation

## Table of Contents
1. [Overview](#overview)
2. [Data Model Structure](#data-model-structure)
3. [UI Categorization Strategy](#ui-categorization-strategy)
4. [ViewModel Logic](#viewmodel-logic)

---

## Overview

The Body Systems Overview allows a comprehensive review of the client's physical health, covering systems like Immune, Digestive, Structural, etc.

**Files Created:**
- [BodySystemsOverview.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Models/BodySystemsOverview.cs)
- [BodySystemsOverviewRepository.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/Repositories/BodySystemsOverviewRepository.cs)
- [BodySystemsOverviewVM.cs](file:///C:/Users/admin/source/repos/Client-Management-System_V4/ViewModel/BodySystemsOverviewVM.cs)
- [BodySystemsOverview.xaml](file:///C:/Users/admin/source/repos/Client-Management-System_V4/View/BodySystemsOverview.xaml)

---

## Data Model Structure

The model is wide (many columns), reflecting the schema.
```csharp
public class BodySystemsOverview
{
    public string? Immune { get; set; }
    public string? Sleep { get; set; }
    public string? Digestion { get; set; }
    // ... ~20 other properties
}
```

---

## UI Categorization Strategy

Because there are so many fields, dumping them all in one list would be overwhelming. We used **GroupBoxes** inside a **ScrollViewer** to organize them logically.

1.  **Lifestyle & Habits**: Sleep, Snore, Exercise, Smoke/Alc...
2.  **Digestive & Elimination**: Digestion, Bowels, Urination...
3.  **Structural & Physical**: Backache, Joint Pain, Skin...
4.  **Other Systems**: Head, ENT, Thyroid, Mind...

### The ScrollViewer
Crucial for usability on smaller screens or when the form expands.
```xml
<Border ... >
    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <Grid> 
            <!-- Long content here --> 
        </Grid>
    </ScrollViewer>
</Border>
```

---

## ViewModel Logic

Standard CRUD operations are implemented.
- **Search**: Checks multiple key fields (`Immune`, `Digestion`, `Client Name`) allowing practitioners to find "Who had digestive issues?".

```csharp
var sql = @"
    SELECT ... 
    WHERE c.Name LIKE @Search 
       OR b.Immune LIKE @Search 
       OR b.Digestion LIKE @Search ...";
```

---

*Created: December 7, 2025*
