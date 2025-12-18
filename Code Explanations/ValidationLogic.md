# Validation Explanation

This document explains the validation logic implemented in the Client Management System V4.

## 1. Email Validation

The goal of the email validation is to ensure that user-entered email addresses follow a standard format before being saved to the database.

### The Logic
1.  **Check for Existence**: We first check if the email field is not empty.
2.  **Trimming**: We remove any leading or trailing whitespace (spaces, tabs) from the user input.
3.  **Pattern Matching (Regex)**: We use a Regular Expression (Regex) to verify the format.

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

---

## 2. Website Validation

The goal of the website validation is to ensure that user-entered URLs are valid.

### The Logic
1.  **Check for Existence**: We check if the website field is not empty.
2.  **Trimming**: Remove leading/trailing whitespace.
3.  **Pattern Matching (Regex)**: We use a Regex to check for a valid domain structure.

### The Regular Expression
The pattern used is:
```regex
^(https?://)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}(/.*)?$
```
**Breakdown:**
- `^`: Start of the string.
- `(https?://)?`: Optionally matches `http://` or `https://`.
- `([a-zA-Z0-9-]+\.)+`: Matches one or more domain segments (e.g., `google.` or `www.google.`).
- `[a-zA-Z]{2,}`: Matches the Top Level Domain (TLD) which must be at least 2 letters (e.g., `com`, `uk`, `io`).
- `(/.*)?`: Optionally matches the path after the domain.
- `$`: End of the string.

## Code Implementation Example

```csharp
// Website Validation
if (!string.IsNullOrWhiteSpace(SelectedModel.Website))
{
    var website = SelectedModel.Website.Trim();
    var pattern = @"^(https?://)?([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}(/.*)?$";
    
    if (!System.Text.RegularExpressions.Regex.IsMatch(website, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
    {
        MessageBox.Show($"The website '{website}' is not valid.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
    }
    SelectedModel.Website = website;
}
```

## Where it is used
This validation logic is applied in:
1.  **Client View**: Email validation.
2.  **Supplements View**: Email and Website validation in the "Manage Distributors" overlay.
3.  **Distributors View**: Email and Website validation.
