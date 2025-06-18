namespace quiz.Domain.Dto;

public class ValidationResult
{
  public bool IsValid { get; private set; }
  public string ErrorMessage { get; private set; }  //properties
  private ValidationResult(bool isValid, string errorMessage = null!)
  {
    IsValid = isValid;
    ErrorMessage = errorMessage;
  }  //constructor
  public static ValidationResult Success() => new ValidationResult(true);
  public static ValidationResult Failure(string errorMessage) => new ValidationResult(false, errorMessage);  // static methods
}