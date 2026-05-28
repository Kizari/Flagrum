using System.ComponentModel.DataAnnotations;
using System.IO;
using Flagrum.Application.Features.Settings.Data;

namespace Flagrum.Application.Utilities;

public class FileExistsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string stringValue && File.Exists(stringValue))
        {
            return ValidationResult.Success;
        }
        
        return new ValidationResult("File could not be found.");
    }
}

public class DirectoryExistsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string stringValue && Directory.Exists(stringValue))
        {
            return ValidationResult.Success;
        }
        
        return new ValidationResult("Directory could not be found.");
    }
}

public class LaunchCommandFileExists(string variableName) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is LaunchConfiguration config)
        {
            var command = config.LaunchCommand;
            
            if (command?.Contains(variableName) == true)
            {
                if (value == null)
                {
                    return new ValidationResult("File referenced in launch command must have a value.");
                }
                
                if (value is string stringValue && !File.Exists(stringValue))
                {
                    return new ValidationResult("File referenced in launch command could not be found.");
                }
            }
        }
        
        return ValidationResult.Success;
    }
}

public class LaunchCommandDirectoryExists(string variableName) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is LaunchConfiguration config)
        {
            var command = config.LaunchCommand;
            
            if (command?.Contains(variableName) == true)
            {
                if (value == null)
                {
                    return new ValidationResult("Directory referenced in launch command must have a value.");
                }

                if (value is string stringValue && !Directory.Exists(stringValue))
                {
                    return new ValidationResult("Directory referenced in launch command could not be found.");
                }
            }
        }
        
        return ValidationResult.Success;
    }
}