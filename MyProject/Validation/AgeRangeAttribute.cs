using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace MyProject.Validation
{
    public class AgeRangeAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        private readonly int _maxAge;

        public AgeRangeAttribute(int minAge, int maxAge)
        {
            _minAge = minAge;
            _maxAge = maxAge;
        }

        // แก้ไข #1: เพิ่ม ? เพื่อบอกว่า value และ return type สามารถเป็น null ได้
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                // ไม่จำเป็นต้องเช็ก null ที่นี่เพราะ Attribute [Required] จะทำงานก่อน
                // แต่มีไว้ก็ไม่เสียหาย
                return ValidationResult.Success;
            }

            if (DateTime.TryParseExact(value.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob))
            {
                var today = DateTime.Today;
                var age = today.Year - dob.Year;

                if (dob.Date > today.AddYears(-age))
                {
                    age--;
                }

                if (age >= _minAge && age <= _maxAge)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(ErrorMessage ?? $"อายุต้องอยู่ระหว่าง {_minAge} และ {_maxAge} ปี");
                }
            }

            return new ValidationResult("รูปแบบวันเกิดไม่ถูกต้อง");
        }
    }
}