using FundooModel.User;
using FundooRepository.Context;
using FundooRepository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundooRepository.Repository
{
    public class UserRepository : IUserRepository
    {
        public readonly UserDbContext context;
        public UserRepository(UserDbContext context)
        {
            this.context = context;
        }
        public Task<int> RegisterUser(Register register)
        {
            var password = EncryptPassword(register.Password);
            register.Password = password;
            this.context.Register.Add(register);
            var result = this.context.SaveChangesAsync();
            return result;
        }
        public Register LoginUser(Login login)
        {
            try
            {
                var result = this.context.Register.Where(x => x.Email.Equals(login.Email)).FirstOrDefault();
                var decryptPassword = DecryptPassword(result.Password);
                if (decryptPassword.Equals(login.Password))
                {
                    return result;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public Register ResetPassword(ResetPassword reset)
        {
            if (reset.NewPassword.Equals(reset.ConfirmPassword))
            {
                var input = this.context.Register.Where(x => x.Email.Equals(reset.Email)).FirstOrDefault();
                if (input != null)
                {
                    var password = EncryptPassword(reset.NewPassword);
                    input.Password = reset.NewPassword;
                    this.context.Register.Update(input);
                    var result = this.context.SaveChangesAsync();
                    return input;
                }
                return null;
            }
            return null;
        }
        //Password Encrypt and Decrypt
        public string EncryptPassword(string password)
        {
            string strmsg = string.Empty;
            byte[] encode = new byte[password.Length];
            encode = Encoding.UTF8.GetBytes(password);
            strmsg = Convert.ToBase64String(encode);
            return strmsg;
        }
        public string DecryptPassword(string encryptpwd)
        {
            string decryptpwd = string.Empty;
            UTF8Encoding encodepwd = new UTF8Encoding();
            Decoder Decode = encodepwd.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encryptpwd);
            int charCount = Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            decryptpwd = new string(decoded_char);
            return decryptpwd;
        }
    }
}
