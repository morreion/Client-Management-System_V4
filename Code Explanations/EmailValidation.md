# Email Validation Explanation

This document explains the email validation logic implemented in the Client Management System V4.

## Overview
The goal of the email validation is to ensure that user-entered email addresses follow a standard format before being saved to the database. This prevents invalid data (like "test", "user@", or spaces) from being stored.

## The Logic
The validation logic consists of three main steps:
1. **Check for Existence**: We first check if the email field is not empty.
2. **Trimming**: We remove any leading or trailing whitespace (spaces, tabs) from the user input.
3. **Pattern Matching (Regex)**: We use a Regular Expression (Regex) to verify the format.

### The Regular Expression
The pattern used is:
```regex
^[^@\s]+@[^@\s]+\.[^@\s]+$
```

**Breakdown:**
- `^`: Start of the string.
- `[^@\s]+`: Matches one or more characters that are NOT an `@` symbol or whitespace (the username part).
- `@`: Matches the literal `@` symbol.
- `[^@\s]+`: Matches one or more characters that are NOT an `@` symbol or whitespace (the domain name).
- `\.`: Matches the literal `.` (dot).
- `[^@\s]+`: Matches one or more characters that are NOT an `@` symbol or whitespace (the top-level domain, e.g., "com", "org").
- `$`: End of the string.

This pattern ensures the email has at least one character before the `@`, a domain part, a dot, and a TLD, without any spaces.

## Code Implementation
Here is a typical example of how this is implemented in the ViewModels (e.g., `SupplementsVM.cs`, `ClientVM.cs`, `DistributorVM.cs`):

```csharp
// 1. Check if the user entered an email
if (!string.IsNullOrWhiteSpace(SelectedModel.Email))
{
    // 2. Trim whitespace
    var email = SelectedModel.Email.Trim();
    
    // 3. Define the Regex pattern
    var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    // 4. Validate using Regex.IsMatch
    if (!System.Text.RegularExpressions.Regex.IsMatch(email, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
    {
        // Show error if invalid
        MessageBox.Show($"The email address '{email}' is not valid.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        return; // Stop execution, do not save
    }

    // 5. Update the model with the trimmed email
    SelectedModel.Email = email;
}
```

## Where it is used
This validation logic is applied in the following locations:
1.  **Client View**: When adding or editing a client (`ClientVM.cs`).
2.  **Supplements View**: When managing distributors via the overlay (`SupplementsVM.cs`).
3.  **Distributors View**: When managing distributors directly (`DistributorVM.cs`).
