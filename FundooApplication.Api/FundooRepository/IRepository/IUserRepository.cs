﻿using FundooModel.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FundooRepository.IRepository
{
    public interface IUserRepository
    {
        public Task<int> RegisterUser(Register register);
        public string LoginUser(Login login);
        public Register ResetPassword(string email1,ResetPassword reset);
        public string ForgetPassword(string email);
    }
}
