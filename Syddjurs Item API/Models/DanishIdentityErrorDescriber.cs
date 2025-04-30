using Microsoft.AspNetCore.Identity;

namespace Syddjurs_Item_API.Models
{
    public class DanishIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() =>
       GetLocalizedError(nameof(DefaultError), base.DefaultError());

        public override IdentityError ConcurrencyFailure() =>
            GetLocalizedError(nameof(ConcurrencyFailure), base.ConcurrencyFailure());

        public override IdentityError PasswordMismatch() =>
            GetLocalizedError(nameof(PasswordMismatch), base.PasswordMismatch());

        public override IdentityError InvalidToken() =>
            GetLocalizedError(nameof(InvalidToken), base.InvalidToken());

        public override IdentityError LoginAlreadyAssociated() =>
            GetLocalizedError(nameof(LoginAlreadyAssociated), base.LoginAlreadyAssociated());

        public override IdentityError InvalidUserName(string userName) =>
            GetLocalizedError(nameof(InvalidUserName), base.InvalidUserName(userName), userName);

        public override IdentityError InvalidEmail(string email) =>
            GetLocalizedError(nameof(InvalidEmail), base.InvalidEmail(email));

        public override IdentityError DuplicateUserName(string userName) =>
            GetLocalizedError(nameof(DuplicateUserName), base.DuplicateUserName(userName), userName);

        public override IdentityError DuplicateEmail(string email) =>
            GetLocalizedError(nameof(DuplicateEmail), base.DuplicateEmail(email), email);

        public override IdentityError InvalidRoleName(string roleName) =>
            GetLocalizedError(nameof(InvalidRoleName), base.InvalidRoleName(roleName), roleName);

        public override IdentityError DuplicateRoleName(string roleName) =>
            GetLocalizedError(nameof(DuplicateRoleName), base.DuplicateRoleName(roleName), roleName);

        public override IdentityError UserAlreadyHasPassword() =>
            GetLocalizedError(nameof(UserAlreadyHasPassword), base.UserAlreadyHasPassword());

        public override IdentityError UserLockoutNotEnabled() =>
            GetLocalizedError(nameof(UserLockoutNotEnabled), base.UserLockoutNotEnabled());

        public override IdentityError UserAlreadyInRole(string role) =>
            GetLocalizedError(nameof(UserAlreadyInRole), base.UserAlreadyInRole(role), role);

        public override IdentityError UserNotInRole(string role) =>
            GetLocalizedError(nameof(UserNotInRole), base.UserNotInRole(role), role);

        public override IdentityError PasswordTooShort(int length) =>
            GetLocalizedError(nameof(PasswordTooShort), base.PasswordTooShort(length), length.ToString());

        public override IdentityError PasswordRequiresDigit() =>
            GetLocalizedError(nameof(PasswordRequiresDigit), base.PasswordRequiresDigit());

        public override IdentityError PasswordRequiresLower() =>
            GetLocalizedError(nameof(PasswordRequiresLower), base.PasswordRequiresLower());

        public override IdentityError PasswordRequiresUpper() =>
            GetLocalizedError(nameof(PasswordRequiresUpper), base.PasswordRequiresUpper());

        public override IdentityError PasswordRequiresNonAlphanumeric() =>
            GetLocalizedError(nameof(PasswordRequiresNonAlphanumeric), base.PasswordRequiresNonAlphanumeric());

        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) =>
            GetLocalizedError(nameof(PasswordRequiresUniqueChars), base.PasswordRequiresUniqueChars(uniqueChars), uniqueChars.ToString());

        private IdentityError GetLocalizedError(string code, IdentityError originalError, params string[] args)
        {
            string translated = code switch
            {
                nameof(DefaultError) => "Der opstod en ukendt fejl.",
                nameof(ConcurrencyFailure) => "Data blev ændret af en anden proces. Prøv igen.",
                nameof(PasswordMismatch) => "Forkert adgangskode.",
                nameof(InvalidToken) => "Ugyldigt token.",
                nameof(LoginAlreadyAssociated) => "En bruger med dette login findes allerede.",
                nameof(InvalidUserName) => $"Brugernavnet '{args[0]}' er ugyldigt. Det må kun indeholde bogstaver eller tal.",
                nameof(InvalidEmail) => "E-mailadressen er ugyldig.",
                nameof(DuplicateUserName) => $"Brugernavnet '{args[0]}' er allerede i brug.",
                nameof(DuplicateEmail) => $"E-mailen '{args[0]}' er allerede i brug.",
                nameof(InvalidRoleName) => $"Rollens navn '{args[0]}' er ugyldigt.",
                nameof(DuplicateRoleName) => $"Rollens navn '{args[0]}' er allerede i brug.",
                nameof(UserAlreadyHasPassword) => "Brugeren har allerede en adgangskode.",
                nameof(UserLockoutNotEnabled) => "Låsning er ikke aktiveret for denne bruger.",
                nameof(UserAlreadyInRole) => $"Brugeren er allerede i rollen '{args[0]}'.",
                nameof(UserNotInRole) => $"Brugeren er ikke i rollen '{args[0]}'.",
                nameof(PasswordTooShort) => $"Adgangskoden skal være mindst {args[0]} tegn lang.",
                nameof(PasswordRequiresDigit) => "Adgangskoden skal indeholde mindst ét tal.",
                nameof(PasswordRequiresLower) => "Adgangskoden skal indeholde mindst ét lille bogstav.",
                nameof(PasswordRequiresUpper) => "Adgangskoden skal indeholde mindst ét stort bogstav.",
                nameof(PasswordRequiresNonAlphanumeric) => "Adgangskoden skal indeholde mindst ét specialtegn.",
                nameof(PasswordRequiresUniqueChars) => $"Adgangskoden skal indeholde mindst {args[0]} forskellige tegn.",
                _ => originalError.Description // fallback to English
            };

            return new IdentityError
            {
                Code = code,
                Description = translated
            };
        }
    }
}
