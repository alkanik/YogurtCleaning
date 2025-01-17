using System.ComponentModel.DataAnnotations;
using YogurtCleaning.DataLayer.Enums;
using YogurtCleaning.Infrastructure;
using YogurtCleaning.Models;
using YogurtCleaning.Tests.ModelSources;

namespace YogurtCleaning.Tests;

public class CleanerModelsRequestTests
{
    [TestCaseSource(typeof(CleanerRegisterRequestTestSource))]
    public void CleanerRegisterRequestValidation_WhenInvalidModelPassed_ValidationErrorsReceived(CleanerRegisterRequest cleaner, string errorMessage)
    {
        //given
        var validationResults = new List<ValidationResult>();  
        
        //when
        var isValid = Validator.TryValidateObject(cleaner, new ValidationContext(cleaner), validationResults, true);

        //then
        Assert.IsFalse(isValid);
        var actualMessage = validationResults[0].ErrorMessage;
        Assert.That(actualMessage, Is.EqualTo(errorMessage));
    }

    [TestCaseSource(typeof(CleanerUpdateRequestTestSource))]
    public void CleanerUpdateRequestValidation_WhenInvalidModelPassed_ValidationErrorsReceived(CleanerUpdateRequest cleaner, string errorMessage)
    {
        //given
        var validationResults = new List<ValidationResult>();

        //when
        var isValid = Validator.TryValidateObject(cleaner, new ValidationContext(cleaner), validationResults, true);

        //then
        Assert.IsFalse(isValid);
        var actualMessage = validationResults[0].ErrorMessage;
        Assert.AreEqual(errorMessage, actualMessage);
    }

    [Test]
    public void CleanerRegisterRequestValidation_WhenInvalidModelPassed_ValidationErrorsReceived()
    {
        //given eaner
        CleanerRegisterRequest cleaner = new CleanerRegisterRequest();
        List<string> expectedMessages = new List<string>() {
            ApiErrorMessages.NameIsRequired, 
            ApiErrorMessages.LastNameIsRequired,
            ApiErrorMessages.PasswordIsRequired,
            ApiErrorMessages.ConfirmPasswordIsRequired,
            ApiErrorMessages.EmailIsRequired,
            ApiErrorMessages.PhoneIsRequired,
            ApiErrorMessages.PassportIsRequired,
        };
        var validationResults = new List<ValidationResult>();

        //when
        var isValid = Validator.TryValidateObject(cleaner, new ValidationContext(cleaner), validationResults, true);

        //then
        Assert.IsFalse(isValid);
        for (int i = 0; i < expectedMessages.Count(); i++)
        {
            var actualMessage = validationResults[i].ErrorMessage;
            Assert.That(actualMessage, Is.EqualTo(expectedMessages[i]));
        }
    }

    [Test]
    public void CleanerUpdateRequestValidation_WhenInvalidModelPassed_ValidationErrorsReceived()
    {
        //given
        CleanerUpdateRequest cleaner = new CleanerUpdateRequest();
        List<string> expectedMessages = new List<string>() {
            ApiErrorMessages.NameIsRequired,
            ApiErrorMessages.LastNameIsRequired,    
            ApiErrorMessages.PhoneIsRequired
        };
        var validationResults = new List<ValidationResult>();

        //when
        var isValid = Validator.TryValidateObject(cleaner, new ValidationContext(cleaner), validationResults, true);

        //then
        Assert.IsFalse(isValid);
        for (int i = 0; i < expectedMessages.Count(); i++)
        {
            var actualMessage = validationResults[i].ErrorMessage;
            Assert.That(actualMessage, Is.EqualTo(expectedMessages[i]));
        }
    }

    [TestCase]
    public void CleanerRegisterRequestValidation_WhenValidModelPassed_NoErrorsReceived()
    {
        //given
        CleanerRegisterRequest cleaner = new CleanerRegisterRequest()
        {
            FirstName = "Adam",
            LastName = "Smith",
            Password = "12345678",
            ConfirmPassword = "12345678",
            Email = "AdamSmith@gmail.com",
            Phone = "85559997264",
            Passport = "0000123456",
            Schedule = Schedule.FullTime,
            BirthDate = DateTime.Today,
            ServicesIds = new List<int>() { 1, 2 },
            Districts = new List<DistrictEnum>() { DistrictEnum.Vasileostrovskiy, DistrictEnum.Primorsky }
        };
        var validationResults = new List<ValidationResult>();

        //when
        var isValid = Validator.TryValidateObject(cleaner, new ValidationContext(cleaner), validationResults, true);

        //then
        Assert.IsTrue(isValid);
        Assert.That(validationResults.Count, Is.EqualTo(0));
    }

    [Test]
    public void CleanerUpdateRequestValidation_WhenValidModelPassed_NoErrorsReceived()
    {
        //given
        CleanerUpdateRequest cleaner = new CleanerUpdateRequest()
        {
            FirstName = "Adam",
            LastName = "Smith",
            Phone = "85559997264",
            BirthDate = DateTime.Today,
            ServicesIds = new List<int>() { 1, 2 },
            Districts = new List<DistrictEnum>() { DistrictEnum.Vasileostrovskiy, DistrictEnum.Primorsky }
        };
        var validationResults = new List<ValidationResult>();

        //when
        var isValid = Validator.TryValidateObject(cleaner, new ValidationContext(cleaner), validationResults, true);

        //then
        Assert.IsTrue(isValid);
        Assert.That(validationResults.Count, Is.EqualTo(0));   
    }
}