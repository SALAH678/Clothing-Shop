using Domain.Common.Results;

namespace Domain.Users;

public static class UserErrors
{
    public static Error FirstNameRequired => Error.Validation(code: "First_Name_Required", description: "First name is required.");
    public static Error InvalidFirstName => Error.Validation(code: "Invalid_First_Name", description: "Invalid first name format.");
    public static Error LastNameRequired => Error.Validation(code: "Last_Name_Required", description: "Last name is required.");
    public static Error InvalidLastName => Error.Validation(code: "Invalid_Last_Name", description: "Invalid last name format.");
    public static Error EmailRequired => Error.Validation(code: "Email_Required", description: "Email is required.");
    public static Error InvalidEmail => Error.Validation(code: "Invalid_Email", description: "Invalid email format.");
    public static Error PhoneNumberRequired => Error.Validation(code: "Phone_Number_Required", description: "Phone number is required.");
    public static Error InvalidPhoneNumber => Error.Validation(code: "Invalid_Phone_Number", description: "Invalid phone number format.");
    public static Error RoleRequired => Error.Validation(code: "User_Role_Required", description: "User role is required.");
    public static Error InvalidRole => Error.Validation(code: "Invalid_User_Role", description: "Invalid user role.");
    public static Error NoAccounts => Error.Validation(code: "User_No_Accounts", description: "User must have at least one account.");
}
