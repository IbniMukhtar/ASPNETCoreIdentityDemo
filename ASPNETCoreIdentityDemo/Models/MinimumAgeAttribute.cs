/*using System.ComponentModel.DataAnnotations;

namespace ASPNETCoreIdentityDemo.Models
{
    public class MinimumAgeAttribute : ValidationAttribute
    {
        int _minimumAge;

        public MinimumAgeAttribute(int minimumAge = 18) // Default minimum age is 18
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                DateTime dob = (DateTime)value;
                DateTime today = DateTime.Today;
                DateTime tomorrow = today.AddDays(1);

                if (dob >= today)
                {
                    return new ValidationResult("DOB must be a past date.");
                }

                int age = today.Year - dob.Year;
                if (dob > today.AddYears(-age))
                {
                    age--;
                }

                if (age < _minimumAge)
                {
                    return new ValidationResult($"Minimum age required is {_minimumAge} years.");
                }
            }
            return ValidationResult.Success;
        }
    }
}






*/
















/*public class MinimumAgeAttribute : ValidationAttribute
{
    int _minimumAge;

    public MinimumAgeAttribute(int minimumAge)
    {
        _minimumAge = minimumAge;
    }

    public override bool IsValid(object value)
    {
        if (value != null)
        {
            DateTime dob = (DateTime)value;
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            if (dob >= today)
            {
                return new ValidationResult("DOB must be a past date.");
            }

            int age = today.Year - dob.Year;
            if (dob > today.AddYears(-age))
            {
                age--;
            }

            if (age < 18)
            {
                return new ValidationResult("Minimum age required is 18 years.");
            }
        }
        return ValidationResult.Success;
    }
}*/