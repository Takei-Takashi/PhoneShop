﻿using PhoneShopShare.DTOs;
using PhoneShopShare.Responses;

namespace PhoneShopServer.Repositories;

public interface IUserAccount
{
    Task<ServiceResponse> Register(UserDTO model);

    Task<LoginResponse> Login(LoginDTO model);

    Task<UserSession> GetUserByToken(string token);

    Task<LoginResponse> GetRefreshToken(PostRefreshTokenDTO model);
}
