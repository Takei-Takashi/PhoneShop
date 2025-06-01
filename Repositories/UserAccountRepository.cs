using Azure.Core;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using PhoneShopClient.Services;
using PhoneShopServer.Data;
using PhoneShopShare.DTOs;
using PhoneShopShare.Responses;
using System;
using System.Security.Cryptography;
using System.Text;

namespace PhoneShopServer.Repositories;

public class UserAccountRepository(AppDbContext _appDbContext) : IUserAccount
{
    public async Task<LoginResponse> GetRefreshToken(PostRefreshTokenDTO model)
    {
        var decodedToken = WebEncoders.Base64UrlDecode(model.RefreshToken!);
        string normalToken = Encoding.UTF8.GetString(decodedToken);
        var getToken = await _appDbContext.TokenInfos
            .FirstOrDefaultAsync(x => x.RefreshToken == normalToken);
        if (getToken is null) return null!;

        // Generate new token
        var (newAccessToken, NewRefreshoken) = await GenerateTokens();

        // add or update Token info
        await SaveToTokenInfo(getToken.UserId, newAccessToken, NewRefreshoken);
        return new LoginResponse(true, "refresh-token-completed", newAccessToken, NewRefreshoken);
    }

    public async Task<UserSession> GetUserByToken(string token)
    {
        var result = await _appDbContext.TokenInfos
            .FirstOrDefaultAsync(_ => _.AccessToken!.Equals(token));
        if (result is null) return null!;

        var getUserInfo = await _appDbContext.UserAccounts
            .FirstOrDefaultAsync(_ => _.Id == result.UserId);
        if (getUserInfo is null) return null!;

        if (result.ExpiryDate < DateTime.Now) return null!;

        var getUserRole = await _appDbContext.UserRoles
            .FirstOrDefaultAsync(_ => _.UserId == getUserInfo.Id);
        if (getUserRole is null) return null!;

        var roleName = await _appDbContext.SystemRoles
            .FirstOrDefaultAsync(_ => _.Id == getUserRole.RoleId);
        if (roleName is null) return null!;

        return new UserSession()
        {
            Email = getUserInfo.Email,
            Name = getUserInfo.Name,
            Role = roleName.Name
        };
    }

    public async Task<LoginResponse> Login(LoginDTO model)
    {
        if (model is null)
        {
            return new LoginResponse(false, "Model is empty");
        }
        var findUser = await _appDbContext.UserAccounts
            .FirstOrDefaultAsync(_ => _.Email!.Equals(model!.Email!));

        if (findUser is null)
            return new LoginResponse(false, "User not found");

        if (!BCrypt.Net.BCrypt.Verify(model!.Password, findUser.Password))
            return new LoginResponse(false, "Invalid UserName/Password");

        var (accessToken, refreshToken) = await GenerateTokens();

        // add or update Token info
        await SaveToTokenInfo(findUser.Id, accessToken, refreshToken);
        return new LoginResponse(true, "Login Successfull", accessToken, refreshToken);
    }

    private async Task SaveToTokenInfo(int userId, string accessToken, string refreshToken)
    {
        var getUser = await _appDbContext.TokenInfos.FirstOrDefaultAsync(_ => _.UserId == userId);
        if (getUser is null)
        {
            _appDbContext.TokenInfos.Add(new TokenInfo()
            { UserId = userId, AccessToken = accessToken, RefreshToken = refreshToken });
            await Commit();
        }
        else
        {
            getUser.RefreshToken = refreshToken;
            getUser.AccessToken = accessToken;
            getUser.ExpiryDate = DateTime.Now.AddDays(1);
            await Commit();
        }
    }

    private async Task<(string AccessToken, string RefreshToken)> GenerateTokens()
    {
        string accessToken = GenerateToken(256);
        string refreshToken = GenerateToken(64);

        while (!await VerifyToken(accessToken))
            accessToken = GenerateToken(256);

        while (!await VerifyToken(refreshToken))
            refreshToken = GenerateToken(256);

        return (accessToken, refreshToken);
    }

    private async Task<bool> VerifyToken(string refreshToken = null!, string accessToken = null!)
    {
        TokenInfo tokenInfo = new();
        if (!string.IsNullOrEmpty(refreshToken))
        {
            var getRefreshToken = await _appDbContext.TokenInfos
                .FirstOrDefaultAsync(_ => _.RefreshToken!.Equals(refreshToken));
            return getRefreshToken is null ? true : false;
        }
        else
        {
            var getAccessToken = await _appDbContext.TokenInfos
                .FirstOrDefaultAsync(_ => _.AccessToken!.Equals(accessToken));
            return getAccessToken is null;
        }
    }

    private static string GenerateToken(int numberOfBytes) =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(numberOfBytes));

    public async Task<ServiceResponse> Register(UserDTO model)
    {
        if (model is null)
        {
            return new ServiceResponse(false, "Model is empty");
        }

        var findUser = await _appDbContext.UserAccounts.FirstOrDefaultAsync(_ => _.Email!.ToLower().Equals(model.Email!.ToLower()));

        if (findUser is not null)
            return new ServiceResponse(false, "User Registered already");

        var user = _appDbContext.UserAccounts.Add(new UserAccount()
        {
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            Name = model.Name,
            Email = model.Email
        }).Entity;

        await Commit();

        var checkIfAdminIsCreated = await _appDbContext.SystemRoles.FirstOrDefaultAsync(_ => _.Name!.ToLower().Equals("admin"));

        if (checkIfAdminIsCreated is null)
        {
            var result = _appDbContext.SystemRoles.Add(new SystemRole() { Name = "Admin" }).Entity;
            await Commit();
            _appDbContext.UserRoles.Add(new UserRole() { RoleId = result.Id, UserId = user.Id });
            await Commit();
        }
        else
        {
            var checkIfUserIsCreated = await _appDbContext.SystemRoles.FirstOrDefaultAsync(_ => _.Name!.ToLower().Equals("user"));
            int RoleId = 0;
            if (checkIfUserIsCreated is null)
            {
                var userResult = _appDbContext.SystemRoles.Add(new SystemRole() { Name = "User" }).Entity;
                await Commit();
                RoleId = userResult.Id;
            }

            _appDbContext.UserRoles.Add(new UserRole()
            {
                RoleId = RoleId == 0 ? checkIfUserIsCreated!.Id : RoleId,
                UserId = user.Id
            });

            await Commit();
        }
        return new ServiceResponse(true, "Account created");
    }

    private async Task Commit() => await _appDbContext.SaveChangesAsync();
}
