using Api.Framework;
using Api.Framework.Extensions;
using Api.Framework.Helper;
using Api.Framework.Models;
using Api.Framework.Result;
using AutoMapper;
using HappyNotes.Entities;
using HappyNotes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HappyNotes.Api.Controllers;

[Authorize]
public class MastodonApplicationController(
    IMapper mapper,
    IOptions<JwtConfig> jwtConfig,
    IRepositoryBase<MastodonApplication> mastodonApplicationsRepository
) : BaseController
{
    private readonly JwtConfig _jwtConfig = jwtConfig.Value;

    [HttpPost]
    public async Task<ApiResult<bool>> Save(PostMastodonApplicationRequest postMastodonApplicationRequest)
    {
        var mastodonApplication = mapper.Map<MastodonApplication>(postMastodonApplicationRequest);
        var existingApplication = await mastodonApplicationsRepository.GetFirstOrDefaultAsync(
            s => s.InstanceUrl == mastodonApplication.InstanceUrl);

        bool result;
        if (existingApplication != null)
        {
            existingApplication.UpdatedAt = DateTime.UtcNow.ToUnixTimeSeconds();
            existingApplication.ClientId =
                TextEncryptionHelper.Encrypt(mastodonApplication.ClientId, _jwtConfig.SymmetricSecurityKey);
            existingApplication.ClientSecret =
                TextEncryptionHelper.Encrypt(mastodonApplication.ClientSecret,
                    _jwtConfig.SymmetricSecurityKey);
            result = await mastodonApplicationsRepository.UpdateAsync(existingApplication);
            return result ? Success() : Fail("0 rows Updated");
        }

        mastodonApplication.CreatedAt = DateTime.UtcNow.ToUnixTimeSeconds();
        mastodonApplication.ClientId =
            TextEncryptionHelper.Encrypt(mastodonApplication.ClientId, _jwtConfig.SymmetricSecurityKey);
        mastodonApplication.ClientSecret =
            TextEncryptionHelper.Encrypt(mastodonApplication.ClientSecret, _jwtConfig.SymmetricSecurityKey);

        result = await mastodonApplicationsRepository.InsertAsync(mastodonApplication);
        return result ? Success() : Fail("0 rows inserted");
    }

    [HttpGet]
    public async Task<ApiResult<MastodonApplication?>> Get(string instanceUrl)
    {
        var application = await mastodonApplicationsRepository.GetFirstOrDefaultAsync(
            s => s.InstanceUrl.Equals(instanceUrl.ToLower().Trim()));
        if (application is not null)
        {
            application.ClientId = TextEncryptionHelper.Decrypt(application.ClientId, _jwtConfig.SymmetricSecurityKey);
            application.ClientSecret =
                TextEncryptionHelper.Decrypt(application.ClientSecret, _jwtConfig.SymmetricSecurityKey);
        }
        return new SuccessfulResult<MastodonApplication?>(application);
    }
}
