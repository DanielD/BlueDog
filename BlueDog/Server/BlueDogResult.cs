using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueDog
{
    public enum BlueDogResult {
        Ok,
        EmailInUse,
        InvalidEmailKey,
        MailNotValidated,
        NoSuchUser,
        BadPassword,
        //Error,
        InvalidPasswordValidationKey,
        PasswordValidationKeyExpired,
        ExpiredToken,
        NotSet,
        BadToken,
        EmailValidationKeyExpired
    }
}
